using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class PasswordResetRequestDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;
    }
}
