using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models;
using Warehouse.Service.Whatsapp;
using static Warehouse.Service.Whatsapp.SendWhatsappService;

namespace Warehouse.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class SendController : ControllerBase
    {

        private readonly ISendWhatsappService _whatsAppService;

        public SendController(ISendWhatsappService whatsAppService)
        {
            _whatsAppService = whatsAppService;

        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SendMessages message)
        {
            // Lógica para procesar el mensaje y generar una respuesta
            var responseMessage = message.message;
            await _whatsAppService.SendWhatsAppMessage(message.to, responseMessage);
            return Ok(new { response = responseMessage });
        }
    }
}
