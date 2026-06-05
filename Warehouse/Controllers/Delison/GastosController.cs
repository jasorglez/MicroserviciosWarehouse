using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Models.DTOs;
using Warehouse.Service.Delison;

namespace Warehouse.Controllers.Delison
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GastosController : ControllerBase
    {
        private readonly IGastosService _service;
        private readonly ILogger<GastosController> _logger;

        public GastosController(IGastosService service, ILogger<GastosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Reporte gerencial de gastos agregado por Sucursal × Departamento.
        /// </summary>
        /// <param name="idCompany">Empresa (obligatorio).</param>
        /// <param name="startDate">Inicio del periodo (yyyy-MM-dd). Default: hoy.</param>
        /// <param name="endDate">Fin del periodo (yyyy-MM-dd). Default: hoy.</param>
        /// <param name="lens">"PAGADO" (default, cash out por fecha_recepcion) o "COMPROMETIDO" (por datecreate de OC).</param>
        [HttpGet("report")]
        public async Task<ActionResult<ExpenseReportDto>> GetExpenseReport(
            [FromQuery] int idCompany,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string lens = "PAGADO")
        {
            try
            {
                if (idCompany <= 0)
                    return BadRequest("idCompany es obligatorio.");

                var start = startDate ?? DateTime.Today;
                var end = endDate ?? DateTime.Today;

                if (end < start)
                    return BadRequest("endDate no puede ser anterior a startDate.");

                var result = await _service.GetExpenseReport(idCompany, start, end, lens);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reporte de gastos. Company={IdCompany}", idCompany);
                return StatusCode(500, "Error generando el reporte de gastos.");
            }
        }

        /// <summary>
        /// Captura de Gastos: entradas pendientes de pago (cerradas en molienda, sin liberar) de la empresa.
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<List<PendingPaymentDto>>> GetPendingPayments([FromQuery] int idCompany)
        {
            try
            {
                if (idCompany <= 0)
                    return BadRequest("idCompany es obligatorio.");

                var result = await _service.GetPendingPayments(idCompany);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando entradas pendientes de pago. Company={IdCompany}", idCompany);
                return StatusCode(500, "Error listando entradas pendientes de pago.");
            }
        }

        /// <summary>
        /// Histórico de Pagos: entradas ya pagadas/liberadas (liberacion=1) de la empresa.
        /// </summary>
        [HttpGet("paid")]
        public async Task<ActionResult<List<PendingPaymentDto>>> GetPaidPayments([FromQuery] int idCompany)
        {
            try
            {
                if (idCompany <= 0)
                    return BadRequest("idCompany es obligatorio.");

                var result = await _service.GetPaidPayments(idCompany);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando pagos. Company={IdCompany}", idCompany);
                return StatusCode(500, "Error listando el histórico de pagos.");
            }
        }

        /// <summary>
        /// Confirma el pago de UNA entrada (Captura de Gastos): genera pago + libera + writeback.
        /// </summary>
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
        {
            try
            {
                if (dto == null || dto.IdEntrada <= 0)
                    return BadRequest("idEntrada es obligatorio.");

                var ok = await _service.ConfirmPayment(dto);
                if (!ok) return NotFound("Entrada no encontrada.");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirmando pago. Entrada={IdEntrada}", dto?.IdEntrada);
                return StatusCode(500, "Error confirmando el pago.");
            }
        }

        /// <summary>
        /// Guarda los campos editables de una entrada SIN concluir el pago (no libera).
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SavePending([FromBody] ConfirmPaymentDto dto)
        {
            try
            {
                if (dto == null || dto.IdEntrada <= 0)
                    return BadRequest("idEntrada es obligatorio.");

                var ok = await _service.SavePending(dto);
                if (!ok) return NotFound("Entrada no encontrada.");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando borrador. Entrada={IdEntrada}", dto?.IdEntrada);
                return StatusCode(500, "Error guardando los cambios.");
            }
        }

        /// <summary>
        /// Ingresa una entrada "a crédito": material disponible (placeholder almacén) + pago pendiente a N días.
        /// </summary>
        [HttpPost("activar-credito")]
        public async Task<IActionResult> ActivarCredito([FromBody] ActivarCreditoDto dto)
        {
            try
            {
                if (dto == null || dto.IdEntrada <= 0)
                    return BadRequest("idEntrada es obligatorio.");

                var ok = await _service.ActivarCredito(dto);
                if (!ok) return NotFound("Entrada no encontrada.");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activando crédito. Entrada={IdEntrada}", dto?.IdEntrada);
                return StatusCode(500, "Error activando el crédito.");
            }
        }

        /// <summary>
        /// Marca el anticipo de una OC como pagado (desde el grid de Órdenes de Compra).
        /// </summary>
        [HttpPost("marcar-anticipo")]
        public async Task<IActionResult> MarcarAnticipo([FromBody] MarcarAnticipoDto dto)
        {
            try
            {
                if (dto == null || dto.IdOc <= 0)
                    return BadRequest("idOc es obligatorio.");

                var ok = await _service.MarcarAnticipo(dto);
                if (!ok) return NotFound("OC no encontrada.");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marcando anticipo. OC={IdOc}", dto?.IdOc);
                return StatusCode(500, "Error marcando el anticipo.");
            }
        }

        /// <summary>
        /// Paga un anticipo EN TRÁMITE desde la Captura de Gastos: lo marca PAGADO con la fecha del pago
        /// y actualiza la OC (anticipo_estado='PAGADO', anticipo_pagado=true).
        /// </summary>
        [HttpPost("confirm-anticipo")]
        public async Task<IActionResult> ConfirmAnticipo([FromBody] ConfirmAnticipoDto dto)
        {
            try
            {
                if (dto == null || dto.IdGastoGeneral <= 0)
                    return BadRequest("idGastoGeneral es obligatorio.");

                var ok = await _service.ConfirmAnticipo(dto);
                if (!ok) return NotFound("Anticipo no encontrado.");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirmando anticipo. Gasto={IdGasto}", dto?.IdGastoGeneral);
                return StatusCode(500, "Error confirmando el anticipo.");
            }
        }
    }
}
