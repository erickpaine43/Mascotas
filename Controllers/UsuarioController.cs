// Controllers/UsuariosController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(MascotaDbContext context, IMapper mapper, ILogger<UsuariosController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/usuarios
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .OrderByDescending(u => u.FechaCreacion)
                .ToListAsync();

            var usuariosDto = _mapper.Map<List<UsuarioDto>>(usuarios);
            return Ok(usuariosDto);
        }

        // GET: api/usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Verificar permisos
            var usuarioActualId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var esAdministrador = User.IsInRole("Administrador");

            if (!esAdministrador && usuarioActualId != id)
            {
                return Forbid("No tienes permisos para ver este usuario");
            }

            var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
            return usuarioDto;
        }

        // GET: api/usuarios/perfil
        [HttpGet("perfil")]
        public async Task<ActionResult<UsuarioDto>> GetPerfil()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
            return usuarioDto;
        }

        // POST: api/usuarios
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario(CreateUsuarioDto createUsuarioDto)
        {
            // Verificar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == createUsuarioDto.Email))
            {
                return BadRequest("El email ya está registrado");
            }

            var usuario = _mapper.Map<Usuario>(createUsuarioDto);
            usuario.PasswordHash = HashPassword(createUsuarioDto.Password);
            usuario.Activo = true;
            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.EmailVerificado = false;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuarioDto);
        }

        // PUT: api/usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, UpdateUsuarioDto updateUsuarioDto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Verificar permisos
            var usuarioActualId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var esAdministrador = User.IsInRole("Administrador");

            if (!esAdministrador && usuarioActualId != id)
            {
                return Forbid("No tienes permisos para actualizar este usuario");
            }

            // Solo administradores pueden cambiar el rol
            if (updateUsuarioDto.Rol.HasValue && !esAdministrador)
            {
                return Forbid("No tienes permisos para cambiar el rol");
            }

            // Solo administradores pueden cambiar el estado activo
            if (updateUsuarioDto.Activo.HasValue && !esAdministrador)
            {
                return Forbid("No tienes permisos para cambiar el estado del usuario");
            }

            // Solo administradores pueden cambiar la verificación de email
            if (updateUsuarioDto.EmailVerificado.HasValue && !esAdministrador)
            {
                return Forbid("No tienes permisos para cambiar la verificación de email");
            }

            // Usar AutoMapper para actualizar las propiedades
            _mapper.Map(updateUsuarioDto, usuario);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/usuarios/5/estado
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstadoUsuario(int id, CambiarEstadoUsuarioDto cambiarEstadoDto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // No permitir desactivarse a sí mismo
            var usuarioActualId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (usuarioActualId == id)
            {
                return BadRequest("No puedes desactivar tu propio usuario");
            }

            usuario.Activo = cambiarEstadoDto.Activo;
            await _context.SaveChangesAsync();

            var accion = cambiarEstadoDto.Activo ? "activado" : "desactivado";
            return Ok(new { mensaje = $"Usuario {accion} exitosamente" });
        }

        // PATCH: api/usuarios/5/rol
        [HttpPatch("{id}/rol")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarRolUsuario(int id, CambiarRolUsuarioDto cambiarRolDto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // No permitir cambiar el propio rol
            var usuarioActualId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (usuarioActualId == id)
            {
                return BadRequest("No puedes cambiar tu propio rol");
            }

            usuario.Rol = cambiarRolDto.Rol;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Rol cambiado a {cambiarRolDto.Rol} exitosamente" });
        }

        // DELETE: api/usuarios/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // No permitir eliminarse a sí mismo
            var usuarioActualId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (usuarioActualId == id)
            {
                return BadRequest("No puedes eliminar tu propio usuario");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }

        // GET: api/usuarios/estadisticas
        [HttpGet("estadisticas")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> GetEstadisticas()
        {
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo);
            var usuariosPorRol = await _context.Usuarios
                .GroupBy(u => u.Rol)
                .Select(g => new
                {
                    Rol = g.Key.ToString(),
                    Cantidad = g.Count()
                })
                .ToListAsync();

            var nuevosUsuariosEsteMes = await _context.Usuarios
                .CountAsync(u => u.FechaCreacion.Month == DateTime.UtcNow.Month &&
                                u.FechaCreacion.Year == DateTime.UtcNow.Year);

            return Ok(new
            {
                totalUsuarios,
                usuariosActivos,
                usuariosInactivos = totalUsuarios - usuariosActivos,
                usuariosPorRol,
                nuevosUsuariosEsteMes
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}