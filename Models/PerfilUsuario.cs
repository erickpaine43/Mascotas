using OfficeOpenXml.Drawing.Chart;
using System.ComponentModel.DataAnnotations;

namespace Mascotas.Models
{
    public class PerfilUsuario
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(200)]
        public string? FotoUrl { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
        public virtual ICollection<MascotaCliente> Mascotas { get; set; } = new List<MascotaCliente>();
        public virtual PreferenciasUsuario? Preferencias { get; set; }
    }
}

