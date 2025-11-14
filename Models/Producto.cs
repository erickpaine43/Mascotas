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
        public bool EntregaRapida { get; set; } = false;
        public bool RetiroEnTienda { get; set; } = true;
        public bool EnvioGratis { get; set; } = false;
        public bool Preorden { get; set; } = false;
        public int? DiasEntrega { get; set; } // 1, 2, 3, etc.
        public string? EspecieDestinada { get; set; } // Perro, Gato, etc.
        public string? RazaDestinada { get; set; } // Labrador, Siames, etc.
        public string? EtapaVida { get; set; } // Cachorro, Adulto, Senior
        public string? NecesidadesEspeciales { get; set; } // Esterilizado, Pelo Largo, etc.
        public string? Material { get; set; } // Plástico, Tela, Metal
        public string? Color { get; set; }
        public string? Dimensiones { get; set; } // "10x5x15 cm"
        public string? TipoTratamiento { get; set; } // Desparasitación, Vacuna, etc.
        public string? DuracionTratamiento { get; set; } // "30 días", "6 meses"
        public decimal Peso { get; set; } = 0.5m;
        public decimal Alto { get; set; } = 10;
        public decimal Ancho { get; set; } = 10;
        public decimal Largo { get; set; } = 10;
        public bool EsFragil { get; set; }
    }
}
