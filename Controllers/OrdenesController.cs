
using Mascotas.Dto;
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using System.Security.Claims;
using Stripe.Checkout;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenesController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly IReservaService _reservaService;
        private readonly ILogger<OrdenesController> _logger;

        public OrdenesController(
            MascotaDbContext context,
            IStripeService stripeService,
            IReservaService reservaService,
            ILogger<OrdenesController> logger)
        {
            _context = context;
            _stripeService = stripeService;
            _reservaService = reservaService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetOrdenes()
        {
            var ordenes = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Animal)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Categoria)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();

            var ordenesDto = ordenes.Select(o => new OrdenDto
            {
                Id = o.Id,
                NumeroOrden = o.NumeroOrden,
                ClienteId = o.ClienteId,
                Subtotal = o.Subtotal,
                Impuesto = o.Impuesto,
                Descuento = o.Descuento,
                Total = o.Total,
                Estado = o.Estado.ToString(),
                MetodoPago = o.MetodoPago.ToString(),
                Comentarios = o.Comentarios,
                FechaCreacion = o.FechaCreacion,
                ReservaActiva = o.ReservaActiva,
                FechaExpiracionReserva = o.FechaExpiracionReserva,
                Cliente = new ClienteDto
                {
                    Id = o.Cliente.Id,
                    Nombre = o.Cliente.Nombre,
                    Email = o.Cliente.Email,
                    Telefono = o.Cliente.Telefono,
                    Direccion = o.Cliente.Direccion,
                    Ciudad = o.Cliente.Ciudad,
                    CodigoPostal = o.Cliente.CodigoPostal,
                    FechaRegistro = o.Cliente.FechaRegistro
                },
                Items = o.Items.Select(i => new OrdenItemDto
                {
                    Id = i.Id,
                    AnimalId = i.AnimalId,
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal,
                    Animal = i.Animal == null ? null : new AnimalDto
                    {
                        Id = i.Animal.Id,
                        Nombre = i.Animal.Nombre,
                        Especie = i.Animal.Especie,
                        Raza = i.Animal.Raza,
                        Edad = i.Animal.Edad,
                        Sexo = i.Animal.Sexo,
                        Precio = i.Animal.Precio,
                        Descripcion = i.Animal.Descripcion,
                        Disponible = i.Animal.Disponible,
                        Reservado = i.Animal.Reservado,
                        Vacunado = i.Animal.Vacunado,
                        Esterilizado = i.Animal.Esterilizado,
                        FechaNacimiento = i.Animal.FechaNacimiento,
                        FechaCreacion = i.Animal.FechaCreacion
                    },
                    Producto = i.Producto == null ? null : new ProductoDto
                    {
                        Id = i.Producto.Id,
                        Nombre = i.Producto.Nombre,
                        CategoriaId = i.Producto.CategoriaId,
                        CategoriaNombre = i.Producto.Categoria.Nombre,
                        Descripcion = i.Producto.Descripcion,
                        DescripcionCorta = i.Producto.DescripcionCorta,
                        Precio = i.Producto.Precio,
                        PrecioOriginal = i.Producto.PrecioOriginal,
                        StockTotal = i.Producto.StockTotal,
                        StockDisponible = i.Producto.StockDisponible,
                        StockReservado = i.Producto.StockReservado,
                        StockVendido = i.Producto.StockVendido,
                        Descuento = i.Producto.Descuento,
                        ImagenUrl = i.Producto.ImagenUrl,
                        Activo = i.Producto.Activo,
                        Destacado = i.Producto.Destacado,
                        EnOferta = i.Producto.EnOferta,
                        Rating = i.Producto.Rating,
                        TotalValoraciones = i.Producto.TotalValoraciones,
                        SKU = i.Producto.SKU,
                        Marca = i.Producto.Marca,
                        FechaCreacion = i.Producto.FechaCreacion
                    }
                }).ToList()
            }).ToList();

            return Ok(ordenesDto);
        }

        [HttpGet("mis-ordenes")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> GetMisOrdenes()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado");
            }

            var ordenes = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Animal)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(o => o.ClienteId == cliente.Id)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();

            var ordenesDto = ordenes.Select(o => new OrdenDto
            {
                Id = o.Id,
                NumeroOrden = o.NumeroOrden,
                ClienteId = o.ClienteId,
                Subtotal = o.Subtotal,
                Impuesto = o.Impuesto,
                Descuento = o.Descuento,
                Total = o.Total,
                Estado = o.Estado.ToString(),
                MetodoPago = o.MetodoPago.ToString(),
                Comentarios = o.Comentarios,
                FechaCreacion = o.FechaCreacion,
                ReservaActiva = o.ReservaActiva,
                FechaExpiracionReserva = o.FechaExpiracionReserva,
                Cliente = new ClienteDto
                {
                    Id = o.Cliente.Id,
                    Nombre = o.Cliente.Nombre,
                    Email = o.Cliente.Email,
                    Telefono = o.Cliente.Telefono,
                    Direccion = o.Cliente.Direccion,
                    Ciudad = o.Cliente.Ciudad,
                    CodigoPostal = o.Cliente.CodigoPostal,
                    FechaRegistro = o.Cliente.FechaRegistro
                },
                Items = o.Items.Select(i => new OrdenItemDto
                {
                    Id = i.Id,
                    AnimalId = i.AnimalId,
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal,
                    Animal = i.Animal == null ? null : new AnimalDto
                    {
                        Id = i.Animal.Id,
                        Nombre = i.Animal.Nombre,
                        Especie = i.Animal.Especie,
                        Raza = i.Animal.Raza,
                        Edad = i.Animal.Edad,
                        Sexo = i.Animal.Sexo,
                        Precio = i.Animal.Precio,
                        Descripcion = i.Animal.Descripcion,
                        Disponible = i.Animal.Disponible,
                        Reservado = i.Animal.Reservado,
                        Vacunado = i.Animal.Vacunado,
                        Esterilizado = i.Animal.Esterilizado,
                        FechaNacimiento = i.Animal.FechaNacimiento,
                        FechaCreacion = i.Animal.FechaCreacion
                    },
                    Producto = i.Producto == null ? null : new ProductoDto
                    {
                        Id = i.Producto.Id,
                        Nombre = i.Producto.Nombre,
                        CategoriaId = i.Producto.CategoriaId,
                        CategoriaNombre = i.Producto.Categoria.Nombre,
                        Descripcion = i.Producto.Descripcion,
                        DescripcionCorta = i.Producto.DescripcionCorta,
                        Precio = i.Producto.Precio,
                        PrecioOriginal = i.Producto.PrecioOriginal,
                        StockTotal = i.Producto.StockTotal,
                        StockDisponible = i.Producto.StockDisponible,
                        StockReservado = i.Producto.StockReservado,
                        StockVendido = i.Producto.StockVendido,
                        Descuento = i.Producto.Descuento,
                        ImagenUrl = i.Producto.ImagenUrl,
                        Activo = i.Producto.Activo,
                        Destacado = i.Producto.Destacado,
                        EnOferta = i.Producto.EnOferta,
                        Rating = i.Producto.Rating,
                        TotalValoraciones = i.Producto.TotalValoraciones,
                        SKU = i.Producto.SKU,
                        Marca = i.Producto.Marca,
                        FechaCreacion = i.Producto.FechaCreacion
                    }
                }).ToList()
            }).ToList();

            return Ok(ordenesDto);
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<CheckoutResponseDto>> CreateOrden(CreateOrdenDto createOrdenDto)
        {
            try
            {
                // 1. Obtener cliente autenticado
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (cliente == null)
                    return BadRequest("Cliente no encontrado");

                // 2. Verificar disponibilidad
                if (!await _reservaService.VerificarDisponibilidadAsync(createOrdenDto))
                    return BadRequest("Uno o más items no están disponibles");

                // 3. Validar items
                if (createOrdenDto.Items == null || !createOrdenDto.Items.Any())
                    return BadRequest("La orden debe tener al menos un item");

                // 4. Crear orden
                var orden = new Orden
                {
                    NumeroOrden = GenerateOrderNumber(),
                    ClienteId = cliente.Id,
                    Estado = OrdenEstado.Pendiente,
                    MetodoPago = MetodoPago.Stripe,
                    Comentarios = createOrdenDto.Comentarios,
                    FechaCreacion = DateTime.UtcNow,
                    FechaExpiracionReserva = DateTime.UtcNow.AddMinutes(15),
                    ReservaActiva = false
                };

                // 5. Calcular items y totales (SIN RESERVAR AÚN)
                decimal subtotal = 0;
                bool tieneItemsValidos = false;

                foreach (var itemDto in createOrdenDto.Items)
                {
                    // ✅ PERMITIR que un item tenga AMBOS: animal Y producto
                    if (itemDto.AnimalId.HasValue && itemDto.AnimalId.Value > 0)
                    {
                        var animal = await _context.Animales.FindAsync(itemDto.AnimalId.Value);

                        if (animal == null)
                            return BadRequest($"Animal con ID {itemDto.AnimalId.Value} no encontrado");

                        if (!animal.Disponible || animal.Reservado)
                            return BadRequest($"El animal {animal.Nombre} no está disponible");

                        var ordenItem = new OrdenItem
                        {
                            AnimalId = animal.Id,
                            Cantidad = 1, // Siempre 1 para animales
                            PrecioUnitario = animal.Precio,
                            Subtotal = animal.Precio
                        };
                        orden.Items.Add(ordenItem);
                        subtotal += animal.Precio;
                        tieneItemsValidos = true;
                    }

                    // ✅ PERMITIR que un item tenga producto (con o sin animal)
                    if (itemDto.ProductoId.HasValue && itemDto.ProductoId.Value > 0)
                    {
                        var producto = await _context.Productos.FindAsync(itemDto.ProductoId.Value);

                        if (producto == null)
                            return BadRequest($"Producto con ID {itemDto.ProductoId.Value} no encontrado");

                        if (!producto.Activo)
                            return BadRequest($"El producto {producto.Nombre} no está disponible");

                        if (producto.StockDisponible < itemDto.Cantidad)
                            return BadRequest($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.StockDisponible}, Solicitado: {itemDto.Cantidad}");

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
                {
                    return BadRequest("La orden debe contener al menos un producto o animal válido");
                }

                orden.Subtotal = subtotal;
                orden.Impuesto = subtotal * 0.16m;
                orden.Total = orden.Subtotal + orden.Impuesto;

                // 6. Guardar orden
                _context.Ordenes.Add(orden);
                await _context.SaveChangesAsync();

                // 7. RESERVAR items temporalmente usando el servicio
                var (reservaExitosa, mensajeError) = await _reservaService.ReservarItemsAsync(orden);
                if (!reservaExitosa)
                {
                    // Si falla la reserva, eliminar la orden
                    _context.Ordenes.Remove(orden);
                    await _context.SaveChangesAsync();
                    return BadRequest(mensajeError);
                }

                
                // 8. Crear checkout de Stripe
                try
                {
                    var successUrl = $"{Request.Scheme}://{Request.Host}/checkout/success?session_id={{CHECKOUT_SESSION_ID}}";
                    var cancelUrl = $"{Request.Scheme}://{Request.Host}/checkout/cancel";

                    var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(orden, successUrl, cancelUrl);

                    // ✅ RECARGAR la orden con las relaciones INCLUIDAS
                    var ordenCompleta = await _context.Ordenes
                        .Include(o => o.Cliente)  // ← CARGAR cliente
                        .Include(o => o.Items)    // ← CARGAR items
                            .ThenInclude(i => i.Animal)  // ← CARGAR animal de cada item
                        .Include(o => o.Items)
                            .ThenInclude(i => i.Producto)  // ← CARGAR producto de cada item
                                .ThenInclude(p => p.Categoria)  // ← CARGAR categoría del producto
                        .FirstOrDefaultAsync(o => o.Id == orden.Id);

                    // ✅ MAPEAR correctamente a DTO
                    var ordenDto = new OrdenDto
                    {
                        Id = ordenCompleta.Id,
                        NumeroOrden = ordenCompleta.NumeroOrden,
                        ClienteId = ordenCompleta.ClienteId,
                        Subtotal = ordenCompleta.Subtotal,
                        Impuesto = ordenCompleta.Impuesto,
                        Descuento = ordenCompleta.Descuento,
                        Total = ordenCompleta.Total,
                        Estado = ordenCompleta.Estado.ToString(),
                        MetodoPago = ordenCompleta.MetodoPago.ToString(),
                        Comentarios = ordenCompleta.Comentarios,
                        ReservaActiva = ordenCompleta.ReservaActiva,
                        FechaExpiracionReserva = ordenCompleta.FechaExpiracionReserva,
                        FechaCreacion = ordenCompleta.FechaCreacion,
                        Cliente = new ClienteDto
                        {
                            Id = ordenCompleta.Cliente.Id,
                            Nombre = ordenCompleta.Cliente.Nombre,
                            Email = ordenCompleta.Cliente.Email,
                            Telefono = ordenCompleta.Cliente.Telefono,
                            Direccion = ordenCompleta.Cliente.Direccion,
                            Ciudad = ordenCompleta.Cliente.Ciudad,
                            CodigoPostal = ordenCompleta.Cliente.CodigoPostal,
                            FechaRegistro = ordenCompleta.Cliente.FechaRegistro
                        },
                        Items = ordenCompleta.Items.Select(i => new OrdenItemDto
                        {
                            Id = i.Id,
                            AnimalId = i.AnimalId,
                            ProductoId = i.ProductoId,
                            Cantidad = i.Cantidad,
                            PrecioUnitario = i.PrecioUnitario,
                            Subtotal = i.Subtotal,
                            Animal = i.Animal == null ? null : new AnimalDto
                            {
                                Id = i.Animal.Id,
                                Nombre = i.Animal.Nombre,
                                Especie = i.Animal.Especie,
                                Raza = i.Animal.Raza,
                                Edad = i.Animal.Edad,
                                Sexo = i.Animal.Sexo,
                                Precio = i.Animal.Precio,
                                Descripcion = i.Animal.Descripcion,
                                Disponible = i.Animal.Disponible,
                                Reservado = i.Animal.Reservado,
                                Vacunado = i.Animal.Vacunado,
                                Esterilizado = i.Animal.Esterilizado,
                                FechaNacimiento = i.Animal.FechaNacimiento,
                                FechaCreacion = i.Animal.FechaCreacion
                            },
                            Producto = i.Producto == null ? null : new ProductoDto
                            {
                                Id = i.Producto.Id,
                                Nombre = i.Producto.Nombre,
                                CategoriaId = i.Producto.CategoriaId,
                                CategoriaNombre = i.Producto.Categoria?.Nombre ?? "", // ← Seguro contra null
                                Descripcion = i.Producto.Descripcion,
                                DescripcionCorta = i.Producto.DescripcionCorta,
                                Precio = i.Producto.Precio,
                                PrecioOriginal = i.Producto.PrecioOriginal,
                                StockTotal = i.Producto.StockTotal,
                                StockDisponible = i.Producto.StockDisponible,
                                StockReservado = i.Producto.StockReservado,
                                StockVendido = i.Producto.StockVendido,
                                Descuento = i.Producto.Descuento,
                                ImagenUrl = i.Producto.ImagenUrl,
                                Activo = i.Producto.Activo,
                                Destacado = i.Producto.Destacado,
                                EnOferta = i.Producto.EnOferta,
                                Rating = i.Producto.Rating,
                                TotalValoraciones = i.Producto.TotalValoraciones,
                                SKU = i.Producto.SKU,
                                Marca = i.Producto.Marca,
                                FechaCreacion = i.Producto.FechaCreacion
                            }
                        }).ToList()
                    };

                    var response = new CheckoutResponseDto
                    {
                        SessionUrl = sessionUrl,
                        SessionId = orden.StripeSessionId,
                        Orden = ordenDto  // ← Usar el DTO completo
                    };

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    // Revertir reserva si falla Stripe
                    await _reservaService.LiberarReservaAsync(orden.Id);
                    return StatusCode(500, $"Error procesando el pago: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenDto>> GetOrden(int id)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuarioRol = User.FindFirst(ClaimTypes.Role)?.Value;

            var orden = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Animal)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return NotFound();
            }

            // Verificar permisos: Solo admins/gerentes o el dueño pueden ver
            if (usuarioRol != "Administrador" && usuarioRol != "Gerente")
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

                if (cliente == null || orden.ClienteId != cliente.Id)
                {
                    return Forbid("No tienes permisos para ver esta orden");
                }
            }

            var ordenDto = new OrdenDto
            {
                Id = orden.Id,
                NumeroOrden = orden.NumeroOrden,
                ClienteId = orden.ClienteId,
                Subtotal = orden.Subtotal,
                Impuesto = orden.Impuesto,
                Descuento = orden.Descuento,
                Total = orden.Total,
                Estado = orden.Estado.ToString(),
                MetodoPago = orden.MetodoPago.ToString(),
                Comentarios = orden.Comentarios,
                FechaCreacion = orden.FechaCreacion,
                ReservaActiva = orden.ReservaActiva,
                FechaExpiracionReserva = orden.FechaExpiracionReserva,
                Cliente = new ClienteDto
                {
                    Id = orden.Cliente.Id,
                    Nombre = orden.Cliente.Nombre,
                    Email = orden.Cliente.Email,
                    Telefono = orden.Cliente.Telefono,
                    Direccion = orden.Cliente.Direccion,
                    Ciudad = orden.Cliente.Ciudad,
                    CodigoPostal = orden.Cliente.CodigoPostal,
                    FechaRegistro = orden.Cliente.FechaRegistro
                },
                Items = orden.Items.Select(i => new OrdenItemDto
                {
                    Id = i.Id,
                    AnimalId = i.AnimalId,
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal,
                    Animal = i.Animal == null ? null : new AnimalDto
                    {
                        Id = i.Animal.Id,
                        Nombre = i.Animal.Nombre,
                        Especie = i.Animal.Especie,
                        Raza = i.Animal.Raza,
                        Edad = i.Animal.Edad,
                        Sexo = i.Animal.Sexo,
                        Precio = i.Animal.Precio,
                        Descripcion = i.Animal.Descripcion,
                        Disponible = i.Animal.Disponible,
                        Reservado = i.Animal.Reservado,
                        Vacunado = i.Animal.Vacunado,
                        Esterilizado = i.Animal.Esterilizado,
                        FechaNacimiento = i.Animal.FechaNacimiento,
                        FechaCreacion = i.Animal.FechaCreacion
                    },
                    Producto = i.Producto == null ? null : new ProductoDto
                    {
                        Id = i.Producto.Id,
                        Nombre = i.Producto.Nombre,
                        CategoriaId = i.Producto.CategoriaId,
                        CategoriaNombre = i.Producto.Categoria.Nombre,
                        Descripcion = i.Producto.Descripcion,
                        DescripcionCorta = i.Producto.DescripcionCorta,
                        Precio = i.Producto.Precio,
                        PrecioOriginal = i.Producto.PrecioOriginal,
                        StockTotal = i.Producto.StockTotal,
                        StockDisponible = i.Producto.StockDisponible,
                        StockReservado = i.Producto.StockReservado,
                        StockVendido = i.Producto.StockVendido,
                        Descuento = i.Producto.Descuento,
                        ImagenUrl = i.Producto.ImagenUrl,
                        Activo = i.Producto.Activo,
                        Destacado = i.Producto.Destacado,
                        EnOferta = i.Producto.EnOferta,
                        Rating = i.Producto.Rating,
                        TotalValoraciones = i.Producto.TotalValoraciones,
                        SKU = i.Producto.SKU,
                        Marca = i.Producto.Marca,
                        FechaCreacion = i.Producto.FechaCreacion
                    }
                }).ToList()
            };

            return ordenDto;
        }

        // En tu OrdenesController
        [HttpPost("verificar-pago/{ordenId}")]
        public async Task<ActionResult> VerificarPago(int ordenId)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null)
                return NotFound("Orden no encontrada");

            if (string.IsNullOrEmpty(orden.StripeSessionId))
                return BadRequest("Esta orden no tiene sesión de Stripe");

            try
            {
                // Verificar el estado en Stripe
                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(orden.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    // ✅ PAGO COMPLETADO
                    orden.Estado = OrdenEstado.Completada;
                    orden.ReservaActiva = false;

                    // Liberar reserva y actualizar stock vendido
                    foreach (var item in orden.Items)
                    {
                        if (item.ProductoId.HasValue)
                        {
                            var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                            if (producto != null)
                            {
                                producto.StockReservado -= item.Cantidad;
                                producto.StockVendido += item.Cantidad;
                            }
                        }
                        else if (item.AnimalId.HasValue)
                        {
                            var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                            if (animal != null)
                            {
                                animal.Reservado = false;
                                animal.Disponible = false; // Ya se vendió
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new { mensaje = "Pago verificado - Orden completada", estado = "Completada" });
                }
                else
                {
                    return Ok(new { mensaje = "Pago aún pendiente", estado = "Pendiente" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error verificando pago: {ex.Message}");
            }
        }
    }
}