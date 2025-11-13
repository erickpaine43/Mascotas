using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class PreferenciasUsuario
    {
        public int Id { get; set; }

        [Required]
        public int PerfilUsuarioId { get; set; }

        public bool RecibirNotificacionesEmail { get; set; } = true;
        public bool RecibirNotificacionesSMS { get; set; } = false;
        public bool RecibirOfertasEspeciales { get; set; } = true;

        [StringLength(50)]
        public string? CategoriaFavorita { get; set; }

        [StringLength(10)]
        public string Idioma { get; set; } = "es";

        // Navigation property
        public virtual PerfilUsuario PerfilUsuario { get; set; } = null!;
    }
}
