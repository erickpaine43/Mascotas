using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CreateProductoDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int CategoriaId { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public string DescripcionCorta { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PrecioOriginal { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Range(0, 100)]
        public decimal Descuento { get; set; }

        public string ImagenUrl { get; set; } = string.Empty;
        public List<string>? ImagenesAdicionales { get; set; }
        public bool Destacado { get; set; }
        public bool EnOferta { get; set; }
        public string? SKU { get; set; }
        public string? Marca { get; set; }
    }
}
