namespace Mascotas.Dto
{
    public class CarritoItemDto
    {
        public int Id { get; set; }
        public int? ProductoId { get; set; }
        public int? MascotaId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;

        public ProductoDto? Producto { get; set; }
        public AnimalDto? Mascota { get; set; }
    }
}
