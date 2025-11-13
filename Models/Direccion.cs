using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Direccion
    {
        public int Id { get; set; }

        [Required]
        public int PerfilUsuarioId { get; set; }

        [Required]
        [StringLength(200)]
        public string Calle { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Departamento { get; set; }

        [Required]
        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Provincia { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CodigoPostal { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Pais { get; set; } = "Argentina";

        public bool EsPrincipal { get; set; } = false;

        [StringLength(50)]
        public string Tipo { get; set; } = "Envío"; // Envío, Facturación

        [StringLength(100)]
        public string? Alias { get; set; } // "Casa", "Trabajo", etc.

        // Navigation property
        public virtual PerfilUsuario PerfilUsuario { get; set; } = null!;
    }
}
