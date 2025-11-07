using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IReservaService
    {
        Task<bool> VerificarDisponibilidadAsync(CreateOrdenDto ordenDto);
        Task<(bool success, string errorMessage)> ReservarItemsAsync(Orden orden);
        Task ConfirmarReservaAsync(int ordenId);
        Task LiberarReservaAsync(int ordenId);
        Task LiberarReservasExpiradasAsync();
    }
}
