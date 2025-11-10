using Mascotas.Dto;
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderTrackingController : ControllerBase
    {
        private readonly IOrderTrackingService _trackingService;

        public OrderTrackingController(IOrderTrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpGet("{trackingNumber}")]
        public async Task<ActionResult<OrderTrackingDto>> GetOrderTracking(string trackingNumber)
        {
            try
            {
                var trackingInfo = await _trackingService.GetOrderTrackingAsync(trackingNumber);

                if (trackingInfo == null)
                    return NotFound($"No se encontró orden con el número de tracking: {trackingNumber}");

                return Ok(trackingInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<OrderTrackingDto>>> GetClientOrders(int clienteId)
        {
            try
            {
                var orders = await _trackingService.GetUserOrderHistoryAsync(clienteId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("{orderId}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                await _trackingService.UpdateOrderStatusAsync(orderId, request.NewStatus, request.Description);
                return Ok(new { message = "Estado actualizado correctamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("{orderId}/generate-tracking")]
        public async Task<ActionResult<string>> GenerateTrackingNumber(int orderId)
        {
            try
            {
                var trackingNumber = await _trackingService.GenerateTrackingNumberAsync(orderId);
                return Ok(new { trackingNumber });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpPost("{orderId}/tracking-event")]
        public async Task<ActionResult> AddTrackingEvent(int orderId, [FromBody] AddTrackingEventRequest request)
        {
            try
            {
                var result = await _trackingService.AddTrackingEventAsync(orderId, request.Status, request.Description, request.Location);

                if (!result)
                    return NotFound($"Orden con ID {orderId} no encontrada");

                return Ok(new { message = "Evento de tracking agregado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }

    
}