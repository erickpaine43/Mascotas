using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Stripe.Checkout;
using System.Security.Claims;

namespace Mascotas.Services
{
    public class OrdenService : IOrdenService
    {
        private readonly MascotaDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly IReservaService _reservaService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdenService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrdenService(
            MascotaDbContext context,
            IStripeService stripeService,
            IReservaService reservaService,
            IMapper mapper,
            ILogger<OrdenService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _stripeService = stripeService;
            _reservaService = reservaService;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrdenDto?> GetOrdenAsync(int id, int usuarioId, string usuarioRol)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Animal)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                            .ThenInclude(p => p.Categoria)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (orden == null) return null;

                // Verificar permisos
                if (usuarioRol != "Administrador" && usuarioRol != "Gerente")
                {
                    var cliente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                    if (cliente == null || orden.ClienteId != cliente.Id)
                    {
                        return null;
                    }
                }

                return _mapper.Map<OrdenDto>(orden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo orden {id}");
                throw;
            }
        }

        public async Task<List<OrdenDto>> GetOrdenesAsync(int? clienteId = null)
        {
            try
            {
                var query = _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Animal)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                            .ThenInclude(p => p.Categoria)
                    .AsQueryable();

                if (clienteId.HasValue)
                {
                    query = query.Where(o => o.ClienteId == clienteId.Value);
                }

                var ordenes = await query
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return _mapper.Map<List<OrdenDto>>(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo órdenes");
                throw;
            }
        }

        public async Task<CheckoutResponseDto> CrearOrdenDesdeCarritoAsync(int clienteId, string? comentarios = null)
        {


            try
            {
                // 1. Obtener carrito del cliente
                var carrito = await _context.Carritos
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Producto)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Mascota)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

                if (carrito == null || !carrito.Items.Any())
                    throw new InvalidOperationException("El carrito está vacío");

                // 2. Crear orden desde carrito
                var orden = new Orden
                {
                    NumeroOrden = GenerateOrderNumber(),
                    ClienteId = clienteId,
                    Estado = OrdenEstado.Pendiente,
                    MetodoPago = MetodoPago.Stripe,
                    Comentarios = comentarios,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracionReserva = DateTime.UtcNow.AddMinutes(15),
                    ReservaActiva = false
                };

