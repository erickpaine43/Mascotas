using Mascotas.Data;
using Mascotas.Models;

namespace Mascotas.Services
{
    public class OrdenNotificacionService : IOrdenNotificacionService
    {
        private readonly IEmailService _emailService;
        private readonly INotificacionService _notificacionService;
        private readonly MascotaDbContext _context;
        private readonly ILogger<OrdenNotificacionService> _logger;

        public OrdenNotificacionService(
            IEmailService emailService,
            INotificacionService notificacionService,
            MascotaDbContext context,
            ILogger<OrdenNotificacionService> logger)
        {
            _emailService = emailService;
            _notificacionService = notificacionService;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> EnviarNotificacionConfirmacionAsync(Orden orden)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(orden.ClienteId);
                if (cliente == null) return false;

                var asunto = $"✅ Confirmación de tu pedido #{orden.NumeroOrden}";
                var mensaje = $@"
Hola {cliente.Nombre},

¡Tu pedido ha sido confirmado! 

📦 **Número de orden:** {orden.NumeroOrden}
🔢 **Número de tracking:** {orden.TrackingNumber}
💰 **Total:** ${orden.Total:N2}
📅 **Fecha estimada de entrega:** {DateTime.UtcNow.AddDays(3 - 5).ToShortDateString()}

Puedes seguir el estado de tu pedido en cualquier momento usando tu número de tracking.

Gracias por confiar en nosotros,
Equipo Mascotas";

                var resultado = await _emailService.EnviarRecordatorioResenaAsync(
                    cliente.Email,
                    cliente.Nombre,
                    asunto,
                    mensaje
                );

                if (resultado)
                {
                    _logger.LogInformation($"✅ Notificación de confirmación enviada para orden {orden.NumeroOrden}");

                    // También crear notificación interna
                    await _notificacionService.CrearNotificacionAsync(new Notificacion
                    {
                        UsuarioId = orden.ClienteId.ToString(),
                        Titulo = "Orden Confirmada",
                        Mensaje = $"Tu orden #{orden.NumeroOrden} ha sido confirmada",
                        Tipo = TipoNotificacion.Orden.ToString(),
                        Leida = false,
                        EnviarEmail = false, // Ya enviamos email específico
                        FechaCreacion = DateTime.UtcNow
                    });
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando notificación de confirmación para orden {orden.Id}");
                return false;
            }
        }

        public async Task<bool> EnviarNotificacionEnvioAsync(Orden orden, string infoEnvio)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(orden.ClienteId);
                if (cliente == null) return false;

                var asunto = $"🚚 Tu pedido #{orden.NumeroOrden} ha sido enviado";
                var mensaje = $@"
Hola {cliente.Nombre},

¡Buenas noticias! Tu pedido está en camino.

📦 **Número de orden:** {orden.NumeroOrden}
🔢 **Número de tracking:** {orden.TrackingNumber}
🚚 **Información de envío:** {infoEnvio}
📍 **Estado actual:** En tránsito

Puedes rastrear tu pedido en tiempo real usando el número de tracking proporcionado.

Equipo Mascotas";

                return await _emailService.EnviarRecordatorioResenaAsync(
                    cliente.Email,
                    cliente.Nombre,
                    asunto,
                    mensaje
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando notificación de envío para orden {orden.Id}");
                return false;
            }
        }

        public async Task<bool> EnviarNotificacionEntregaAsync(Orden orden)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(orden.ClienteId);
                if (cliente == null) return false;

                var asunto = $"🎉 Tu pedido #{orden.NumeroOrden} ha sido entregado";
                var mensaje = $@"
Hola {cliente.Nombre},

¡Tu pedido ha sido entregado exitosamente!

📦 **Número de orden:** {orden.NumeroOrden}
✅ **Estado:** Entregado
📅 **Fecha de entrega:** {DateTime.UtcNow.ToShortDateString()}

Esperamos que tanto tú como tu mascota disfruten los productos. En unos días te enviaremos una solicitud para que compartas tu experiencia.

Gracias por tu compra,
Equipo Mascotas";

                var resultado = await _emailService.EnviarRecordatorioResenaAsync(
                    cliente.Email,
                    cliente.Nombre,
                    asunto,
                    mensaje
                );

                if (resultado)
                {
                    // Programar recordatorio de reseña para 3 días después
                    await ProgramarRecordatorioResenaAsync(orden, 3);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando notificación de entrega para orden {orden.Id}");
                return false;
            }
        }

        public async Task ProgramarRecordatorioResenaAsync(Orden orden, int diasDelay = 3)
        {
            try
            {
                // Usar tu BackgroundService existente para programar
                var fechaEnvio = DateTime.UtcNow.AddDays(diasDelay);

                // Aquí podrías almacenar en base de datos para que tu MonitorBackgroundService lo procese
                await _notificacionService.CrearNotificacionAsync(new Notificacion
                {
                    UsuarioId = orden.ClienteId.ToString(),
                    Titulo = "¿Cómo fue tu experiencia?",
                    Mensaje = $"Cuéntanos qué tal tu pedido #{orden.NumeroOrden}",
                    Tipo = TipoNotificacion.Resena.ToString(),
                    Leida = false,
                    EnviarEmail = true,
                    FechaCreacion = DateTime.UtcNow,
                });

                _logger.LogInformation($"✅ Recordatorio de reseña programado para orden {orden.NumeroOrden} en {fechaEnvio}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error programando recordatorio de reseña para orden {orden.Id}");
            }
        }
        // En Services/OrdenNotificacionService.cs - agregar este método:
        public async Task<bool> EnviarNotificacionEstadoAsync(Orden orden, OrdenEstado nuevoEstado, string descripcionPersonalizada = null)
        {
            try
            {
                switch (nuevoEstado)
                {
                    case OrdenEstado.Confirmada:
                        return await EnviarNotificacionConfirmacionAsync(orden);

                    case OrdenEstado.Enviada:
                        return await EnviarNotificacionEnvioAsync(orden, descripcionPersonalizada ?? "En camino");

                    case OrdenEstado.Entregada:
                        return await EnviarNotificacionEntregaAsync(orden);

                    default:
                        _logger.LogInformation($"No se envía notificación para estado: {nuevoEstado}");
                        return true; // No es error, simplemente no notificamos este estado
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en EnviarNotificacionEstadoAsync para orden {orden.Id}");
                return false;
            }
        }
    }
}
