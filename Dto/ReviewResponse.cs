namespace Mascotas.Dto
{
    public class ReviewResponse
    {
        public int ReviewId { get; set; }
        public int ProductoId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string? ProductoNombre { get; set; }
    }
}
