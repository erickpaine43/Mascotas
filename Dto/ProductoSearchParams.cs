namespace Mascotas.Dto
{
    public class ProductoSearchParams
    {
        public string? SearchTerm { get; set; }
        public int? CategoriaId { get; set; }
        public string? Marca { get; set; }
        public bool? Destacado { get; set; }
        public bool? EnOferta { get; set; }
        public bool? Activo { get; set; } = true;

        // Filtros de precio
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }

        // Filtros de stock
        public bool? EnStock { get; set; }
        public int? StockMin { get; set; }

        // Filtros de rating
        public decimal? RatingMin { get; set; }

        // Filtros de descuento
        public decimal? DescuentoMin { get; set; }

        // Ordenamiento
        public string? SortBy { get; set; } // "precio", "nombre", "rating", "masVendidos", "novedades", "descuento"
        public bool? SortDescending { get; set; }

        // Paginación
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

    }
}
