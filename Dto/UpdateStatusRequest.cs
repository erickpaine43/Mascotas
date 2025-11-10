using Mascotas.Models;

namespace Mascotas.Dto
{
    public class UpdateStatusRequest
    {
        public OrdenEstado NewStatus { get; set; }
        public string? Description { get; set; }
    }
}
