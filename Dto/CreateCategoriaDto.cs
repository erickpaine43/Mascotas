using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CreateCategoriaDto
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public string ImagenUrl { get; set; } = string.Empty;
        public int Orden { get; set; } = 0;
    }
}
