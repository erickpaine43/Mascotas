using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class FiltroGuardado
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // JSON con todos los parámetros de búsqueda
        [Required]
        public string ParametrosBusqueda { get; set; } = string.Empty;

        public bool EsFavorito { get; set; } = false;
        public int VecesUtilizado { get; set; } = 0;

        [StringLength(50)]
        public string? CategoriaFiltro { get; set; } // "mascotas", "alimentos", "medicamentos"

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaUltimoUso { get; set; } = DateTime.UtcNow;
        public bool MonitorearNuevosProductos { get; set; } = true;
        public bool MonitorearBajasPrecio { get; set; } = true;
        public bool MonitorearStock { get; set; } = false;
        public decimal? PorcentajeBajaMinima { get; set; }
        public DateTime FechaUltimaRevision { get; set; } = DateTime.UtcNow;
        public int TotalNotificacionesEnviadas { get; set; } = 0;
    }
}
