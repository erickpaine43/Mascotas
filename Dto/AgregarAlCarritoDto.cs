using System.ComponentModel.DataAnnotations;

namespace Mascotas.Dto
{
    public class AgregarAlCarritoDto : IValidatableObject
    {
        public int? ProductoId { get; set; }
        public int? MascotaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; } = 1;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // ✅ MEJORAR: Considerar null o 0 como no válidos
            var productoIdValido = ProductoId.HasValue && ProductoId.Value > 0;
            var mascotaIdValido = MascotaId.HasValue && MascotaId.Value > 0;

            if (!productoIdValido && !mascotaIdValido)
            {
                yield return new ValidationResult(
                    "Debe proporcionar un ProductoId o MascotaId válido (mayor a 0)",
                    new[] { nameof(ProductoId), nameof(MascotaId) });
            }

            if (productoIdValido && mascotaIdValido)
            {
                yield return new ValidationResult(
                    "Solo se puede agregar un producto o una mascota, no ambos en el mismo item",
                    new[] { nameof(ProductoId), nameof(MascotaId) });
            }

            // ✅ VALIDACIÓN EXTRA: Si es mascota, cantidad debe ser 1
            if (mascotaIdValido && Cantidad != 1)
            {
                yield return new ValidationResult(
                    "Para mascotas, la cantidad debe ser 1",
                    new[] { nameof(Cantidad) });
            }
        }
    }
}