namespace Mascotas.Models
{
    public class ResultadoCambio
    {
        public int Id { get; set; }
        public int FiltroGuardadoId { get; set; }
        public int ProductoId { get; set; }
        public string TipoCambio { get; set; } = string.Empty; // "nuevo_producto", "baja_precio", "nuevo_stock"
        public string Descripcion { get; set; } = string.Empty;
        public decimal? PrecioAnterior { get; set; }
        public decimal? PrecioNuevo { get; set; }
        public DateTime FechaDetectado { get; set; } = DateTime.UtcNow;

        public FiltroGuardado FiltroGuardado { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
    }
}
