namespace Mascotas.Dto
{
    public class CalculoEnvioRequest
    {
        public int DireccionId { get; set; }
        public int MetodoEnvioId { get; set; }
        public List<ItemEnvioRequest> Items { get; set; } = new();
    }
}
