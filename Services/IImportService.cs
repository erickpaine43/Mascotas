using Mascotas.Dto;

namespace Mascotas.Services
{
    public interface IImportService
    {
        Task<ImportResultDto> ImportarCategoriasAsync(Stream fileStream);
        Task<ImportResultDto> ImportarProductosAsync(Stream fileStream);
        Task<ImportResultDto> ImportarAnimalesAsync(Stream fileStream);
        byte[] GenerarPlantilla(string tipo);
        Task<string> DetectarTipoAsync(Stream fileStream);
        Task<ImportResultDto> ImportarProductosFlexibleAsync(Stream fileStream);
        Task<ImportResultDto> ImportarAnimalesFlexibleAsync(Stream fileStream);
        Task<ImportResultDto> ImportarCategoriasFlexibleAsync(Stream fileStream);
        Task<PreviewResultDto> ObtenerPreviewAsync(Stream fileStream);
        List<string> ObtenerColumnasExcel(Stream fileStream, bool tieneEncabezados = true);

    }
}
