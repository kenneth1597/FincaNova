using FincaNova.Web.Areas.Finanzas.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Catalogos;
using FincaNova.Web.Services.Reportes;
using FincaNova.Web.ViewSupport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Finanzas.Controllers;

/// <summary>
/// Control general de ingresos y egresos: balance neto, gastos por categoría y
/// exportación a PDF/Excel (RF-25, RF-27, RF-28, HU-21, HU-26, HU-27, HU-29).
/// </summary>
[Area("Finanzas")]
[Authorize(Roles = Roles.Gestion)]
public class BalanceController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICatalogoService _catalogo;
    private readonly IReporteExportService _export;

    public BalanceController(AppDbContext db, ICatalogoService catalogo, IReporteExportService export)
    {
        _db = db;
        _catalogo = catalogo;
        _export = export;
    }

    [HttpGet]
    public async Task<IActionResult> Index(BalanceViewModel filtro)
    {
        filtro.Periodos = await _catalogo.PeriodosAsync();
        await CalcularAsync(filtro);
        return View(filtro);
    }

    [HttpGet]
    public async Task<IActionResult> Exportar(DateTime? desde, DateTime? hasta, int? periodoId, string formato = "pdf")
    {
        var vm = new BalanceViewModel { Desde = desde, Hasta = hasta, PeriodoId = periodoId };
        await CalcularAsync(vm);

        var filas = new List<IReadOnlyList<string>>
        {
            new[] { "Ingresos por ventas", Ui.Money(vm.TotalIngresos) },
            new[] { "Gastos totales", Ui.Money(vm.TotalGastos) }
        };
        filas.AddRange(vm.GastosPorCategoria.Select(g => (IReadOnlyList<string>)new[] { "  · " + g.Categoria, Ui.Money(g.Monto) }));

        var reporte = new ReporteTabla(
            "Balance de ingresos y egresos",
            vm.PeriodoNombre ?? Rango(vm.Desde, vm.Hasta),
            new[] { "Concepto", "Monto" },
            filas,
            new[] { "BALANCE NETO", Ui.Money(vm.BalanceNeto) });

        return Descargar(reporte, "balance", formato);
    }

    // ---------- helpers ----------
    private async Task CalcularAsync(BalanceViewModel vm)
    {
        var gastos = _db.Gastos.AsNoTracking().AsQueryable();
        var ventas = _db.VentasCafe.AsNoTracking().AsQueryable();

        if (vm.PeriodoId is { } pid)
        {
            gastos = gastos.Where(g => g.PeriodoProductivoId == pid);
            ventas = ventas.Where(v => v.PeriodoProductivoId == pid);
            vm.PeriodoNombre = await _db.PeriodosProductivos.Where(p => p.Id == pid).Select(p => p.Nombre).FirstOrDefaultAsync();
        }
        if (vm.Desde is { } d) { gastos = gastos.Where(g => g.Fecha >= d); ventas = ventas.Where(v => v.Fecha >= d); }
        if (vm.Hasta is { } h) { gastos = gastos.Where(g => g.Fecha <= h); ventas = ventas.Where(v => v.Fecha <= h); }

        vm.TotalGastos = await gastos.SumAsync(g => (decimal?)g.Monto) ?? 0m;
        vm.TotalIngresos = await ventas.SumAsync(v => (decimal?)v.Total) ?? 0m;
        vm.GastosPorCategoria = (await gastos
            .GroupBy(g => g.Categoria)
            .Select(g => new { g.Key, Monto = g.Sum(x => x.Monto) })
            .ToListAsync())
            .Select(g => new BalanceViewModel.GastoPorCategoria(Ui.CategoriaGastoTexto(g.Key), g.Monto))
            .OrderByDescending(g => g.Monto)
            .ToList();

        vm.Calculado = true;
    }

    private FileContentResult Descargar(ReporteTabla reporte, string nombre, string formato)
        => formato.Equals("excel", StringComparison.OrdinalIgnoreCase) || formato.Equals("xlsx", StringComparison.OrdinalIgnoreCase)
            ? File(_export.ToExcel(reporte), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nombre}-{DateTime.Now:yyyyMMdd}.xlsx")
            : File(_export.ToPdf(reporte), "application/pdf", $"{nombre}-{DateTime.Now:yyyyMMdd}.pdf");

    private static string Rango(DateTime? d, DateTime? h)
        => (d, h) switch
        {
            (null, null) => "Todos los movimientos",
            ({ } dd, null) => $"Desde {dd:dd/MM/yyyy}",
            (null, { } hh) => $"Hasta {hh:dd/MM/yyyy}",
            ({ } dd, { } hh) => $"{dd:dd/MM/yyyy} — {hh:dd/MM/yyyy}"
        };
}
