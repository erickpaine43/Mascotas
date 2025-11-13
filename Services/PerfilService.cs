// Services/PerfilService.cs
using AutoMapper;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;

namespace Mascotas.Services
{
    public class PerfilService : IPerfilService
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PerfilService> _logger;

        public PerfilService(MascotaDbContext context, IMapper mapper, ILogger<PerfilService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PerfilUsuarioDto> ObtenerPerfilCompletoAsync(int usuarioId)
        {
            var perfil = await _context.PerfilesUsuarios
                .Include(p => p.Usuario)
                .Include(p => p.Direcciones)
                .Include(p => p.Mascotas)
                .Include(p => p.Preferencias)
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

            if (perfil == null)
            {
                // Crear perfil automáticamente si no existe
                perfil = await CrearPerfilAutomaticoAsync(usuarioId);
            }

            return _mapper.Map<PerfilUsuarioDto>(perfil);
        }

        public async Task<PerfilUsuario> ActualizarPerfilBasicoAsync(int usuarioId, ActualizarPerfilDto perfilDto)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);

            perfil.Telefono = perfilDto.Telefono;
            perfil.FechaNacimiento = perfilDto.FechaNacimiento;
            perfil.FotoUrl = perfilDto.FotoUrl;
            perfil.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return perfil;
        }

        public async Task<Direccion> AgregarDireccionAsync(int usuarioId, CrearDireccionDto direccionDto)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);

            // Si es principal, quitar principal de otras direcciones
            if (direccionDto.EsPrincipal)
            {
                await _context.Direcciones
                    .Where(d => d.PerfilUsuarioId == perfil.Id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.EsPrincipal, false));
            }

            var direccion = _mapper.Map<Direccion>(direccionDto);
            direccion.PerfilUsuarioId = perfil.Id;

            _context.Direcciones.Add(direccion);
            await _context.SaveChangesAsync();

            return direccion;
        }

        public async Task<Direccion> ActualizarDireccionAsync(int usuarioId, int direccionId, CrearDireccionDto direccionDto)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);
            var direccion = await _context.Direcciones
                .FirstOrDefaultAsync(d => d.Id == direccionId && d.PerfilUsuarioId == perfil.Id);

            if (direccion == null)
                throw new KeyNotFoundException("Dirección no encontrada");

            // Si es principal, quitar principal de otras direcciones
            if (direccionDto.EsPrincipal && !direccion.EsPrincipal)
            {
                await _context.Direcciones
                    .Where(d => d.PerfilUsuarioId == perfil.Id && d.Id != direccionId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.EsPrincipal, false));
            }

            _mapper.Map(direccionDto, direccion);
            await _context.SaveChangesAsync();

            return direccion;
        }

        public async Task<bool> EliminarDireccionAsync(int usuarioId, int direccionId)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);
            var direccion = await _context.Direcciones
                .FirstOrDefaultAsync(d => d.Id == direccionId && d.PerfilUsuarioId == perfil.Id);

            if (direccion == null)
                return false;

            _context.Direcciones.Remove(direccion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MascotaCliente> AgregarMascotaAsync(int usuarioId, CrearMascotaClienteDto mascotaDto)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);

            var mascota = _mapper.Map<MascotaCliente>(mascotaDto);
            mascota.PerfilUsuarioId = perfil.Id;

            _context.MascotasClientes.Add(mascota);
            await _context.SaveChangesAsync();

            return mascota;
        }

        public async Task<MascotaCliente> ActualizarMascotaAsync(int usuarioId, int mascotaId, CrearMascotaClienteDto mascotaDto)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);
            var mascota = await _context.MascotasClientes
                .FirstOrDefaultAsync(m => m.Id == mascotaId && m.PerfilUsuarioId == perfil.Id);

            if (mascota == null)
                throw new KeyNotFoundException("Mascota no encontrada");

            _mapper.Map(mascotaDto, mascota);
            await _context.SaveChangesAsync();

            return mascota;
        }

        public async Task<bool> EliminarMascotaAsync(int usuarioId, int mascotaId)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);
            var mascota = await _context.MascotasClientes
                .FirstOrDefaultAsync(m => m.Id == mascotaId && m.PerfilUsuarioId == perfil.Id);

            if (mascota == null)
                return false;

            _context.MascotasClientes.Remove(mascota);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PreferenciasUsuario> ActualizarPreferenciasAsync(int usuarioId, PreferenciasUsuarioDto preferenciasDto)
        {
            var perfil = await ObtenerOcrearPerfilAsync(usuarioId);

            if (perfil.Preferencias == null)
            {
                perfil.Preferencias = new PreferenciasUsuario
                {
                    PerfilUsuarioId = perfil.Id
                };
                _context.PreferenciasUsuarios.Add(perfil.Preferencias);
            }

            _mapper.Map(preferenciasDto, perfil.Preferencias);
            await _context.SaveChangesAsync();

            return perfil.Preferencias;
        }

        public async Task<PerfilUsuario> CrearPerfilAutomaticoAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            var perfil = new PerfilUsuario
            {
                UsuarioId = usuarioId,
                Telefono = null,
                FechaNacimiento = null,
                FotoUrl = null,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.PerfilesUsuarios.Add(perfil);
            await _context.SaveChangesAsync();

            // Crear preferencias por defecto
            var preferencias = new PreferenciasUsuario
            {
                PerfilUsuarioId = perfil.Id,
                RecibirNotificacionesEmail = true,
                RecibirOfertasEspeciales = true,
                Idioma = "es"
            };
            _context.PreferenciasUsuarios.Add(preferencias);

            await _context.SaveChangesAsync();
            return perfil;
        }

        private async Task<PerfilUsuario> ObtenerOcrearPerfilAsync(int usuarioId)
        {
            var perfil = await _context.PerfilesUsuarios
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

            if (perfil == null)
            {
                perfil = await CrearPerfilAutomaticoAsync(usuarioId);
            }

            return perfil;
        }
    }
}