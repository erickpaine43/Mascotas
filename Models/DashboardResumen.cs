namespace Mascotas.Models
{
    public class DashboardResumen
    {
        public decimal IngresosHoy { get; set; }
        public decimal IngresosMes { get; set; }
        public int OrdenesHoy { get; set; }
        public int OrdenesPendientes { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosBajoStock { get; set; }
        public int TotalAnimales { get; set; }
        public int AnimalesDisponibles { get; set; }
        public int TotalClientes { get; set; }
        public int ClientesNuevosMes { get; set; }
        public string EspecieMasPopular { get; set; } = string.Empty;
    }
}
