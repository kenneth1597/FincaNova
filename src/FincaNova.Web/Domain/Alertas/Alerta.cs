using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;

namespace FincaNova.Web.Domain.Alertas;

/// <summary>
/// Alerta generada automáticamente por el sistema: enfermedad recurrente en un
/// lote (HU-10, RF-19) o insumo en stock mínimo (HU-25, RF-26).
/// </summary>
public class Alerta : AuditableEntity
{
    public TipoAlerta Tipo { get; set; }

    [Required, StringLength(300)]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Id de la entidad relacionada (lote o insumo) según el tipo.</summary>
    public int? ReferenciaId { get; set; }

    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    public bool Atendida { get; set; }

    public DateTime? FechaAtencion { get; set; }

    [StringLength(150)]
    public string? AtendidaPor { get; set; }
}
