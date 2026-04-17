using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonitormessageController : ControllerBase
    {
        private readonly ILogger<MonitormessageController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public MonitormessageController(
            IServiceScopeFactory scopeFactory,
            ILogger<MonitormessageController> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        [HttpGet("status")]
        public IActionResult GetMonitorStatus()
        {
            try
            {
                return Ok(new { Status = "Monitor service is running" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking monitor status");
                return StatusCode(500, "Error checking monitor status");
            }
        }

        [HttpGet("check-now")]
        public async Task<IActionResult> ForceCheck()
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<DbWarehouseContext>();
                //var whatsappService = scope.ServiceProvider.GetRequiredService<ISendWhatsappService>();

                var productosBajos = await (from d in context.Detailsinandout
                                            join m in context.Inandouts on d.IdInandout equals m.Id
                                            join t in context.Materials on d.IdProduct equals t.Id
                                            join w in context.Warehouses on m.IdWarehouse equals w.Id
                                            group d by new { t.Description, w.Name, t.StockMin, m.IdWarehouse } into g
                                            where g.Sum(x => x.Quantity) <= g.Key.StockMin
                                            orderby g.Key.Name
                                            select new
                                            {
                                                Description = g.Key.Description,
                                                Existencia = g.Sum(x => x.Quantity),
                                                StockMin = g.Key.StockMin,
                                                Warehouse = g.Key.Name
                                            }).ToListAsync();

                if (productosBajos.Any())
                {
                    foreach (var producto in productosBajos)
                    {
                        var mensaje = $"⚡Producto {producto.Description} con existencia baja ({producto.Existencia} unidades) en el almacén {producto.Warehouse}.";
                        // await whatsappService.SendWhatsAppMessage("5212292063157", mensaje);
                       // await whatsappService.SendWhatsAppMessage("5219241550400", mensaje);
                    }

                    return Ok(new
                    {
                        Message = "Verificación completada",
                        ProductosBajos = productosBajos,
                        TotalProductos = productosBajos.Count
                    });
                }
                else
                {
                    return Ok(new { Message = "No hay productos con stock bajo" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al forzar verificación de stock");
                return StatusCode(500, "Error al verificar stock");
            }
        }
    }
}
