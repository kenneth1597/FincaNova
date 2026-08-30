using FincaNova.Web.Areas.Produccion.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Produccion;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Produccion.Controllers;

/// <summary>
/// Seguimiento del beneficiado del café: pesos de café seco y procesado, merma
/// entre etapas y comparativa de rendimiento por lote (RF-21, RF-22, HU-14 a HU-16).
/// </summary>
[Area("Produccion")]
[Authorize]
public class ProduccionController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;

    public ProduccionController(AppDbContext db, IAuditService audit, ICatalogoService catalogo)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
    }

    // ---------- Resumen y merma por lote + período (HU-14, HU-15) ----------
    [HttpGet]
    public async Task<IActionResult> Index(int? loteId, int? periodoId)
    {
        var vm = new ProduccionResumenViewModel
        {
            LoteId = loteId,
            PeriodoId = periodoId ?? await _catalogo.PeriodoActivoIdAsync(),
            Lotes = await _catalogo.LotesSeleccionablesAsync(),
            Periodos = await _catalogo.PeriodosAsync()
        };

        if (vm.LoteId is not { } lid || vm.PeriodoId is not { } pid)
            return View(vm);

        vm.LoteNombre = await _db.Lotes.Where(l => l.Id == lid).Select(l => l.Codigo + " — " + l.Nombre).FirstOrDefaultAsync();
        vm.PeriodoNombre = await _db.PeriodosProductivos.Where(p => p.Id == pid).Select(p => p.Nombre).FirstOrDefaultAsync();
        if (vm.LoteNombre is null || vm.PeriodoNombre is null) return View(vm);

        vm.Calculado = true;

        vm.Cajuelas = await _db.Recolecciones
            .Where(r => r.LoteId == lid && r.PeriodoProductivoId == pid)
            .SumAsync(r => (decimal?)r.Cajuelas) ?? 0m;
        vm.KgRecolectadoEstimado = await _db.Recolecciones
            .Where(r => r.LoteId == lid && r.PeriodoProductivoId == pid)
            .SumAsync(r => (decimal?)r.PesoEstimadoKg) ?? 0m;

        var registros = await _db.RegistrosProduccion.AsNoTracking()
            .Where(r => r.LoteId == lid && r.PeriodoProductivoId == pid)
            .OrderBy(r => r.Fecha)
            .Select(r => new ProduccionResumenViewModel.LineaEtapa(r.Id, r.Etapa, r.Fecha, r.PesoKg, r.Observaciones))
            .ToListAsync();
        vm.Registros = registros;
        vm.KgCafeSeco = registros.Where(r => r.Etapa == EtapaProduccion.CafeSeco).Sum(r => r.PesoKg);
        vm.KgCafeProcesado = registros.Where(r => r.Etapa == EtapaProduccion.CafeProcesado).Sum(r => r.PesoKg);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Crear(int? loteId, int? periodoId, EtapaProduccion etapa = EtapaProduccion.CafeSeco)
    {
        var vm = new RegistroProduccionFormViewModel
        {
            LoteId = loteId,
            PeriodoProductivoId = periodoId ?? await _catalogo.PeriodoActivoIdAsync(),
            Etapa = etapa == EtapaProduccion.Recoleccion ? EtapaProduccion.CafeSeco : etapa
        };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(RegistroProduccionFormViewModel model)
    {
        await CargarAsync(model);
        if (model.Etapa == EtapaProduccion.Recoleccion)
            ModelState.AddModelError(nameof(model.Etapa), "Use el módulo de recolección para el café recién recolectado.");
        if (!ModelState.IsValid)
            return View(model);

        var registro = new RegistroProduccion
        {
            LoteId = model.LoteId!.Value,
            PeriodoProductivoId = model.PeriodoProductivoId!.Value,
            Etapa = model.Etapa,
            Fecha = model.Fecha.Date,
            PesoKg = model.PesoKg,
            Observaciones = model.Observaciones?.Trim()
        };
        _db.RegistrosProduccion.Add(registro);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(RegistroProduccion), registro.Id.ToString(),
            $"Registro de {model.Etapa} ({model.PesoKg:N2} kg) en el lote {model.LoteId}");

        TempData["Ok"] = "Peso de la etapa registrado.";
        return RedirectToAction(nameof(Index), new { loteId = model.LoteId, periodoId = model.PeriodoProductivoId });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await _db.RegistrosProduccion.FindAsync(id);
        if (r is null) return NotFound();

        var vm = new RegistroProduccionFormViewModel
        {
            Id = r.Id,
            LoteId = r.LoteId,
            PeriodoProductivoId = r.PeriodoProductivoId,
            Etapa = r.Etapa,
            Fecha = r.Fecha,
            PesoKg = r.PesoKg,
            Observaciones = r.Observaciones
        };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(RegistroProduccionFormViewModel model)
    {
        var r = await _db.RegistrosProduccion.FindAsync(model.Id);
        if (r is null) return NotFound();

        await CargarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        r.LoteId = model.LoteId!.Value;
        r.PeriodoProductivoId = model.PeriodoProductivoId!.Value;
        r.Etapa = model.Etapa;
        r.Fecha = model.Fecha.Date;
        r.PesoKg = model.PesoKg;
        r.Observaciones = model.Observaciones?.Trim();
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(RegistroProduccion), r.Id.ToString(),
            $"Edición de registro de producción #{r.Id}");

        TempData["Ok"] = "Registro de producción actualizado.";
        return RedirectToAction(nameof(Index), new { loteId = model.LoteId, periodoId = model.PeriodoProductivoId });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await _db.RegistrosProduccion.FindAsync(id);
        if (r is null) return NotFound();

        var (lote, per) = (r.LoteId, r.PeriodoProductivoId);
        _db.RegistrosProduccion.Remove(r);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(RegistroProduccion), id.ToString(),
            "Eliminación de un registro de producción");

        TempData["Ok"] = "Registro eliminado.";
        return RedirectToAction(nameof(Index), new { loteId = lote, periodoId = per });
    }

    // ---------- Comparativa de rendimiento por lote y período (HU-16) ----------
    [HttpGet]
    public async Task<IActionResult> Rendimiento()
    {
        var periodos = await _db.PeriodosProductivos.AsNoTracking()
            .OrderBy(p => p.FechaInicio)
            .Select(p => new { p.Id, p.Nombre })
            .ToListAsync();

        var lotes = await _db.Lotes.AsNoTracking()
            .Where(l => !l.Eliminado)
            .OrderBy(l => l.Codigo)
            .Select(l => new { l.Id, Nombre = l.Codigo + " — " + l.Nombre })
            .ToListAsync();

        // kg de café procesado por lote y período
        var datos = await _db.RegistrosProduccion.AsNoTracking()
            .Where(r => r.Etapa == EtapaProduccion.CafeProcesado)
            .GroupBy(r => new { r.LoteId, r.PeriodoProductivoId })
            .Select(g => new { g.Key.LoteId, g.Key.PeriodoProductivoId, Kg = g.Sum(x => x.PesoKg) })
            .ToListAsync();

        var filas = lotes.Select(l => new RendimientoViewModel.FilaLote(
            l.Nombre,
            periodos.Select(p => datos.FirstOrDefault(d => d.LoteId == l.Id && d.PeriodoProductivoId == p.Id)?.Kg ?? 0m).ToList()
        )).ToList();

        var vm = new RendimientoViewModel
        {
            Periodos = periodos.Select(p => p.Nombre).ToList(),
            Lotes = filas,
            MaximoKg = datos.Count > 0 ? datos.Max(d => d.Kg) : 0m
        };
        return View(vm);
    }

    private async Task CargarAsync(RegistroProduccionFormViewModel vm)
    {
        vm.Lotes = await _catalogo.LotesSeleccionablesAsync();
        vm.Periodos = await _catalogo.PeriodosAsync();
    }
}
