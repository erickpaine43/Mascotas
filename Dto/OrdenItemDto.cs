namespace Mascotas.Dto
{
    public class OrdenItemDto
    {
        public int Id { get; set; }
        public int? AnimalId { get; set; }
        public int? ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public AnimalDto? Animal { get; set; }
        public ProductoDto? Producto { get; set; }
    }
}
