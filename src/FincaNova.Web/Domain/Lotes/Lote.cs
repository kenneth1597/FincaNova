using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;

namespace FincaNova.Web.Domain.Lotes;

/// <summary>
/// Lote o micro lote de café. Es la unidad central del sistema: todas las labores,
/// enfermedades, recolecciones y gastos se asocian a un lote (RF-06..10, HU-30..41).
/// Un micro lote referencia a su lote padre mediante <see cref="LotePadreId"/>.
/// </summary>
public class Lote : AuditableEntity
{
    public int FincaId { get; set; }

    public Finca Finca { get; set; } = null!;

    /// <summary>Código identificador único dentro de la finca (HU-30 esc. 3, HU-36 esc. 2).</summary>
    [Required, StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    public TipoLote Tipo { get; set; } = TipoLote.Lote;

    public int? LotePadreId { get; set; }

    public Lote? LotePadre { get; set; }

    public ICollection<Lote> MicroLotes { get; set; } = new List<Lote>();

    [Range(0, 100000)]
    public decimal AreaHectareas { get; set; }

    [StringLength(100)]
    public string? VariedadCafe { get; set; }

    [Range(1900, 2100)]
    public int? AnioSiembra { get; set; }

    [StringLength(200)]
    public string? Ubicacion { get; set; }

    public EstadoLote Estado { get; set; } = EstadoLote.Activo;

    public DateTime? FechaUltimoCambioEstado { get; set; }

    /// <summary>Usuario que realizó el último cambio de estado (HU-39 esc. 2).</summary>
    [StringLength(256)]
    public string? EstadoCambiadoPor { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    /// <summary>Borrado lógico: los lotes con historial no se eliminan (HU-35, HU-38).</summary>
    public bool Eliminado { get; set; }
}
