using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IReporteService
    {
        Task<DashboardResumen> ObtenerDashboardResumenAsync();
        Task<List<ReporteVentas>> ObtenerVentasMensualesAsync(int meses = 6);
        Task<List<ProductoMasVendido>> ObtenerProductosMasVendidosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null);
        Task<List<Animal>> ObtenerAnimalesMasVendidosAsync();
    }
}
