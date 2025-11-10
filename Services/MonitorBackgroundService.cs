using Microsoft.EntityFrameworkCore;
using Mascotas.Services;

public class MonitorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MonitorBackgroundService> _logger;

    public MonitorBackgroundService(IServiceProvider services, ILogger<MonitorBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Servicio de Monitor de Búsquedas iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var monitorService = scope.ServiceProvider
                        .GetRequiredService<IMonitorBusquedasService>();

                    var notificacionService = scope.ServiceProvider
                        .GetRequiredService<INotificacionService>();

                    // 1. Verificar cambios en búsquedas guardadas
                    _logger.LogInformation("🔍 Verificando búsquedas guardadas...");
                    await monitorService.VerificarCambiosBusquedasGuardadasAsync();

                    // 2. Enviar notificaciones pendientes
                    _logger.LogInformation("📧 Enviando notificaciones pendientes...");
                    var enviadas = await notificacionService.EnviarNotificacionesPendientesAsync();
                    _logger.LogInformation($"✅ {enviadas} notificaciones enviadas");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en el servicio background");
            }

            // Esperar 6 horas antes de la siguiente verificación
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}