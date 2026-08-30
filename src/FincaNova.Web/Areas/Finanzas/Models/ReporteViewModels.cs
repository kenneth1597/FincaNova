using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Areas.Finanzas.Models;

/// <summary>Balance de ingresos y egresos por rango de fechas o período (HU-21, HU-29).</summary>
public class BalanceViewModel
{
    [DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [DataType(DataType.Date)] public DateTime? Hasta { get; set; }
    public int? PeriodoId { get; set; }

    public bool Calculado { get; set; }
    public string? PeriodoNombre { get; set; }

    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal BalanceNeto => TotalIngresos - TotalGastos;
    public bool SinTransacciones => TotalIngresos == 0 && TotalGastos == 0;

    public record GastoPorCategoria(string Categoria, decimal Monto);
    public IReadOnlyList<GastoPorCategoria> GastosPorCategoria { get; set; } = Array.Empty<GastoPorCategoria>();

    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Reporte de producción por lote y período: cajuelas, kg, merma, rendimiento (HU-28).</summary>
public class ReporteProduccionViewModel
{
    public int? PeriodoId { get; set; }
    public bool Calculado { get; set; }
    public string? PeriodoNombre { get; set; }

    public record Fila(string Lote, decimal Cajuelas, decimal KgRecolectado, decimal KgSeco, decimal KgProcesado,
        decimal MermaPct, decimal RendimientoPct);

    public IReadOnlyList<Fila> Filas { get; set; } = Array.Empty<Fila>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Costo real de producción por cajuela para un lote y período (HU-17).</summary>
public class CostoPorCajuelaViewModel
{
    public int? LoteId { get; set; }
    public int? PeriodoId { get; set; }
    public bool Calculado { get; set; }
    public string? LoteNombre { get; set; }
    public string? PeriodoNombre { get; set; }

    public decimal CostoManoDeObra { get; set; }
    public decimal CostoInsumos { get; set; }
    public decimal OtrosGastos { get; set; }
    public decimal GastoTotal => CostoManoDeObra + CostoInsumos + OtrosGastos;
    public decimal Cajuelas { get; set; }
    public bool ProduccionIniciada => Cajuelas > 0;
    public decimal CostoUnitario => ProduccionIniciada ? decimal.Round(GastoTotal / Cajuelas, 2) : 0m;

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}
