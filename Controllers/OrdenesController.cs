// Controllers/OrdenesController.cs
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenesController : ControllerBase
    {
        private readonly IOrdenService _ordenService;
        private readonly ILogger<OrdenesController> _logger;
        private readonly MascotaDbContext _context;

        public OrdenesController(
            IOrdenService ordenService,
            ILogger<OrdenesController> logger,
            MascotaDbContext context)
        {
            _ordenService = ordenService;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetOrdenes()
        {
            try
            {
                var ordenes = await _ordenService.GetOrdenesAsync();
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo órdenes");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("mis-ordenes")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetMisOrdenes()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (cliente == null)
                    return NotFound(new { mensaje = "Cliente no encontrado" });

                var ordenes = await _ordenService.GetOrdenesAsync(cliente.Id);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mis órdenes");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("desde-carrito")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<CheckoutResponseDto>> CrearOrdenDesdeCarrito([FromBody] CrearOrdenDesdeCarritoDto crearOrdenDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (cliente == null)
                    return BadRequest(new { mensaje = "Cliente no encontrado" });

                var response = await _ordenService.CrearOrdenDesdeCarritoAsync(cliente.Id, crearOrdenDto.Comentarios);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando orden desde carrito");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<CheckoutResponseDto>> CreateOrden([FromBody] CreateOrdenDto createOrdenDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (cliente == null)
                    return BadRequest(new { mensaje = "Cliente no encontrado" });

                var response = await _ordenService.CrearOrdenDirectaAsync(createOrdenDto, cliente.Id);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando orden directa");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenDto>> GetOrden(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var usuarioRol = User.FindFirst(ClaimTypes.Role)?.Value;

                var orden = await _ordenService.GetOrdenAsync(id, usuarioId, usuarioRol);
                if (orden == null)
                    return NotFound(new { mensaje = "Orden no encontrada" });

                return orden;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo orden {OrdenId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("verificar-pago/{ordenId}")]
        public async Task<ActionResult> VerificarPago(int ordenId)
        {
            try
            {
                var resultado = await _ordenService.VerificarPagoAsync(ordenId);
                if (resultado)
                    return Ok(new { mensaje = "Pago verificado - Orden completada", estado = "Completada" });
                else
                    return Ok(new { mensaje = "Pago aún pendiente", estado = "Pendiente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando pago para orden {OrdenId}", ordenId);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("cancelar/{id}")]
        public async Task<ActionResult> CancelarOrden(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var usuarioRol = User.FindFirst(ClaimTypes.Role)?.Value;

                var resultado = await _ordenService.CancelarOrdenAsync(id, usuarioId, usuarioRol);
                if (!resultado)
                    return BadRequest(new { mensaje = "No se pudo cancelar la orden" });

                return Ok(new { mensaje = "Orden cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando orden {OrdenId}", id);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("estado/{estado}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetOrdenesPorEstado(OrdenEstado estado)
        {
            try
            {
                var ordenes = await _ordenService.GetOrdenesPorEstadoAsync(estado);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo órdenes por estado {Estado}", estado);
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("confirmar-pago")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmarPago([FromQuery] string session_id)
        {
            try
            {
                // Este endpoint es llamado por Stripe después del pago exitoso
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(session_id);

                if (session.PaymentStatus == "paid")
                {
                    // Buscar la orden por session_id
                    var orden = await _context.Ordenes
                        .FirstOrDefaultAsync(o => o.StripeSessionId == session_id);

                    if (orden != null)
                    {
                        await _ordenService.VerificarPagoAsync(orden.Id);
                        return Redirect($"/checkout/success?orden_id={orden.Id}");
                    }
                }

                return Redirect("/checkout/error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirmando pago para session {SessionId}", session_id);
                return Redirect("/checkout/error");
            }
        }
    }
}