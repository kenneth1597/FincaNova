using System.Diagnostics;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    /// <summary>Panel principal: tablero de indicadores del período productivo activo (Hito 7).</summary>
    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            Finca = await _db.Fincas.Select(f => f.Nombre).FirstOrDefaultAsync() ?? "Finca",
            TotalLotes = await _db.Lotes.CountAsync(l => !l.Eliminado),
            LotesEnOperacion = await _db.Lotes.CountAsync(l => !l.Eliminado && l.Estado != EstadoLote.Inactivo),
            LotesPorEstado = await _db.Lotes.Where(l => !l.Eliminado)
                .GroupBy(l => l.Estado)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count),
            AlertasPendientes = await _db.Alertas.CountAsync(a => !a.Atendida),
            UltimasAlertas = (await _db.Alertas.AsNoTracking()
                .Where(a => !a.Atendida)
                .OrderByDescending(a => a.FechaGeneracion)
                .Take(4)
                .Select(a => new { a.Tipo, a.Mensaje, a.FechaGeneracion })
                .ToListAsync())
                .Select(a => new DashboardViewModel.AlertaMini(a.Tipo, a.Mensaje, a.FechaGeneracion))
                .ToList()
        };

        var periodo = await _db.PeriodosProductivos.AsNoTracking().FirstOrDefaultAsync(p => p.Activo);
        if (periodo is null)
            return View(vm);

        vm.PeriodoActivo = periodo.Nombre;
        vm.PeriodoInicio = periodo.FechaInicio;
        vm.PeriodoFin = periodo.FechaFin;
        var pid = periodo.Id;

        vm.Cajuelas = await _db.Recolecciones.Where(r => r.PeriodoProductivoId == pid).SumAsync(r => (decimal?)r.Cajuelas) ?? 0m;
        vm.KgRecolectado = await _db.Recolecciones.Where(r => r.PeriodoProductivoId == pid).SumAsync(r => (decimal?)r.PesoEstimadoKg) ?? 0m;
        vm.KgSeco = await _db.RegistrosProduccion.Where(r => r.PeriodoProductivoId == pid && r.Etapa == EtapaProduccion.CafeSeco).SumAsync(r => (decimal?)r.PesoKg) ?? 0m;
        vm.KgProcesado = await _db.RegistrosProduccion.Where(r => r.PeriodoProductivoId == pid && r.Etapa == EtapaProduccion.CafeProcesado).SumAsync(r => (decimal?)r.PesoKg) ?? 0m;

        vm.TopRecolectores = (await _db.Recolecciones.AsNoTracking()
            .Where(r => r.PeriodoProductivoId == pid)
            .GroupBy(r => r.Colaborador.Nombre)
            .Select(g => new { Nombre = g.Key, Cajuelas = g.Sum(x => x.Cajuelas) })
            .OrderByDescending(x => x.Cajuelas)
            .Take(5)
            .ToListAsync())
            .Select(x => new DashboardViewModel.TopRecolector(x.Nombre, x.Cajuelas))
            .ToList();

        vm.LaboresCount = await _db.Labores.CountAsync(l => l.PeriodoProductivoId == pid);
        vm.CostoManoObra = await _db.Labores.Where(l => l.PeriodoProductivoId == pid).SumAsync(l => (decimal?)l.CostoCalculado) ?? 0m;

        vm.EnfermedadesActivas = await _db.RegistrosEnfermedad
            .CountAsync(r => r.PeriodoProductivoId == pid && r.Estado != EstadoEnfermedad.Controlada);

        vm.Ingresos = await _db.VentasCafe.Where(v => v.PeriodoProductivoId == pid).SumAsync(v => (decimal?)v.Total) ?? 0m;
        vm.Gastos = await _db.Gastos.Where(g => g.PeriodoProductivoId == pid).SumAsync(g => (decimal?)g.Monto) ?? 0m;

        // Costo por cajuela de los lotes con recolección en el período
        var recolPorLote = await _db.Recolecciones.AsNoTracking()
            .Where(r => r.PeriodoProductivoId == pid)
            .GroupBy(r => new { r.LoteId, r.Lote.Codigo, r.Lote.Nombre })
            .Select(g => new { g.Key.LoteId, g.Key.Codigo, g.Key.Nombre, Cajuelas = g.Sum(x => x.Cajuelas) })
            .ToListAsync();

        var costoLotes = new List<DashboardViewModel.CostoLote>();
        foreach (var r in recolPorLote.Where(x => x.Cajuelas > 0))
        {
            var mano = await _db.Labores.Where(l => l.LoteId == r.LoteId && l.PeriodoProductivoId == pid)
                .SumAsync(l => (decimal?)l.CostoCalculado) ?? 0m;
            var gastoLote = await _db.Gastos.Where(g => g.LoteId == r.LoteId && g.PeriodoProductivoId == pid)
                .SumAsync(g => (decimal?)g.Monto) ?? 0m;
            costoLotes.Add(new DashboardViewModel.CostoLote(
                $"{r.Codigo} — {r.Nombre}", r.Cajuelas, decimal.Round((mano + gastoLote) / r.Cajuelas, 2)));
        }
        vm.CostoLotes = costoLotes.OrderByDescending(c => c.CostoUnitario).Take(5).ToList();

        return View(vm);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
