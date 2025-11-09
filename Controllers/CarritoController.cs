// Controllers/CarritoController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using System.Security.Claims;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Cliente")]
    public class CarritoController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(MascotaDbContext context, IMapper mapper, ILogger<CarritoController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/carrito
        [HttpGet]
        public async Task<ActionResult<CarritoDto>> GetCarrito()
        {
            try
            {
                var cliente = await ObtenerClienteConCarrito();
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Producto)
                            .ThenInclude(p => p.Categoria)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Mascota)
                    .FirstOrDefaultAsync(c => c.ClienteId == cliente.Id);

                if (carrito == null)
                {
                    // Crear carrito si no existe
                    carrito = new Carrito { ClienteId = cliente.Id };
                    _context.Carritos.Add(carrito);
                    await _context.SaveChangesAsync();

                    // Recargar con relaciones
                    carrito = await _context.Carritos
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.Id == carrito.Id);
                }

                return Ok(_mapper.Map<CarritoDto>(carrito));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/carrito/agregar
        [HttpPost("agregar")]
        public async Task<ActionResult<CarritoDto>> AgregarAlCarrito([FromBody] AgregarAlCarritoDto agregarDto)
        {
            try
            {
                // ✅ VERIFICAR SI EL MODELO ES VÁLIDO (IValidatableObject)
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                    return BadRequest(new
                    {
                        mensaje = "Datos de entrada inválidos",
                        errores = errors
                    });
                }

                var cliente = await ObtenerClienteConCarrito();
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                // Obtener o crear carrito
                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.ClienteId == cliente.Id);

                if (carrito == null)
                {
                    carrito = new Carrito { ClienteId = cliente.Id };
                    _context.Carritos.Add(carrito);
                    await _context.SaveChangesAsync();
                }

                // ✅ USAR LA MISMA LÓGICA QUE EL DTO - Considerar 0 como no válido
                bool tieneProducto = agregarDto.ProductoId.HasValue && agregarDto.ProductoId.Value > 0;
                bool tieneMascota = agregarDto.MascotaId.HasValue && agregarDto.MascotaId.Value > 0;

                // Verificar disponibilidad y obtener precio
                decimal precioUnitario = 0;
                CarritoItem? itemExistente = null;
                string? nombreItem = null;

                if (tieneProducto)
                {
                    var producto = await _context.Productos.FindAsync(agregarDto.ProductoId!.Value);
                    if (producto == null || !producto.Activo)
                        return BadRequest("Producto no encontrado o no disponible");

                    if (producto.StockDisponible < agregarDto.Cantidad)
                        return BadRequest($"Stock insuficiente. Disponible: {producto.StockDisponible}");

                    precioUnitario = producto.Precio * (1 - producto.Descuento / 100);
                    itemExistente = carrito.Items.FirstOrDefault(i => i.ProductoId == agregarDto.ProductoId);
                    nombreItem = producto.Nombre;
                }
                else if (tieneMascota)
                {
                    var mascota = await _context.Animales.FindAsync(agregarDto.MascotaId!.Value);
                    if (mascota == null || !mascota.Disponible || mascota.Reservado)
                        return BadRequest("Mascota no encontrada o no disponible");

                    // ✅ Esta validación ya está en el DTO, pero la mantenemos por seguridad
                    if (agregarDto.Cantidad != 1)
                        return BadRequest("Para mascotas, la cantidad debe ser 1");

                    precioUnitario = mascota.Precio;
                    itemExistente = carrito.Items.FirstOrDefault(i => i.MascotaId == agregarDto.MascotaId);
                    nombreItem = mascota.Nombre;
                }

                // Agregar o actualizar item
                if (itemExistente != null)
                {
                    itemExistente.Cantidad += agregarDto.Cantidad;
                    itemExistente.PrecioUnitario = precioUnitario;
                    _logger.LogInformation($"Actualizado item en carrito: {nombreItem}, Cantidad: {itemExistente.Cantidad}");
                }
                else
                {
                    var nuevoItem = new CarritoItem
                    {
                        CarritoId = carrito.Id,
                        ProductoId = tieneProducto ? agregarDto.ProductoId : null,
                        MascotaId = tieneMascota ? agregarDto.MascotaId : null,
                        Cantidad = agregarDto.Cantidad,
                        PrecioUnitario = precioUnitario
                    };
                    carrito.Items.Add(nuevoItem);
                    _logger.LogInformation($"Agregado nuevo item al carrito: {nombreItem}, Cantidad: {agregarDto.Cantidad}");
                }

                carrito.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Recargar carrito con relaciones para el DTO
                await _context.Entry(carrito)
                    .Collection(c => c.Items)
                    .Query()
                    .Include(i => i.Producto)
                        .ThenInclude(p => p.Categoria)
                    .Include(i => i.Mascota)
                    .LoadAsync();

                var carritoDto = _mapper.Map<CarritoDto>(carrito);
                return Ok(carritoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando al carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/carrito/actualizar-cantidad/{itemId}
        [HttpPut("actualizar-cantidad/{itemId}")]
        public async Task<ActionResult<CarritoDto>> ActualizarCantidad(int itemId, [FromBody] ActualizarCantidadCarritoDto actualizarDto)
        {
            try
            {
                var cliente = await ObtenerClienteConCarrito();
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.ClienteId == cliente.Id);

                if (carrito == null)
                    return NotFound("Carrito no encontrado");

                var item = carrito.Items.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                    return NotFound("Item no encontrado en el carrito");

                // Validar stock si es producto
                if (item.ProductoId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                    if (producto == null || producto.StockDisponible < actualizarDto.Cantidad)
                        return BadRequest("Stock insuficiente");
                }
                else if (item.MascotaId.HasValue && actualizarDto.Cantidad != 1)
                {
                    return BadRequest("Para animales, la cantidad debe ser 1");
                }

                item.Cantidad = actualizarDto.Cantidad;
                carrito.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Actualizada cantidad del item {itemId} a {actualizarDto.Cantidad}");

                return await GetCarrito();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error actualizando cantidad del item {itemId}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/carrito/remover/{itemId}
        [HttpDelete("remover/{itemId}")]
        public async Task<ActionResult<CarritoDto>> RemoverDelCarrito(int itemId)
        {
            try
            {
                var cliente = await ObtenerClienteConCarrito();
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.ClienteId == cliente.Id);

                if (carrito == null)
                    return NotFound("Carrito no encontrado");

                var item = carrito.Items.FirstOrDefault(i => i.Id == itemId);
                if (item == null)
                    return NotFound("Item no encontrado en el carrito");

                carrito.Items.Remove(item);
                carrito.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Removido item {itemId} del carrito");

                return await GetCarrito();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removiendo item {itemId} del carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/carrito/vaciar
        [HttpDelete("vaciar")]
        public async Task<ActionResult> VaciarCarrito()
        {
            try
            {
                var cliente = await ObtenerClienteConCarrito();
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.ClienteId == cliente.Id);

                if (carrito == null)
                    return NotFound("Carrito no encontrado");

                carrito.Items.Clear();
                carrito.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Carrito vaciado exitosamente");

                return Ok(new { mensaje = "Carrito vaciado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error vaciando carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/carrito/resumen
        [HttpGet("resumen")]
        public async Task<ActionResult> GetResumenCarrito()
        {
            try
            {
                var cliente = await ObtenerClienteConCarrito();
                if (cliente == null)
                    return NotFound("Cliente no encontrado");

                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.ClienteId == cliente.Id);

                if (carrito == null)
                {
                    return Ok(new
                    {
                        totalItems = 0,
                        total = 0m,
                        tieneItems = false
                    });
                }

                return Ok(new
                {
                    totalItems = carrito.Items.Sum(i => i.Cantidad),
                    total = carrito.Items.Sum(i => i.Cantidad * i.PrecioUnitario),
                    tieneItems = carrito.Items.Any()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen del carrito");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task<Cliente?> ObtenerClienteConCarrito()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
        }
    }
}