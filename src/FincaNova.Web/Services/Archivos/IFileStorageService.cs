namespace FincaNova.Web.Services.Archivos;

/// <summary>
/// Almacena comprobantes (facturas de gastos, etc.) fuera de wwwroot, para que
/// solo sean accesibles mediante una acción autorizada (HU-19, HU-52).
/// </summary>
public interface IFileStorageService
{
    /// <summary>Extensiones permitidas para comprobantes.</summary>
    IReadOnlySet<string> ExtensionesPermitidas { get; }

    /// <summary>Tamaño máximo permitido, en MB.</summary>
    int TamanoMaximoMb { get; }

    /// <summary>
    /// Valida y guarda el archivo. Devuelve el nombre almacenado (no la ruta completa)
    /// o lanza <see cref="InvalidOperationException"/> si no cumple las restricciones.
    /// </summary>
    Task<string> GuardarComprobanteAsync(IFormFile archivo, CancellationToken ct = default);

    /// <summary>Ruta física absoluta de un comprobante almacenado, o null si no existe.</summary>
    string? RutaFisica(string? nombreAlmacenado);

    void Eliminar(string? nombreAlmacenado);
}
