// Controllers/AuthController.cs
using AutoMapper;
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mascotas.Data;

namespace PetStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtServices _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            MascotaDbContext context,
            IMapper mapper,
            IJwtServices jwtService,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("crear-admin-inicial")]
        [AllowAnonymous]
        public async Task<ActionResult> CrearAdminInicial()
        {
            if (await _context.Usuarios.AnyAsync())
            {
                return BadRequest(new
                {
                    mensaje = "Ya existen usuarios en el sistema.",
                    instruccion = "Si quieres empezar de nuevo, elimina todos los usuarios de la base de datos."
                });
            }

            var admin = new Usuario
            {
                Email = "admin@petstore.com",
                PasswordHash = HashPassword("admin123"),
                Nombre = "Administrador Principal",
                Rol = UsuarioRol.Administrador,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                EmailVerificado = true
            };

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Usuario administrador creado exitosamente",
                credenciales = new
                {
                    email = "admin@petstore.com",
                    password = "admin123"
                },
                advertencia = "Cambia la contraseña inmediatamente después del primer login",
                instrucciones = new
                {
                    paso1 = "Usa estas credenciales para hacer login en /api/Auth/login",
                    paso2 = "Luego podrás crear más usuarios en /api/Auth/registro",
                    paso3 = "Cambia la contraseña en /api/Auth/cambiar-password"
                }
            });
        }

        
        [HttpPost("registro-publico")]
        [AllowAnonymous]
        public async Task<ActionResult> RegistroPublico(RegistroPublicoDto registroDto)
        {
            try
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
                {
                    return BadRequest(new { mensaje = "El email ya está registrado" });
                }

                var codigoVerificacion = GenerateRandomCode();

                // 1. Crear en tabla Usuarios
                var usuario = new Usuario
                {
                    Email = registroDto.Email,
                    PasswordHash = HashPassword(registroDto.Password),
                    Nombre = registroDto.Nombre,
                    Rol = UsuarioRol.Cliente,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    EmailVerificado = false,
                    CodigoVerificacion = codigoVerificacion,
                    ExpiracionCodigoVerificacion = DateTime.UtcNow.AddHours(24)
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync(); // Guardar para obtener el ID

                // 2. Crear en tabla Clientes
                var cliente = new Cliente
                {
                    Nombre = registroDto.Nombre,
                    Email = registroDto.Email,
                    Telefono = "", // Opcional
                    Direccion = "", // Opcional
                    Ciudad = "", // Opcional
                    CodigoPostal = "", // Opcional
                    FechaRegistro = DateTime.UtcNow,
                    UsuarioId = usuario.Id // ← Enlazar con el usuario
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                // Enviar email de verificación
                var emailEnviado = await _emailService.EnviarEmailVerificacionAsync(
                    usuario.Email,
                    usuario.Nombre,
                    codigoVerificacion
                );

                return Ok(new
                {
                    mensaje = "Registro exitoso. Revisa tu email para verificar tu cuenta.",
                    email = usuario.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro público");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("verificar-email")]
        [AllowAnonymous]
        public async Task<ActionResult> VerificarEmail(VerificarEmailDto verificarDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == verificarDto.Email &&
                                             u.CodigoVerificacion == verificarDto.Codigo &&
                                             u.ExpiracionCodigoVerificacion > DateTime.UtcNow);

                if (usuario == null)
                {
                    return BadRequest(new { mensaje = "Código inválido o expirado" });
                }

                usuario.EmailVerificado = true;
                usuario.CodigoVerificacion = null;
                usuario.ExpiracionCodigoVerificacion = null;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Email verificado exitosamente. Ya puedes iniciar sesión." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando email");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("reenviar-codigo")]
        [AllowAnonymous]
        public async Task<ActionResult> ReenviarCodigo(ReenviarCodigoDto reenviarDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == reenviarDto.Email && !u.EmailVerificado);

                if (usuario == null)
                {
                    return BadRequest(new { mensaje = "Email no encontrado o ya verificado" });
                }

                var nuevoCodigo = GenerateRandomCode();
                usuario.CodigoVerificacion = nuevoCodigo;
                usuario.ExpiracionCodigoVerificacion = DateTime.UtcNow.AddHours(24);

                await _context.SaveChangesAsync();

                var emailEnviado = await _emailService.EnviarEmailVerificacionAsync(
                    usuario.Email,
                    usuario.Nombre,
                    nuevoCodigo
                );

                if (!emailEnviado)
                {
                    _logger.LogWarning($"No se pudo reenviar email a {usuario.Email}");
                }

                return Ok(new
                {
                    mensaje = "Código reenviado. Revisa tu email.",
                    codigoDebug = emailEnviado ? null : nuevoCodigo // Solo en caso de error
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reenviando código");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Activo);

                if (usuario == null || !VerifyPassword(loginDto.Password, usuario.PasswordHash))
                {
                    return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
                }

                if (!usuario.EmailVerificado)
                {
                    return Unauthorized(new
                    {
                        mensaje = "Por favor verifica tu email antes de iniciar sesión",
                        emailNoVerificado = true
                    });
                }

                usuario.UltimoLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(usuario);
                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);

                var response = new LoginResponseDto
                {
                    Token = token,
                    Usuario = usuarioDto,
                    Expiracion = DateTime.Now.AddHours(8)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        // REEMPLAZA el método CambiarPassword por este:

        [Authorize]
        [HttpPost("cambiar-password")]
        public async Task<ActionResult> CambiarPassword(CambiarPasswordDto cambiarDto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var usuario = await _context.Usuarios.FindAsync(usuarioId);

                if (usuario == null)
                {
                    return NotFound(new { mensaje = "Usuario no encontrado" });
                }

                // 1. Verificar contraseña actual (SIEMPRE requerida)
                if (!VerifyPassword(cambiarDto.PasswordActual, usuario.PasswordHash))
                {
                    return BadRequest(new { mensaje = "La contraseña actual es incorrecta" });
                }

                var cambios = new List<string>();
                var emailCambiado = false;

                // 2. Cambiar contraseña si se proporcionó
                if (!string.IsNullOrEmpty(cambiarDto.NuevaPassword))
                {
                    // Validar que venga la confirmación
                    if (string.IsNullOrEmpty(cambiarDto.ConfirmarPassword))
                    {
                        return BadRequest(new { mensaje = "La confirmación de contraseña es requerida" });
                    }

                    usuario.PasswordHash = HashPassword(cambiarDto.NuevaPassword);
                    cambios.Add("contraseña");
                }

                // 3. Cambiar email si se proporcionó y es diferente
                if (!string.IsNullOrEmpty(cambiarDto.NuevoEmail) &&
                    cambiarDto.NuevoEmail != usuario.Email)
                {
                    // Verificar que el nuevo email no esté en uso
                    if (await _context.Usuarios.AnyAsync(u => u.Email == cambiarDto.NuevoEmail && u.Id != usuarioId))
                    {
                        return BadRequest(new { mensaje = "El email ya está en uso por otro usuario" });
                    }

                    var emailAnterior = usuario.Email;
                    usuario.Email = cambiarDto.NuevoEmail;

                    // Si cambia el email, debe verificar nuevamente
                    usuario.EmailVerificado = false;
                    usuario.CodigoVerificacion = GenerateRandomCode();
                    usuario.ExpiracionCodigoVerificacion = DateTime.UtcNow.AddHours(24);

                    cambios.Add("email");
                    emailCambiado = true;
                }

                // 4. Si no hay cambios, retornar error
                if (cambios.Count == 0)
                {
                    return BadRequest(new { mensaje = "No se proporcionaron cambios para actualizar" });
                }

                await _context.SaveChangesAsync();

                // 5. Si cambió el email, enviar código de verificación
                if (emailCambiado)
                {
                    await _emailService.EnviarEmailVerificacionAsync(
                        usuario.Email,
                        usuario.Nombre,
                        usuario.CodigoVerificacion
                    );

                    return Ok(new
                    {
                        mensaje = $"Perfil actualizado exitosamente. Se han actualizado: {string.Join(", ", cambios)}.",
                        advertencia = "Se ha enviado un código de verificación a tu nuevo email.",
                        email = usuario.Email,
                        requiereVerificacion = true
                    });
                }

                return Ok(new
                {
                    mensaje = $"Perfil actualizado exitosamente. Se han actualizado: {string.Join(", ", cambios)}."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando perfil");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [Authorize(Roles = "Administrador,Gerente")]
        [HttpPost("registro")]
        public async Task<ActionResult<UsuarioDto>> Registro(RegisterDto registroDto)
        {
            try
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
                {
                    return BadRequest(new { mensaje = "El email ya está registrado" });
                }

                var usuario = _mapper.Map<Usuario>(registroDto);
                usuario.PasswordHash = HashPassword(registroDto.Password);
                usuario.EmailVerificado = true;
                usuario.Activo = true;
                usuario.FechaCreacion = DateTime.UtcNow;

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var usuarioDto = _mapper.Map<UsuarioDto>(usuario);
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro administrativo");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [Authorize(Roles = "Administrador,Gerente")]
        [HttpPost("promover-a-empleado/{userId}")]
        public async Task<ActionResult> PromoverAEmpleado(int userId)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(userId);

                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                usuario.Rol = UsuarioRol.Empleado;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = $"{usuario.Email} ahora es empleado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoviendo usuario");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios/{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return _mapper.Map<UsuarioDto>(usuario);
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Ok(_mapper.Map<List<UsuarioDto>>(usuarios));
        }

        // 🔐 MÉTODOS PRIVADOS
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }

        private string GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}