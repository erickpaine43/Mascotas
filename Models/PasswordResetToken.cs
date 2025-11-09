using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; }
        public bool Utilizado { get; set; } = false;
    }
}
