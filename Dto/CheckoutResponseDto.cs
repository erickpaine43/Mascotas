namespace Mascotas.Dto
{
    public class CheckoutResponseDto
    {
        public string SessionUrl { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public OrdenDto Orden { get; set; } = new OrdenDto();
    }
}
