namespace Mascotas.Models
{
    public class ReporteVentas
    {
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public int ProductosVendidos { get; set; }
        public int AnimalesVendidos { get; set; }
    }
}
