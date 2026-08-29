using FincaNova.Web.Domain;

namespace FincaNova.Web.Services.Auditoria;

/// <summary>
/// Escribe entradas en la bitácora de auditoría inmodificable (RQNF-013, HU-18).
/// </summary>
public interface IAuditService
{
    Task RegistrarAsync(
        AccionAuditoria accion,
        string entidad,
        string? entidadId = null,
        string? descripcion = null,
        object? valorAnterior = null,
        object? valorNuevo = null,
        CancellationToken cancellationToken = default);
}
