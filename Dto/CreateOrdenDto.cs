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
        public int DireccionEnvioId { get; set; }
        public int MetodoEnvioId { get; set; }
        public decimal CostoEnvio { get; set; }
        public int DiasEntregaEstimados { get; set; }
        public bool EsRetiroEnTienda { get; set; }
    }
}
