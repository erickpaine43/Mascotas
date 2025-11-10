using Mascotas.Models;

namespace Mascotas.Services
{
    public interface INotificacionService
    {
        Task<Notificacion> CrearNotificacionAsync(Notificacion notificacion);
        Task<List<Notificacion>> ObtenerNotificacionesUsuarioAsync(string usuarioId, bool noLeidas = false);
        Task MarcarComoLeidaAsync(int notificacionId, string usuarioId);
        Task<int> EnviarNotificacionesPendientesAsync();
        Task<int> EnviarNotificacionIndividualAsync(int notificacionId);
        Task<string?> ObtenerEmailUsuarioAsync(string usuarioId);
    }
}
