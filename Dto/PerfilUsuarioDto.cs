namespace Mascotas.Dto
{
    public class PerfilUsuarioDto
    {
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? FotoUrl { get; set; }
        public List<DireccionDto> Direcciones { get; set; } = new();
        public List<MascotaClienteDto> Mascotas { get; set; } = new();
        public PreferenciasUsuarioDto Preferencias { get; set; } = new();
    }
}
