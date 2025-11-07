using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Animal
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty; // Perro, Gato, Ave, etc.

        [StringLength(50)]
        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; } // En meses
        public string Sexo { get; set; } = "Macho"; // Macho, Hembra

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        public bool Disponible { get; set; } = true;
        public bool Vacunado { get; set; } = true;
        public bool Reservado { get; set; } = false;
        public DateTime? ExpiracionReserva { get; set; }
        public bool Esterilizado { get; set; } = false;
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
    }
}
