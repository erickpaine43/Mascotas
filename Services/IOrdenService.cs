using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IOrdenService
    {
        Task<OrdenDto?> GetOrdenAsync(int id, int usuarioId, string usuarioRol);
        Task<List<OrdenDto>> GetOrdenesAsync(int? clienteId = null);
        Task<CheckoutResponseDto> CrearOrdenDesdeCarritoAsync(int clienteId, string? comentarios = null);
        Task<CheckoutResponseDto> CrearOrdenDirectaAsync(CreateOrdenDto createOrdenDto, int clienteId);
        Task<bool> VerificarPagoAsync(int ordenId);
        Task<bool> CancelarOrdenAsync(int ordenId, int usuarioId, string usuarioRol);
        Task<List<OrdenDto>> GetOrdenesPorEstadoAsync(OrdenEstado estado);
    }
}