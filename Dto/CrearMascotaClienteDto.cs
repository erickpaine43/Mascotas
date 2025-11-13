using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CrearMascotaClienteDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Especie { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }
        public string Sexo { get; set; } = "Macho";
        public bool Esterilizado { get; set; }
        public string? NotasMedicas { get; set; }
        public string? Alergias { get; set; }
    }
}
