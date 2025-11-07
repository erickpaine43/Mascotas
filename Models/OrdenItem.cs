namespace Mascotas.Models
{
    public class OrdenItem
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public int? AnimalId { get; set; }
        public int? ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Navigation properties
        public Orden Orden { get; set; } = null!;
        public Animal? Animal { get; set; }
        public Producto? Producto { get; set; }
    }
}
