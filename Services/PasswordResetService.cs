// Services/PasswordResetService.cs
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using System.Security.Cryptography;
using System.Text;

namespace Mascotas.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly MascotaDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            MascotaDbContext context,
            IEmailService emailService,
            ILogger<PasswordResetService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SolicitarResetPasswordAsync(string email)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Activo);

                if (usuario == null)
                {
                    // Por seguridad, siempre retornar true
                    _logger.LogInformation($"Solicitud de reset para email no encontrado: {email}");
                    return true;
                }

                // Generar código de 6 dígitos
                var codigo = GenerateRandomCode();
                var expiracion = DateTime.UtcNow.AddMinutes(15);

                // Guardar código en el usuario
                usuario.CodigoVerificacion = codigo;
                usuario.ExpiracionCodigoVerificacion = expiracion;

                await _context.SaveChangesAsync();

                // Enviar email con el código
                await _emailService.EnviarEmailResetPasswordAsync(email, usuario.Nombre, codigo);

                _logger.LogInformation($"Código de reset enviado a: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error solicitando reset de password para: {email}");
                return false;
            }
        }

        public async Task<bool> ResetearPasswordConCodigoAsync(string email, string codigo, string nuevaPassword)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email &&
                                             u.CodigoVerificacion == codigo &&
                                             u.ExpiracionCodigoVerificacion > DateTime.UtcNow);

                if (usuario == null)
                    return false;

                // Cambiar contraseña
                usuario.PasswordHash = HashPassword(nuevaPassword);

                // Limpiar código usado
                usuario.CodigoVerificacion = null;
                usuario.ExpiracionCodigoVerificacion = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reseteado para: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reseteando password con código para: {email}");
                return false;
            }
        }

        private string GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
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