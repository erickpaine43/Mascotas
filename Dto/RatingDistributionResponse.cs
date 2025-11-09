namespace Mascotas.Dto
{
    public class RatingDistributionResponse
    {
        public List<RatingCount> Distribution { get; set; } = new();
        public int TotalReviews { get; set; }
    }
}
