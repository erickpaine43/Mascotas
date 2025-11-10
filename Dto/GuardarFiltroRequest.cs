namespace Mascotas.Dto
{
    public class GuardarFiltroRequest
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public Dictionary<string, object> Parametros { get; set; } = new();
    }
}
