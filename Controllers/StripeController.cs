// Controllers/StripeController.cs
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Stripe;
using System.Text;
using Stripe.Checkout;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IReservaService _reservaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeController> _logger;

        public StripeController(
            MascotaDbContext context,
            IReservaService reservaService,
            IConfiguration configuration,
            ILogger<StripeController> logger)
        {
            _context = context;
            _reservaService = reservaService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                _logger.LogInformation($"Webhook recibido: {stripeEvent.Type}");

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;

                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;

                    case "checkout.session.async_payment_succeeded":
                        await HandleCheckoutSessionAsyncPaymentSucceeded(stripeEvent);
                        break;

                    case "checkout.session.async_payment_failed":
                        await HandleCheckoutSessionAsyncPaymentFailed(stripeEvent);
                        break;

                    case "checkout.session.expired":
                        await HandleCheckoutSessionExpired(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation($"Evento no manejado: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError($"Error de Stripe: {e.Message}");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error general: {e.Message}");
                return StatusCode(500);
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            _logger.LogInformation($"PaymentIntent succeeded: {paymentIntent.Id}");

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntent.Id);

            if (orden != null && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ CONFIRMAR RESERVA (mover de reservado a vendido)
                await _reservaService.ConfirmarReservaAsync(orden.Id);

                orden.Estado = OrdenEstado.Confirmada;
                orden.FechaConfirmacion = DateTime.UtcNow;
                orden.ReservaActiva = false;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Orden {orden.NumeroOrden} confirmada - Reserva procesada");
            }
        }

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            _logger.LogInformation($"Checkout session completed: {session.Id}, PaymentStatus: {session.PaymentStatus}");

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (orden == null) return;

            if (session.PaymentStatus == "paid" && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ CONFIRMAR RESERVA (mover de reservado a vendido)
                await _reservaService.ConfirmarReservaAsync(orden.Id);

                orden.Estado = OrdenEstado.Confirmada;
                orden.FechaConfirmacion = DateTime.UtcNow;
                orden.StripePaymentIntentId = session.PaymentIntentId;
                orden.ReservaActiva = false;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Checkout completado - Orden {orden.NumeroOrden} confirmada - Reserva procesada");
            }
            else if (session.PaymentStatus == "unpaid" && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ LIBERAR reserva si el pago no se completó
                await _reservaService.LiberarReservaAsync(orden.Id);
                _logger.LogWarning($"Checkout no pagado - Orden {orden.NumeroOrden} cancelada - Reserva liberada");
            }
        }

        private async Task HandleCheckoutSessionAsyncPaymentSucceeded(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            _logger.LogInformation($"Async payment succeeded: {session.Id}");

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (orden != null && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ CONFIRMAR RESERVA (mover de reservado a vendido)
                await _reservaService.ConfirmarReservaAsync(orden.Id);

                orden.Estado = OrdenEstado.Confirmada;
                orden.FechaConfirmacion = DateTime.UtcNow;
                orden.ReservaActiva = false;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Async payment succeeded - Orden {orden.NumeroOrden} confirmada");
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            _logger.LogWarning($"PaymentIntent failed: {paymentIntent.Id}");

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntent.Id);

            if (orden != null && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ LIBERAR reserva si el pago falla
                await _reservaService.LiberarReservaAsync(orden.Id);
                _logger.LogWarning($"Pago fallido - Orden {orden.NumeroOrden} cancelada - Reserva liberada");
            }
        }

        private async Task HandleCheckoutSessionAsyncPaymentFailed(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            _logger.LogWarning($"Async payment failed: {session.Id}");

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (orden != null && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ LIBERAR reserva si el pago async falla
                await _reservaService.LiberarReservaAsync(orden.Id);
                _logger.LogWarning($"Async payment failed - Orden {orden.NumeroOrden} cancelada - Reserva liberada");
            }
        }

        private async Task HandleCheckoutSessionExpired(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            _logger.LogInformation($"Checkout session expired: {session.Id}");

            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (orden != null && orden.Estado == OrdenEstado.Pendiente)
            {
                // ✅ LIBERAR reserva si la sesión expira
                await _reservaService.LiberarReservaAsync(orden.Id);
                _logger.LogInformation($"Sesión expirada - Orden {orden.NumeroOrden} cancelada - Reserva liberada");
            }
        }

        [HttpGet("session-status/{sessionId}")]
        public async Task<IActionResult> GetSessionStatus(string sessionId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                // Obtener información de la orden asociada
                var orden = await _context.Ordenes
                    .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);

                return Ok(new
                {
                    sessionId = session.Id,
                    paymentStatus = session.PaymentStatus,
                    status = session.Status,
                    paymentIntentId = session.PaymentIntentId,
                    orden = orden == null ? null : new
                    {
                        id = orden.Id,
                        numeroOrden = orden.NumeroOrden,
                        estado = orden.Estado.ToString(),
                        reservaActiva = orden.ReservaActiva,
                        expiracionReserva = orden.FechaExpiracionReserva
                    }
                });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        // ✅ NUEVO: Endpoint para forzar liberación de reserva (útil para testing)
        [HttpPost("liberar-reserva/{ordenId}")]
        public async Task<IActionResult> LiberarReserva(int ordenId)
        {
            try
            {
                var orden = await _context.Ordenes.FindAsync(ordenId);
                if (orden == null)
                    return NotFound("Orden no encontrada");

                if (orden.Estado != OrdenEstado.Pendiente)
                    return BadRequest("Solo se pueden liberar reservas de órdenes pendientes");

                await _reservaService.LiberarReservaAsync(ordenId);

                return Ok(new { mensaje = $"Reserva liberada para orden {orden.NumeroOrden}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error liberando reserva para orden {ordenId}");
                return StatusCode(500, "Error liberando reserva");
            }
        }

        // ✅ NUEVO: Endpoint para verificar estado de reservas
        [HttpGet("estado-reservas")]
        public async Task<IActionResult> GetEstadoReservas()
        {
            var reservasActivas = await _context.Ordenes
                .Where(o => o.ReservaActiva && o.Estado == OrdenEstado.Pendiente)
                .Select(o => new
                {
                    ordenId = o.Id,
                    numeroOrden = o.NumeroOrden,
                    fechaCreacion = o.FechaCreacion,
                    expiracionReserva = o.FechaExpiracionReserva,
                    minutosRestantes = (int)(o.FechaExpiracionReserva - DateTime.UtcNow).TotalMinutes,
                    itemsCount = o.Items.Count
                })
                .ToListAsync();

            var productosReservados = await _context.Productos
                .Where(p => p.StockReservado > 0)
                .Select(p => new
                {
                    productoId = p.Id,
                    nombre = p.Nombre,
                    stockReservado = p.StockReservado,
                    stockDisponible = p.StockDisponible,
                    expiracionReserva = p.ExpiracionReserva
                })
                .ToListAsync();

            var animalesReservados = await _context.Animales
                .Where(a => a.Reservado)
                .Select(a => new
                {
                    animalId = a.Id,
                    nombre = a.Nombre,
                    expiracionReserva = a.ExpiracionReserva
                })
                .ToListAsync();

            return Ok(new
            {
                reservasActivas,
                productosReservados,
                animalesReservados,
                totalReservasActivas = reservasActivas.Count,
                timestamp = DateTime.UtcNow
            });
        }
    }
}