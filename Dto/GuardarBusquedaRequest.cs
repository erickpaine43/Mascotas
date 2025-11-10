using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class GuardarBusquedaRequest
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public Dictionary<string, object> Parametros { get; set; } = new();

        public bool MonitorearNuevos { get; set; } = true;
        public bool MonitorearPrecios { get; set; } = true;
        public bool MonitorearStock { get; set; } = false; // ✅ Agregado
        public decimal? PorcentajeMinimo { get; set; }
    }
}
