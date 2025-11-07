using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CreateClienteDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [StringLength(10)]
        public string CodigoPostal { get; set; } = string.Empty;
    }
}
