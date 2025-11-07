using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class UpdateAnimalDto
    {
        [StringLength(100)]
        public string? Nombre { get; set; }

        [StringLength(50)]
        public string? Especie { get; set; }

        public string? Raza { get; set; }
        public int? Edad { get; set; }
        public string? Sexo { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Precio { get; set; }

        public string? Descripcion { get; set; }
        public bool? Disponible { get; set; }
        public bool? Vacunado { get; set; }
        public bool? Esterilizado { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
