using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class UpdateClienteDto
    {
        [StringLength(100)]
        public string? Nombre { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Ciudad { get; set; }

        [StringLength(10)]
        public string? CodigoPostal { get; set; }
    }
}
