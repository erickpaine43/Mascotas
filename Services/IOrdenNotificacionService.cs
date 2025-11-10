using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IOrdenNotificacionService
    {
        Task<bool> EnviarNotificacionConfirmacionAsync(Orden orden);
        Task<bool> EnviarNotificacionEnvioAsync(Orden orden, string infoEnvio);
        Task<bool> EnviarNotificacionEntregaAsync(Orden orden);
        Task ProgramarRecordatorioResenaAsync(Orden orden, int diasDelay = 3);
        Task<bool> EnviarNotificacionEstadoAsync(Orden orden, OrdenEstado nuevoEstado, string descripcionPersonalizada = null);
    }
}
