using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CreateOrdenItemDto
    {
        public int? AnimalId { get; set; }
        public int? ProductoId { get; set; }

        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; } = 1;
    }
}
