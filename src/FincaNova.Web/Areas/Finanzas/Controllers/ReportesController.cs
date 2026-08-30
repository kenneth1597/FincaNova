using FincaNova.Web.Areas.Finanzas.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Catalogos;
using FincaNova.Web.Services.Reportes;
using FincaNova.Web.ViewSupport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Finanzas.Controllers;

/// <summary>
/// Reportes de producción y financieros, exportables a PDF y Excel
/// (RF-27, RF-28, HU-17, HU-26, HU-27, HU-28, HU-29).
/// </summary>
[Area("Finanzas")]
[Authorize(Roles = Roles.Gestion)]
public class ReportesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICatalogoService _catalogo;
    private readonly IReporteExportService _export;

    public ReportesController(AppDbContext db, ICatalogoService catalogo, IReporteExportService export)
    {
        _db = db;
        _catalogo = catalogo;
        _export = export;
    }

    [HttpGet]
    public IActionResult Index() => View();

    // ---------- Producción por lote y período (HU-28) ----------
    [HttpGet]
    public async Task<IActionResult> Produccion(int? periodoId, string? formato)
    {
        var vm = new ReporteProduccionViewModel
        {
            PeriodoId = periodoId ?? await _catalogo.PeriodoActivoIdAsync(),
            Periodos = await _catalogo.PeriodosAsync()
        };
        if (vm.PeriodoId is { } pid)
        {
            vm.Calculado = true;
            vm.PeriodoNombre = await _db.PeriodosProductivos.Where(p => p.Id == pid).Select(p => p.Nombre).FirstOrDefaultAsync();
            vm.Filas = await CalcularProduccionAsync(pid);
        }

        if (!string.IsNullOrEmpty(formato) && vm.Calculado)
        {
            var reporte = new ReporteTabla(
                "Reporte de producción", $"Período: {vm.PeriodoNombre}",
                new[] { "Lote", "Cajuelas", "Recolectado (kg)", "Café seco (kg)", "Procesado (kg)", "Merma %", "Rendimiento %" },
                vm.Filas.Select(f => (IReadOnlyList<string>)new[]
                {
                    f.Lote, f.Cajuelas.ToString("N2"), f.KgRecolectado.ToString("N2"),
                    f.KgSeco.ToString("N2"), f.KgProcesado.ToString("N2"),
                    f.MermaPct.ToString("N1") + "%", f.RendimientoPct.ToString("N1") + "%"
                }).ToList());
            return Descargar(reporte, "produccion", formato);
        }

        return View(vm);
    }

    // ---------- Consolidado financiero por período (HU-29) ----------
    [HttpGet]
    public async Task<IActionResult> Consolidado(int? periodoId, string? formato)
    {
        var vm = new BalanceViewModel
        {
            PeriodoId = periodoId ?? await _catalogo.PeriodoActivoIdAsync(),
            Periodos = await _catalogo.PeriodosAsync()
        };
        if (vm.PeriodoId is { } pid)
        {
            vm.Calculado = true;
            vm.PeriodoNombre = await _db.PeriodosProductivos.Where(p => p.Id == pid).Select(p => p.Nombre).FirstOrDefaultAsync();
            vm.TotalGastos = await _db.Gastos.Where(g => g.PeriodoProductivoId == pid).SumAsync(g => (decimal?)g.Monto) ?? 0m;
            vm.TotalIngresos = await _db.VentasCafe.Where(v => v.PeriodoProductivoId == pid).SumAsync(v => (decimal?)v.Total) ?? 0m;
        }

        if (!string.IsNullOrEmpty(formato) && vm.Calculado)
        {
            var reporte = new ReporteTabla(
                "Consolidado de ingresos y gastos", $"Período: {vm.PeriodoNombre}",
                new[] { "Concepto", "Monto" },
                new List<IReadOnlyList<string>>
                {
                    new[] { "Ingresos", Ui.Money(vm.TotalIngresos) },
                    new[] { "Gastos", Ui.Money(vm.TotalGastos) }
                },
                new[] { "Utilidad", Ui.Money(vm.BalanceNeto) });
            return Descargar(reporte, "consolidado", formato);
        }

        return View(vm);
    }

    // ---------- Costo real por cajuela (HU-17) ----------
    [HttpGet]
    public async Task<IActionResult> CostoPorCajuela(int? loteId, int? periodoId, string? formato)
    {
        var vm = new CostoPorCajuelaViewModel
        {
            LoteId = loteId,
            PeriodoId = periodoId ?? await _catalogo.PeriodoActivoIdAsync(),
            Lotes = await _catalogo.LotesSeleccionablesAsync(),
            Periodos = await _catalogo.PeriodosAsync()
        };

        if (vm.LoteId is { } lid && vm.PeriodoId is { } pid)
        {
            vm.Calculado = true;
            vm.LoteNombre = await _db.Lotes.Where(l => l.Id == lid).Select(l => l.Codigo + " — " + l.Nombre).FirstOrDefaultAsync();
            vm.PeriodoNombre = await _db.PeriodosProductivos.Where(p => p.Id == pid).Select(p => p.Nombre).FirstOrDefaultAsync();

            vm.CostoManoDeObra = await _db.Labores
                .Where(l => l.LoteId == lid && l.PeriodoProductivoId == pid)
                .SumAsync(l => (decimal?)l.CostoCalculado) ?? 0m;
            vm.CostoInsumos = await _db.Gastos
                .Where(g => g.LoteId == lid && g.PeriodoProductivoId == pid && g.Categoria == CategoriaGasto.Insumos)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;
            vm.OtrosGastos = await _db.Gastos
                .Where(g => g.LoteId == lid && g.PeriodoProductivoId == pid && g.Categoria != CategoriaGasto.Insumos)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;
            vm.Cajuelas = await _db.Recolecciones
                .Where(r => r.LoteId == lid && r.PeriodoProductivoId == pid)
                .SumAsync(r => (decimal?)r.Cajuelas) ?? 0m;

            if (!string.IsNullOrEmpty(formato))
            {
                var reporte = new ReporteTabla(
                    "Costo real de producción por cajuela",
                    $"{vm.LoteNombre} · {vm.PeriodoNombre}",
                    new[] { "Concepto", "Monto" },
                    new List<IReadOnlyList<string>>
                    {
                        new[] { "Mano de obra (labores)", Ui.Money(vm.CostoManoDeObra) },
                        new[] { "Insumos", Ui.Money(vm.CostoInsumos) },
                        new[] { "Otros gastos", Ui.Money(vm.OtrosGastos) },
                        new[] { "Cajuelas recolectadas", vm.Cajuelas.ToString("N2") }
                    },
                    new[] { "COSTO POR CAJUELA", vm.ProduccionIniciada ? Ui.Money(vm.CostoUnitario) : "Producción no iniciada" });
                return Descargar(reporte, "costo-cajuela", formato);
            }
        }

        return View(vm);
    }

    // ---------- helpers ----------
    private async Task<List<ReporteProduccionViewModel.Fila>> CalcularProduccionAsync(int periodoId)
    {
        var lotes = await _db.Lotes.AsNoTracking().Where(l => !l.Eliminado)
            .OrderBy(l => l.Codigo)
            .Select(l => new { l.Id, Nombre = l.Codigo + " — " + l.Nombre })
            .ToListAsync();

        var recol = await _db.Recolecciones.Where(r => r.PeriodoProductivoId == periodoId)
            .GroupBy(r => r.LoteId)
            .Select(g => new { LoteId = g.Key, Caj = g.Sum(x => x.Cajuelas), Kg = g.Sum(x => x.PesoEstimadoKg) })
            .ToListAsync();
        var prod = await _db.RegistrosProduccion.Where(r => r.PeriodoProductivoId == periodoId)
            .GroupBy(r => new { r.LoteId, r.Etapa })
            .Select(g => new { g.Key.LoteId, g.Key.Etapa, Kg = g.Sum(x => x.PesoKg) })
            .ToListAsync();

        var filas = new List<ReporteProduccionViewModel.Fila>();
        foreach (var l in lotes)
        {
            var r = recol.FirstOrDefault(x => x.LoteId == l.Id);
            var seco = prod.Where(x => x.LoteId == l.Id && x.Etapa == EtapaProduccion.CafeSeco).Sum(x => x.Kg);
            var proc = prod.Where(x => x.LoteId == l.Id && x.Etapa == EtapaProduccion.CafeProcesado).Sum(x => x.Kg);
            var kgRec = r?.Kg ?? 0m;
            if (kgRec == 0 && seco == 0 && proc == 0) continue;

            var merma = kgRec > 0 ? decimal.Round((kgRec - proc) / kgRec * 100, 1) : 0m;
            var rend = kgRec > 0 ? decimal.Round(proc / kgRec * 100, 1) : 0m;
            filas.Add(new ReporteProduccionViewModel.Fila(l.Nombre, r?.Caj ?? 0m, kgRec, seco, proc, merma, rend));
        }
        return filas;
    }

    private FileContentResult Descargar(ReporteTabla reporte, string nombre, string formato)
        => formato.Equals("excel", StringComparison.OrdinalIgnoreCase) || formato.Equals("xlsx", StringComparison.OrdinalIgnoreCase)
            ? File(_export.ToExcel(reporte), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nombre}-{DateTime.Now:yyyyMMdd}.xlsx")
            : File(_export.ToPdf(reporte), "application/pdf", $"{nombre}-{DateTime.Now:yyyyMMdd}.pdf");
}
