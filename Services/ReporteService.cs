// Services/ReporteService.cs
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Models;

namespace Mascotas.Services
{
    public class ReporteService : IReporteService
    {
        private readonly MascotaDbContext _context;

        public ReporteService(MascotaDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResumen> ObtenerDashboardResumenAsync()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var resumen = new DashboardResumen();

            // Ventas de hoy
            var ordenesHoy = await _context.Ordenes
                .Where(o => o.FechaCreacion.Date == hoy && o.Estado == OrdenEstado.Completada)
                .ToListAsync();

            resumen.IngresosHoy = ordenesHoy.Sum(o => o.Total);
            resumen.OrdenesHoy = ordenesHoy.Count;

            // Ventas del mes
            var ordenesMes = await _context.Ordenes
                .Where(o => o.FechaCreacion.Date >= inicioMes && o.FechaCreacion.Date <= finMes && o.Estado == OrdenEstado.Completada)
                .ToListAsync();

            resumen.IngresosMes = ordenesMes.Sum(o => o.Total);

            // Órdenes pendientes
            resumen.OrdenesPendientes = await _context.Ordenes
                .CountAsync(o => o.Estado == OrdenEstado.Pendiente || o.Estado == OrdenEstado.EnProceso);

            // Productos
            resumen.TotalProductos = await _context.Productos.CountAsync(p => p.Activo);
            resumen.ProductosBajoStock = await _context.Productos.CountAsync(p => p.StockDisponible < 10 && p.Activo);

            // Animales
            resumen.TotalAnimales = await _context.Animales.CountAsync();
            resumen.AnimalesDisponibles = await _context.Animales.CountAsync(a => a.Disponible && !a.Reservado);

            // Clientes
            resumen.TotalClientes = await _context.Clientes.CountAsync();
            resumen.ClientesNuevosMes = await _context.Clientes
                .CountAsync(c => c.FechaRegistro >= inicioMes);

            // Especie más popular
            var especieMasPopular = await _context.OrdenItems
                .Where(oi => oi.AnimalId != null && oi.Orden.Estado == OrdenEstado.Completada)
                .GroupBy(oi => oi.Animal.Especie)
                .Select(g => new { Especie = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            resumen.EspecieMasPopular = especieMasPopular?.Especie ?? "No hay datos";

            return resumen;
        }

        public async Task<List<ReporteVentas>> ObtenerVentasMensualesAsync(int meses = 6)
        {
            var fechaInicio = DateTime.Today.AddMonths(-meses);

            // Versión simple que SÍ funciona
            var ordenes = await _context.Ordenes
                .Where(o => o.FechaCreacion >= fechaInicio && o.Estado == OrdenEstado.Completada)
                .Include(o => o.Items)
                .ToListAsync();

            var ventas = ordenes
                .GroupBy(o => new { o.FechaCreacion.Year, o.FechaCreacion.Month })
                .Select(g => new ReporteVentas
                {
                    Fecha = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalVentas = g.Sum(o => o.Total),
                    CantidadVentas = g.Count(),
                    ProductosVendidos = g.SelectMany(o => o.Items).Count(oi => oi.ProductoId != null),
                    AnimalesVendidos = g.SelectMany(o => o.Items).Count(oi => oi.AnimalId != null)
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            return ventas;
        }

        public async Task<List<ProductoMasVendido>> ObtenerProductosMasVendidosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _context.OrdenItems
                .Where(oi => oi.ProductoId != null && oi.Orden.Estado == OrdenEstado.Completada);

            if (fechaInicio.HasValue)
                query = query.Where(oi => oi.Orden.FechaCreacion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(oi => oi.Orden.FechaCreacion <= fechaFin.Value);

            var productos = await query
                .GroupBy(oi => new {
                    oi.Producto.Id,
                    ProductoNombre = oi.Producto.Nombre,
                    CategoriaNombre = oi.Producto.Categoria.Nombre
                })
                .Select(g => new ProductoMasVendido
                {
                    Nombre = g.Key.ProductoNombre,
                    CantidadVendida = g.Sum(oi => oi.Cantidad),
                    TotalVendido = g.Sum(oi => oi.Subtotal),
                    Categoria = g.Key.CategoriaNombre
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(10)
                .ToListAsync();

            return productos;
        }

        public async Task<List<Animal>> ObtenerAnimalesMasVendidosAsync()
        {
            var animalesVendidos = await _context.OrdenItems
                .Where(oi => oi.AnimalId != null && oi.Orden.Estado == OrdenEstado.Completada)
                .GroupBy(oi => oi.Animal)
                .Select(g => new { Animal = g.Key, VecesVendido = g.Count() })
                .OrderByDescending(x => x.VecesVendido)
                .Take(5)
                .Select(x => x.Animal)
                .ToListAsync();

            return animalesVendidos;
        }
    }
}