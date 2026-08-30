namespace FincaNova.Web.Areas.Labores.Models;

/// <summary>
/// Cálculo automático del pago de un colaborador en un período productivo,
/// sumando jornadas/horas de labores y cajuelas recolectadas (HU-06, RF-15).
/// </summary>
public class PlanillaViewModel
{
    public int? ColaboradorId { get; set; }
    public int? PeriodoId { get; set; }

    public string? ColaboradorNombre { get; set; }
    public string? PeriodoNombre { get; set; }

    public record LineaLabor(DateTime Fecha, string Lote, string TipoLabor, string Modalidad, decimal Cantidad, decimal Costo);
    public record LineaRecoleccion(DateTime Fecha, string Lote, decimal Cajuelas, decimal Tarifa, decimal Monto);

    public IReadOnlyList<LineaLabor> Labores { get; set; } = Array.Empty<LineaLabor>();
    public IReadOnlyList<LineaRecoleccion> Recolecciones { get; set; } = Array.Empty<LineaRecoleccion>();

    public decimal SubtotalLabores => Labores.Sum(l => l.Costo);
    public decimal SubtotalRecoleccion => Recolecciones.Sum(r => r.Monto);
    public decimal Total => SubtotalLabores + SubtotalRecoleccion;

    public bool Calculada { get; set; }

    public IEnumerable<(int Id, string Texto)> ColaboradoresDisponibles { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> PeriodosDisponibles { get; set; } = Enumerable.Empty<(int, string)>();
}
