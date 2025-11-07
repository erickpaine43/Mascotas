using Mascotas.Models;
using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CambiarRolUsuarioDto
    {
        [Required(ErrorMessage = "El rol es requerido")]
        public UsuarioRol Rol { get; set; }
    }
}
