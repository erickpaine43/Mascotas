using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IFiltroGuardadoService
    {
        Task<FiltroGuardado> GuardarFiltroAsync(string usuarioId, string nombre, Dictionary<string, object> parametros);
        Task<List<FiltroGuardado>> ObtenerFiltrosUsuarioAsync(string usuarioId);
        Task<List<FiltroGuardado>> ObtenerFiltrosFavoritosAsync(string usuarioId);
        Task IncrementarUsoFiltroAsync(int filtroId);
        Task EliminarFiltroAsync(int filtroId, string usuarioId);
    }
}
