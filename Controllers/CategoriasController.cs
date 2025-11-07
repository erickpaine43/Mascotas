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
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Productos)
                .Where(c => c.Activo)
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
    }
}