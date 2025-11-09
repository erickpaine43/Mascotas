using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class UpdateReviewRequest
    {
        [Required]
        [Range(1, 5, ErrorMessage = "El rating debe estar entre 1 y 5")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "El comentario no puede exceder 500 caracteres")]
        public string? Comentario { get; set; }
    }
}
