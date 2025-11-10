using Mascotas.Models;
namespace Mascotas.Dto
{
    public class OrderTrackingDto
    {
        public string NumeroOrden { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public OrdenEstado CurrentStatus { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<TrackingEventDto> History { get; set; } = new();
        public decimal Total { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
    }
}
