using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Labores;
using FincaNova.Web.Domain.Lotes;

namespace FincaNova.Web.Domain.Produccion;

/// <summary>
/// Recolección diaria de café por colaborador, medida en cajuelas. El peso
/// estimado en kilogramos se calcula con la regla de conversión de la finca
/// (RF-20..21, HU-11..13).
/// </summary>
public class Recoleccion : AuditableEntity
{
    public int LoteId { get; set; }

    public Lote Lote { get; set; } = null!;

    public int PeriodoProductivoId { get; set; }

    public PeriodoProductivo PeriodoProductivo { get; set; } = null!;

    public int ColaboradorId { get; set; }

    public Colaborador Colaborador { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [Range(0.0, 100000)]
    public decimal Cajuelas { get; set; }

    /// <summary>Peso estimado en kg = <see cref="Cajuelas"/> × KilogramosPorCajuela.</summary>
    [Range(0, 10_000_000)]
    public decimal PesoEstimadoKg { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }
}

/// <summary>
/// Registro de peso del café al cerrar una etapa del beneficiado (café seco o
/// café procesado sin cáscara). Permite comparar merma entre etapas (RF-22, HU-14, HU-15).
/// </summary>
public class RegistroProduccion : AuditableEntity
{
    public int LoteId { get; set; }

    public Lote Lote { get; set; } = null!;

    public int PeriodoProductivoId { get; set; }

    public PeriodoProductivo PeriodoProductivo { get; set; } = null!;

    public EtapaProduccion Etapa { get; set; }

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [Range(0, 10_000_000)]
    public decimal PesoKg { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }
}
