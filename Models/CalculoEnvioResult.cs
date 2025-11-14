namespace Mascotas.Models
{
    public class CalculoEnvioResult
    {
        public decimal CostoEnvio { get; set; }
        public int DiasEntrega { get; set; }
        public DateTime FechaEstimada { get; set; }
        public string MetodoEnvio { get; set; } = string.Empty;
        public bool EnvioGratis { get; set; }
        public string? Mensaje { get; set; }
    }
}
