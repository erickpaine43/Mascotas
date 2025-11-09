namespace Mascotas.Dto
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public List<CarritoItemDto> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int TotalItems => Items.Sum(i => i.Cantidad);
    }
}
