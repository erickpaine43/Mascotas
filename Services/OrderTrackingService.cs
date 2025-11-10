using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Mascotas.Services
{
    public class OrderTrackingService : IOrderTrackingService
    {
        private readonly MascotaDbContext _context;
        private readonly IOrdenNotificacionService _ordenNotificacionService;

        public OrderTrackingService(MascotaDbContext context, IOrdenNotificacionService ordenNotificacionService)
        {
            _context = context;
            _ordenNotificacionService = ordenNotificacionService;
        }

        public async Task<OrderTrackingDto?> GetOrderTrackingAsync(string trackingNumber)
        {
            var orden = await _context.Ordenes
                .Include(o => o.TrackingHistory)
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.TrackingNumber == trackingNumber);

            if (orden == null) return null;

            return new OrderTrackingDto
            {
                NumeroOrden = orden.NumeroOrden,
                TrackingNumber = orden.TrackingNumber,
                CurrentStatus = orden.Estado,
                LastUpdate = orden.TrackingHistory.Max(th => th.UpdateDate),
                Total = orden.Total,
                ClienteNombre = orden.Cliente.Nombre,
                History = orden.TrackingHistory
                    .OrderBy(th => th.UpdateDate)
                    .Select(th => new TrackingEventDto
                    {
                        Status = th.Status,
                        Date = th.UpdateDate,
                        Description = th.Description,
                        Location = th.Location
                    }).ToList()
            };
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrdenEstado newStatus, string description = null)
        {
            var orden = await _context.Ordenes
                .Include(o => o.TrackingHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orden == null) throw new ArgumentException("Orden no encontrada");

            var estadoAnterior = orden.Estado;
            orden.Estado = newStatus;

            // Actualizar fechas según el estado
            switch (newStatus)
            {
                case OrdenEstado.Confirmada:
                    orden.FechaConfirmacion = DateTime.UtcNow;
                    break;
                case OrdenEstado.Enviada:
                    orden.FechaEnvio = DateTime.UtcNow;
                    break;
                case OrdenEstado.Entregada:
                    orden.FechaEntrega = DateTime.UtcNow;
                    break;
            }

            // Agregar evento de tracking
            var trackingEvent = new OrderTracking
            {
                OrdenId = orderId,
                Status = newStatus,
                UpdateDate = DateTime.UtcNow,
                Description = description ?? GetDefaultDescription(newStatus),
                Location = GetDefaultLocation(newStatus),
                TrackingNumber = orden.TrackingNumber
            };

            orden.TrackingHistory.Add(trackingEvent);
            await _context.SaveChangesAsync();

            // ✅ ENVIAR NOTIFICACIÓN ASINCRONA
            _ = Task.Run(async () =>
            {
                try
                {
                    await _ordenNotificacionService.EnviarNotificacionEstadoAsync(orden, newStatus, description);
                }
                catch (Exception ex)
                {
                    // Log error pero no fallar la actualización del estado
                    var logger = _context.GetService<ILogger<OrderTrackingService>>();
                    logger.LogError(ex, $"Error enviando notificación para orden {orden.Id}");
                }
            });
        }

        public async Task<string> GenerateTrackingNumberAsync(int orderId)
        {
            var orden = await _context.Ordenes.FindAsync(orderId);
            if (orden == null) throw new ArgumentException("Orden no encontrada");

            // Generar número de tracking: TRK-{YYYYMMDD}-{ORDERID}
            var trackingNumber = $"TRK-{DateTime.UtcNow:yyyyMMdd}-{orderId:D6}";

            orden.TrackingNumber = trackingNumber;
            await _context.SaveChangesAsync();

            return trackingNumber;
        }

        public async Task<List<OrderTrackingDto>> GetUserOrderHistoryAsync(int clienteId)
        {
            var ordenes = await _context.Ordenes
                .Where(o => o.ClienteId == clienteId)
                .Include(o => o.TrackingHistory)
                .Include(o => o.Cliente)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();

            return ordenes.Select(orden => new OrderTrackingDto
            {
                NumeroOrden = orden.NumeroOrden,
                TrackingNumber = orden.TrackingNumber,
                CurrentStatus = orden.Estado,
                LastUpdate = orden.TrackingHistory.Any()
                    ? orden.TrackingHistory.Max(th => th.UpdateDate)
                    : orden.FechaCreacion,
                Total = orden.Total,
                ClienteNombre = orden.Cliente.Nombre,
                History = orden.TrackingHistory
                    .OrderBy(th => th.UpdateDate)
                    .Select(th => new TrackingEventDto
                    {
                        Status = th.Status,
                        Date = th.UpdateDate,
                        Description = th.Description,
                        Location = th.Location
                    }).ToList()
            }).ToList();
        }

        public async Task<bool> AddTrackingEventAsync(int orderId, OrdenEstado status, string description, string location = "")
        {
            var orden = await _context.Ordenes
                .Include(o => o.TrackingHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orden == null) return false;

            var trackingEvent = new OrderTracking
            {
                OrdenId = orderId,
                Status = status,
                UpdateDate = DateTime.UtcNow,
                Description = description,
                Location = location,
                TrackingNumber = orden.TrackingNumber
            };

            orden.TrackingHistory.Add(trackingEvent);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GetDefaultDescription(OrdenEstado status)
        {
            return status switch
            {
                OrdenEstado.Pendiente => "Orden creada y pendiente de pago",
                OrdenEstado.Confirmada => "Pago confirmado, preparando pedido",
                OrdenEstado.EnProceso => "Pedido en proceso de preparación",
                OrdenEstado.Enviada => "Pedido enviado al cliente",
                OrdenEstado.Entregada => "Pedido entregado satisfactoriamente",
                OrdenEstado.Cancelada => "Orden cancelada",
                OrdenEstado.Completada => "Orden completada",
                _ => "Actualización de estado"
            };
        }

        private string GetDefaultLocation(OrdenEstado status)
        {
            return status switch
            {
                OrdenEstado.Pendiente => "Tienda Online",
                OrdenEstado.Confirmada => "Centro de Distribución",
                OrdenEstado.EnProceso => "Centro de Distribución",
                OrdenEstado.Enviada => "En tránsito",
                OrdenEstado.Entregada => "Ubicación del Cliente",
                _ => "Tienda de Mascotas"
            };
        }

        public async Task<bool> EnviarNotificacionEstadoAsync(int ordenId, OrdenEstado nuevoEstado, string descripcionPersonalizada = null)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null) return false;

            switch (nuevoEstado)
            {
                case OrdenEstado.Confirmada:
                    return await _ordenNotificacionService.EnviarNotificacionConfirmacionAsync(orden);

                case OrdenEstado.Enviada:
                    return await _ordenNotificacionService.EnviarNotificacionEnvioAsync(orden, descripcionPersonalizada ?? "En camino");

                case OrdenEstado.Entregada:
                    return await _ordenNotificacionService.EnviarNotificacionEntregaAsync(orden);

                default:
                    return true; // No enviar notificación para otros estados
            }
        }
    }
}