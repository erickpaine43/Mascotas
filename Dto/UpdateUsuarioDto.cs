using Mascotas.Models;
using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class UpdateUsuarioDto
    {
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string? Nombre { get; set; }

        public UsuarioRol? Rol { get; set; }

        public bool? Activo { get; set; }

        public bool? EmailVerificado { get; set; }
    }
}
