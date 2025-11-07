// Controllers/ClientesController.cs
using Mascotas.Dto;
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;

namespace PetStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly MascotaDbContext _context;
        private readonly IStripeService _stripeService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(MascotaDbContext context, IStripeService stripeService, ILogger<ClientesController> logger)
        {
            _context = context;
            _stripeService = stripeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _context.Clientes.OrderByDescending(c => c.FechaRegistro).ToListAsync();

            var clientesDto = clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Telefono = c.Telefono,
                Direccion = c.Direccion,
                Ciudad = c.Ciudad,
                CodigoPostal = c.CodigoPostal,
                FechaRegistro = c.FechaRegistro
            }).ToList();

            return Ok(clientesDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            var clienteDto = new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                Ciudad = cliente.Ciudad,
                CodigoPostal = cliente.CodigoPostal,
                FechaRegistro = cliente.FechaRegistro
            };

            return clienteDto;
        }

        [HttpPost]
        [AllowAnonymous] // Permitir registro sin autenticación
        public async Task<ActionResult<ClienteDto>> CreateCliente(CreateClienteDto createClienteDto)
        {
            // Verificar si el email ya existe
            if (await _context.Clientes.AnyAsync(c => c.Email == createClienteDto.Email))
            {
                return BadRequest("El email ya está registrado");
            }

            var cliente = new Cliente
            {
                Nombre = createClienteDto.Nombre,
                Email = createClienteDto.Email,
                Telefono = createClienteDto.Telefono,
                Direccion = createClienteDto.Direccion,
                Ciudad = createClienteDto.Ciudad,
                CodigoPostal = createClienteDto.CodigoPostal,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Crear cliente en Stripe
            try
            {
                await _stripeService.CreateCustomerAsync(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando cliente en Stripe");
                // Continuar aunque falle Stripe
            }

            var clienteDto = new ClienteDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                Ciudad = cliente.Ciudad,
                CodigoPostal = cliente.CodigoPostal,
                FechaRegistro = cliente.FechaRegistro
            };

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, clienteDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, UpdateClienteDto updateClienteDto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateClienteDto.Nombre))
                cliente.Nombre = updateClienteDto.Nombre;

            if (!string.IsNullOrEmpty(updateClienteDto.Email))
                cliente.Email = updateClienteDto.Email;

            if (updateClienteDto.Telefono != null)
                cliente.Telefono = updateClienteDto.Telefono;

            if (updateClienteDto.Direccion != null)
                cliente.Direccion = updateClienteDto.Direccion;

            if (updateClienteDto.Ciudad != null)
                cliente.Ciudad = updateClienteDto.Ciudad;

            if (updateClienteDto.CodigoPostal != null)
                cliente.CodigoPostal = updateClienteDto.CodigoPostal;

            cliente.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}