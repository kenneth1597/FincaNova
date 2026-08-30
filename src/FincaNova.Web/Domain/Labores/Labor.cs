using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Lotes;

namespace FincaNova.Web.Domain.Labores;

/// <summary>
/// Labor agrícola realizada en un lote (poda, fertilización, limpia, atomización…).
/// Alimenta la bitácora del lote y el costo operativo (RF-11..15, HU-01..03).
/// </summary>
public class Labor : AuditableEntity
{
    public int LoteId { get; set; }

    public Lote Lote { get; set; } = null!;

    public int? PeriodoProductivoId { get; set; }

    public PeriodoProductivo? PeriodoProductivo { get; set; }

    [Required, StringLength(120)]
    public string TipoLabor { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    public ModalidadPago Modalidad { get; set; } = ModalidadPago.PorJornada;

    /// <summary>Duración en horas (modalidad por hora). Debe ser mayor que cero (HU-02 esc. 3).</summary>
    [Range(0, 24)]
    public decimal DuracionHoras { get; set; }

    /// <summary>Cantidad de jornadas trabajadas (modalidad por jornada).</summary>
    [Range(0, 100)]
    public decimal Jornadas { get; set; }

    [Range(0, 10_000_000)]
    public decimal CostoUnitario { get; set; }

    /// <summary>Costo total de la labor calculado por el sistema (RF-15).</summary>
    [Range(0, 1_000_000_000)]
    public decimal CostoCalculado { get; set; }

    [StringLength(600)]
    public string? Observaciones { get; set; }

    public ICollection<LaborColaborador> Colaboradores { get; set; } = new List<LaborColaborador>();
}

/// <summary>
/// Relación N:N entre una labor y los colaboradores que la ejecutaron (HU-03).
/// Guarda la cantidad (jornadas u horas) y el costo atribuido a cada colaborador,
/// para que la planilla sea una simple suma (HU-06).
/// </summary>
public class LaborColaborador
{
    public int LaborId { get; set; }

    public Labor Labor { get; set; } = null!;

    public int ColaboradorId { get; set; }

    public Colaborador Colaborador { get; set; } = null!;

    /// <summary>Jornadas u horas que aportó este colaborador a la labor.</summary>
    public decimal Cantidad { get; set; }

    /// <summary>Monto correspondiente a este colaborador por esta labor.</summary>
    public decimal Costo { get; set; }
}
