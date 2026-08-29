using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Domain.Auditoria;

/// <summary>
/// Bitácora de auditoría inmodificable. Registra las operaciones críticas:
/// inicio/cierre de sesión y creación, modificación, eliminación o cambio de
/// estado de la información relevante (RQNF-013, RQNF-015, HU-18).
/// El sistema no expone edición ni borrado sobre esta tabla.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    [StringLength(450)]
    public string? UsuarioId { get; set; }

    [StringLength(256)]
    public string? Usuario { get; set; }

    public AccionAuditoria Accion { get; set; }

    [StringLength(100)]
    public string Entidad { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EntidadId { get; set; }

    [StringLength(45)]
    public string? DireccionIp { get; set; }

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    [StringLength(400)]
    public string? Descripcion { get; set; }
}
