using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IJwtServices
    {
        string GenerateToken(Usuario usuario);
    }
}
