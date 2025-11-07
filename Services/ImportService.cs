// Services/ImportService.cs
using Mascotas.Dto;
using Mascotas.Models;
using Microsoft.EntityFrameworkCore;
using Mascotas.Data;
using OfficeOpenXml;
using System.Globalization;

namespace Mascotas.Services
{
    public class ImportService : IImportService
    {
        private readonly MascotaDbContext _context;
        private readonly ILogger<ImportService> _logger;

        public ImportService(MascotaDbContext context, ILogger<ImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ImportResultDto> ImportarCategoriasAsync(Stream fileStream)
        {
            var resultado = new ImportResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                var categorias = new List<Categoria>();
                var errores = new List<string>();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var nombre = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var descripcion = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var imagenUrl = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var ordenStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(nombre))
                        {
                            errores.Add($"Fila {row}: Nombre es requerido");
                            continue;
                        }

                        if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre))
                        {
                            errores.Add($"Fila {row}: La categoría '{nombre}' ya existe");
                            continue;
                        }

                        var categoria = new Categoria
                        {
                            Nombre = nombre,
                            Descripcion = descripcion ?? string.Empty,
                            ImagenUrl = imagenUrl ?? string.Empty,
                            Orden = int.TryParse(ordenStr, out int orden) ? orden : 0,
                            Activo = true,
                            FechaCreacion = DateTime.UtcNow
                        };

