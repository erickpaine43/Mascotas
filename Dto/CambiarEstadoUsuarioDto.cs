using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CambiarEstadoUsuarioDto
    {
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Activo { get; set; }
    }
}
