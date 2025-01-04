namespace Warehouse.Service.Whatsapp
{

    public class MonitormessageService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitormessageService> _logger;
        private readonly ISendWhatsappService _sendWhatsAppService;
        private readonly PeriodicTimer _timer;

        public MonitormessageService(
            IServiceScopeFactory scopeFactory,
            ISendWhatsappService sendWhatsAppService,
            ILogger<MonitormessageService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _sendWhatsAppService = sendWhatsAppService;
            _logger = logger;

            // Configura el intervalo (ejemplo: cada 30 minutos)
            var intervalMinutes = configuration.GetValue<int>("Monitoring:IntervalMinutes", 30);
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        }

        
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Advanced Monitor Service iniciado a las: {time}", DateTimeOffset.Now);

            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();

                    // Obtener el contexto de la base de datos
                    var contexto = scope.ServiceProvider.GetRequiredService<tuContexto>();

                    // Obtener los productos con existencias bajas
                    var productosBajos = await (from d in contexto.detailsinandout
                                                join m in contexto.inandout on d.id_inandout equals m.id
                                                join t in contexto.materials on d.id_product equals t.id
                                                join w in contexto.warehouses on m.id_warehouse equals w.id
                                                group d by new { t.description, w.name, t.stockmin, m.id_warehouse } into g
                                                where g.Sum(x => x.quantity) <= g.Key.stockmin
                                                orderby g.Key.name
                                                select new
                                                {
                                                    Description = g.Key.description,
                                                    Existencia = g.Sum(x => x.quantity),
                                                    StockMin = g.Key.stockmin,
                                                    Warehouse = g.Key.name
                                                }).ToListAsync();

                    // Enviar los productos con existencias bajas por WhatsApp
                    var whatsappService = scope.ServiceProvider.GetRequiredService<ISendWhatsappService>();
                    foreach (var producto in productosBajos)
                    {
                        var mensaje = $"Producto {producto.Description} con existencia baja ({producto.Existencia} unidades) en el almacén {producto.Warehouse}.";
                        await whatsappService.SendWhatsAppMessage("tu_numero_de_telefono", mensaje);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de monitoreo detenido");
            }
        }
        
    }
}