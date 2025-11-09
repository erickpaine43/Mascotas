using Mascotas.Models;

namespace Mascotas.Services
    {
        public interface IReviewReminderService
        {
            Task ProgramarRecordatorios(int ordenId);
            Task EnviarRecordatoriosPendientes();
            Task<bool> ClientePuedeResenar(int clienteId, int? productoId = null, int? animalId = null);
            Task<List<ReviewReminder>> ObtenerRecordatoriosPendientes(int clienteId);
            Task MarcarResenaCompletada(int clienteId, int? productoId = null, int? animalId = null);
        }
    }


