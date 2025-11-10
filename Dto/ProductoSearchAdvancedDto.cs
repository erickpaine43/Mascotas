namespace Mascotas.Dto
{
    public class ProductoSearchAdvancedDto
    {
        public bool? EntregaRapida { get; set; }
        public bool? RetiroEnTienda { get; set; }
        public bool? EnvioGratis { get; set; }
        public bool? Preorden { get; set; }

        // Filtros de especificaciones técnicas
        public string? EspecieDestinada { get; set; }
        public string? EtapaVida { get; set; }
        public string? NecesidadesEspeciales { get; set; }
        public string? Material { get; set; }
        public string? TipoTratamiento { get; set; }

        // Ordenamiento
        public string? SortBy { get; set; } // "relevancia", "precio", "rating", "mas_vendidos"
        public bool SortDescending { get; set; } = true;
    }
}
