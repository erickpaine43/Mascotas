using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;

namespace Mascotas.Services
    {
        public class BusquedaAvanzadaService : IBusquedaAvanzadaService
        {
            private readonly MascotaDbContext _context;

            public BusquedaAvanzadaService(MascotaDbContext context)
            {
                _context = context;
            }

            public async Task<(List<Producto> Productos, int TotalCount)> BuscarProductosAvanzadoAsync(
                ProductoSearchParams searchParams, ProductoSearchAdvancedDto? advancedParams = null)
            {
                var query = _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo)
                    .AsQueryable();

                // Filtros básicos existentes
                query = AplicarFiltrosBasicos(query, searchParams);

                // Filtros avanzados
                if (advancedParams != null)
                {
                    query = AplicarFiltrosAvanzados(query, advancedParams);
                }

                // Ordenamiento avanzado
                query = AplicarOrdenamientoAvanzado(query, searchParams, advancedParams);

                var totalCount = await query.CountAsync();

                var productos = await query
                    .Skip((searchParams.Page - 1) * searchParams.PageSize)
                    .Take(searchParams.PageSize)
                    .ToListAsync();

                return (productos, totalCount);
            }

            private IQueryable<Producto> AplicarFiltrosBasicos(IQueryable<Producto> query, ProductoSearchParams searchParams)
            {
                // Tu lógica existente del controlador
                if (!string.IsNullOrEmpty(searchParams.SearchTerm))
                {
                    var searchTerm = searchParams.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.Nombre.ToLower().Contains(searchTerm) ||
                        p.Descripcion.ToLower().Contains(searchTerm) ||
                        p.DescripcionCorta.ToLower().Contains(searchTerm) ||
                        (p.Marca != null && p.Marca.ToLower().Contains(searchTerm)) ||
                        (p.SKU != null && p.SKU.ToLower().Contains(searchTerm))
                    );
                }

                if (searchParams.CategoriaId.HasValue)
                    query = query.Where(p => p.CategoriaId == searchParams.CategoriaId.Value);

                if (searchParams.PrecioMin.HasValue)
                    query = query.Where(p => p.Precio >= searchParams.PrecioMin.Value);

                if (searchParams.PrecioMax.HasValue)
                    query = query.Where(p => p.Precio <= searchParams.PrecioMax.Value);

                // ... otros filtros básicos existentes

                return query;
            }

            private IQueryable<Producto> AplicarFiltrosAvanzados(IQueryable<Producto> query, ProductoSearchAdvancedDto advancedParams)
            {
                // Filtros de disponibilidad y envío
                if (advancedParams.EntregaRapida.HasValue)
                    query = query.Where(p => p.EntregaRapida == advancedParams.EntregaRapida.Value);

                if (advancedParams.RetiroEnTienda.HasValue)
                    query = query.Where(p => p.RetiroEnTienda == advancedParams.RetiroEnTienda.Value);

                if (advancedParams.EnvioGratis.HasValue)
                    query = query.Where(p => p.EnvioGratis == advancedParams.EnvioGratis.Value);

                if (advancedParams.Preorden.HasValue)
                    query = query.Where(p => p.Preorden == advancedParams.Preorden.Value);

                // Filtros de especificaciones técnicas
                if (!string.IsNullOrEmpty(advancedParams.EspecieDestinada))
                    query = query.Where(p => p.EspecieDestinada == advancedParams.EspecieDestinada);

                if (!string.IsNullOrEmpty(advancedParams.EtapaVida))
                    query = query.Where(p => p.EtapaVida == advancedParams.EtapaVida);

                if (!string.IsNullOrEmpty(advancedParams.NecesidadesEspeciales))
                    query = query.Where(p => p.NecesidadesEspeciales == advancedParams.NecesidadesEspeciales);

                if (!string.IsNullOrEmpty(advancedParams.Material))
                    query = query.Where(p => p.Material == advancedParams.Material);

                if (!string.IsNullOrEmpty(advancedParams.TipoTratamiento))
                    query = query.Where(p => p.TipoTratamiento == advancedParams.TipoTratamiento);

                return query;
            }

        private IQueryable<Producto> AplicarOrdenamientoAvanzado(IQueryable<Producto> query,
ProductoSearchParams searchParams, ProductoSearchAdvancedDto? advancedParams)
        {
            // Si hay parámetros avanzados y se solicita orden por relevancia
            if (advancedParams != null &&
                string.Equals(advancedParams.SortBy, "relevancia", StringComparison.OrdinalIgnoreCase))
            {
                return query.OrderByDescending(p =>
                    (p.Rating * 0.3m) + // Usar 0.3m (decimal) en lugar de 0.3 (double)
                    (p.StockVendido * 0.2m) +
                    (p.Destacado ? 0.3m : 0m) +
                    (p.EnOferta ? 0.2m : 0m)
                );
            }

            // Ordenamiento normal - manejar SortDescending nullable
            bool sortDescending = searchParams.SortDescending ?? true; // Valor por defecto true

            return searchParams.SortBy?.ToLower() switch
            {
                "precio" => sortDescending ?
                    query.OrderByDescending(p => p.Precio) : query.OrderBy(p => p.Precio),

                "rating" => sortDescending ?
                    query.OrderByDescending(p => p.Rating) : query.OrderBy(p => p.Rating),

                "mas_vendidos" => query.OrderByDescending(p => p.StockVendido),

                "novedades" => query.OrderByDescending(p => p.FechaCreacion),

                "descuento" => query.OrderByDescending(p => p.Descuento),

                _ => query.OrderByDescending(p => p.Destacado)
                         .ThenByDescending(p => p.FechaCreacion)
            };
        }

        public async Task<List<string>> ObtenerSugerenciasBusquedaAsync(string termino)
            {
                if (string.IsNullOrEmpty(termino) || termino.Length < 2)
                    return new List<string>();

                return await _context.Productos
                    .Where(p => p.Activo &&
                        (p.Nombre.Contains(termino) ||
                        (p.Marca != null && p.Marca.Contains(termino)) ||
                        (p.EspecieDestinada != null && p.EspecieDestinada.Contains(termino)) ||
                        (p.EtapaVida != null && p.EtapaVida.Contains(termino))))
                    .Select(p => p.Nombre)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();
            }

            public async Task<List<Producto>> BusquedaContextualAsync(string query)
            {
                // Tu lógica existente de búsqueda contextual
                // (puedes moverla aquí desde el controlador)
                throw new NotImplementedException("Mover la lógica de BusquedaContextual aquí");
            }
        }
    }

