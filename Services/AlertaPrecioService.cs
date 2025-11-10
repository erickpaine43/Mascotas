using Mascotas.Data;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;

namespace Mascotas.Services
    {
        public class AlertaPrecioService : IAlertaPrecioService
        {
            private readonly MascotaDbContext _context;

            public AlertaPrecioService(MascotaDbContext context)
            {
                _context = context;
            }

            public async Task<AlertaPrecio> CrearAlertaAsync(string usuarioId, int productoId, decimal precioObjetivo)
            {
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null)
                    throw new ArgumentException("Producto no encontrado");

                var alertaExistente = await _context.AlertaPrecios
                    .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId && a.ProductoId == productoId && a.Activa);

                if (alertaExistente != null)
                {
                    alertaExistente.PrecioObjetivo = precioObjetivo;
                    await _context.SaveChangesAsync();
                    return alertaExistente;
                }

                var alerta = new AlertaPrecio
                {
                    UsuarioId = usuarioId,
                    ProductoId = productoId,
                    PrecioObjetivo = precioObjetivo,
                    Activa = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.AlertaPrecios.Add(alerta);
                await _context.SaveChangesAsync();

                return alerta;
            }

            public async Task<List<AlertaPrecio>> ObtenerAlertasUsuarioAsync(string usuarioId)
            {
                return await _context.AlertaPrecios
                    .Include(a => a.Producto)
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.FechaCreacion)
                    .ToListAsync();
            }

            public async Task<List<AlertaPrecio>> ObtenerAlertasActivasAsync()
            {
                return await _context.AlertaPrecios
                    .Include(a => a.Producto)
                    .Where(a => a.Activa)
                    .ToListAsync();
            }

            public async Task DesactivarAlertaAsync(int alertaId, string usuarioId)
            {
                var alerta = await _context.AlertaPrecios
                    .FirstOrDefaultAsync(a => a.Id == alertaId && a.UsuarioId == usuarioId);

                if (alerta != null)
                {
                    alerta.Activa = false;
                    await _context.SaveChangesAsync();
                }
            }

            public async Task VerificarAlertasPrecioAsync()
            {
                var alertasActivas = await _context.AlertaPrecios
                    .Include(a => a.Producto)
                    .Where(a => a.Activa && !a.Notificado)
                    .ToListAsync();

                foreach (var alerta in alertasActivas)
                {
                    if (alerta.Producto.Precio <= alerta.PrecioObjetivo)
                    {
                        // Aquí podrías implementar notificaciones (email, push, etc.)
                        alerta.Notificado = true;
                        alerta.FechaActivacion = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }

