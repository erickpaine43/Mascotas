using Mascotas.Models;

namespace Mascotas.Services
{
    public interface ICalculoEnvioService
    {
        Task<CalculoEnvioResult> CalcularEnvioAsync(int direccionId, List<ItemCalculoEnvio> items, int metodoEnvioId);
        Task<List<MetodoEnvio>> ObtenerMetodosDisponiblesAsync(int direccionId, decimal pesoTotal);
    }
}
