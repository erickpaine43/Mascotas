namespace Mascotas.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailVerificacionAsync(string email, string nombre, string codigoVerificacion);
    }
}
