using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class AlertaPrecio
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PrecioObjetivo { get; set; }

        public bool Activa { get; set; } = true;
        public bool Notificado { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActivacion { get; set; }

        // Navigation property
        public Producto Producto { get; set; } = null!;
    }
}
