using Mascotas.Data;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;

namespace Mascotas.Services
{
    public class ReviewReminderService : IReviewReminderService
    {
        private readonly MascotaDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReviewReminderService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReviewReminderService(
            MascotaDbContext context,
            IEmailService emailService,
            ILogger<ReviewReminderService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task ProgramarRecordatorios(int ordenId)
        {
            try
            {
                var orden = await _context.Ordenes
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Producto)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Animal)
                    .Include(o => o.Cliente)
                    .FirstOrDefaultAsync(o => o.Id == ordenId && o.Estado == OrdenEstado.Completada);

                if (orden == null)
                {
                    _logger.LogWarning($"Orden {ordenId} no encontrada o no completada para programar recordatorios");
                    return;
                }

                foreach (var item in orden.Items)
                {
                    // Crear recordatorio para cada item de la orden
                    var reminder = new ReviewReminder
                    {
                        OrdenId = ordenId,
                        ClienteId = orden.ClienteId,
                        ProductoId = item.ProductoId,
                        AnimalId = item.AnimalId,
                        TipoItem = item.ProductoId.HasValue ? "Producto" : "Animal",
                        NombreItem = item.Producto?.Nombre ?? item.Animal?.Nombre ?? "Item no disponible",
                        FechaCreacion = DateTime.UtcNow
                    };

                    _context.ReviewReminders.Add(reminder);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Programados {orden.Items.Count} recordatorios para orden {ordenId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error programando recordatorios para orden {ordenId}");
            }
        }

        public async Task EnviarRecordatoriosPendientes()
        {
            try
            {
                var ahora = DateTime.UtcNow;

                // Primer recordatorio (1 minuto después de creación - PARA TESTING)
                var primerosRecordatorios = await _context.ReviewReminders
                    .Include(r => r.Cliente)
                    .Include(r => r.Producto)
                    .Include(r => r.Animal)
                    .Where(r => !r.PrimerRecordatorioEnviado.HasValue
                             && r.FechaCreacion <= ahora.AddMinutes(-1) // 1 minuto para testing
                             && !r.ResenaCompletada)
                    .ToListAsync();

                foreach (var reminder in primerosRecordatorios)
                {
                    await EnviarEmailRecordatorio(reminder, esPrimero: true);
                    reminder.PrimerRecordatorioEnviado = ahora;
                    _logger.LogInformation($"Primer recordatorio enviado para {reminder.NombreItem} a cliente {reminder.ClienteId}");
                }

                // Segundo recordatorio (5 minutos después del primero - PARA TESTING)
                var segundosRecordatorios = await _context.ReviewReminders
                    .Include(r => r.Cliente)
                    .Include(r => r.Producto)
                    .Include(r => r.Animal)
                    .Where(r => r.PrimerRecordatorioEnviado.HasValue
                             && !r.SegundoRecordatorioEnviado.HasValue
                             && r.PrimerRecordatorioEnviado.Value <= ahora.AddMinutes(-5) // 5 minutos para testing
                             && !r.ResenaCompletada)
                    .ToListAsync();

                foreach (var reminder in segundosRecordatorios)
                {
                    await EnviarEmailRecordatorio(reminder, esPrimero: false);
                    reminder.SegundoRecordatorioEnviado = ahora;
                    _logger.LogInformation($"Segundo recordatorio enviado para {reminder.NombreItem} a cliente {reminder.ClienteId}");
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando recordatorios pendientes");
            }
        }

        public async Task<bool> ClientePuedeResenar(int clienteId, int? productoId = null, int? animalId = null)
        {
            return await _context.ReviewReminders
                .AnyAsync(r => r.ClienteId == clienteId
                            && ((productoId.HasValue && r.ProductoId == productoId) ||
                                (animalId.HasValue && r.AnimalId == animalId))
                            && !r.ResenaCompletada);
        }

        public async Task<List<ReviewReminder>> ObtenerRecordatoriosPendientes(int clienteId)
        {
            return await _context.ReviewReminders
                .Include(r => r.Producto)
                .Include(r => r.Animal)
                .Where(r => r.ClienteId == clienteId && !r.ResenaCompletada)
                .ToListAsync();
        }

        public async Task MarcarResenaCompletada(int clienteId, int? productoId = null, int? animalId = null)
        {
            var reminders = await _context.ReviewReminders
                .Where(r => r.ClienteId == clienteId
                         && ((productoId.HasValue && r.ProductoId == productoId) ||
                             (animalId.HasValue && r.AnimalId == animalId))
                         && !r.ResenaCompletada)
                .ToListAsync();

            foreach (var reminder in reminders)
            {
                reminder.ResenaCompletada = true;
            }

            await _context.SaveChangesAsync();
        }

        private async Task EnviarEmailRecordatorio(ReviewReminder reminder, bool esPrimero)
        {
            try
            {
                var cliente = reminder.Cliente;
                var itemNombre = reminder.NombreItem;
                var tipoItem = reminder.TipoItem.ToLower();

                var enlaceResena = GenerarEnlaceResena(reminder);

                var asunto = esPrimero
                    ? $"¿Qué tal tu {tipoItem} {itemNombre}?"
                    : $"Recordatorio: Cuéntanos sobre {itemNombre}";

                var mensaje = esPrimero
                    ? $@"Esperamos que tu {tipoItem} '{itemNombre}' esté siendo genial para tu mascota. 
¿Te gustaría compartir tu experiencia con otros dueños de mascotas?

Tu opinión ayuda a otros clientes a tomar mejores decisiones.

¡Deja tu opinión ahora!
{enlaceResena}
Si el enlace no funciona, visita nuestra app y busca '{itemNombre}' en tus pedidos."
                    : $@"Vemos que aún no has dejado tu opinión sobre '{itemNombre}'. 
Tu opinión es muy valiosa para nosotros y para la comunidad de mascotas.

¿Podrías tomarte un momento para compartir tu experiencia?";

                // ✅ AQUÍ VA LA LÍNEA CORREGIDA - Usando los parámetros correctos
                await _emailService.EnviarRecordatorioResenaAsync(
                    cliente.Email,
                    cliente.Nombre,
                    asunto,
                    mensaje
                );

                _logger.LogInformation($"✅ Email de recordatorio ENVIADO a {cliente.Email}: {asunto}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error enviando email de recordatorio para cliente {reminder.ClienteId}");
            }
        }   
            private string GenerarEnlaceResena(ReviewReminder reminder)
            {
            var httpContext = _httpContextAccessor.HttpContext;
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            if (reminder.ProductoId.HasValue)
            {
                return $"{baseUrl}/producto/{reminder.ProductoId}/reseña?cliente={reminder.ClienteId}&orden={reminder.OrdenId}";
            }
            else if (reminder.AnimalId.HasValue)
            {
                return $"{baseUrl}/animal/{reminder.AnimalId}/reseña?cliente={reminder.ClienteId}&orden={reminder.OrdenId}";
            }

            return $"{baseUrl}/mis-pedidos"; // Fallback
        }
    }
    }
