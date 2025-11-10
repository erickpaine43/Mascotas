using Mascotas.Models;
namespace Mascotas.Services
{
    public interface IMonitorBusquedasService
    {
        Task VerificarCambiosBusquedasGuardadasAsync();
        Task<List<ResultadoCambio>> ObtenerCambiosParaBusqueda(int filtroGuardadoId);
    }
}
