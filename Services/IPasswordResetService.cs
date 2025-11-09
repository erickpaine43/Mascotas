namespace Mascotas.Services
{
    public interface IPasswordResetService
    {
        Task<bool> SolicitarResetPasswordAsync(string email);
        Task<bool> ResetearPasswordConCodigoAsync(string email, string codigo, string nuevaPassword);
    }
}
