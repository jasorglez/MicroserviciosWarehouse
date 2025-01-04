namespace Warehouse.Service.Whatsapp
{

    public class AlertService : IAlertService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AlertService> _logger;

        public AlertService(
            INotificationService notificationService,
            ILogger<AlertService> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ProcessAlert(ProductAlert alert)
        {
            string message = GenerateAlertMessage(alert);
            await _notificationService.SendWhatsAppNotificationAsync(message);
        }

        private string GenerateAlertMessage(ProductAlert alert)
        {
            return alert.Type switch
            {
                AlertType.NoStock =>
                    $"⚠️ URGENTE: El producto {alert.Product.Name} (ID: {alert.Product.Id}) " +
                    $"no tiene existencias. Se requiere reabastecimiento inmediato.",

                AlertType.LowStock =>
                    $"⚡ ALERTA: Stock bajo detectado\n" +
                    $"Producto: {alert.Product.Name}\n" +
                    $"Stock actual: {alert.Product.Stock}\n" +
                    $"Por favor, considere realizar un pedido pronto.",

                AlertType.NearExpiry =>
                    $"📅 AVISO: Producto próximo a caducar\n" +
                    $"Producto: {alert.Product.Name}\n" +
                    $"Fecha de caducidad: {alert.Product.ExpiryDate:dd/MM/yyyy}\n" +
                    $"Stock actual: {alert.Product.Stock}",

                _ => throw new ArgumentException($"Tipo de alerta no soportado: {alert.Type}")
            };
        }
    }

    public interface IAlertService
    {
        Task ProcessAlert(ProductAlert alert);
    }
}

