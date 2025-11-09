using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailVerificacionAsync(string email, string nombre, string codigoVerificacion);
        Task<bool> EnviarEmailResetPasswordAsync(string email, string nombre, string codigoReset);
        Task<bool> EnviarRecordatorioResenaAsync(string email, string nombre, string asunto, string mensaje);
    }
}
