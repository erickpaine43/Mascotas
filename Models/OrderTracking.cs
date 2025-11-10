using Stripe.Climate;

namespace Mascotas.Models
{
    public class OrderTracking
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public OrdenEstado Status { get; set; }
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;

        // Navigation property
        public Orden Orden { get; set; } = null!;
    }
}
