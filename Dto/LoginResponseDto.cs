namespace Mascotas.Dto
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UsuarioDto Usuario { get; set; } = new UsuarioDto();
        public DateTime Expiracion { get; set; }
    }
}
