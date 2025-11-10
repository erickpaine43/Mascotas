using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Mascotas.Services
{
    public class MonitorBusquedasService : IMonitorBusquedasService
    {
            private readonly MascotaDbContext _context;
            private readonly IBusquedaAvanzadaService _busquedaService;
            private readonly ILogger<MonitorBusquedasService> _logger;
            private readonly INotificacionService _notificacionService;

        public MonitorBusquedasService(MascotaDbContext context, IBusquedaAvanzadaService busquedaService, ILogger<MonitorBusquedasService> logger, INotificacionService notificacionService)
        {
            _context = context;
            _busquedaService = busquedaService;
            _logger = logger;
            _notificacionService = notificacionService;
        }


        public async Task VerificarCambiosBusquedasGuardadasAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando verificación de búsquedas guardadas...");

                var busquedasActivas = await _context.FiltroGuardados
                    .Where(f => f.MonitorearNuevosProductos || f.MonitorearBajasPrecio)
                    .ToListAsync();

                _logger.LogInformation($"Encontradas {busquedasActivas.Count} búsquedas activas para monitorear");

                foreach (var busqueda in busquedasActivas)
                {
                    var cambios = await ObtenerCambiosParaBusqueda(busqueda.Id);
                    if (cambios.Any())
                    {
                        _logger.LogInformation($"Enviando {cambios.Count} notificaciones para búsqueda {busqueda.Id}");
                        // Aquí enviarías notificaciones reales (email, push, etc.)
                        await EnviarNotificacionAmazonStyle(busqueda, cambios);

                        busqueda.FechaUltimaRevision = DateTime.UtcNow;
                        busqueda.TotalNotificacionesEnviadas += cambios.Count;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Verificación de búsquedas guardadas completada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en VerificarCambiosBusquedasGuardadasAsync");
            }
        }

        public async Task<List<ResultadoCambio>> ObtenerCambiosParaBusqueda(int filtroGuardadoId)
        {
            var busqueda = await _context.FiltroGuardados.FindAsync(filtroGuardadoId);
            if (busqueda == null) return new List<ResultadoCambio>();

            var parametros = JsonSerializer.Deserialize<ProductoSearchParams>(busqueda.ParametrosBusqueda);
            if (parametros == null) return new List<ResultadoCambio>();

            // Ejecutar búsqueda actual
            var (productosActuales, _) = await _busquedaService.BuscarProductosAvanzadoAsync(parametros);

            var cambios = new List<ResultadoCambio>();

            // Lógica simplificada de detección de cambios
            // En una implementación real, compararías con resultados anteriores
            foreach (var producto in productosActuales.Take(5)) // Limitar para demo
            {
                // Simular detección de cambios (implementación real necesitaría historial)
                if (producto.EnOferta && busqueda.MonitorearBajasPrecio)
                {
                    cambios.Add(new ResultadoCambio
                    {
                        FiltroGuardadoId = busqueda.Id,
                        ProductoId = producto.Id,
                        TipoCambio = "baja_precio",
                        Descripcion = $"{producto.Nombre} ahora en oferta - {producto.Descuento}% de descuento",
                        PrecioNuevo = producto.Precio,
                        FechaDetectado = DateTime.UtcNow
                    });
                }
            }

            return cambios;
        }
        private async Task EnviarNotificacionAmazonStyle(FiltroGuardado busqueda, List<ResultadoCambio> cambios)
        {
            // Guardar cambios en base de datos
            _context.ResultadoCambios.AddRange(cambios);
            await _context.SaveChangesAsync();

            // Aquí implementarías:
            // - Envío de email
            // - Notificación push
            // - Notificación en la web
            var notificacion = new Notificacion
            {
                UsuarioId = busqueda.UsuarioId,
                Titulo = $"🎯 Novedades en tu búsqueda: '{busqueda.Nombre}'",
                Mensaje = $"Encontramos {cambios.Count} cambios que pueden interesarte: " +
                 string.Join(", ", cambios.Select(c => c.Descripcion)),
                Tipo = "alerta_busqueda",
                FiltroGuardadoId = busqueda.Id,
                EnviarEmail = true,
                MostrarEnWeb = true,
                FechaCreacion = DateTime.UtcNow
            };

            await _notificacionService.CrearNotificacionAsync(notificacion);

            _logger.LogInformation($"Notificación Amazon Style creada para {busqueda.UsuarioId}");
            _logger.LogInformation($"NOTIFICACIÓN AMAZON STYLE para {busqueda.UsuarioId}: {cambios.Count} cambios en '{busqueda.Nombre}'");

            foreach (var cambio in cambios)
            {
                _logger.LogInformation($" - {cambio.Descripcion}");
            }
        }
    }

}
