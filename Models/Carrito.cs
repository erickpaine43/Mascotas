namespace Mascotas.Models
{
    public class Carrito
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
        public List<CarritoItem> Items { get; set; } = new();
    }
}
