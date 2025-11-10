namespace Mascotas.Dto
{
    public class CategoriaSearchParams
    {
        public string? SearchTerm { get; set; }
        public bool? Activo { get; set; } = true;
        public bool? ConProductos { get; set; } // Solo categorías con productos
        public int? ProductosMin { get; set; } // Mínimo de productos
        public string? SortBy { get; set; } // "nombre", "orden", "productosCount", "fecha"
        public bool? SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50; // Más grande porque son categorías
    }
}
