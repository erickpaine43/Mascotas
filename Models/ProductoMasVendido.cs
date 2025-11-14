namespace Mascotas.Models
{
    public class ProductoMasVendido
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
        public string Categoria { get; set; } = string.Empty;
    }
}
