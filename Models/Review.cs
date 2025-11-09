using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mascotas.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        [Required]
        [ForeignKey("Producto")]
        public int? ProductoId { get; set; }
        [Required]
        [ForeignKey("Cliente")]
        public int ClienteId { get; set; }
        [ForeignKey("Animal")]
        public int? AnimalId { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "El rating debe estar entre 1 y 5")]
        public int Rating { get; set; } // 1-5 estrellas

        [StringLength(500)]
        public string Comment { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; } = true;

        // Navigation property
        public virtual Producto? Producto { get; set; }
        public virtual Animal? Animal { get; set; }
        public virtual Cliente? Cliente { get; set; }
        public bool EsValida()
        {
            return (ProductoId.HasValue && !AnimalId.HasValue) ||
                   (!ProductoId.HasValue && AnimalId.HasValue);
        }
    }
}
