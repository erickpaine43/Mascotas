namespace Mascotas.Models
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int? MascotaId { get; set; }
        public int? ProductoId { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal PrecioUnitario { get; set; }

        // Navigation properties
        public Carrito Carrito { get; set; } = null!;
        public Animal? Mascota { get; set; }
        public Producto? Producto { get; set; }
    }
}
