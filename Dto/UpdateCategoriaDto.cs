using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class UpdateCategoriaDto
    {
        [StringLength(50)]
        public string? Nombre { get; set; }

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public string? ImagenUrl { get; set; }
        public int? Orden { get; set; }
        public bool? Activo { get; set; }
    }
}
