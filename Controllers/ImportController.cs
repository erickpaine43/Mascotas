// Controllers/ImportController.cs
using Mascotas.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mascotas.Services;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Gerente")]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IImportService importService, ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        // ✅ MANTENER tus endpoints originales (para compatibilidad)
        [HttpPost("categorias")]
        public async Task<ActionResult<ImportResultDto>> ImportarCategorias(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new ImportResultDto { Message = "No se proporcionó archivo" });
            }

            if (!Path.GetExtension(archivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ImportResultDto { Message = "Solo se permiten archivos Excel (.xlsx)" });
            }

            try
            {
                using var stream = archivo.OpenReadStream();
                var resultado = await _importService.ImportarCategoriasAsync(stream);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importando categorías");
                return StatusCode(500, new ImportResultDto { Message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("productos")]
        public async Task<ActionResult<ImportResultDto>> ImportarProductos(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new ImportResultDto { Message = "No se proporcionó archivo" });
            }

            try
            {
                using var stream = archivo.OpenReadStream();
                var resultado = await _importService.ImportarProductosAsync(stream);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importando productos");
                return StatusCode(500, new ImportResultDto { Message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("animales")]
        public async Task<ActionResult<ImportResultDto>> ImportarAnimales(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new ImportResultDto { Message = "No se proporcionó archivo" });
            }

            try
            {
                using var stream = archivo.OpenReadStream();
                var resultado = await _importService.ImportarAnimalesAsync(stream);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importando animales");
                return StatusCode(500, new ImportResultDto { Message = $"Error interno: {ex.Message}" });
            }
        }

        // ✅ NUEVO: Endpoint FLEXIBLE que detecta automáticamente
        [HttpPost("auto-detect")]
        public async Task<ActionResult<ImportResultDto>> ImportarAutomatico(IFormFile archivo, [FromQuery] string tipo = "auto")
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new ImportResultDto { Message = "No se proporcionó archivo" });
            }

            try
            {
                using var stream = archivo.OpenReadStream();

                // Si el tipo es "auto", detectar automáticamente
                if (tipo == "auto")
                {
                    tipo = await _importService.DetectarTipoAsync(stream);
                }

                ImportResultDto resultado = tipo.ToLower() switch
                {
                    "productos" => await _importService.ImportarProductosFlexibleAsync(stream),
                    "animales" => await _importService.ImportarAnimalesFlexibleAsync(stream),
                    "categorias" => await _importService.ImportarCategoriasFlexibleAsync(stream),
                    _ => new ImportResultDto { Message = $"Tipo no soportado: {tipo}" }
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en importación automática");
                return StatusCode(500, new ImportResultDto { Message = $"Error interno: {ex.Message}" });
            }
        }

        // ✅ NUEVO: Preview para ver qué detecta el sistema
        [HttpPost("preview")]
        public async Task<ActionResult<PreviewResultDto>> PreviewArchivo(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest("No se proporcionó archivo");
            }

            try
            {
                using var stream = archivo.OpenReadStream();
                var preview = await _importService.ObtenerPreviewAsync(stream);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en preview");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("plantilla/{tipo}")]
        public IActionResult DescargarPlantilla(string tipo)
        {
            try
            {
                var plantillaBytes = _importService.GenerarPlantilla(tipo);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"plantilla-{tipo}-{DateTime.Now:yyyyMMdd}.xlsx";

                return File(plantillaBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando plantilla");
                return BadRequest("Error generando plantilla");
            }
        }
    }
}