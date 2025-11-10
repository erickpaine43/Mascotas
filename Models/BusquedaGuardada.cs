namespace Mascotas.Models
{
    public class BusquedaGuardada
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ParametrosBusqueda { get; set; } = string.Empty; // JSON
        public int VecesUtilizada { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimoUso { get; set; }
    }
}
