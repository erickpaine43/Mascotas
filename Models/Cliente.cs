using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [StringLength(10)]
        public string CodigoPostal { get; set; } = string.Empty;

        public string StripeCustomerId { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
        public int? UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}
