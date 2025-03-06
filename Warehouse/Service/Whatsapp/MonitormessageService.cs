using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Warehouse.Models;


namespace Warehouse.Service.Whatsapp
{
    public class MonitormessageService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitormessageService> _logger;
        private readonly List<string> _phoneNumbers;

        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MonitormessageService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonitormessageService> logger,
            IConfiguration configuration,
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor
            )
        {
              _scopeFactory = scopeFactory;
              _logger = logger;
              _httpClient = httpClient;
              _httpContextAccessor = httpContextAccessor;

            // Obtener la lista de números telefónicos desde la configuración
            _phoneNumbers = configuration.GetSection("Monitoring:PhoneNumbers")
                                      .Get<List<string>>() ?? new List<string> { "5212292063157" };
        }

        private async Task<List<Local>> GetExternalApiData(string type, int idUser)
        {
            try
            {
                //var token = ObtenerTokenDeAutorizacion();
                                   
                var request = new HttpRequestMessage(HttpMethod.Get, "http://198.71.49.16:5004/api/Root/2fields");
                
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var result = JsonSerializer.Deserialize<List<Local>>(content, options);

                _logger.LogInformation($"JSON recibido: {content}");
                _logger.LogInformation($"Datos deserializados: {JsonSerializer.Serialize(result)}");

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("No se obtuvieron datos de la API o la deserialización falló");
                    return new List<Local>();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de la API externa");
                throw;
            }
        }



        private string ObtenerTokenDeAutorizacion()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No se encontró el token de autorización");
            }
            return token;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Advanced Monitor Service iniciado a las: {time}", DateTimeOffset.Now);

            // y aqui el endpoint de los permisos por type=contract y idUse=13 ejemplo
            List<Local> apiData = await GetExternalApiData("root", 0);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var configurationService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();

                    // Obtener la configuración activa
                    var configs = await configurationService.GetAllConfigurations(1);
                    var activeConfig = configs.FirstOrDefault();

                    if (activeConfig == null)
                    {
                        _logger.LogError("No se encontró una configuración activa");
                        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                        continue;
                    }

                    // Determinar el intervalo según MessageDiary
                    int hoursInterval = activeConfig.MessageDiary switch
                    {
                        8 => 8,   // 3 veces al día
                        24 => 24, // 1 vez al día
                        _ => 24   // Por defecto, una vez al día
                    };

                    _logger.LogInformation("Servicio configurado para ejecutarse cada {hours} horas", hoursInterval);

                    // Procesar los productos
                    await ProcessProducts(scope, hoursInterval, stoppingToken);

                    // Esperar hasta el próximo intervalo
                    await Task.Delay(TimeSpan.FromHours(hoursInterval), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de monitoreo detenido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de monitoreo");
                throw;
            }
        }

        private async Task ProcessProducts(IServiceScope scope, int hoursInterval, CancellationToken stoppingToken)
        {
            var context = scope.ServiceProvider.GetRequiredService<DbWarehouseContext>();
            var whatsappService = scope.ServiceProvider.GetRequiredService<ISendWhatsappService>();

            // Obtener los productos con existencias bajas
            var productosBajos = await (from d in context.Detailsinandout
                                        join m in context.Inandouts on d.IdInandout equals m.Id
                                        join t in context.Materials on d.IdProduct equals t.Id
                                        join w in context.Warehouses on m.IdWarehouse equals w.Id
                                        group d by new { t.IdCompany,t.Description, w.Name, t.StockMin, m.IdWarehouse } into g
                                        where g.Sum(x => x.Quantity) <= g.Key.StockMin
                                        orderby g.Key.Name
                                        select new
                                        {
                                            idEmpresa   = g.Key.IdCompany,
                                            Description = g.Key.Description,
                                            Existencia  = g.Sum(x => x.Quantity),
                                            StockMin    = g.Key.StockMin,
                                            Warehouse   = g.Key.Name
                                        })
                                      .ToListAsync(stoppingToken);

            if (productosBajos.Any())
            {
                var currentTime = DateTime.Now;
                string reportTime;

                if (hoursInterval == 24)
                {
                    reportTime = "DIARIO";
                }
                else
                {
                    reportTime = currentTime.Hour < 8 ? "MATUTINO" :
                               currentTime.Hour < 16 ? "VESPERTINO" : "NOCTURNO";
                }

                foreach (var phoneNumber in _phoneNumbers)
                {
                    // Mensaje inicial con hora del reporte
                    var mensajeInicial = $"Hola, ¡un gusto saludarte! Soy BPI, tu asistente virtual 🤖 *HCO*.\n\n⚠️ REPORTE {reportTime} ({currentTime:HH:mm}) - Productos con Existencias Críticas:";
                    await whatsappService.SendWhatsAppMessage(phoneNumber, mensajeInicial);

                    // Agrupar productos por almacén
                    var productosPorAlmacen = productosBajos.GroupBy(p => p.Warehouse);

                    foreach (var grupo in productosPorAlmacen)
                    {
                        var mensajeAlmacen = $"\n\n📍 ALMACÉN: {grupo.Key}\n";
                        mensajeAlmacen += grupo.Select(p =>
                            $"• {p.Description}\n  Existencia: {p.Existencia} unidades\n  Mínimo requerido: {p.StockMin}")
                            .Aggregate((current, next) => current + "\n\n" + next);

                        await whatsappService.SendWhatsAppMessage(phoneNumber, mensajeAlmacen);
                    }
                }
            }
        }

    }
   
}

