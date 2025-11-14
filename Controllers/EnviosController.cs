// Controllers/EnviosController.cs
using Mascotas.Models;
using Mascotas.Services;
using Microsoft.AspNetCore.Mvc;
using Mascotas.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EnviosController : ControllerBase
{
    private readonly ICalculoEnvioService _calculoEnvioService;
    private readonly MascotaDbContext _context;

    public EnviosController(ICalculoEnvioService calculoEnvioService, MascotaDbContext context)
    {
        _calculoEnvioService = calculoEnvioService;
        _context = context;
    }

    [HttpPost("calcular")]
    public async Task<ActionResult<CalculoEnvioResult>> CalcularEnvio([FromBody] CalculoEnvioRequest request)
    {
        try
        {
            // Obtener productos para calcular peso y fragilidad
            var productos = await _context.Productos
                .Where(p => request.Items.Select(i => i.ProductoId).Contains(p.Id))
                .ToListAsync();

            var itemsCalculo = request.Items.Select(item =>
            {
                var producto = productos.FirstOrDefault(p => p.Id == item.ProductoId);
                return new ItemCalculoEnvio
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    Peso = producto?.Peso ?? 0.5m,
                    EsFragil = producto?.EsFragil ?? false,
                    PrecioUnitario = producto?.Precio ?? 0
                };
            }).ToList();

            var resultado = await _calculoEnvioService.CalcularEnvioAsync(
                request.DireccionId, itemsCalculo, request.MetodoEnvioId);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("metodos-disponibles/{direccionId}")]
    public async Task<ActionResult<List<MetodoEnvio>>> ObtenerMetodosDisponibles(int direccionId)
    {
        try
        {
            // Calcular peso estimado del carrito (puedes modificar según tu lógica)
            decimal pesoEstimado = 2.0m; // Peso base estimado

            var metodos = await _calculoEnvioService.ObtenerMetodosDisponiblesAsync(direccionId, pesoEstimado);
            return Ok(metodos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class CalculoEnvioRequest
{
    public int DireccionId { get; set; }
    public int MetodoEnvioId { get; set; }
    public List<ItemEnvioRequest> Items { get; set; } = new();
}

public class ItemEnvioRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}