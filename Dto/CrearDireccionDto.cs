using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CrearDireccionDto
    {
        [Required]
        public string Calle { get; set; } = string.Empty;
        public string? Departamento { get; set; }

        [Required]
        public string Ciudad { get; set; } = string.Empty;

        [Required]
        public string Provincia { get; set; } = string.Empty;

        [Required]
        public string CodigoPostal { get; set; } = string.Empty;
        public string Pais { get; set; } = "Argentina";
        public bool EsPrincipal { get; set; }
        public string Tipo { get; set; } = "Envío";
        public string? Alias { get; set; }
    }
}
