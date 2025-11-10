// Controllers/ProductosController.cs
using AutoMapper;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductosController> _logger;
        private readonly IBusquedaAvanzadaService _busquedaService;
        private readonly IFiltroGuardadoService _filtroService;
        private readonly IAlertaPrecioService _alertaService;
        private readonly INotificacionService _notificacionService;

        public ProductosController(
            MascotaDbContext context,
            IMapper mapper,
            ILogger<ProductosController> logger,
            IBusquedaAvanzadaService busquedaService,
            IFiltroGuardadoService filtroService,
            IAlertaPrecioService alertaService,
            INotificacionService notificacionService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _busquedaService = busquedaService;
            _filtroService = filtroService;
            _alertaService = alertaService;
            _notificacionService = notificacionService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos(
            [FromQuery] int? categoriaId = null,
            [FromQuery] bool? destacados = null,
            [FromQuery] bool? enOferta = null,
            [FromQuery] string? buscar = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo);

            // Filtros
            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            if (destacados.HasValue)
                query = query.Where(p => p.Destacado == destacados.Value);

            if (enOferta.HasValue)
                query = query.Where(p => p.EnOferta == enOferta.Value);

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(p => p.Nombre.Contains(buscar) ||
                                        p.Descripcion.Contains(buscar) ||
                                        (p.Marca != null && p.Marca.Contains(buscar)));
            }

            // Paginación
            var total = await query.CountAsync();
            var productos = await query
                .OrderByDescending(p => p.Destacado)
                .ThenByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            var productosDto = _mapper.Map<List<ProductoDto>>(productos);

            // Manejar manualmente ImagenesAdicionales
            foreach (var productoDto in productosDto)
            {
                var producto = productos.First(p => p.Id == productoDto.Id);
                productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
            }

            return Ok(new
            {
                productos = productosDto,
                paginacion = new
                {
                    pagina,
                    porPagina,
                    total,
                    totalPaginas = (int)Math.Ceiling(total / (double)porPagina)
                }
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null || !producto.Activo)
            {
                return NotFound();
            }

            var productoDto = _mapper.Map<ProductoDto>(producto);
            productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);

            return productoDto;
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<ProductoDto>> CreateProducto(CreateProductoDto createProductoDto)
        {
            // Verificar que la categoría existe
            var categoria = await _context.Categorias.FindAsync(createProductoDto.CategoriaId);
            if (categoria == null)
            {
                return BadRequest("La categoría especificada no existe");
            }

            var producto = _mapper.Map<Producto>(createProductoDto);
            producto.Activo = true;
            producto.FechaCreacion = DateTime.UtcNow;
            producto.EnOferta = createProductoDto.Descuento > 0;

            // Manejar manualmente ImagenesAdicionales
            producto.ImagenesAdicionales = SerializarImagenes(createProductoDto.ImagenesAdicionales);

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Cargar la categoría para el DTO
            await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();

            var productoDto = _mapper.Map<ProductoDto>(producto);
            productoDto.ImagenesAdicionales = createProductoDto.ImagenesAdicionales ?? new List<string>();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, productoDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> UpdateProducto(int id, UpdateProductoDto updateProductoDto)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            // Verificar categoría si se está actualizando
            if (updateProductoDto.CategoriaId.HasValue)
            {
                var categoria = await _context.Categorias.FindAsync(updateProductoDto.CategoriaId.Value);
                if (categoria == null)
                {
                    return BadRequest("La categoría especificada no existe");
                }
            }

            _mapper.Map(updateProductoDto, producto);

            // Manejar manualmente ImagenesAdicionales si se proporciona
            if (updateProductoDto.ImagenesAdicionales != null)
            {
                producto.ImagenesAdicionales = SerializarImagenes(updateProductoDto.ImagenesAdicionales);
            }

            // Actualizar EnOferta basado en descuento
            if (updateProductoDto.Descuento.HasValue)
            {
                producto.EnOferta = updateProductoDto.Descuento.Value > 0;
            }

            producto.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            // Soft delete
            producto.Activo = false;
            producto.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("categorias/{categoriaId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosPorCategoria(int categoriaId)
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.CategoriaId == categoriaId)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            var productosDto = _mapper.Map<List<ProductoDto>>(productos);

            // Manejar manualmente ImagenesAdicionales
            foreach (var productoDto in productosDto)
            {
                var producto = productos.First(p => p.Id == productoDto.Id);
                productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
            }

            return Ok(productosDto);
        }

        [HttpGet("destacados")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosDestacados()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Destacado)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(10)
                .ToListAsync();

            var productosDto = _mapper.Map<List<ProductoDto>>(productos);

            // Manejar manualmente ImagenesAdicionales
            foreach (var productoDto in productosDto)
            {
                var producto = productos.First(p => p.Id == productoDto.Id);
                productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
            }

            return Ok(productosDto);
        }

        [HttpGet("ofertas")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosEnOferta()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.EnOferta)
                .OrderByDescending(p => p.Descuento)
                .Take(15)
                .ToListAsync();

            var productosDto = _mapper.Map<List<ProductoDto>>(productos);

            // Manejar manualmente ImagenesAdicionales
            foreach (var productoDto in productosDto)
            {
                var producto = productos.First(p => p.Id == productoDto.Id);
                productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
            }

            return Ok(productosDto);
        }

        // Métodos helper para serializar/deserializar
        private static List<string> DeserializarImagenes(string? jsonImagenes)
        {
            if (string.IsNullOrEmpty(jsonImagenes))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(jsonImagenes) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string? SerializarImagenes(List<string>? imagenes)
        {
            return imagenes != null && imagenes.Any()
                ? JsonSerializer.Serialize(imagenes)
                : null;
        }
        [HttpGet("todos-productos")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetTodosProductos()
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();

            var productoDto = _mapper.Map<List<ProductoDto>>(producto);   
            return Ok(productoDto);

        }
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> SearchProductos([FromQuery] ProductoSearchParams searchParams)
        {
            try
            {
                var query = _context.Productos
                    .Include(p => p.Categoria)
                    .AsQueryable();

                // 🔍 Búsqueda por texto inteligente
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

                // 📂 Filtro por categoría
                if (searchParams.CategoriaId.HasValue)
                    query = query.Where(p => p.CategoriaId == searchParams.CategoriaId.Value);

                // 🏷️ Filtro por marca
                if (!string.IsNullOrEmpty(searchParams.Marca))
                    query = query.Where(p => p.Marca == searchParams.Marca);

                // ⭐ Filtros booleanos
                if (searchParams.Destacado.HasValue)
                    query = query.Where(p => p.Destacado == searchParams.Destacado.Value);

                if (searchParams.EnOferta.HasValue)
                    query = query.Where(p => p.EnOferta == searchParams.EnOferta.Value);

                if (searchParams.Activo.HasValue)
                    query = query.Where(p => p.Activo == searchParams.Activo.Value);

                // 💰 Filtro por rango de precio
                if (searchParams.PrecioMin.HasValue)
                    query = query.Where(p => p.Precio >= searchParams.PrecioMin.Value);

                if (searchParams.PrecioMax.HasValue)
                    query = query.Where(p => p.Precio <= searchParams.PrecioMax.Value);

                // 📦 Filtros de stock
                if (searchParams.EnStock.HasValue && searchParams.EnStock.Value)
                    query = query.Where(p => p.StockDisponible > 0);

                if (searchParams.StockMin.HasValue)
                    query = query.Where(p => p.StockDisponible >= searchParams.StockMin.Value);

                // ⭐ Filtro por rating
                if (searchParams.RatingMin.HasValue)
                    query = query.Where(p => p.Rating >= searchParams.RatingMin.Value);

                // 🎯 Filtro por descuento
                if (searchParams.DescuentoMin.HasValue)
                    query = query.Where(p => p.Descuento >= searchParams.DescuentoMin.Value);

                // 📊 Ordenamiento avanzado
                query = searchParams.SortBy?.ToLower() switch
                {
                    "precio" => searchParams.SortDescending == true ?
                        query.OrderByDescending(p => p.Precio) : query.OrderBy(p => p.Precio),

                    "nombre" => searchParams.SortDescending == true ?
                        query.OrderByDescending(p => p.Nombre) : query.OrderBy(p => p.Nombre),

                    "rating" => searchParams.SortDescending == true ?
                        query.OrderByDescending(p => p.Rating) : query.OrderBy(p => p.Rating),

                    "masVendidos" => query.OrderByDescending(p => p.StockVendido),

                    "novedades" => query.OrderByDescending(p => p.FechaCreacion),

                    "descuento" => query.OrderByDescending(p => p.Descuento),

                    _ => query.OrderByDescending(p => p.Destacado)
                              .ThenByDescending(p => p.FechaCreacion) // Default
                };

                // 📄 Paginación
                var totalCount = await query.CountAsync();
                var productos = await query
                    .Skip((searchParams.Page - 1) * searchParams.PageSize)
                    .Take(searchParams.PageSize)
                    .ToListAsync();

                var productosDto = _mapper.Map<List<ProductoDto>>(productos);

                // Manejar manualmente ImagenesAdicionales
                foreach (var productoDto in productosDto)
                {
                    var producto = productos.First(p => p.Id == productoDto.Id);
                    productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
                }

                // 📦 Respuesta con metadata
                var response = new
                {
                    Data = productosDto,
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
                _logger.LogError(ex, "Error al buscar productos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("filters/options")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetProductosFilterOptions()
        {
            var marcas = await _context.Productos
                .Where(p => p.Activo && !string.IsNullOrEmpty(p.Marca))
                .Select(p => p.Marca)
                .Distinct()
                .ToListAsync();

            var precios = await _context.Productos
                .Where(p => p.Activo)
                .Select(p => p.Precio)
                .ToListAsync();

            var categorias = await _context.Categorias
                .Where(c => c.Activo)
                .Select(c => new { c.Id, c.Nombre })
                .ToListAsync();

            return new
            {
                Marcas = marcas,
                Categorias = categorias,
                PrecioMin = precios.Any() ? precios.Min() : 0,
                PrecioMax = precios.Any() ? precios.Max() : 0,
                RatingMax = 5, // Rating máximo siempre es 5
                DescuentoMax = 100 // Descuento máximo siempre es 100%
            };
        }

        [HttpGet("suggestions")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetSearchSuggestions([FromQuery] string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Ok(new List<string>());

            var suggestions = await _context.Productos
                .Where(p => p.Activo &&
                           (p.Nombre.Contains(term) ||
                            (p.Marca != null && p.Marca.Contains(term)) ||
                            (p.DescripcionCorta != null && p.DescripcionCorta.Contains(term))))
                .Select(p => p.Nombre)
                .Distinct()
                .Take(10)
                .ToListAsync();

            return Ok(suggestions);
        }
        [HttpGet("{id}/relacionados")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosRelacionados(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            // Productos de la misma categoría y marca (si tiene)
            var relacionados = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo &&
                           p.Id != id &&
                           p.CategoriaId == producto.CategoriaId &&
                           (string.IsNullOrEmpty(producto.Marca) || p.Marca == producto.Marca))
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.Destacado)
                .Take(8)
                .ToListAsync();

            var relacionadosDto = _mapper.Map<List<ProductoDto>>(relacionados);

            foreach (var productoDto in relacionadosDto)
            {
                var prod = relacionados.First(p => p.Id == productoDto.Id);
                productoDto.ImagenesAdicionales = DeserializarImagenes(prod.ImagenesAdicionales);
            }

            return Ok(relacionadosDto);
        }
        [HttpGet("{id}/recomendaciones")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetRecomendaciones(int id)
        {
            // Lógica para productos complementarios
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            var recomendaciones = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo &&
                           p.Id != id &&
                           (p.CategoriaId == producto.CategoriaId ||
                            p.EspecieDestinada == producto.EspecieDestinada))
                .OrderByDescending(p => p.Rating)
                .Take(6)
                .ToListAsync();

            return Ok(_mapper.Map<List<ProductoDto>>(recomendaciones));
        }
        [HttpGet("busqueda-contextual")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> BusquedaContextual([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                return BadRequest("La consulta de búsqueda debe tener al menos 2 caracteres");
            }

            try
            {
                var terminos = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var productosQuery = _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo && p.StockDisponible > 0)
                    .AsQueryable();

                // 🐾 DETECTAR ESPECIE
                if (terminos.Any(t => t.Contains("perro") || t.Contains("canino") || t.Contains("can") || t == "perros"))
                    productosQuery = productosQuery.Where(p => p.EspecieDestinada == "Perro" || p.EspecieDestinada == null);
                else if (terminos.Any(t => t.Contains("gato") || t.Contains("felino") || t.Contains("gat") || t == "gatos"))
                    productosQuery = productosQuery.Where(p => p.EspecieDestinada == "Gato" || p.EspecieDestinada == null);
                else if (terminos.Any(t => t.Contains("ave") || t.Contains("pájaro") || t.Contains("loro") || t.Contains("canario")))
                    productosQuery = productosQuery.Where(p => p.EspecieDestinada == "Ave" || p.EspecieDestinada == null);
                else if (terminos.Any(t => t.Contains("pez") || t.Contains("peces") || t.Contains("acuario") || t.Contains("pescado")))
                    productosQuery = productosQuery.Where(p => p.EspecieDestinada == "Pez" || p.EspecieDestinada == null);
                else if (terminos.Any(t => t.Contains("roedor") || t.Contains("hámster") || t.Contains("cobaya") || t.Contains("conejo") || t.Contains("ratón")))
                    productosQuery = productosQuery.Where(p => p.EspecieDestinada == "Roedor" || p.EspecieDestinada == null);
                else if (terminos.Any(t => t.Contains("reptil") || t.Contains("tortuga") || t.Contains("lagarto") || t.Contains("serpiente")))
                    productosQuery = productosQuery.Where(p => p.EspecieDestinada == "Reptil" || p.EspecieDestinada == null);

                // 🎂 DETECTAR ETAPA DE VIDA
                if (terminos.Any(t => t.Contains("cachorro") || t.Contains("cachorrito") || t.Contains("puppy") || t.Contains("gatito") || t.Contains("cachorro")))
                    productosQuery = productosQuery.Where(p => p.EtapaVida == "Cachorro" || p.EtapaVida == null);
                else if (terminos.Any(t => t.Contains("adulto") || t.Contains("adult") || t.Contains("maduro")))
                    productosQuery = productosQuery.Where(p => p.EtapaVida == "Adulto" || p.EtapaVida == null);
                else if (terminos.Any(t => t.Contains("senior") || t.Contains("anciano") || t.Contains("veterano") || t.Contains("mayor")))
                    productosQuery = productosQuery.Where(p => p.EtapaVida == "Senior" || p.EtapaVida == null);

                // 🏥 DETECTAR NECESIDADES ESPECIALES/SALUD
                if (terminos.Any(t => t.Contains("esterilizado") || t.Contains("castrado") || t.Contains("sterilized")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Esterilizado" || p.NecesidadesEspeciales == null);
                else if (terminos.Any(t => t.Contains("pelo largo") || t.Contains("pelolargo") || t.Contains("longhair")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Pelo Largo" || p.NecesidadesEspeciales == null);
                else if (terminos.Any(t => t.Contains("alergia") || t.Contains("alérgico") || t.Contains("sensible") || t.Contains("hypoallergenic")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Alergias" || p.NecesidadesEspeciales == null);
                else if (terminos.Any(t => t.Contains("obesidad") || t.Contains("sobrepeso") || t.Contains("peso") || t.Contains("dieta")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Control Peso" || p.NecesidadesEspeciales == null);
                else if (terminos.Any(t => t.Contains("urinary") || t.Contains("urinario") || t.Contains("riñón") || t.Contains("renal")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Salud Urinaria" || p.NecesidadesEspeciales == null);

                // 🏠 DETECTAR ESTILO DE VIDA
                if (terminos.Any(t => t.Contains("indoor") || t.Contains("interior") || t.Contains("apartamento") || t.Contains("casa")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Indoor" || p.NecesidadesEspeciales == null);
                else if (terminos.Any(t => t.Contains("outdoor") || t.Contains("exterior") || t.Contains("campo") || t.Contains("jardín")))
                    productosQuery = productosQuery.Where(p => p.NecesidadesEspeciales == "Outdoor" || p.NecesidadesEspeciales == null);

                // 🐕 DETECTAR RAZAS COMUNES
                var razasPerro = new[] { "labrador", "pastor", "bulldog", "poodle", "chihuahua", "beagle", "boxer", "doberman", "rottweiler", "husky", "golden" };
                var razasGato = new[] { "siamés", "persa", "angora", "bengala", "esfinge", "sphynx", "mainecoon", "ragdoll" };

                var razaDetectada = terminos.FirstOrDefault(t => razasPerro.Concat(razasGato).Any(r => t.Contains(r)));
                if (!string.IsNullOrEmpty(razaDetectada))
                    productosQuery = productosQuery.Where(p => p.RazaDestinada.Contains(razaDetectada) || p.RazaDestinada == null);

                // 📏 DETECTAR TAMAÑO
                if (terminos.Any(t => t.Contains("pequeño") || t.Contains("pequeña") || t.Contains("small") || t.Contains("mini") || t.Contains("toy")))
                    productosQuery = productosQuery.Where(p => p.Dimensiones.Contains("pequeño") || p.Dimensiones == null);
                else if (terminos.Any(t => t.Contains("mediano") || t.Contains("mediana") || t.Contains("medium") || t.Contains("standard")))
                    productosQuery = productosQuery.Where(p => p.Dimensiones.Contains("mediano") || p.Dimensiones == null);
                else if (terminos.Any(t => t.Contains("grande") || t.Contains("large") || t.Contains("xl") || t.Contains("gigante")))
                    productosQuery = productosQuery.Where(p => p.Dimensiones.Contains("grande") || p.Dimensiones == null);

                // 💊 DETECTAR TIPO DE PRODUCTO POR PALABRAS CLAVE
                if (terminos.Any(t => t.Contains("alimento") || t.Contains("comida") || t.Contains("food") || t.Contains("pienso") || t.Contains("croquetas")))
                    productosQuery = productosQuery.Where(p => p.Categoria.Nombre.Contains("Alimento") || p.Descripcion.Contains("alimento"));

                else if (terminos.Any(t => t.Contains("juguete") || t.Contains("juego") || t.Contains("toy") || t.Contains("pelota") || t.Contains("mordedor")))
                    productosQuery = productosQuery.Where(p => p.Categoria.Nombre.Contains("Juguete") || p.Descripcion.Contains("juguete"));

                else if (terminos.Any(t => t.Contains("medicamento") || t.Contains("medicina") || t.Contains("vacuna") || t.Contains("desparasitador") || t.Contains("antipulgas")))
                    productosQuery = productosQuery.Where(p => p.Categoria.Nombre.Contains("Medicamento") || p.Categoria.Nombre.Contains("Salud"));


        else if (terminos.Any(t => t.Contains("accesorio") || t.Contains("correa") || t.Contains("arnés") || t.Contains("collar") || t.Contains("cama") || t.Contains("transportín")))
                    productosQuery = productosQuery.Where(p => p.Categoria.Nombre.Contains("Accesorio") || p.Descripcion.Contains("accesorio"));


        else if (terminos.Any(t => t.Contains("higiene") || t.Contains("shampoo") || t.Contains("champú") || t.Contains("cepillo") || t.Contains("limpieza")))
                    productosQuery = productosQuery.Where(p => p.Categoria.Nombre.Contains("Higiene") || p.Descripcion.Contains("higiene"));

                // 🎯 BÚSQUEDA POR TEXTO EN CAMPOS PRINCIPALES (como fallback)
                var searchQuery = string.Join(" ", terminos);
                productosQuery = productosQuery.Where(p =>
                    p.Nombre.ToLower().Contains(searchQuery) ||
                    p.Descripcion.ToLower().Contains(searchQuery) ||
                    p.DescripcionCorta.ToLower().Contains(searchQuery) ||
                    (p.Marca != null && p.Marca.ToLower().Contains(searchQuery)) ||
                    p.Categoria.Nombre.ToLower().Contains(searchQuery)
                );

                // 📊 ORDENAMIENTO INTELIGENTE POR RELEVANCIA
                // Priorizar productos que coincidan con más criterios
                var productos = await productosQuery
                    .OrderByDescending(p =>
                        (p.Nombre.ToLower().Contains(searchQuery) ? 3 : 0) +
                        (p.Descripcion.ToLower().Contains(searchQuery) ? 2 : 0) +
                        (p.Marca != null && p.Marca.ToLower().Contains(searchQuery) ? 2 : 0) +
                        (p.Categoria.Nombre.ToLower().Contains(searchQuery) ? 1 : 0))
                    .ThenByDescending(p => p.Rating)
                    .ThenByDescending(p => p.Destacado)
                    .Take(25)
                    .ToListAsync();

                var productosDto = _mapper.Map<List<ProductoDto>>(productos);

                // Manejar manualmente ImagenesAdicionales
                foreach (var productoDto in productosDto)
                {
                    var producto = productos.First(p => p.Id == productoDto.Id);
                    productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
                }

                // 📦 METADATA DE BÚSQUEDA
                var metadata = new
                {
                    QueryOriginal = query,
                    TerminosDetectados = terminos,
                    TotalResultados = productosDto.Count,
                    EspecieDetectada = productosQuery.Any(p => p.EspecieDestinada != null) ?
                        productos.FirstOrDefault(p => p.EspecieDestinada != null)?.EspecieDestinada : "Varias",
                    CategoriaPrincipal = productos.GroupBy(p => p.Categoria.Nombre)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key
                };

                return Ok(new
                {
                    Data = productosDto,
                    Metadata = metadata
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda contextual para query: {Query}", query);
                return StatusCode(500, "Error interno en la búsqueda contextual");
            }
        }
        [HttpGet("search-avanzado")]
        [AllowAnonymous]
        public async Task<ActionResult> SearchProductosAvanzado(
        [FromQuery] ProductoSearchParams searchParams,
        [FromQuery] ProductoSearchAdvancedDto? advancedParams = null)
        {
            try
            {
                var (productos, totalCount) = await _busquedaService
                    .BuscarProductosAvanzadoAsync(searchParams, advancedParams);

                var productosDto = _mapper.Map<List<ProductoDto>>(productos);

                // Manejar manualmente ImagenesAdicionales
                foreach (var productoDto in productosDto)
                {
                    var producto = productos.First(p => p.Id == productoDto.Id);
                    productoDto.ImagenesAdicionales = DeserializarImagenes(producto.ImagenesAdicionales);
                }

                var response = new
                {
                    Data = productosDto,
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
                _logger.LogError(ex, "Error en búsqueda avanzada");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpPost("filtros-guardados")]
        public async Task<ActionResult> GuardarFiltro([FromBody] GuardarFiltroRequest request)
        {
            try
            {
                var filtro = await _filtroService.GuardarFiltroAsync(
                    request.UsuarioId,
                    request.Nombre,
                    request.Parametros);

                return Ok(new { FiltroId = filtro.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar filtro");
                return StatusCode(500, "Error al guardar filtro");
            }
        }

        [HttpGet("filtros-guardados/{usuarioId}")]
        public async Task<ActionResult> ObtenerFiltrosGuardados(string usuarioId)
        {
            var filtros = await _filtroService.ObtenerFiltrosUsuarioAsync(usuarioId);
            return Ok(filtros);
        }

        // NUEVO: Endpoint para alertas de precio
        [HttpPost("alertas-precio")]
        public async Task<ActionResult> CrearAlertaPrecio([FromBody] CrearAlertaRequest request)
        {
            try
            {
                var alerta = await _alertaService.CrearAlertaAsync(
                    request.UsuarioId,
                    request.ProductoId,
                    request.PrecioObjetivo);

                return Ok(new { AlertaId = alerta.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear alerta de precio");
                return StatusCode(500, "Error al crear alerta");
            }
        }
        [HttpPost("busquedas-guardadas")]
        public async Task<ActionResult> GuardarBusquedaAmazon([FromBody] GuardarBusquedaRequest request)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized("Usuario no autenticado");

                if (string.IsNullOrWhiteSpace(request.Nombre))
                    return BadRequest("El nombre de la búsqueda es requerido");

                if (request.Parametros == null || !request.Parametros.Any())
                    return BadRequest("Los parámetros de búsqueda son requeridos");

                var busqueda = new FiltroGuardado
                {
                    UsuarioId = usuarioId,
                    Nombre = request.Nombre,
                    ParametrosBusqueda = JsonSerializer.Serialize(request.Parametros),
                    MonitorearNuevosProductos = request.MonitorearNuevos,
                    MonitorearBajasPrecio = request.MonitorearPrecios,
                    MonitorearStock = request.MonitorearStock,
                    PorcentajeBajaMinima = request.PorcentajeMinimo,
                    FechaCreacion = DateTime.UtcNow,
                    FechaUltimoUso = DateTime.UtcNow
                };

                _context.FiltroGuardados.Add(busqueda);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"✅ Búsqueda Amazon guardada por usuario {usuarioId}: {request.Nombre}");

                return Ok(new
                {
                    BusquedaId = busqueda.Id,
                    Mensaje = "✅ Búsqueda guardada al estilo Amazon. Te notificaremos de novedades automáticamente.",

                    MonitoreoActivado = new
                    {
                        NuevosProductos = request.MonitorearNuevos,
                        BajasPrecio = request.MonitorearPrecios,
                        Stock = request.MonitorearStock
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando búsqueda para usuario {UsuarioId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("notificaciones")]
        [Authorize]
        public async Task<ActionResult> ObtenerNotificacionesUsuario()
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized();

                var notificaciones = await _notificacionService.ObtenerNotificacionesUsuarioAsync(usuarioId, noLeidas: true);

                return Ok(new
                {
                    Notificaciones = notificaciones,
                    TotalNoLeidas = notificaciones.Count(n => !n.Leida)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo notificaciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("notificaciones/{id}/leer")]
        [Authorize]
        public async Task<ActionResult> MarcarNotificacionComoLeida(int id)
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized();

                await _notificacionService.MarcarComoLeidaAsync(id, usuarioId);

                return Ok(new { Mensaje = "Notificación marcada como leída" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marcando notificación como leída");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}