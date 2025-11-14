namespace Mascotas.Services
{
    public class ItemCalculoEnvio
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Peso { get; set; }
        public bool EsFragil { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
