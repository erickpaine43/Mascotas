namespace Mascotas.Dto
{
    public class DireccionDto
    {
        public int Id { get; set; }
        public string Calle { get; set; } = string.Empty;
        public string? Departamento { get; set; }
        public string Ciudad { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Pais { get; set; } = "Argentina";
        public bool EsPrincipal { get; set; }
        public string Tipo { get; set; } = "Envío";
        public string? Alias { get; set; }
    }
}
