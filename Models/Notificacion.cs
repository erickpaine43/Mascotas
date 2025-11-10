using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Notificacion
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Mensaje { get; set; } = string.Empty;

        [StringLength(50)]
        public string Tipo { get; set; } = "info"; // "info", "alerta_precio", "nuevo_producto", "stock"

        public int? ProductoId { get; set; }
        public int? FiltroGuardadoId { get; set; }

        public bool Leida { get; set; } = false;
        public bool Enviada { get; set; } = false;

        // Métodos de entrega
        public bool EnviarEmail { get; set; } = false;
        public bool EnviarPush { get; set; } = false;
        public bool MostrarEnWeb { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaEnvio { get; set; }
        public DateTime? FechaLectura { get; set; }

        // Navigation properties
        public Producto? Producto { get; set; }
        public FiltroGuardado? FiltroGuardado { get; set; }
    }
}
