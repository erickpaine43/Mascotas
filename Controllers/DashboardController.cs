// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Mascotas.Services;
using Mascotas.Models;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public DashboardController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<DashboardResumen>> GetResumen()
        {
            try
            {
                var resumen = await _reporteService.ObtenerDashboardResumenAsync();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el resumen: {ex.Message}");
            }
        }

        [HttpGet("ventas-mensuales")]
        public async Task<ActionResult<List<ReporteVentas>>> GetVentasMensuales([FromQuery] int meses = 6)
        {
            try
            {
                var ventas = await _reporteService.ObtenerVentasMensualesAsync(meses);
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener ventas mensuales: {ex.Message}");
            }
        }

        [HttpGet("productos-mas-vendidos")]
        public async Task<ActionResult<List<ProductoMasVendido>>> GetProductosMasVendidos(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                var productos = await _reporteService.ObtenerProductosMasVendidosAsync(fechaInicio, fechaFin);
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener productos más vendidos: {ex.Message}");
            }
        }

        [HttpGet("animales-mas-vendidos")]
        public async Task<ActionResult<List<Animal>>> GetAnimalesMasVendidos()
        {
            try
            {
                var animales = await _reporteService.ObtenerAnimalesMasVendidosAsync();
                return Ok(animales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener animales más vendidos: {ex.Message}");
            }
        }
    }
}