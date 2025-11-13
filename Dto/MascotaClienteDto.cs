namespace Mascotas.Dto
{
    public class MascotaClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }
        public string Sexo { get; set; } = "Macho";
        public bool Esterilizado { get; set; }
        public string? NotasMedicas { get; set; }
        public string? Alergias { get; set; }
    }
}
