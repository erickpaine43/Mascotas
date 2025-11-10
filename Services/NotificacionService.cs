using Mascotas.Data;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;

namespace Mascotas.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly MascotaDbContext _context;
        private readonly ILogger<NotificacionService> _logger;
        private readonly IEmailService _emailService;

        public NotificacionService(MascotaDbContext context, ILogger<NotificacionService> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<Notificacion> CrearNotificacionAsync(Notificacion notificacion)
        {
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Notificación creada: {notificacion.Titulo} para usuario {notificacion.UsuarioId}");

            return notificacion;
        }
        public async Task<List<Notificacion>> ObtenerNotificacionesUsuarioAsync(string usuarioId, bool noLeidas = false)
        {
            var query = _context.Notificaciones
                .Include(n => n.Producto)
                .Include(n => n.FiltroGuardado)
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion);

            if (noLeidas)
            {
                query = query.Where(n => !n.Leida) as IOrderedQueryable<Notificacion>;
            }

            return await query.ToListAsync();
        }

        public async Task MarcarComoLeidaAsync(int notificacionId, string usuarioId)
        {
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);

            if (notificacion != null && !notificacion.Leida)
            {
                notificacion.Leida = true;
                notificacion.FechaLectura = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> EnviarNotificacionesPendientesAsync()
        {
            var notificacionesPendientes = await _context.Notificaciones
                .Include(n => n.Producto)
                .Where(n => !n.Enviada && (n.EnviarEmail || n.EnviarPush))
                .Take(50) // Limitar por ejecución
                .ToListAsync();

            var enviadasCount = 0;

            foreach (var notificacion in notificacionesPendientes)
            {
                try
                {
                    await EnviarNotificacionIndividualAsync(notificacion.Id);
                    enviadasCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error enviando notificación {notificacion.Id}");
                }
            }

            return enviadasCount;
        }

        public async Task<int> EnviarNotificacionIndividualAsync(int notificacionId)
        {
            var notificacion = await _context.Notificaciones
                .Include(n => n.Producto)
                .FirstOrDefaultAsync(n => n.Id == notificacionId);

            if (notificacion == null) return 0;

            var enviada = false;

            // Envío por Email
            if (notificacion.EnviarEmail && !notificacion.Enviada)
            {
                await EnviarEmailNotificacionAsync(notificacion);
                enviada = true;
            }

            // Envío por Push (futura implementación)
            if (notificacion.EnviarPush && !notificacion.Enviada)
            {
                await EnviarPushNotificacionAsync(notificacion);
                enviada = true;
            }

            if (enviada)
            {
                notificacion.Enviada = true;
                notificacion.FechaEnvio = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return 1;
            }

            return 0;
        }

        private async Task EnviarEmailNotificacionAsync(Notificacion notificacion)
        {
            try
            {
                var emailUsuario = await ObtenerEmailUsuarioAsync(notificacion.UsuarioId);
                if (string.IsNullOrEmpty(emailUsuario))
                {
                    _logger.LogWarning($"No se pudo obtener email para usuario {notificacion.UsuarioId}");
                    return;
                }

                // ✅ OBTENER NOMBRE DEL USUARIO TAMBIÉN
                var usuario = await _context.Usuarios
                    .Where(u => u.Id == int.Parse(notificacion.UsuarioId) && u.Activo)
                    .Select(u => new { u.Nombre, u.Email })
                    .FirstOrDefaultAsync();

                var nombreUsuario = usuario?.Nombre ?? "Cliente";

                var asunto = $"[Mascotas] {notificacion.Titulo}";

                // ✅ EMAIL HTML PROFESIONAL - ESTILO AMAZON
                var cuerpoHtml = @$"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px 20px; text-align: center; }}
        .content {{ padding: 30px 20px; background: #f9f9f9; }}
        .product-card {{ background: white; border: 1px solid #e0e0e0; border-radius: 10px; padding: 20px; margin: 15px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .button {{ background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; background: white; }}
        .changes-list {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .change-item {{ padding: 8px 0; border-bottom: 1px solid #eee; }}
        .change-item:last-child {{ border-bottom: none; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎯 Novedades en Tu Tienda Mascotas</h1>
            <p>Encontramos cambios que pueden interesarte</p>
        </div>
        
        <div class='content'>
            <h2>¡Hola {nombreUsuario}!</h2>
            <p>Hemos detectado novedades en una de tus búsquedas guardadas:</p>
            
            <div class='changes-list'>
                <h3>📋 Resumen de cambios:</h3>
                <p><strong>{notificacion.Mensaje}</strong></p>
            </div>

            {(notificacion.Producto != null ? $@"
            <div class='product-card'>
                <h3>🛍️ Producto destacado</h3>
                <h4>{notificacion.Producto.Nombre}</h4>
                <p><strong>💰 Precio:</strong> ${notificacion.Producto.Precio:N2}</p>
                {(notificacion.Producto.Descuento > 0 ? $@"<p><strong>🎉 Descuento:</strong> {notificacion.Producto.Descuento}% OFF</p>" : "")}
                <p>{notificacion.Producto.DescripcionCorta}</p>
                <a href='https://tutienda.com/productos/{notificacion.Producto.Id}' class='button'>Ver Producto</a>
            </div>
            " : "")}
            
            <div style='margin-top: 20px; padding: 15px; background: #e8f4fd; border-radius: 8px;'>
                <p><strong>💡 Tip:</strong> Puedes gestionar tus búsquedas guardadas y notificaciones en tu cuenta.</p>
            </div>
        </div>
        
        <div class='footer'>
            <p><strong>Equipo Mascotas</strong></p>
            <p><a href='https://tutienda.com' style='color: #667eea;'>Visitar nuestra tienda</a></p>
            <p><small>Si no deseas recibir estas notificaciones, puedes desactivarlas en tu perfil de usuario.</small></p>
        </div>
    </div>
</body>
</html>";

                // ✅ USAR TU EMAILSERVICE EXISTENTE
                var resultado = await _emailService.EnviarRecordatorioResenaAsync(
                    emailUsuario,
                    nombreUsuario,
                    asunto,
                    cuerpoHtml
                );

                if (resultado)
                {
                    _logger.LogInformation($"✅ EMAIL REAL ENVIADO a {emailUsuario} ({nombreUsuario}): {asunto}");

                    // Marcar como enviada en la base de datos
                    notificacion.Enviada = true;
                    notificacion.FechaEnvio = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError($"❌ FALLÓ el envío de email a {emailUsuario}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email real para notificación {notificacion.Id}");
            }
        }

        private async Task EnviarPushNotificacionAsync(Notificacion notificacion)
        {
            // Implementación futura para push notifications
            _logger.LogInformation($"PUSH NOTIFICATION para {notificacion.UsuarioId}: {notificacion.Titulo}");
            await Task.CompletedTask;
        }
        public async Task<string?> ObtenerEmailUsuarioAsync(string usuarioId)
        {
            try
            {
                // ✅ CONVERTIR usuarioId string a int (asumiendo que es el Id numérico)
                if (int.TryParse(usuarioId, out int usuarioIdInt))
                {
                    var usuario = await _context.Usuarios
                        .Where(u => u.Id == usuarioIdInt && u.Activo && u.EmailVerificado)
                        .Select(u => new { u.Email, u.Nombre })
                        .FirstOrDefaultAsync();

                    if (usuario != null)
                    {
                        _logger.LogInformation($"✅ Email encontrado para usuario {usuarioId}: {usuario.Email}");
                        return usuario.Email;
                    }
                }

                _logger.LogWarning($"❌ No se encontró usuario activo y verificado con ID: {usuarioId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error obteniendo email para usuario {usuarioId}");
                return null;
            }
        }
    }
}

