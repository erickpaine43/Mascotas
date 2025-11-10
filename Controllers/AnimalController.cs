// Controllers/AnimalesController.cs
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;

namespace PetStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnimalesController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly ILogger<AnimalesController> _logger;

        public AnimalesController(MascotaDbContext context, ILogger<AnimalesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AnimalDto>>> GetAnimales([FromQuery] bool? disponibles = null)
        {
            var query = _context.Animales.AsQueryable();

            if (disponibles.HasValue)
            {
                query = query.Where(a => a.Disponible == disponibles.Value);
            }

            var animales = await query.OrderByDescending(a => a.FechaCreacion).ToListAsync();

            var animalesDto = animales.Select(a => new AnimalDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Especie = a.Especie,
                Raza = a.Raza,
                Edad = a.Edad,
                Sexo = a.Sexo,
                Precio = a.Precio,
                Descripcion = a.Descripcion,
                Disponible = a.Disponible,
                Vacunado = a.Vacunado,
                Esterilizado = a.Esterilizado,
                FechaNacimiento = a.FechaNacimiento,
                FechaCreacion = a.FechaCreacion
            }).ToList();

            return Ok(animalesDto);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AnimalDto>> GetAnimal(int id)
        {
            var animal = await _context.Animales.FindAsync(id);

            if (animal == null)
            {
                return NotFound();
            }

            var animalDto = new AnimalDto
            {
                Id = animal.Id,
                Nombre = animal.Nombre,
                Especie = animal.Especie,
                Raza = animal.Raza,
                Edad = animal.Edad,
                Sexo = animal.Sexo,
                Precio = animal.Precio,
                Descripcion = animal.Descripcion,
                Disponible = animal.Disponible,
                Vacunado = animal.Vacunado,
                Esterilizado = animal.Esterilizado,
                FechaNacimiento = animal.FechaNacimiento,
                FechaCreacion = animal.FechaCreacion
            };

            return animalDto;
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<AnimalDto>> CreateAnimal(CreateAnimalDto createAnimalDto)
        {
            var animal = new Animal
            {
                Nombre = createAnimalDto.Nombre,
                Especie = createAnimalDto.Especie,
                Raza = createAnimalDto.Raza,
                Edad = createAnimalDto.Edad,
                Sexo = createAnimalDto.Sexo,
                Precio = createAnimalDto.Precio,
                Descripcion = createAnimalDto.Descripcion,
                Vacunado = createAnimalDto.Vacunado,
                Esterilizado = createAnimalDto.Esterilizado,
                FechaNacimiento = createAnimalDto.FechaNacimiento,
                FechaCreacion = DateTime.UtcNow,
                Disponible = true
            };

            _context.Animales.Add(animal);
            await _context.SaveChangesAsync();

            var animalDto = new AnimalDto
            {
                Id = animal.Id,
                Nombre = animal.Nombre,
                Especie = animal.Especie,
                Raza = animal.Raza,
                Edad = animal.Edad,
                Sexo = animal.Sexo,
                Precio = animal.Precio,
                Descripcion = animal.Descripcion,
                Disponible = animal.Disponible,
                Vacunado = animal.Vacunado,
                Esterilizado = animal.Esterilizado,
                FechaNacimiento = animal.FechaNacimiento,
                FechaCreacion = animal.FechaCreacion
            };

            return CreatedAtAction(nameof(GetAnimal), new { id = animal.Id }, animalDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> UpdateAnimal(int id, UpdateAnimalDto updateAnimalDto)
        {
            var animal = await _context.Animales.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            // Actualizar solo las propiedades que se enviaron
            if (!string.IsNullOrEmpty(updateAnimalDto.Nombre))
                animal.Nombre = updateAnimalDto.Nombre;

            if (!string.IsNullOrEmpty(updateAnimalDto.Especie))
                animal.Especie = updateAnimalDto.Especie;

            if (updateAnimalDto.Raza != null)
                animal.Raza = updateAnimalDto.Raza;

            if (updateAnimalDto.Edad.HasValue)
                animal.Edad = updateAnimalDto.Edad.Value;

            if (!string.IsNullOrEmpty(updateAnimalDto.Sexo))
                animal.Sexo = updateAnimalDto.Sexo;

            if (updateAnimalDto.Precio.HasValue)
                animal.Precio = updateAnimalDto.Precio.Value;

            if (updateAnimalDto.Descripcion != null)
                animal.Descripcion = updateAnimalDto.Descripcion;

            if (updateAnimalDto.Disponible.HasValue)
                animal.Disponible = updateAnimalDto.Disponible.Value;

            if (updateAnimalDto.Vacunado.HasValue)
                animal.Vacunado = updateAnimalDto.Vacunado.Value;

            if (updateAnimalDto.Esterilizado.HasValue)
                animal.Esterilizado = updateAnimalDto.Esterilizado.Value;

            if (updateAnimalDto.FechaNacimiento.HasValue)
                animal.FechaNacimiento = updateAnimalDto.FechaNacimiento.Value;

            animal.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteAnimal(int id)
        {
            var animal = await _context.Animales.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            _context.Animales.Remove(animal);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AnimalDto>>> SearchAnimales([FromQuery] AnimalSearchParams searchParams)
        {
            try
            {
                var query = _context.Animales.AsQueryable();

                // 🔍 Búsqueda por texto (Nombre, Especie, Raza, Descripción)
                if (!string.IsNullOrEmpty(searchParams.SearchTerm))
                {
                    var searchTerm = searchParams.SearchTerm.ToLower();
                    query = query.Where(a =>
                        a.Nombre.ToLower().Contains(searchTerm) ||
                        a.Especie.ToLower().Contains(searchTerm) ||
                        a.Raza.ToLower().Contains(searchTerm) ||
                        a.Descripcion.ToLower().Contains(searchTerm)
                    );
                }

                // 🐾 Filtros por especie y raza
                if (!string.IsNullOrEmpty(searchParams.Especie))
                    query = query.Where(a => a.Especie == searchParams.Especie);

                if (!string.IsNullOrEmpty(searchParams.Raza))
                    query = query.Where(a => a.Raza == searchParams.Raza);

                // ⚥ Filtro por sexo
                if (!string.IsNullOrEmpty(searchParams.Sexo))
                    query = query.Where(a => a.Sexo == searchParams.Sexo);

                // ✅ Filtros booleanos
                if (searchParams.Disponible.HasValue)
                    query = query.Where(a => a.Disponible == searchParams.Disponible.Value);

                if (searchParams.Vacunado.HasValue)
                    query = query.Where(a => a.Vacunado == searchParams.Vacunado.Value);

                if (searchParams.Esterilizado.HasValue)
                    query = query.Where(a => a.Esterilizado == searchParams.Esterilizado.Value);

                if (searchParams.Reservado.HasValue)
                    query = query.Where(a => a.Reservado == searchParams.Reservado.Value);

                // 💰 Filtro por rango de precio
                if (searchParams.PrecioMin.HasValue)
                    query = query.Where(a => a.Precio >= searchParams.PrecioMin.Value);

                if (searchParams.PrecioMax.HasValue)
                    query = query.Where(a => a.Precio <= searchParams.PrecioMax.Value);

                // 🎂 Filtro por edad (en meses)
                if (searchParams.EdadMin.HasValue)
                    query = query.Where(a => a.Edad >= searchParams.EdadMin.Value);

                if (searchParams.EdadMax.HasValue)
                    query = query.Where(a => a.Edad <= searchParams.EdadMax.Value);

                // 📅 Filtro por fecha de nacimiento
                if (searchParams.FechaNacimientoDesde.HasValue)
                    query = query.Where(a => a.FechaNacimiento >= searchParams.FechaNacimientoDesde.Value);

                if (searchParams.FechaNacimientoHasta.HasValue)
                    query = query.Where(a => a.FechaNacimiento <= searchParams.FechaNacimientoHasta.Value);

                // 📊 Ordenamiento
                query = searchParams.SortBy?.ToLower() switch
                {
                    "precio" => searchParams.SortDescending == true ?
                        query.OrderByDescending(a => a.Precio) : query.OrderBy(a => a.Precio),
                    "edad" => searchParams.SortDescending == true ?
                        query.OrderByDescending(a => a.Edad) : query.OrderBy(a => a.Edad),
                    "nombre" => searchParams.SortDescending == true ?
                        query.OrderByDescending(a => a.Nombre) : query.OrderBy(a => a.Nombre),
                    "fecha" => searchParams.SortDescending == true ?
                        query.OrderByDescending(a => a.FechaCreacion) : query.OrderBy(a => a.FechaCreacion),
                    _ => query.OrderByDescending(a => a.FechaCreacion) // Default
                };

                // 📄 Paginación
                var totalCount = await query.CountAsync();
                var animales = await query
                    .Skip((searchParams.Page - 1) * searchParams.PageSize)
                    .Take(searchParams.PageSize)
                    .ToListAsync();

                var animalesDto = animales.Select(a => new AnimalDto
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Especie = a.Especie,
                    Raza = a.Raza,
                    Edad = a.Edad,
                    Sexo = a.Sexo,
                    Precio = a.Precio,
                    Descripcion = a.Descripcion,
                    Disponible = a.Disponible,
                    Vacunado = a.Vacunado,
                    Esterilizado = a.Esterilizado,
                    FechaNacimiento = a.FechaNacimiento,
                    FechaCreacion = a.FechaCreacion
                }).ToList();

                // 📦 Respuesta con metadata de paginación
                var response = new
                {
                    Data = animalesDto,
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
                _logger.LogError(ex, "Error al buscar animales");
                return StatusCode(500, "Error interno del servidor");
            }
        }
        [HttpGet("filters/options")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFilterOptions()
        {
            var especies = await _context.Animales
                .Where(a => !string.IsNullOrEmpty(a.Especie))
                .Select(a => a.Especie)
                .Distinct()
                .ToListAsync();

            var razas = await _context.Animales
                .Where(a => !string.IsNullOrEmpty(a.Raza))
                .Select(a => a.Raza)
                .Distinct()
                .ToListAsync();

            var precios = await _context.Animales
                .Where(a => a.Disponible)
                .Select(a => a.Precio)
                .ToListAsync();

            return new
            {
                Especies = especies,
                Razas = razas,
                Sexos = new List<string> { "Macho", "Hembra" },
                PrecioMin = precios.Any() ? precios.Min() : 0,
                PrecioMax = precios.Any() ? precios.Max() : 0,
                EdadMin = await _context.Animales.MinAsync(a => (int?)a.Edad) ?? 0,
                EdadMax = await _context.Animales.MaxAsync(a => (int?)a.Edad) ?? 0
            };
        }
    }
}
