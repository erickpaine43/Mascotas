namespace Mascotas.Dto
{
    public class OrdenDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public string? Comentarios { get; set; }
        public bool ReservaActiva { get; set; }
        public DateTime FechaExpiracionReserva { get; set; }
        public DateTime FechaCreacion { get; set; }
        public ClienteDto Cliente { get; set; } = new ClienteDto();
        public List<OrdenItemDto> Items { get; set; } = new();
        public decimal CostoEnvio { get; set; }
        public int MetodoEnvioId { get; set; }
        public string? MetodoEnvioNombre { get; set; }
        public int DireccionEnvioId { get; set; }
        public string? DireccionCompleta { get; set; }
        public int DiasEntregaEstimados { get; set; }
        public DateTime? FechaEstimadaEntrega { get; set; }
    }
}
