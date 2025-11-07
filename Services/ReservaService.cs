using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Services
{
    public class ReservaService : IReservaService
    {
        private readonly MascotaDbContext _context;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(MascotaDbContext context, ILogger<ReservaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> VerificarDisponibilidadAsync(CreateOrdenDto ordenDto)
        {
            foreach (var item in ordenDto.Items)
            {
                if (item.ProductoId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                    if (producto == null || !producto.Activo || producto.StockDisponible < item.Cantidad)
                    {
                        _logger.LogWarning($"Producto {item.ProductoId} no disponible");
                        return false;
                    }
                }
                else if (item.AnimalId.HasValue)
                {
                    var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                    if (animal == null || !animal.Disponible || animal.Reservado)
                    {
                        _logger.LogWarning($"Animal {item.AnimalId} no disponible");
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<(bool success, string errorMessage)> ReservarItemsAsync(Orden orden)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in orden.Items)
                {
                    if (item.ProductoId.HasValue)
                    {
                        var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                        if (producto.StockDisponible < item.Cantidad)
                        {
                            await transaction.RollbackAsync();
                            return (false, $"Stock insuficiente para {producto.Nombre}");
                        }

                        // Reservar stock
                        producto.StockReservado += item.Cantidad;
                        producto.StockDisponible -= item.Cantidad;
                        producto.ExpiracionReserva = orden.FechaExpiracionReserva;
                    }
                    else if (item.AnimalId.HasValue)
                    {
                        var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                        if (animal.Reservado)
                        {
                            await transaction.RollbackAsync();
                            return (false, $"Animal {animal.Nombre} ya está reservado");
                        }

                        // Reservar animal
                        animal.Reservado = true;
                        animal.ExpiracionReserva = orden.FechaExpiracionReserva;
                    }
                }

                orden.ReservaActiva = true;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Reserva creada para orden {orden.NumeroOrden}");
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error reservando items para orden {orden.NumeroOrden}");
                return (false, "Error interno al procesar la reserva");
            }
        }

        public async Task ConfirmarReservaAsync(int ordenId)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null) return;

            foreach (var item in orden.Items)
            {
                if (item.ProductoId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                    producto.StockReservado -= item.Cantidad;
                    producto.StockVendido += item.Cantidad;
                    producto.ExpiracionReserva = null;
                }
                else if (item.AnimalId.HasValue)
                {
                    var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                    animal.Reservado = false;
                    animal.Disponible = false; // Ya vendido
                    animal.ExpiracionReserva = null;
                }
            }

            orden.ReservaActiva = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reserva confirmada para orden {orden.NumeroOrden}");
        }

        public async Task LiberarReservaAsync(int ordenId)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null) return;

            foreach (var item in orden.Items)
            {
                if (item.ProductoId.HasValue)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId.Value);
                    producto.StockReservado -= item.Cantidad;
                    producto.StockDisponible += item.Cantidad;
                    producto.ExpiracionReserva = null;
                }
                else if (item.AnimalId.HasValue)
                {
                    var animal = await _context.Animales.FindAsync(item.AnimalId.Value);
                    animal.Reservado = false;
                    animal.ExpiracionReserva = null;
                }
            }

            orden.ReservaActiva = false;
            orden.Estado = OrdenEstado.Cancelada;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reserva liberada para orden {orden.NumeroOrden}");
        }

        public async Task LiberarReservasExpiradasAsync()
        {
            var ordenesExpiradas = await _context.Ordenes
                .Include(o => o.Items)
                .Where(o => o.ReservaActiva && o.FechaExpiracionReserva < DateTime.UtcNow)
                .ToListAsync();

            foreach (var orden in ordenesExpiradas)
            {
                _logger.LogInformation($"Liberando reserva expirada para orden {orden.NumeroOrden}");
                await LiberarReservaAsync(orden.Id);
            }

            // También liberar reservas directas en productos/animales por si acaso
            var productosReservadosExpirados = await _context.Productos
                .Where(p => p.StockReservado > 0 && p.ExpiracionReserva < DateTime.UtcNow)
                .ToListAsync();

            foreach (var producto in productosReservadosExpirados)
            {
                producto.StockDisponible += producto.StockReservado;
                producto.StockReservado = 0;
                producto.ExpiracionReserva = null;
            }

            var animalesReservadosExpirados = await _context.Animales
                .Where(a => a.Reservado && a.ExpiracionReserva < DateTime.UtcNow)
                .ToListAsync();

            foreach (var animal in animalesReservadosExpirados)
            {
                animal.Reservado = false;
                animal.ExpiracionReserva = null;
            }

            await _context.SaveChangesAsync();
        }
    }
}