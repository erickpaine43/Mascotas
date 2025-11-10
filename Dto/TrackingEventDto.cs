using Mascotas.Models;

namespace Mascotas.Dto
{
    public class TrackingEventDto
    {
        public OrdenEstado Status { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    
}
}
