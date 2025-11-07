// Controllers/ProductosController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Dto;
using Mascotas.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

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

        public ProductosController(MascotaDbContext context, IMapper mapper, ILogger<ProductosController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
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
    }
}