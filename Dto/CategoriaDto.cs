namespace Mascotas.Dto
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int Orden { get; set; }
        public int TotalProductos { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
