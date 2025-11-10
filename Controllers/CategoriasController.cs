using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(MascotaDbContext context, IMapper mapper, ILogger<CategoriasController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias(
    [FromQuery] bool? conProductos = null,
    [FromQuery] bool? incluirInactivas = false)
        {
            var query = _context.Categorias
                .Include(c => c.Productos.Where(p => p.Activo))
                .AsQueryable();

            // Filtro por estado activo
            if (!incluirInactivas.HasValue || !incluirInactivas.Value)
                query = query.Where(c => c.Activo);

            // Filtro por categorías con productos
            if (conProductos.HasValue && conProductos.Value)
                query = query.Where(c => c.Productos.Any(p => p.Activo));

            var categorias = await query
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.Nombre)
                .ToListAsync();

            var categoriasDto = _mapper.Map<List<CategoriaDto>>(categorias);
            return Ok(categoriasDto);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoriaDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (categoria == null)
            {
                return NotFound();
            }

            var categoriaDto = _mapper.Map<CategoriaDto>(categoria);
            return categoriaDto;
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<CategoriaDto>> CreateCategoria(CreateCategoriaDto createCategoriaDto)
        {
            var categoria = _mapper.Map<Categoria>(createCategoriaDto);
            categoria.Activo = true;
            categoria.FechaCreacion = DateTime.UtcNow;

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var categoriaDto = _mapper.Map<CategoriaDto>(categoria);
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoriaDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> UpdateCategoria(int id, UpdateCategoriaDto updateCategoriaDto)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _mapper.Map(updateCategoriaDto, categoria);
            categoria.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            // Verificar si hay productos en esta categoría
            var tieneProductos = await _context.Productos.AnyAsync(p => p.CategoriaId == id && p.Activo);
            if (tieneProductos)
            {
                return BadRequest("No se puede eliminar la categoría porque tiene productos asociados");
            }

            // Soft delete
            categoria.Activo = false;
            categoria.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> SearchCategorias([FromQuery] CategoriaSearchParams searchParams)
        {
            try
            {
                var query = _context.Categorias
                    .Include(c => c.Productos.Where(p => p.Activo)) // Solo productos activos
                    .AsQueryable();

                // 🔍 Búsqueda por texto
                if (!string.IsNullOrEmpty(searchParams.SearchTerm))
                {
                    var searchTerm = searchParams.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.Nombre.ToLower().Contains(searchTerm) ||
                        c.Descripcion.ToLower().Contains(searchTerm)
                    );
                }

                // ✅ Filtro por estado activo
                if (searchParams.Activo.HasValue)
                    query = query.Where(c => c.Activo == searchParams.Activo.Value);

                // 📦 Filtro por categorías con productos
                if (searchParams.ConProductos.HasValue && searchParams.ConProductos.Value)
                    query = query.Where(c => c.Productos.Any(p => p.Activo));

                // 🔢 Filtro por mínimo de productos
                if (searchParams.ProductosMin.HasValue)
                    query = query.Where(c => c.Productos.Count(p => p.Activo) >= searchParams.ProductosMin.Value);

                // 📊 Ordenamiento avanzado
                query = searchParams.SortBy?.ToLower() switch
                {
                    "nombre" => searchParams.SortDescending == true ?
                        query.OrderByDescending(c => c.Nombre) : query.OrderBy(c => c.Nombre),

                    "orden" => searchParams.SortDescending == true ?
                        query.OrderByDescending(c => c.Orden) : query.OrderBy(c => c.Orden),

                    "productosCount" => searchParams.SortDescending == true ?
                        query.OrderByDescending(c => c.Productos.Count(p => p.Activo)) :
                        query.OrderBy(c => c.Productos.Count(p => p.Activo)),

                    "fecha" => searchParams.SortDescending == true ?
                        query.OrderByDescending(c => c.FechaCreacion) : query.OrderBy(c => c.FechaCreacion),

                    _ => query.OrderBy(c => c.Orden).ThenBy(c => c.Nombre) // Default
                };

                // 📄 Paginación
                var totalCount = await query.CountAsync();
                var categorias = await query
                    .Skip((searchParams.Page - 1) * searchParams.PageSize)
                    .Take(searchParams.PageSize)
                    .ToListAsync();

                var categoriasDto = _mapper.Map<List<CategoriaDto>>(categorias);

                // 📦 Respuesta con metadata
                var response = new
                {
                    Data = categoriasDto,
                    Pagination = new
                    {
                        Page = searchParams.Page,
                        PageSize = searchParams.PageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)searchParams.PageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar categorías");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("filters/options")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetCategoriasFilterOptions()
        {
            var categoriasConProductos = await _context.Categorias
                .Where(c => c.Activo && c.Productos.Any(p => p.Activo))
                .Select(c => new { c.Id, c.Nombre, ProductosCount = c.Productos.Count(p => p.Activo) })
                .ToListAsync();

            var totalCategorias = await _context.Categorias.CountAsync(c => c.Activo);
            var categoriasConProductosCount = categoriasConProductos.Count;

            return new
            {
                CategoriasConProductos = categoriasConProductos,
                Estadisticas = new
                {
                    TotalCategorias = totalCategorias,
                    CategoriasConProductos = categoriasConProductosCount,
                    CategoriasSinProductos = totalCategorias - categoriasConProductosCount
                }
            };
        }

        [HttpGet("populares")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategoriasPopulares([FromQuery] int top = 10)
        {
            var categoriasPopulares = await _context.Categorias
                .Include(c => c.Productos)
                .Where(c => c.Activo && c.Productos.Any(p => p.Activo))
                .Select(c => new
                {
                    Categoria = c,
                    ProductosCount = c.Productos.Count(p => p.Activo)
                })
                .OrderByDescending(x => x.ProductosCount)
                .ThenBy(x => x.Categoria.Nombre)
                .Take(top)
                .Select(x => x.Categoria)
                .ToListAsync();

            var categoriasDto = _mapper.Map<List<CategoriaDto>>(categoriasPopulares);
            return Ok(categoriasDto);
        }

        [HttpGet("con-productos")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategoriasConProductos()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Productos.Where(p => p.Activo))
                .Where(c => c.Activo && c.Productos.Any(p => p.Activo))
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.Nombre)
                .ToListAsync();

            var categoriasDto = _mapper.Map<List<CategoriaDto>>(categorias);
            return Ok(categoriasDto);
        }

        [HttpGet("{id}/estadisticas")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetEstadisticasCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (categoria == null)
                return NotFound();

            var productosActivos = categoria.Productos.Count(p => p.Activo);
            var productosDestacados = categoria.Productos.Count(p => p.Activo && p.Destacado);
            var productosEnOferta = categoria.Productos.Count(p => p.Activo && p.EnOferta);
            var stockTotal = categoria.Productos.Where(p => p.Activo).Sum(p => p.StockDisponible);

            return new
            {
                CategoriaId = categoria.Id,
                CategoriaNombre = categoria.Nombre,
                TotalProductos = productosActivos,
                ProductosDestacados = productosDestacados,
                ProductosEnOferta = productosEnOferta,
                StockTotal = stockTotal,
                PorcentajeDestacados = productosActivos > 0 ? (productosDestacados * 100.0 / productosActivos) : 0,
                PorcentajeOfertas = productosActivos > 0 ? (productosEnOferta * 100.0 / productosActivos) : 0
            };
        }
    }
}