                        categorias.Add(categoria);
                        resultado.RegistrosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {row}: Error - {ex.Message}");
                        resultado.RegistrosFallidos++;
                    }
                }

                resultado.TotalRegistros = worksheet.Dimension.End.Row - 1;

                if (categorias.Any())
                {
                    _context.Categorias.AddRange(categorias);
                    await _context.SaveChangesAsync();
                }

                resultado.Success = categorias.Any();
                resultado.Message = categorias.Any()
                    ? $"Importación completada: {resultado.RegistrosExitosos} categorías importadas"
                    : "No se importaron categorías";
                resultado.Errores = errores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ImportarCategoriasAsync");
                resultado.Message = $"Error interno: {ex.Message}";
                resultado.Success = false;
            }

            return resultado;
        }

        public async Task<ImportResultDto> ImportarProductosAsync(Stream fileStream)
        {
            var resultado = new ImportResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                var productos = new List<Producto>();
                var errores = new List<string>();
                var advertencias = new List<string>();

                var categorias = await _context.Categorias.ToListAsync();
                var categoriasDict = categorias.ToDictionary(c => c.Nombre.Trim().ToLower(), c => c.Id);

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var nombre = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var categoriaNombre = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var descripcion = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var descripcionCorta = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var precioStr = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        var precioOriginalStr = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        var stockStr = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                        var descuentoStr = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                        var imagenUrl = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                        var marca = worksheet.Cells[row, 10].Value?.ToString()?.Trim();
                        var sku = worksheet.Cells[row, 11].Value?.ToString()?.Trim();
                        var destacadoStr = worksheet.Cells[row, 12].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(nombre))
                        {
                            errores.Add($"Fila {row}: Nombre es requerido");
                            continue;
                        }

                        if (string.IsNullOrEmpty(categoriaNombre))
                        {
                            errores.Add($"Fila {row}: Categoría es requerida");
                            continue;
                        }

                        var categoriaKey = categoriaNombre.ToLower();
                        if (!categoriasDict.ContainsKey(categoriaKey))
                        {
                            errores.Add($"Fila {row}: Categoría '{categoriaNombre}' no existe");
                            continue;
                        }

                        var categoriaId = categoriasDict[categoriaKey];

                        if (!decimal.TryParse(precioStr, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal precio))
                        {
                            errores.Add($"Fila {row}: Precio inválido");
                            continue;
                        }

                        decimal precioOriginal = precio;
                        if (!string.IsNullOrEmpty(precioOriginalStr))
                        {
                            decimal.TryParse(precioOriginalStr, NumberStyles.Currency, CultureInfo.InvariantCulture, out precioOriginal);
                        }

                        if (!int.TryParse(stockStr, out int stock))
                        {
                            stock = 0;
                            advertencias.Add($"Fila {row}: Stock inválido, se usará 0");
                        }

                        if (!decimal.TryParse(descuentoStr, out decimal descuento))
                        {
                            descuento = 0;
                        }

                        bool destacado = destacadoStr?.ToLower() == "si" || destacadoStr?.ToLower() == "true";

                        var producto = new Producto
                        {
                            Nombre = nombre,
                            CategoriaId = categoriaId,
                            Descripcion = descripcion ?? string.Empty,
                            DescripcionCorta = descripcionCorta ?? string.Empty,
                            Precio = precio,
                            PrecioOriginal = precioOriginal,
                            StockTotal = stock,
                            StockDisponible = stock,
                            StockReservado = 0,
                            StockVendido = 0,
                            Descuento = descuento,
                            ImagenUrl = imagenUrl ?? string.Empty,
                            Marca = marca ?? string.Empty,
                            SKU = sku ?? $"PROD-{DateTime.Now:yyyyMMdd}-{row}",
                            Activo = true,
                            Destacado = destacado,
                            EnOferta = descuento > 0,
                            FechaCreacion = DateTime.UtcNow
                        };

                        productos.Add(producto);
                        resultado.RegistrosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {row}: Error - {ex.Message}");
                        resultado.RegistrosFallidos++;
                    }
                }

                resultado.TotalRegistros = worksheet.Dimension.End.Row - 1;

                if (productos.Any())
                {
                    _context.Productos.AddRange(productos);
                    await _context.SaveChangesAsync();
                }

                resultado.Success = productos.Any();
                resultado.Message = productos.Any()
                    ? $"Importación completada: {resultado.RegistrosExitosos} productos importados"
                    : "No se importaron productos";
                resultado.Errores = errores;
                resultado.Advertencias = advertencias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ImportarProductosAsync");
                resultado.Message = $"Error interno: {ex.Message}";
                resultado.Success = false;
            }

            return resultado;
        }

        public async Task<ImportResultDto> ImportarAnimalesAsync(Stream fileStream)
        {
            var resultado = new ImportResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                var animales = new List<Animal>();
                var errores = new List<string>();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var nombre = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var especie = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var raza = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var edadStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var sexo = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        var precioStr = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        var descripcion = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                        var vacunadoStr = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                        var esterilizadoStr = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                        var fechaNacimientoStr = worksheet.Cells[row, 10].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(especie))
                        {
                            errores.Add($"Fila {row}: Nombre y especie son requeridos");
                            continue;
                        }

                        if (!int.TryParse(edadStr, out int edad))
                        {
                            errores.Add($"Fila {row}: Edad inválida");
                            continue;
                        }

                        if (!decimal.TryParse(precioStr, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal precio))
                        {
                            errores.Add($"Fila {row}: Precio inválido");
                            continue;
                        }

                        DateTime fechaNacimiento;
                        if (!DateTime.TryParse(fechaNacimientoStr, out fechaNacimiento))
                        {
                            fechaNacimiento = DateTime.UtcNow.AddMonths(-edad);
                        }

                        bool vacunado = vacunadoStr?.ToLower() == "si" || vacunadoStr?.ToLower() == "true";
                        bool esterilizado = esterilizadoStr?.ToLower() == "si" || esterilizadoStr?.ToLower() == "true";

                        var animal = new Animal
                        {
                            Nombre = nombre,
                            Especie = especie,
                            Raza = raza ?? string.Empty,
                            Edad = edad,
                            Sexo = sexo ?? "Macho",
                            Precio = precio,
                            Descripcion = descripcion ?? string.Empty,
                            Disponible = true,
                            Reservado = false,
                            Vacunado = vacunado,
                            Esterilizado = esterilizado,
                            FechaNacimiento = fechaNacimiento,
                            FechaCreacion = DateTime.UtcNow
                        };

                        animales.Add(animal);
                        resultado.RegistrosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {row}: Error - {ex.Message}");
                        resultado.RegistrosFallidos++;
                    }
                }

                resultado.TotalRegistros = worksheet.Dimension.End.Row - 1;

                if (animales.Any())
                {
                    _context.Animales.AddRange(animales);
                    await _context.SaveChangesAsync();
                }

                resultado.Success = animales.Any();
                resultado.Message = animales.Any()
                    ? $"Importación completada: {resultado.RegistrosExitosos} animales importados"
                    : "No se importaron animales";
                resultado.Errores = errores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ImportarAnimalesAsync");
                resultado.Message = $"Error interno: {ex.Message}";
                resultado.Success = false;
            }

            return resultado;
        }

        public byte[] GenerarPlantilla(string tipo)
        {
            using var package = new ExcelPackage();

            switch (tipo.ToLower())
            {
                case "categorias":
                    var wsCategorias = package.Workbook.Worksheets.Add("Categorias");
                    wsCategorias.Cells[1, 1].Value = "Nombre*";
                    wsCategorias.Cells[1, 2].Value = "Descripción";
                    wsCategorias.Cells[1, 3].Value = "ImagenUrl";
                    wsCategorias.Cells[1, 4].Value = "Orden";
                    break;

                case "productos":
                    var wsProductos = package.Workbook.Worksheets.Add("Productos");
                    wsProductos.Cells[1, 1].Value = "Nombre*";
                    wsProductos.Cells[1, 2].Value = "Categoría*";
                    wsProductos.Cells[1, 3].Value = "Descripción";
                    wsProductos.Cells[1, 4].Value = "Descripción Corta";
                    wsProductos.Cells[1, 5].Value = "Precio*";
                    wsProductos.Cells[1, 6].Value = "Precio Original";
                    wsProductos.Cells[1, 7].Value = "Stock";
                    wsProductos.Cells[1, 8].Value = "Descuento";
                    wsProductos.Cells[1, 9].Value = "ImagenUrl";
                    wsProductos.Cells[1, 10].Value = "Marca";
                    wsProductos.Cells[1, 11].Value = "SKU";
                    wsProductos.Cells[1, 12].Value = "Destacado (Si/No)";
                    break;

                case "animales":
                    var wsAnimales = package.Workbook.Worksheets.Add("Animales");
                    wsAnimales.Cells[1, 1].Value = "Nombre*";
                    wsAnimales.Cells[1, 2].Value = "Especie*";
                    wsAnimales.Cells[1, 3].Value = "Raza";
                    wsAnimales.Cells[1, 4].Value = "Edad (meses)*";
                    wsAnimales.Cells[1, 5].Value = "Sexo";
                    wsAnimales.Cells[1, 6].Value = "Precio*";
                    wsAnimales.Cells[1, 7].Value = "Descripción";
                    wsAnimales.Cells[1, 8].Value = "Vacunado (Si/No)";
                    wsAnimales.Cells[1, 9].Value = "Esterilizado (Si/No)";
                    wsAnimales.Cells[1, 10].Value = "Fecha Nacimiento";
                    break;

                default:
                    throw new ArgumentException("Tipo de plantilla no válido");
            }

            return package.GetAsByteArray();
        }
        public async Task<string> DetectarTipoAsync(Stream fileStream)
        {
            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet?.Dimension == null) return "desconocido";

                // Leer primera fila para detectar columnas
                var columnas = new List<string>();
                for (int col = 1; col <= Math.Min(10, worksheet.Dimension.End.Column); col++)
                {
                    var valor = worksheet.Cells[1, col].Value?.ToString()?.ToLower() ?? "";
                    columnas.Add(valor);
                }

                // Detectar por palabras clave en columnas
                var columnasStr = string.Join(" ", columnas);

                if (columnasStr.Contains("especie") || columnasStr.Contains("raza") || columnasStr.Contains("edad") || columnasStr.Contains("vacunado"))
                    return "animales";

                if (columnasStr.Contains("categoría") || columnasStr.Contains("categoria") || columnasStr.Contains("precio") || columnasStr.Contains("stock"))
                    return "productos";

                if (columnasStr.Contains("nombre") && columnas.Count <= 4)
                    return "categorias";

                return "productos"; // Por defecto
            }
            catch
            {
                return "desconocido";
            }
        }

        // ✅ NUEVO: Importación flexible de productos
        public async Task<ImportResultDto> ImportarProductosFlexibleAsync(Stream fileStream)
        {
            var resultado = new ImportResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                var productos = new List<Producto>();
                var errores = new List<string>();
                var categoriasCreadas = new List<string>();

                // Leer encabezados para mapeo dinámico
                var encabezados = new Dictionary<string, int>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.Trim().ToLower() ?? $"col{col}";
                    encabezados[header] = col;
                }

                // ✅ MAPEO EXTENDIDO CORREGIDO (sin duplicados)
                var mapeo = new Dictionary<string, string>
{
    { "nombre", "nombre" },
    { "producto", "nombre" },
    { "nombre producto", "nombre" },
    { "nombre del producto", "nombre" },
    { "descripcion", "descripcion" },
    { "descripción", "descripcion" }, 
    { "categoria", "categoria" },
    { "categoría", "categoria" },
    { "precio", "precio" },
    { "stock", "stock" },
    { "imagen", "imagenurl" },
    { "url", "imagenurl" },
    { "marca", "marca" },
    { "sku", "sku" }
    
};

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var producto = new Producto
                        {
                            Activo = true,
                            FechaCreacion = DateTime.UtcNow
                        };

                        // Buscar valores dinámicamente
                        foreach (var header in encabezados)
                        {
                            var valor = worksheet.Cells[row, header.Value].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(valor)) continue;

                            var headerLower = header.Key.ToLower();

                            // Asignar valores según mapeo
                            if (mapeo.ContainsKey(headerLower))
                            {
                                var propiedad = mapeo[headerLower];

                                switch (propiedad)
                                {
                                    case "nombre":
                                        producto.Nombre = valor;
                                        break;
                                    case "descripcion":
                                        producto.Descripcion = valor;
                                        break;
                                    case "categoria":
                                        // ✅ CREAR CATEGORÍA AUTOMÁTICAMENTE SI NO EXISTE
                                        if (!string.IsNullOrEmpty(valor))
                                        {
                                            // Primero verificar si ya existe
                                            var categoriaExistente = await _context.Categorias
                                                .FirstOrDefaultAsync(c => c.Nombre.ToLower() == valor.ToLower());

                                            if (categoriaExistente != null)
                                            {
                                                producto.CategoriaId = categoriaExistente.Id;
                                            }
                                            else
                                            {
                                                // Crear nueva categoría
                                                var nuevaCategoria = new Categoria
                                                {
                                                    Nombre = valor.Trim(),
                                                    Descripcion = $"Categoría {valor}",
                                                    Activo = true,
                                                    Orden = await _context.Categorias.CountAsync() + 1,
                                                    FechaCreacion = DateTime.UtcNow
                                                };

                                                _context.Categorias.Add(nuevaCategoria);
                                                await _context.SaveChangesAsync(); // Guardar para obtener el ID

                                                producto.CategoriaId = nuevaCategoria.Id;
                                                categoriasCreadas.Add(valor);

                                                _logger.LogInformation($"Categoría creada automáticamente: {valor}");
                                            }
                                        }
                                        break;
                                    case "precio":
                                        if (decimal.TryParse(valor, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal precio))
                                            producto.Precio = precio;
                                        break;
                                    case "stock":
                                        if (int.TryParse(valor, out int stock))
                                        {
                                            producto.StockTotal = stock;
                                            producto.StockDisponible = stock;
                                        }
                                        break;
                                    case "imagenurl":
                                        producto.ImagenUrl = valor;
                                        break;
                                    case "marca":
                                        producto.Marca = valor;
                                        break;
                                    case "sku":
                                        producto.SKU = valor;
                                        break;
                                }
                            }
                        }

                        // Validaciones mínimas
                        if (string.IsNullOrEmpty(producto.Nombre))
                        {
                            errores.Add($"Fila {row}: Nombre es requerido");
                            continue;
                        }

                        if (producto.CategoriaId == 0)
                        {
                            // Si no tiene categoría, usar una por defecto
                            var categoriaDefault = await _context.Categorias.FirstOrDefaultAsync();
                            if (categoriaDefault != null)
                            {
                                producto.CategoriaId = categoriaDefault.Id;
                            }
                            else
                            {
                                // Crear categoría por defecto si no hay ninguna
                                var categoriaDefaultNueva = new Categoria
                                {
                                    Nombre = "General",
                                    Descripcion = "Categoría general",
                                    Activo = true,
                                    Orden = 1,
                                    FechaCreacion = DateTime.UtcNow
                                };
                                _context.Categorias.Add(categoriaDefaultNueva);
                                await _context.SaveChangesAsync();
                                producto.CategoriaId = categoriaDefaultNueva.Id;
                            }
                        }

                        // Generar SKU si no viene
                        if (string.IsNullOrEmpty(producto.SKU))
                            producto.SKU = $"PROD-{DateTime.Now:yyyyMMdd}-{row}";

                        productos.Add(producto);
                        resultado.RegistrosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {row}: {ex.Message}");
                        resultado.RegistrosFallidos++;
                    }
                }

                if (productos.Any())
                {
                    _context.Productos.AddRange(productos);
                    await _context.SaveChangesAsync();
                }

                resultado.Success = productos.Any();

                // Mensaje informativo con categorías creadas
                var mensaje = productos.Any()
                    ? $"Importados {resultado.RegistrosExitosos} productos"
                    : "No se importaron productos";

                if (categoriasCreadas.Any())
                {
                    mensaje += $". Se crearon {categoriasCreadas.Count} categorías nuevas: {string.Join(", ", categoriasCreadas.Distinct())}";
                }

                resultado.Message = mensaje;
                resultado.Errores = errores;

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ImportarProductosFlexibleAsync");
                resultado.Message = $"Error: {ex.Message}";
                resultado.Success = false;
                return resultado;
            }
        }

        // ✅ NUEVO: Preview del archivo
        public async Task<PreviewResultDto> ObtenerPreviewAsync(Stream fileStream)
        {
            var preview = new PreviewResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet?.Dimension == null)
                {
                    preview.Message = "Archivo vacío";
                    return preview;
                }

                // Detectar tipo
                preview.TipoDetectado = await DetectarTipoAsync(fileStream);

                // Obtener columnas
                preview.Columnas = new List<string>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var columna = worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? $"Columna {col}";
                    preview.Columnas.Add(columna);
                }

                // Obtener primeras filas de datos
                preview.PreviewDatos = new List<List<string>>();
                for (int row = 2; row <= Math.Min(6, worksheet.Dimension.End.Row); row++)
                {
                    var fila = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        fila.Add(worksheet.Cells[row, col].Value?.ToString()?.Trim() ?? "");
                    }
                    preview.PreviewDatos.Add(fila);
                }

                preview.TotalFilas = worksheet.Dimension.End.Row - 1;
                preview.TotalColumnas = worksheet.Dimension.End.Column;
                preview.Success = true;

                return preview;
            }
            catch (Exception ex)
            {
                preview.Message = $"Error: {ex.Message}";
                return preview;
            }
        }
        // Agrega estos métodos a tu ImportService existente

        public async Task<ImportResultDto> ImportarAnimalesFlexibleAsync(Stream fileStream)
        {
            var resultado = new ImportResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                var animales = new List<Animal>();
                var errores = new List<string>();

                // Leer encabezados para mapeo dinámico
                var encabezados = new Dictionary<string, int>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.Trim().ToLower() ?? $"col{col}";
                    encabezados[header] = col;
                }

                // Mapeo flexible de columnas para animales
                var mapeo = new Dictionary<string, string>
        {
            { "nombre", "nombre" },
            { "especie", "especie" },
            { "raza", "raza" },
            { "edad", "edad" },
            { "sexo", "sexo" },
            { "precio", "precio" },
            { "descripcion", "descripcion" },
            { "descripción", "descripcion" },
            { "vacunado", "vacunado" },
            { "esterilizado", "esterilizado" },
            { "fecha nacimiento", "fechanacimiento" },
            { "fechanacimiento", "fechanacimiento" }
        };

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var animal = new Animal
                        {
                            Disponible = true,
                            FechaCreacion = DateTime.UtcNow
                        };

                        // Buscar valores dinámicamente
                        foreach (var header in encabezados)
                        {
                            var valor = worksheet.Cells[row, header.Value].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(valor)) continue;

                            var headerLower = header.Key.ToLower();

                            if (mapeo.ContainsKey(headerLower))
                            {
                                var propiedad = mapeo[headerLower];

                                switch (propiedad)
                                {
                                    case "nombre":
                                        animal.Nombre = valor;
                                        break;
                                    case "especie":
                                        animal.Especie = valor;
                                        break;
                                    case "raza":
                                        animal.Raza = valor;
                                        break;
                                    case "edad":
                                        if (int.TryParse(valor, out int edad))
                                            animal.Edad = edad;
                                        break;
                                    case "sexo":
                                        animal.Sexo = valor;
                                        break;
                                    case "precio":
                                        if (decimal.TryParse(valor, out decimal precio))
                                            animal.Precio = precio;
                                        break;
                                    case "descripcion":
                                        animal.Descripcion = valor;
                                        break;
                                    case "vacunado":
                                        animal.Vacunado = valor.ToLower() == "si" || valor.ToLower() == "true";
                                        break;
                                    case "esterilizado":
                                        animal.Esterilizado = valor.ToLower() == "si" || valor.ToLower() == "true";
                                        break;
                                    case "fechanacimiento":
                                        if (DateTime.TryParse(valor, out DateTime fecha))
                                            animal.FechaNacimiento = fecha;
                                        break;
                                }
                            }
                        }

                        // Validaciones mínimas
                        if (string.IsNullOrEmpty(animal.Nombre))
                        {
                            errores.Add($"Fila {row}: Nombre es requerido");
                            continue;
                        }

                        if (string.IsNullOrEmpty(animal.Especie))
                        {
                            errores.Add($"Fila {row}: Especie es requerida");
                            continue;
                        }

                        // Si no tiene fecha de nacimiento, calcular desde edad
                        if (animal.FechaNacimiento == DateTime.MinValue && animal.Edad > 0)
                        {
                            animal.FechaNacimiento = DateTime.UtcNow.AddMonths(-animal.Edad);
                        }

                        animales.Add(animal);
                        resultado.RegistrosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {row}: {ex.Message}");
                        resultado.RegistrosFallidos++;
                    }
                }

                if (animales.Any())
                {
                    _context.Animales.AddRange(animales);
                    await _context.SaveChangesAsync();
                }

                resultado.Success = animales.Any();
                resultado.Message = animales.Any()
                    ? $"Importados {resultado.RegistrosExitosos} animales"
                    : "No se importaron animales";
                resultado.Errores = errores;

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ImportarAnimalesFlexibleAsync");
                resultado.Message = $"Error: {ex.Message}";
                resultado.Success = false;
                return resultado;
            }
        }

        public async Task<ImportResultDto> ImportarCategoriasFlexibleAsync(Stream fileStream)
        {
            var resultado = new ImportResultDto();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                var categorias = new List<Categoria>();
                var errores = new List<string>();

                // Leer encabezados para mapeo dinámico
                var encabezados = new Dictionary<string, int>();
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.Trim().ToLower() ?? $"col{col}";
                    encabezados[header] = col;
                }

                // Mapeo flexible de columnas para categorías
                var mapeo = new Dictionary<string, string>
        {
            { "nombre", "nombre" },
            { "categoria", "nombre" },
            { "descripcion", "descripcion" },
            { "descripción", "descripcion" },
            { "imagen", "imagenurl" },
            { "url", "imagenurl" },
            { "orden", "orden" }
        };

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    try
                    {
                        var categoria = new Categoria
                        {
                            Activo = true,
                            FechaCreacion = DateTime.UtcNow
                        };

                        // Buscar valores dinámicamente
                        foreach (var header in encabezados)
                        {
                            var valor = worksheet.Cells[row, header.Value].Value?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(valor)) continue;

                            var headerLower = header.Key.ToLower();

                            if (mapeo.ContainsKey(headerLower))
                            {
                                var propiedad = mapeo[headerLower];

                                switch (propiedad)
                                {
                                    case "nombre":
                                        categoria.Nombre = valor;
                                        break;
                                    case "descripcion":
                                        categoria.Descripcion = valor;
                                        break;
                                    case "imagenurl":
                                        categoria.ImagenUrl = valor;
                                        break;
                                    case "orden":
                                        if (int.TryParse(valor, out int orden))
                                            categoria.Orden = orden;
                                        break;
                                }
                            }
                        }

                        // Validaciones mínimas
                        if (string.IsNullOrEmpty(categoria.Nombre))
                        {
                            errores.Add($"Fila {row}: Nombre es requerido");
                            continue;
                        }

                        // Verificar si ya existe
                        if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
                        {
                            errores.Add($"Fila {row}: La categoría '{categoria.Nombre}' ya existe");
                            continue;
                        }

                        categorias.Add(categoria);
                        resultado.RegistrosExitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {row}: {ex.Message}");
                        resultado.RegistrosFallidos++;
                    }
                }

                if (categorias.Any())
                {
                    _context.Categorias.AddRange(categorias);
                    await _context.SaveChangesAsync();
                }

                resultado.Success = categorias.Any();
                resultado.Message = categorias.Any()
                    ? $"Importadas {resultado.RegistrosExitosos} categorías"
                    : "No se importaron categorías";
                resultado.Errores = errores;

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ImportarCategoriasFlexibleAsync");
                resultado.Message = $"Error: {ex.Message}";
                resultado.Success = false;
                return resultado;
            }
        }

        public List<string> ObtenerColumnasExcel(Stream fileStream, bool tieneEncabezados = true)
        {
            var columnas = new List<string>();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet?.Dimension == null) return columnas;

                var fila = tieneEncabezados ? 1 : 0;

                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var nombreColumna = tieneEncabezados
                        ? worksheet.Cells[fila, col].Value?.ToString()?.Trim()
                        : $"Columna {col}";

                    if (!string.IsNullOrEmpty(nombreColumna))
                    {
                        columnas.Add(nombreColumna);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo columnas del Excel");
            }

            return columnas;
        }
    }
}