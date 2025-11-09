using Microsoft.AspNetCore.Mvc;
using Mascotas.Services;

namespace Mascotas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewReminderController : ControllerBase
    {
        private readonly IReviewReminderService _reviewReminderService;

        public ReviewReminderController(IReviewReminderService reviewReminderService)
        {
            _reviewReminderService = reviewReminderService;
        }

        [HttpPost("programar/{ordenId}")]
        public async Task<IActionResult> ProgramarRecordatorios(int ordenId)
        {
            await _reviewReminderService.ProgramarRecordatorios(ordenId);
            return Ok(new { message = "Recordatorios programados correctamente" });
        }

        [HttpPost("enviar-pendientes")]
        public async Task<IActionResult> EnviarRecordatoriosPendientes()
        {
            await _reviewReminderService.EnviarRecordatoriosPendientes();
            return Ok(new { message = "Procesamiento de recordatorios completado" });
        }

        [HttpGet("cliente/{clienteId}/pendientes")]
        public async Task<IActionResult> GetRecordatoriosPendientes(int clienteId)
        {
            var recordatorios = await _reviewReminderService.ObtenerRecordatoriosPendientes(clienteId);
            return Ok(recordatorios);
        }

        [HttpGet("puede-resenar")]
        public async Task<IActionResult> PuedeResenar(int clienteId, int? productoId = null, int? animalId = null)
        {
            var puede = await _reviewReminderService.ClientePuedeResenar(clienteId, productoId, animalId);
            return Ok(new { puedeResenar = puede });
        }
    }
}