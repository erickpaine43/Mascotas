// Services/IPerfilService.cs
using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IPerfilService
    {
        Task<PerfilUsuarioDto> ObtenerPerfilCompletoAsync(int usuarioId);
        Task<PerfilUsuario> ActualizarPerfilBasicoAsync(int usuarioId, ActualizarPerfilDto perfilDto);
        Task<Direccion> AgregarDireccionAsync(int usuarioId, CrearDireccionDto direccionDto);
        Task<Direccion> ActualizarDireccionAsync(int usuarioId, int direccionId, CrearDireccionDto direccionDto);
        Task<bool> EliminarDireccionAsync(int usuarioId, int direccionId);
        Task<MascotaCliente> AgregarMascotaAsync(int usuarioId, CrearMascotaClienteDto mascotaDto);
        Task<MascotaCliente> ActualizarMascotaAsync(int usuarioId, int mascotaId, CrearMascotaClienteDto mascotaDto);
        Task<bool> EliminarMascotaAsync(int usuarioId, int mascotaId);
        Task<PreferenciasUsuario> ActualizarPreferenciasAsync(int usuarioId, PreferenciasUsuarioDto preferenciasDto);
        Task<PerfilUsuario> CrearPerfilAutomaticoAsync(int usuarioId);
    }
}