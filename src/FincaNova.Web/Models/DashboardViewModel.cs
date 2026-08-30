using FincaNova.Web.Domain;

namespace FincaNova.Web.Models;

/// <summary>Tablero de indicadores del panel principal (Hito 7).</summary>
public class DashboardViewModel
{
    public string Finca { get; set; } = string.Empty;

    public string? PeriodoActivo { get; set; }
    public DateTime? PeriodoInicio { get; set; }
    public DateTime? PeriodoFin { get; set; }
    public bool HayPeriodoActivo => PeriodoActivo is not null;

    // ----- Lotes -----
    public int TotalLotes { get; set; }
    public int LotesEnOperacion { get; set; }
    public Dictionary<EstadoLote, int> LotesPorEstado { get; set; } = new();

    // ----- Recolección y producción del período activo -----
    public decimal Cajuelas { get; set; }
    public decimal KgRecolectado { get; set; }
    public decimal KgSeco { get; set; }
    public decimal KgProcesado { get; set; }
    public decimal MermaPct => KgRecolectado > 0 ? decimal.Round((KgRecolectado - KgProcesado) / KgRecolectado * 100, 1) : 0;
    public decimal RendimientoPct => KgRecolectado > 0 ? decimal.Round(KgProcesado / KgRecolectado * 100, 1) : 0;

    public record TopRecolector(string Colaborador, decimal Cajuelas);
    public IReadOnlyList<TopRecolector> TopRecolectores { get; set; } = Array.Empty<TopRecolector>();

    // ----- Labores del período activo -----
    public int LaboresCount { get; set; }
    public decimal CostoManoObra { get; set; }

    // ----- Enfermedades -----
    public int EnfermedadesActivas { get; set; }

    // ----- Finanzas del período activo -----
    public decimal Ingresos { get; set; }
    public decimal Gastos { get; set; }
    public decimal BalanceNeto => Ingresos - Gastos;

    // ----- Alertas -----
    public int AlertasPendientes { get; set; }
    public record AlertaMini(TipoAlerta Tipo, string Mensaje, DateTime Fecha);
    public IReadOnlyList<AlertaMini> UltimasAlertas { get; set; } = Array.Empty<AlertaMini>();

    // ----- Costo por cajuela (lotes con producción en el período) -----
    public record CostoLote(string Lote, decimal Cajuelas, decimal CostoUnitario);
    public IReadOnlyList<CostoLote> CostoLotes { get; set; } = Array.Empty<CostoLote>();
}
