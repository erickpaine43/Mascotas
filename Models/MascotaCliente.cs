using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class MascotaCliente
    {
        public int Id { get; set; }

        [Required]
        public int PerfilUsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Raza { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public decimal? Peso { get; set; } // en kg

        [StringLength(10)]
        public string Sexo { get; set; } = "Macho";

        public bool Esterilizado { get; set; } = false;

        [StringLength(500)]
        public string? NotasMedicas { get; set; }

        [StringLength(500)]
        public string? Alergias { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual PerfilUsuario PerfilUsuario { get; set; } = null!;
    }
}
