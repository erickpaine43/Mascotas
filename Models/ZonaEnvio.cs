namespace Mascotas.Models
{
    public class ZonaEnvio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string? RangoCodigosPostales { get; set; }
        public decimal TarifaBaseEstandar { get; set; } = 800;
        public decimal TarifaBaseExpress { get; set; } = 1200;
        public decimal CostoPorKiloExtra { get; set; } = 150;
        public decimal RecargoFragil { get; set; } = 200;
        public int DiasEstandar { get; set; } = 5;
        public int DiasExpress { get; set; } = 1;
        public decimal MontoMinimoEnvioGratis { get; set; } = 15000;
        public bool Activo { get; set; } = true;
    }
}
