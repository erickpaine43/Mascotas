// Services/CalculoEnvioService.cs
using Mascotas.Models;
using Mascotas.Services;
using Mascotas.Data;
using Microsoft.EntityFrameworkCore;

public class CalculoEnvioService : ICalculoEnvioService
{
    private readonly MascotaDbContext _context;

    public CalculoEnvioService(MascotaDbContext context)
    {
        _context = context;
    }

    public async Task<CalculoEnvioResult> CalcularEnvioAsync(int direccionId, List<ItemCalculoEnvio> items, int metodoEnvioId)
    {
        // 1. Obtener dirección del perfil de usuario
        var direccion = await _context.Direcciones
            .Include(d => d.PerfilUsuario)
            .FirstOrDefaultAsync(d => d.Id == direccionId);

        if (direccion == null)
            throw new ArgumentException("Dirección no encontrada");

        // 2. Obtener método de envío
        var metodoEnvio = await _context.MetodoEnvios
            .FirstOrDefaultAsync(m => m.Id == metodoEnvioId && m.Activo);

        if (metodoEnvio == null)
            throw new ArgumentException("Método de envío no disponible");

        // 3. Si es retiro en local, costo cero
        if (metodoEnvio.RetiroEnLocal)
        {
            return new CalculoEnvioResult
            {
                CostoEnvio = 0,
                DiasEntrega = 0,
                FechaEstimada = DateTime.UtcNow,
                MetodoEnvio = metodoEnvio.Nombre,
                EnvioGratis = true,
                Mensaje = "Retiro en tienda disponible"
            };
        }

        // 4. Buscar zona de envío por provincia/código postal
        var zonaEnvio = await _context.ZonaEnvios
            .FirstOrDefaultAsync(z => z.Provincia == direccion.Provincia && z.Activo);

        if (zonaEnvio == null)
            throw new ArgumentException("No hay envío disponible para esta zona");

        // 5. Calcular peso total y recargos
        decimal pesoTotal = items.Sum(i => i.Peso * i.Cantidad);
        int itemsFragiles = items.Count(i => i.EsFragil);
        decimal subtotalPedido = items.Sum(i => i.PrecioUnitario * i.Cantidad);

        // 6. Calcular costo base
        decimal costoEnvio = metodoEnvio.Nombre.Contains("Express", StringComparison.OrdinalIgnoreCase)
            ? zonaEnvio.TarifaBaseExpress
            : zonaEnvio.TarifaBaseEstandar;

        // 7. Agregar costo por peso extra
        if (pesoTotal > 5) // Primeros 5 kg incluidos
        {
            decimal kilosExtra = pesoTotal - 5;
            costoEnvio += kilosExtra * zonaEnvio.CostoPorKiloExtra;
        }

        // 8. Agregar recargo por frágiles
        if (itemsFragiles > 0)
        {
            costoEnvio += itemsFragiles * zonaEnvio.RecargoFragil;
        }

        // 9. Verificar envío gratis
        bool envioGratis = subtotalPedido >= zonaEnvio.MontoMinimoEnvioGratis;
        if (envioGratis)
        {
            costoEnvio = 0;
        }

        // 10. Calcular fecha estimada
        int diasEntrega = metodoEnvio.Nombre.Contains("Express", StringComparison.OrdinalIgnoreCase)
            ? zonaEnvio.DiasExpress
            : zonaEnvio.DiasEstandar;

        return new CalculoEnvioResult
        {
            CostoEnvio = costoEnvio,
            DiasEntrega = diasEntrega,
            FechaEstimada = DateTime.UtcNow.AddDays(diasEntrega),
            MetodoEnvio = metodoEnvio.Nombre,
            EnvioGratis = envioGratis,
            Mensaje = envioGratis ? "¡Envío gratis por compra mayor a $" + zonaEnvio.MontoMinimoEnvioGratis : null
        };
    }

    public async Task<List<MetodoEnvio>> ObtenerMetodosDisponiblesAsync(int direccionId, decimal pesoTotal)
    {
        var direccion = await _context.Direcciones
            .FirstOrDefaultAsync(d => d.Id == direccionId);

        if (direccion == null)
            return new List<MetodoEnvio>();

        var metodos = await _context.MetodoEnvios
            .Where(m => m.Activo)
            .ToListAsync();

        // Filtrar métodos que no soportan el peso (ej: retiro siempre disponible)
        return metodos.Where(m => m.RetiroEnLocal || pesoTotal <= 25).ToList();
    }
}