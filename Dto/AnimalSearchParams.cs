namespace Mascotas.Dto
{
    public class AnimalSearchParams
    {
        public string? SearchTerm { get; set; }
        public string? Especie { get; set; }
        public string? Raza { get; set; }
        public string? Sexo { get; set; }
        public bool? Disponible { get; set; }
        public bool? Vacunado { get; set; }
        public bool? Esterilizado { get; set; }
        public bool? Reservado { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public int? EdadMin { get; set; } // En meses
        public int? EdadMax { get; set; } // En meses
        public DateTime? FechaNacimientoDesde { get; set; }
        public DateTime? FechaNacimientoHasta { get; set; }
        public string? SortBy { get; set; } // "precio", "edad", "nombre", "fecha"
        public bool? SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
