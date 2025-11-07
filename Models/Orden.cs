namespace Mascotas.Models
{
    public class Orden
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty; // ORD-2024-001
        public int ClienteId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public OrdenEstado Estado { get; set; } = OrdenEstado.Pendiente;
        public MetodoPago MetodoPago { get; set; } = MetodoPago.Stripe;
        public string? StripePaymentIntentId { get; set; }
        public string? StripeSessionId { get; set; }
        public string? Comentarios { get; set; }
        public DateTime FechaExpiracionReserva { get; set; } = DateTime.UtcNow.AddMinutes(15);
        public bool ReservaActiva { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaConfirmacion { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public DateTime? FechaEntrega { get; set; }

        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
        public List<OrdenItem> Items { get; set; } = new();

    }
}
