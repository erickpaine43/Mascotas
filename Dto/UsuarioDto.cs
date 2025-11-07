using Mascotas.Models;

namespace Mascotas.Dto
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public UsuarioRol Rol { get; set; }
        public string RolNombre => Rol.ToString();
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoLogin { get; set; }
    }
}
