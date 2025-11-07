using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Relación con Categoría
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(500)]
        public string DescripcionCorta { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PrecioOriginal { get; set; }
        public int StockTotal { get; set; }           
        public int StockDisponible { get; set; }      
        public int StockReservado { get; set; }      
        public int StockVendido { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Range(0, 100)]
        public decimal Descuento { get; set; } = 0;

        public string ImagenUrl { get; set; } = string.Empty;
        public string? ImagenesAdicionales { get; set; } // JSON array de URLs

        public bool Activo { get; set; } = true;
        public bool Destacado { get; set; } = false;
        public bool EnOferta { get; set; } = false;

        [Range(0, 5)]
        public decimal Rating { get; set; } = 0;
        public int TotalValoraciones { get; set; } = 0;

        public string? SKU { get; set; }
        public string? Marca { get; set; }
        public DateTime? ExpiracionReserva { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
    }
}
