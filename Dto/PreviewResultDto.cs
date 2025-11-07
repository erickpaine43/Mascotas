namespace Mascotas.Dto
{
    public class PreviewResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TipoDetectado { get; set; } = string.Empty;
        public List<string> Columnas { get; set; } = new List<string>();
        public List<List<string>> PreviewDatos { get; set; } = new List<List<string>>();
        public int TotalFilas { get; set; }
        public int TotalColumnas { get; set; }
    }
}
