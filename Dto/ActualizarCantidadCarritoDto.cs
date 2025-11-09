using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class ActualizarCantidadCarritoDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }
    }
}
