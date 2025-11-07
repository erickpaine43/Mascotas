using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string DescripcionCorta { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal PrecioOriginal { get; set; }
        public int Stock { get; set; }
        public decimal Descuento { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
        public List<string> ImagenesAdicionales { get; set; } = new();
        public bool Activo { get; set; }
        public bool Destacado { get; set; }
        public bool EnOferta { get; set; }
        public decimal Rating { get; set; }
        public int TotalValoraciones { get; set; }
        public string? SKU { get; set; }
        public string? Marca { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int  StockTotal { get; set; }
        public int StockDisponible { get; set; }
        public int StockReservado { get; set; }
        public int StockVendido { get; set; }
    }
}