                // 3. Convertir items del carrito a items de orden
                decimal subtotal = 0;
                foreach (var carritoItem in carrito.Items)
                {
                    // Validar disponibilidad
                    if (carritoItem.ProductoId.HasValue)
                    {
                        var producto = carritoItem.Producto;
                        if (producto == null || !producto.Activo)
                            throw new InvalidOperationException($"Producto no disponible o no encontrado");

                        if (producto.StockDisponible < carritoItem.Cantidad)
                            throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockDisponible}");

                        var ordenItem = new OrdenItem
                        {
                            ProductoId = producto.Id,
                            Cantidad = carritoItem.Cantidad,
                            PrecioUnitario = carritoItem.PrecioUnitario,
                            Subtotal = carritoItem.PrecioUnitario * carritoItem.Cantidad
                        };
                        orden.Items.Add(ordenItem);
                        subtotal += ordenItem.Subtotal;
                    }
                    else if (carritoItem.MascotaId.HasValue)
                    {
                        var mascota = carritoItem.Mascota;
                        if (mascota == null || !mascota.Disponible || mascota.Reservado)
                            throw new InvalidOperationException($"Mascota no disponible o no encontrada");

                        var ordenItem = new OrdenItem
                        {
                            AnimalId = mascota.Id,
                            Cantidad = 1,
                            PrecioUnitario = mascota.Precio,
                            Subtotal = mascota.Precio
                        };
                        orden.Items.Add(ordenItem);
                        subtotal += ordenItem.Subtotal;
                    }
                }

                orden.Subtotal = subtotal;
                orden.Impuesto = subtotal * 0.16m;
                orden.Total = orden.Subtotal + orden.Impuesto;

                // 4. Guardar orden (sin transacción explícita)
                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();

                // 5. Reservar items - DELEGAR la gestión de transacciones al servicio de reserva
                var (reservaExitosa, mensajeError) = await _reservaService.ReservarItemsAsync(orden);
                if (!reservaExitosa)
                {
                    // Si falla la reserva, eliminar la orden y hacer rollback implícito
                    _context.Ordenes.Remove(orden);
                    await _context.SaveChangesAsync();
                    throw new InvalidOperationException(mensajeError);
                }

                // 6. Crear checkout de Stripe
                var httpContext = _httpContextAccessor.HttpContext;
                var successUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/ordenes/confirmar-pago?session_id={{CHECKOUT_SESSION_ID}}";
                var cancelUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/checkout/cancel";

                var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(orden, successUrl, cancelUrl);

                // 7. Vaciar carrito solo si todo salió bien
                carrito.Items.Clear();
                carrito.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // 8. Cargar orden completa para DTO
                var ordenCompleta = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Animal)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                            .ThenInclude(p => p.Categoria)
                    .FirstOrDefaultAsync(o => o.Id == orden.Id);

                var ordenDto = _mapper.Map<OrdenDto>(ordenCompleta);

                return new CheckoutResponseDto
                {
                    SessionUrl = sessionUrl,
                    SessionId = orden.StripeSessionId,
                    Orden = ordenDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando orden desde carrito para cliente {ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<CheckoutResponseDto> CrearOrdenDirectaAsync(CreateOrdenDto createOrdenDto, int clienteId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Verificar disponibilidad
                if (!await _reservaService.VerificarDisponibilidadAsync(createOrdenDto))
                    throw new InvalidOperationException("Uno o más items no están disponibles");

                // 2. Validar items
                if (createOrdenDto.Items == null || !createOrdenDto.Items.Any())
                    throw new InvalidOperationException("La orden debe tener al menos un item");

                // 3. Crear orden
                var orden = new Orden
                {
                    NumeroOrden = GenerateOrderNumber(),
                    ClienteId = clienteId,
                    Estado = OrdenEstado.Pendiente,
                    MetodoPago = MetodoPago.Stripe,
                    Comentarios = createOrdenDto.Comentarios,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracionReserva = DateTime.UtcNow.AddMinutes(15),
                    ReservaActiva = false
                };

                // 4. Calcular items y totales
                decimal subtotal = 0;
                bool tieneItemsValidos = false;

                foreach (var itemDto in createOrdenDto.Items)
                {
                    if (itemDto.AnimalId.HasValue && itemDto.AnimalId.Value > 0)
                    {
                        var animal = await _context.Animales.FindAsync(itemDto.AnimalId.Value);
                        if (animal == null)
                            throw new InvalidOperationException($"Animal con ID {itemDto.AnimalId.Value} no encontrado");

                        if (!animal.Disponible || animal.Reservado)
                            throw new InvalidOperationException($"El animal {animal.Nombre} no está disponible");

                        var ordenItem = new OrdenItem
                        {
                            AnimalId = animal.Id,
                            Cantidad = 1,
                            PrecioUnitario = animal.Precio,
                            Subtotal = animal.Precio
                        };
                        orden.Items.Add(ordenItem);
                        subtotal += animal.Precio;
                        tieneItemsValidos = true;
                    }

                    if (itemDto.ProductoId.HasValue && itemDto.ProductoId.Value > 0)
                    {
                        var producto = await _context.Productos.FindAsync(itemDto.ProductoId.Value);
                        if (producto == null)
                            throw new InvalidOperationException($"Producto con ID {itemDto.ProductoId.Value} no encontrado");

                        if (!producto.Activo)
                            throw new InvalidOperationException($"El producto {producto.Nombre} no está disponible");

                        if (producto.StockDisponible < itemDto.Cantidad)
                            throw new InvalidOperationException($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockDisponible}, Solicitado: {itemDto.Cantidad}");

                        var precioConDescuento = producto.Precio * (1 - producto.Descuento / 100);
                        var ordenItem = new OrdenItem
                        {
                            ProductoId = producto.Id,
                            Cantidad = itemDto.Cantidad,
                            PrecioUnitario = precioConDescuento,
                            Subtotal = precioConDescuento * itemDto.Cantidad
                        };
                        orden.Items.Add(ordenItem);
                        subtotal += ordenItem.Subtotal;
                        tieneItemsValidos = true;
                    }
                }

                if (!tieneItemsValidos)
                    throw new InvalidOperationException("La orden debe contener al menos un producto o animal válido");

                orden.Subtotal = subtotal;
                orden.Impuesto = subtotal * 0.16m;
                orden.Total = orden.Subtotal + orden.Impuesto;

                // 5. Guardar orden
                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();

                // 6. Reservar items
                var (reservaExitosa, mensajeError) = await _reservaService.ReservarItemsAsync(orden);
                if (!reservaExitosa)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException(mensajeError);
                }

                // 7. Crear checkout de Stripe
                var httpContext = _httpContextAccessor.HttpContext;
                var successUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/ordenes/confirmar-pago?session_id={{CHECKOUT_SESSION_ID}}";
                var cancelUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/checkout/cancel";

                var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(orden, successUrl, cancelUrl);

                // 8. Commit transaction
                await transaction.CommitAsync();

                // 9. Cargar orden completa para DTO
                var ordenCompleta = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Animal)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                            .ThenInclude(p => p.Categoria)
                    .FirstOrDefaultAsync(o => o.Id == orden.Id);

                var ordenDto = _mapper.Map<OrdenDto>(ordenCompleta);

                return new CheckoutResponseDto
                {
                    SessionUrl = sessionUrl,
                    SessionId = orden.StripeSessionId,
                    Orden = ordenDto
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creando orden directa para cliente {ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<bool> VerificarPagoAsync(int ordenId)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                if (orden == null || string.IsNullOrEmpty(orden.StripeSessionId))
                    return false;

                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(orden.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    orden.Estado = OrdenEstado.Completada;
                    orden.ReservaActiva = false;

                    // Actualizar stock y marcar como vendido
                    foreach (var item in orden.Items)
                    {
                        if (item.ProductoId.HasValue)
                        {
                            var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                            if (producto != null)
                            {
                                producto.StockReservado -= item.Cantidad;
                                producto.StockVendido += item.Cantidad;
                                producto.StockDisponible = producto.StockTotal - producto.StockReservado - producto.StockVendido;
                            }
                        }
                        else if (item.AnimalId.HasValue)
                        {
                            var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                            if (animal != null)
                            {
                                animal.Reservado = false;
                                animal.Disponible = false;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Orden {OrdenId} marcada como completada", ordenId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando pago para orden {OrdenId}", ordenId);
                return false;
            }
        }

        public async Task<bool> CancelarOrdenAsync(int ordenId, int usuarioId, string usuarioRol)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                if (orden == null) return false;

                // Verificar permisos
                if (usuarioRol != "Administrador" && usuarioRol != "Gerente")
                {
                    var cliente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                    if (cliente == null || orden.ClienteId != cliente.Id)
                        return false;
                }

                // Solo se pueden cancelar órdenes pendientes
                if (orden.Estado != OrdenEstado.Pendiente)
                    return false;

                // Liberar reservas
                foreach (var item in orden.Items)
                {
                    if (item.ProductoId.HasValue)
                    {
                        var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                        if (producto != null)
                        {
                            producto.StockReservado -= item.Cantidad;
                            producto.StockDisponible = producto.StockTotal - producto.StockReservado - producto.StockVendido;
                        }
                    }
                    else if (item.AnimalId.HasValue)
                    {
                        var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                        if (animal != null)
                        {
                            animal.Reservado = false;
                        }
                    }
                }

                orden.Estado = OrdenEstado.Cancelada;
                orden.ReservaActiva = false;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Orden {OrdenId} cancelada por usuario {UsuarioId}", ordenId, usuarioId);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelando orden {OrdenId}", ordenId);
                return false;
            }
        }

        public async Task<List<OrdenDto>> GetOrdenesPorEstadoAsync(OrdenEstado estado)
        {
            try
            {
                var ordenes = await _context.Ordenes
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Animal)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                            .ThenInclude(p => p.Categoria)
                    .Where(o => o.Estado == estado)
                    .OrderByDescending(o => o.FechaCreacion)
                    .ToListAsync();

                return _mapper.Map<List<OrdenDto>>(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo órdenes por estado {Estado}", estado);
                throw;
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}