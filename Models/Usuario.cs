using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public UsuarioRol Rol { get; set; } = UsuarioRol.Cliente;

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoLogin { get; set; }

        public bool EmailVerificado { get; set; } = false;
        public string? CodigoVerificacion { get; set; }
        public DateTime? ExpiracionCodigoVerificacion { get; set; } = DateTime.UtcNow;
    }
       
}
