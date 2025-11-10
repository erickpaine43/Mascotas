using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IOrderTrackingService
    {
        Task<OrderTrackingDto?> GetOrderTrackingAsync(string trackingNumber);
        Task UpdateOrderStatusAsync(int orderId, OrdenEstado newStatus, string description = null);
        Task<string> GenerateTrackingNumberAsync(int orderId);
        Task<List<OrderTrackingDto>> GetUserOrderHistoryAsync(int clienteId);
        Task<bool> AddTrackingEventAsync(int orderId, OrdenEstado status, string description, string location = "");
        Task<bool> EnviarNotificacionEstadoAsync(int ordenId, OrdenEstado nuevoEstado, string descripcionPersonalizada = null);
    }
}
