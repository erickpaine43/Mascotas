// Controllers/ReportesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Models;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly MascotaDbContext _context;

        public ReportesController(MascotaDbContext context)
        {
            _context = context;
        }

        [HttpGet("stock-bajo")]
        public async Task<ActionResult<List<Producto>>> GetStockBajo([FromQuery] int stockMinimo = 10)
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.StockDisponible <= stockMinimo && p.Activo)
                    .Include(p => p.Categoria)
                    .OrderBy(p => p.StockDisponible)
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener productos con stock bajo: {ex.Message}");
            }
        }

        [HttpGet("clientes-frecuentes")]
        public async Task<ActionResult<object>> GetClientesFrecuentes([FromQuery] int top = 10)
        {
            try
            {
                var clientes = await _context.Ordenes
                    .Where(o => o.Estado == OrdenEstado.Completada)
                    .GroupBy(o => new { o.Cliente.Id, o.Cliente.Nombre, o.Cliente.Email })
                    .Select(g => new
                    {
                        ClienteId = g.Key.Id,
                        Nombre = g.Key.Nombre,
                        Email = g.Key.Email,
                        TotalCompras = g.Count(),
                        TotalGastado = g.Sum(o => o.Total),
                        UltimaCompra = g.Max(o => o.FechaCreacion)
                    })
                    .OrderByDescending(c => c.TotalGastado)
                    .Take(top)
                    .ToListAsync();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener clientes frecuentes: {ex.Message}");
            }
        }

        [HttpGet("ventas-por-categoria")]
        public async Task<ActionResult<List<object>>> GetVentasPorCategoria(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var query = _context.OrdenItems
                    .Where(oi => oi.Orden.Estado == OrdenEstado.Completada && oi.ProductoId != null);

                if (fechaInicio.HasValue)
                    query = query.Where(oi => oi.Orden.FechaCreacion >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(oi => oi.Orden.FechaCreacion <= fechaFin.Value);

                var ventasPorCategoria = await query
                    .GroupBy(oi => new { oi.Producto.Categoria.Id, oi.Producto.Categoria.Nombre })
                    .Select(g => new
                    {
                        CategoriaId = g.Key.Id,
                        Categoria = g.Key.Nombre,
                        TotalVendido = g.Sum(oi => oi.Subtotal),
                        CantidadVendida = g.Sum(oi => oi.Cantidad),
                        ProductosUnicos = g.Select(oi => oi.ProductoId).Distinct().Count()
                    })
                    .OrderByDescending(v => v.TotalVendido)
                    .ToListAsync();

                return Ok(ventasPorCategoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener ventas por categoría: {ex.Message}");
            }
        }

        [HttpGet("ordenes-pendientes")]
        public async Task<ActionResult<List<Orden>>> GetOrdenesPendientes()
        {
            try
            {
                var ordenes = await _context.Ordenes
                    .Where(o => o.Estado == OrdenEstado.Pendiente || o.Estado == OrdenEstado.EnProceso)
                    .Include(o => o.Cliente)
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.FechaCreacion)
                    .Take(20)
                    .ToListAsync();

                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener órdenes pendientes: {ex.Message}");
            }
        }
    }
}