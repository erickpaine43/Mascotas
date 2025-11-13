// Controllers/PerfilController.cs
using Mascotas.Dto;
using Mascotas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _perfilService;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(IPerfilService perfilService, ILogger<PerfilController> logger)
        {
            _perfilService = perfilService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PerfilUsuarioDto>> ObtenerPerfil()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var perfil = await _perfilService.ObtenerPerfilCompletoAsync(usuarioId);
                return Ok(perfil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo perfil");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut]
        public async Task<ActionResult> ActualizarPerfil(ActualizarPerfilDto perfilDto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                await _perfilService.ActualizarPerfilBasicoAsync(usuarioId, perfilDto);
                return Ok(new { mensaje = "Perfil actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando perfil");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("direcciones")]
        public async Task<ActionResult<List<DireccionDto>>> ObtenerDirecciones()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var perfil = await _perfilService.ObtenerPerfilCompletoAsync(usuarioId);
                return Ok(perfil.Direcciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo direcciones");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("direcciones")]
        public async Task<ActionResult<DireccionDto>> AgregarDireccion(CrearDireccionDto direccionDto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var direccion = await _perfilService.AgregarDireccionAsync(usuarioId, direccionDto);
                var direccionResponse = new DireccionDto
                {
                    Id = direccion.Id,
                    Calle = direccion.Calle,
                    Departamento = direccion.Departamento,
                    Ciudad = direccion.Ciudad,
                    Provincia = direccion.Provincia,
                    CodigoPostal = direccion.CodigoPostal,
                    Pais = direccion.Pais,
                    EsPrincipal = direccion.EsPrincipal,
                    Tipo = direccion.Tipo,
                    Alias = direccion.Alias
                };
                return Ok(direccionResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando dirección");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("direcciones/{id}")]
        public async Task<ActionResult> ActualizarDireccion(int id, CrearDireccionDto direccionDto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                await _perfilService.ActualizarDireccionAsync(usuarioId, id, direccionDto);
                return Ok(new { mensaje = "Dirección actualizada exitosamente" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "Dirección no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando dirección");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpDelete("direcciones/{id}")]
        public async Task<ActionResult> EliminarDireccion(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var eliminado = await _perfilService.EliminarDireccionAsync(usuarioId, id);

                if (!eliminado)
                    return NotFound(new { mensaje = "Dirección no encontrada" });

                return Ok(new { mensaje = "Dirección eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando dirección");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("mascotas")]
        public async Task<ActionResult<List<MascotaClienteDto>>> ObtenerMascotas()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var perfil = await _perfilService.ObtenerPerfilCompletoAsync(usuarioId);
                return Ok(perfil.Mascotas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mascotas");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("mascotas")]
        public async Task<ActionResult<MascotaClienteDto>> AgregarMascota(CrearMascotaClienteDto mascotaDto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var mascota = await _perfilService.AgregarMascotaAsync(usuarioId, mascotaDto);
                var mascotaResponse = new MascotaClienteDto
                {
                    Id = mascota.Id,
                    Nombre = mascota.Nombre,
                    Especie = mascota.Especie,
                    Raza = mascota.Raza,
                    FechaNacimiento = mascota.FechaNacimiento,
                    Peso = mascota.Peso,
                    Sexo = mascota.Sexo,
                    Esterilizado = mascota.Esterilizado,
                    NotasMedicas = mascota.NotasMedicas,
                    Alergias = mascota.Alergias
                };
                return Ok(mascotaResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando mascota");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("mascotas/{id}")]
        public async Task<ActionResult> ActualizarMascota(int id, CrearMascotaClienteDto mascotaDto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                await _perfilService.ActualizarMascotaAsync(usuarioId, id, mascotaDto);
                return Ok(new { mensaje = "Mascota actualizada exitosamente" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "Mascota no encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando mascota");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpDelete("mascotas/{id}")]
        public async Task<ActionResult> EliminarMascota(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var eliminado = await _perfilService.EliminarMascotaAsync(usuarioId, id);

                if (!eliminado)
                    return NotFound(new { mensaje = "Mascota no encontrada" });

                return Ok(new { mensaje = "Mascota eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando mascota");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("preferencias")]
        public async Task<ActionResult<PreferenciasUsuarioDto>> ObtenerPreferencias()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var perfil = await _perfilService.ObtenerPerfilCompletoAsync(usuarioId);
                return Ok(perfil.Preferencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo preferencias");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("preferencias")]
        public async Task<ActionResult> ActualizarPreferencias(PreferenciasUsuarioDto preferenciasDto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                await _perfilService.ActualizarPreferenciasAsync(usuarioId, preferenciasDto);
                return Ok(new { mensaje = "Preferencias actualizadas exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando preferencias");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int usuarioId))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return usuarioId;
        }
    }
}