namespace FincaNova.Web.Services.Archivos;

/// <inheritdoc />
public class LocalFileStorageService : IFileStorageService
{
    private static readonly Dictionary<string, string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private readonly string _carpeta;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _carpeta = Path.Combine(env.ContentRootPath, "App_Data", "comprobantes");
        Directory.CreateDirectory(_carpeta);
    }

    public IReadOnlySet<string> ExtensionesPermitidas => TiposPermitidos.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

    public int TamanoMaximoMb => 5;

    public async Task<string> GuardarComprobanteAsync(IFormFile archivo, CancellationToken ct = default)
    {
        if (archivo is null || archivo.Length == 0)
            throw new InvalidOperationException("El archivo está vacío.");

        var ext = Path.GetExtension(archivo.FileName);
        if (!TiposPermitidos.ContainsKey(ext))
            throw new InvalidOperationException("Formato no válido. Solo se permiten archivos PDF, JPG o PNG.");

        if (archivo.Length > TamanoMaximoMb * 1024L * 1024L)
            throw new InvalidOperationException($"El archivo supera el tamaño máximo permitido de {TamanoMaximoMb} MB.");

        var nombre = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var destino = Path.Combine(_carpeta, nombre);
        await using var fs = new FileStream(destino, FileMode.CreateNew);
        await archivo.CopyToAsync(fs, ct);
        return nombre;
    }

    public string? RutaFisica(string? nombreAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(nombreAlmacenado)) return null;
        var ruta = Path.Combine(_carpeta, Path.GetFileName(nombreAlmacenado));
        return File.Exists(ruta) ? ruta : null;
    }

    public void Eliminar(string? nombreAlmacenado)
    {
        var ruta = RutaFisica(nombreAlmacenado);
        if (ruta is not null) File.Delete(ruta);
    }

    public static string ContentType(string nombreAlmacenado)
        => TiposPermitidos.TryGetValue(Path.GetExtension(nombreAlmacenado), out var ct) ? ct : "application/octet-stream";
}
