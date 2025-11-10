// Services/FiltroGuardadoService.cs
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using Mascotas.Models;
using System.Text.Json;

namespace Mascotas.Services
{
    public class FiltroGuardadoService : IFiltroGuardadoService
    {
        private readonly MascotaDbContext _context;

        public FiltroGuardadoService(MascotaDbContext context)
        {
            _context = context;
        }

        public async Task<FiltroGuardado> GuardarFiltroAsync(string usuarioId, string nombre, Dictionary<string, object> parametros)
        {
            var filtro = new FiltroGuardado
            {
                UsuarioId = usuarioId,
                Nombre = nombre,
                ParametrosBusqueda = JsonSerializer.Serialize(parametros),
                FechaCreacion = DateTime.UtcNow,
                FechaUltimoUso = DateTime.UtcNow
            };

            _context.FiltroGuardados.Add(filtro);
            await _context.SaveChangesAsync();

            return filtro;
        }

        public async Task<List<FiltroGuardado>> ObtenerFiltrosUsuarioAsync(string usuarioId)
        {
            return await _context.FiltroGuardados
                .Where(f => f.UsuarioId == usuarioId)
                .OrderByDescending(f => f.EsFavorito)
                .ThenByDescending(f => f.FechaUltimoUso)
                .ToListAsync();
        }

        public async Task<List<FiltroGuardado>> ObtenerFiltrosFavoritosAsync(string usuarioId)
        {
            return await _context.FiltroGuardados
                .Where(f => f.UsuarioId == usuarioId && f.EsFavorito)
                .OrderByDescending(f => f.FechaUltimoUso)
                .ToListAsync();
        }

        public async Task IncrementarUsoFiltroAsync(int filtroId)
        {
            var filtro = await _context.FiltroGuardados.FindAsync(filtroId);
            if (filtro != null)
            {
                filtro.VecesUtilizado++;
                filtro.FechaUltimoUso = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task EliminarFiltroAsync(int filtroId, string usuarioId)
        {
            var filtro = await _context.FiltroGuardados
                .FirstOrDefaultAsync(f => f.Id == filtroId && f.UsuarioId == usuarioId);

            if (filtro != null)
            {
                _context.FiltroGuardados.Remove(filtro);
                await _context.SaveChangesAsync();
            }
        }
    }
}