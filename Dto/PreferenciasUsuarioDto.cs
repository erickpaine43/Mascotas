namespace Mascotas.Dto
{
    public class PreferenciasUsuarioDto
    {
        public bool RecibirNotificacionesEmail { get; set; } = true;
        public bool RecibirNotificacionesSMS { get; set; } = false;
        public bool RecibirOfertasEspeciales { get; set; } = true;
        public string? CategoriaFavorita { get; set; }
        public string Idioma { get; set; } = "es";
    }
}
