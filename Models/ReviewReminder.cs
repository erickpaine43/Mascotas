using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
    {
        public class ReviewReminder
        {
            public int Id { get; set; }

            [Required]
            public int OrdenId { get; set; }

            [Required]
            public int ClienteId { get; set; }

            public int? ProductoId { get; set; }
            public int? AnimalId { get; set; }

            [Required]
            public string TipoItem { get; set; } = string.Empty; // "Producto", "Animal"

            [Required]
            public string NombreItem { get; set; } = string.Empty;

            public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
            public DateTime? PrimerRecordatorioEnviado { get; set; }
            public DateTime? SegundoRecordatorioEnviado { get; set; }
            public bool ResenaCompletada { get; set; } = false;

            // Navigation properties
            public virtual Orden Orden { get; set; } = null!;
            public virtual Cliente Cliente { get; set; } = null!;
            public virtual Producto? Producto { get; set; }
            public virtual Animal? Animal { get; set; }
        }
    }


