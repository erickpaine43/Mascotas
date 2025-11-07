using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class UpdateProductoDto
    {
        [StringLength(100)]
        public string? Nombre { get; set; }

        public int? CategoriaId { get; set; }
        public string? Descripcion { get; set; }
        public string? DescripcionCorta { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Precio { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? PrecioOriginal { get; set; }

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        [Range(0, 100)]
        public decimal? Descuento { get; set; }

        public string? ImagenUrl { get; set; }
        public List<string>? ImagenesAdicionales { get; set; }
        public bool? Activo { get; set; }
        public bool? Destacado { get; set; }
        public bool? EnOferta { get; set; }
        public string? SKU { get; set; }
        public string? Marca { get; set; }
    }
}
