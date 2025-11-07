using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CambiarPasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string PasswordActual { get; set; } = string.Empty;

        // Hacer opcional: Nueva contraseña
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        public string? NuevaPassword { get; set; }

        // Hacer opcional: Confirmación (solo requerida si hay nueva contraseña)
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmarPassword { get; set; }

        // Agregar: Nuevo email (opcional)
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? NuevoEmail { get; set; }
    }
}
