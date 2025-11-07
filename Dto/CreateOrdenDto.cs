using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class CreateOrdenDto
    {
        [Required]
        public int ClienteId { get; set; }

        public string? Comentarios { get; set; }

        [Required]
        public List<CreateOrdenItemDto> Items { get; set; } = new();
    }
}
