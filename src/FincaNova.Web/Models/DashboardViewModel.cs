namespace FincaNova.Web.Models;

/// <summary>Resumen mostrado en el panel principal.</summary>
public class DashboardViewModel
{
    public string Finca { get; set; } = string.Empty;
    public int TotalLotes { get; set; }
    public int LotesActivos { get; set; }
    public string? PeriodoActivo { get; set; }
    public int AlertasPendientes { get; set; }
}
