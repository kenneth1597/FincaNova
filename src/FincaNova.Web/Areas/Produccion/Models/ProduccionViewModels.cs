using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;

namespace FincaNova.Web.Areas.Produccion.Models;

/// <summary>
/// Resumen de producción y merma de un lote en un período: compara el peso del
/// café recolectado (estimado desde cajuelas), el café seco y el café procesado
/// (HU-14, HU-15, RF-21, RF-22).
/// </summary>
public class ProduccionResumenViewModel
{
    public int? LoteId { get; set; }
    public int? PeriodoId { get; set; }
    public string? LoteNombre { get; set; }
    public string? PeriodoNombre { get; set; }
    public bool Calculado { get; set; }

    public decimal Cajuelas { get; set; }
    public decimal KgRecolectadoEstimado { get; set; }
    public decimal KgCafeSeco { get; set; }
    public decimal KgCafeProcesado { get; set; }

    public decimal MermaSecadoKg => KgRecolectadoEstimado - KgCafeSeco;
    public decimal MermaSecadoPct => KgRecolectadoEstimado > 0 ? decimal.Round(MermaSecadoKg / KgRecolectadoEstimado * 100, 1) : 0;
    public decimal MermaProcesoKg => KgCafeSeco - KgCafeProcesado;
    public decimal MermaProcesoPct => KgCafeSeco > 0 ? decimal.Round(MermaProcesoKg / KgCafeSeco * 100, 1) : 0;
    public decimal RendimientoGlobalPct => KgRecolectadoEstimado > 0 ? decimal.Round(KgCafeProcesado / KgRecolectadoEstimado * 100, 1) : 0;

    public record LineaEtapa(int Id, EtapaProduccion Etapa, DateTime Fecha, decimal PesoKg, string? Observaciones);
    public IReadOnlyList<LineaEtapa> Registros { get; set; } = Array.Empty<LineaEtapa>();

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Formulario para registrar el peso de una etapa del beneficiado (HU-14, HU-15).</summary>
public class RegistroProduccionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar el lote.")]
    [Display(Name = "Lote / micro lote")]
    public int? LoteId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar el período productivo.")]
    [Display(Name = "Período productivo")]
    public int? PeriodoProductivoId { get; set; }

    [Display(Name = "Etapa")]
    public EtapaProduccion Etapa { get; set; } = EtapaProduccion.CafeSeco;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Range(0.01, 10_000_000, ErrorMessage = "El peso debe ser mayor que cero.")]
    [Display(Name = "Peso (kg)")]
    public decimal PesoKg { get; set; }

    [StringLength(400)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public bool EsEdicion => Id != 0;
    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Comparativa de rendimiento por lote a lo largo de los períodos (HU-16).</summary>
public class RendimientoViewModel
{
    public IReadOnlyList<string> Periodos { get; set; } = Array.Empty<string>();
    public IReadOnlyList<FilaLote> Lotes { get; set; } = Array.Empty<FilaLote>();
    public decimal MaximoKg { get; set; }
    public bool HayDatos => Lotes.Any(l => l.Valores.Any(v => v > 0));

    /// <summary>Una fila = un lote; Valores alineados con <see cref="Periodos"/> (kg de café procesado).</summary>
    public record FilaLote(string Lote, IReadOnlyList<decimal> Valores);
}
