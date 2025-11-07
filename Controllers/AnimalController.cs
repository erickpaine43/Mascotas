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
    }
}
