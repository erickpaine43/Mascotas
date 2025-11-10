using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IAlertaPrecioService
    {
        Task<AlertaPrecio> CrearAlertaAsync(string usuarioId, int productoId, decimal precioObjetivo);
        Task<List<AlertaPrecio>> ObtenerAlertasUsuarioAsync(string usuarioId);
        Task<List<AlertaPrecio>> ObtenerAlertasActivasAsync();
        Task DesactivarAlertaAsync(int alertaId, string usuarioId);
        Task VerificarAlertasPrecioAsync();
    }
}
