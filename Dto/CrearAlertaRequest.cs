namespace Mascotas.Dto
{
    public class CrearAlertaRequest
    {
        public string UsuarioId { get; set; } = string.Empty;
        public int ProductoId { get; set; }
        public decimal PrecioObjetivo { get; set; }
    }
}
