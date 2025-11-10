using Mascotas.Models;

namespace Mascotas.Dto
{
    public class AddTrackingEventRequest
    {
        public OrdenEstado Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
