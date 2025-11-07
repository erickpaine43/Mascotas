using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public string ImagenUrl { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // Navigation property
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
