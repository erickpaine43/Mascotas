// Services/ReservaCleanupService.cs
namespace Mascotas.Services
{
    public class ReservaCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservaCleanupService> _logger;

        public ReservaCleanupService(IServiceProvider serviceProvider, ILogger<ReservaCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de limpieza de reservas iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var reservaService = scope.ServiceProvider.GetRequiredService<IReservaService>();

                    await reservaService.LiberarReservasExpiradasAsync();
                    _logger.LogInformation("Limpieza de reservas expiradas completada");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio de limpieza de reservas");
                }

                // Ejecutar cada minuto
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}