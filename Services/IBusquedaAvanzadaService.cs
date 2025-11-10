using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Services
{
    public interface IBusquedaAvanzadaService
    {
        Task<(List<Producto> Productos, int TotalCount)> BuscarProductosAvanzadoAsync(ProductoSearchParams searchParams, ProductoSearchAdvancedDto? advancedParams = null);
        Task<List<string>> ObtenerSugerenciasBusquedaAsync(string termino);
        Task<List<Producto>> BusquedaContextualAsync(string query);
    }
}
