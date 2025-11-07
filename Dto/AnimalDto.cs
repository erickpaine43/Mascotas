using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class AnimalDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        public string Raza { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Sexo { get; set; } = "Macho";

        [Range(0, double.MaxValue)]
        public decimal Precio { get; set; }
        public bool Reservado { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public bool Disponible { get; set; } = true;
        public bool Vacunado { get; set; } = true;
        public bool Esterilizado { get; set; } = false;
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
