// Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using Mascotas.Dto;

namespace Mascotas.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarEmailVerificacionAsync(string email, string nombre, string codigoVerificacion)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]  // ← Aquí va CON espacios
                    ),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mensaje = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "noreply@petstore.com", "PetStore"),
                    Subject = "Verifica tu email - PetStore",
                    Body = $@"Hola {nombre},

Gracias por registrarte en PetStore. 
Tu código de verificación es: {codigoVerificacion}

Este código expira en 24 horas.

Si no te registraste, ignora este email.

Saludos,
Equipo PetStore",
                    IsBodyHtml = false  // ← Texto plano
                };

                mensaje.To.Add(email);

                await smtpClient.SendMailAsync(mensaje);

                _logger.LogInformation($"✅ Email enviado a {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email a {email}");
                return false;
            }
        }
        // Services/EmailService.cs - agregar este método
        public async Task<bool> EnviarEmailResetPasswordAsync(string email, string nombre, string codigoReset)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mensaje = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "noreply@petstore.com", "PetStore"),
                    Subject = "Código para restablecer contraseña - PetStore",
                    Body = $@"Hola {nombre},

Hemos recibido una solicitud para restablecer la contraseña de tu cuenta.

Tu código de verificación es: {codigoReset}

Ingresa este código en la aplicación junto con tu nueva contraseña.

Este código expirará en 15 minutos.

Si no solicitaste este cambio, ignora este email.

Saludos,
Equipo PetStore",
                    IsBodyHtml = false
                };

                mensaje.To.Add(email);
                await smtpClient.SendMailAsync(mensaje);

                _logger.LogInformation($"✅ Email de reset enviado a {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email de reset a {email}");
                return false;
            }
        }
        public async Task<bool> EnviarRecordatorioResenaAsync(string email, string nombre, string asunto, string mensaje)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mensajeCompleto = $@"Hola {nombre},

{mensaje}

Saludos,
Equipo Mascotas";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? "noreply@mascotas.com", "Mascotas"),
                    Subject = asunto,
                    Body = mensajeCompleto,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"✅ Email de recordatorio enviado a {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email de recordatorio a {email}");
                return false;
            }
        }
    }
}