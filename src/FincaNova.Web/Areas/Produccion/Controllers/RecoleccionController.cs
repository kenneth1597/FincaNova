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
/// Registro diario de la recolección de café en cajuelas por trabajador y su
/// conversión automática a kilogramos (RF-20, RF-21, HU-11 a HU-13).
/// </summary>
[Area("Produccion")]
[Authorize]
public class RecoleccionController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;

    public RecoleccionController(AppDbContext db, IAuditService audit, ICatalogoService catalogo)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
    }

    // ---------- Listado + avance (HU-11, HU-12) ----------
    [HttpGet]
    public async Task<IActionResult> Index(RecoleccionFiltroViewModel filtro)
    {
        filtro.PeriodoId ??= await _catalogo.PeriodoActivoIdAsync();

        var query = _db.Recolecciones.AsNoTracking().AsQueryable();
        if (filtro.LoteId is { } loteId) query = query.Where(r => r.LoteId == loteId);
        if (filtro.PeriodoId is { } perId) query = query.Where(r => r.PeriodoProductivoId == perId);
        if (filtro.ColaboradorId is { } colId) query = query.Where(r => r.ColaboradorId == colId);
        if (filtro.Desde is { } d) query = query.Where(r => r.Fecha >= d);
        if (filtro.Hasta is { } h) query = query.Where(r => r.Fecha <= h);

        filtro.Resultados = await query
            .OrderByDescending(r => r.Fecha).ThenBy(r => r.Colaborador.Nombre)
            .Select(r => new RecoleccionListItemViewModel
            {
                Id = r.Id,
                Fecha = r.Fecha,
                Lote = r.Lote.Codigo + " — " + r.Lote.Nombre,
                Colaborador = r.Colaborador.Nombre,
                Cajuelas = r.Cajuelas,
                PesoEstimadoKg = r.PesoEstimadoKg
            })
            .ToListAsync();

        filtro.TotalCajuelas = filtro.Resultados.Sum(r => r.Cajuelas);
        filtro.TotalKg = filtro.Resultados.Sum(r => r.PesoEstimadoKg);
        filtro.Acumulado = filtro.Resultados
            .GroupBy(r => r.Colaborador)
            .Select(g => new RecoleccionFiltroViewModel.PorColaborador(g.Key, g.Sum(x => x.Cajuelas), g.Sum(x => x.PesoEstimadoKg)))
            .OrderByDescending(x => x.Cajuelas)
            .ToList();

        var config = await _db.ConfiguracionesFinca.AsNoTracking().FirstOrDefaultAsync();
        filtro.KilogramosPorCajuela = config?.KilogramosPorCajuela ?? 12.5m;
        if (filtro.PeriodoId is { } pid)
            filtro.PeriodoNombre = await _db.PeriodosProductivos.Where(p => p.Id == pid).Select(p => p.Nombre).FirstOrDefaultAsync();

        filtro.Lotes = await _catalogo.LotesSeleccionablesAsync();
        filtro.Periodos = await _catalogo.PeriodosAsync();
        filtro.Colaboradores = await _catalogo.ColaboradoresActivosAsync();
        return View(filtro);
    }

    // ---------- Alta ----------
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var vm = new RecoleccionFormViewModel
        {
            PeriodoProductivoId = await _catalogo.PeriodoActivoIdAsync()
        };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(RecoleccionFormViewModel model)
    {
        await CargarAsync(model);
        await ValidarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var recoleccion = new Recoleccion
        {
            LoteId = model.LoteId!.Value,
            PeriodoProductivoId = model.PeriodoProductivoId!.Value,
            ColaboradorId = model.ColaboradorId!.Value,
            Fecha = model.Fecha.Date,
            Cajuelas = model.Cajuelas,
            PesoEstimadoKg = model.PesoEstimadoKg,
            Observaciones = model.Observaciones?.Trim()
        };
        _db.Recolecciones.Add(recoleccion);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Recoleccion), recoleccion.Id.ToString(),
            $"Recolección de {recoleccion.Cajuelas:N2} cajuelas en el lote {recoleccion.LoteId}");

        TempData["Ok"] = $"Recolección registrada: {model.Cajuelas:N2} cajuelas ≈ {model.PesoEstimadoKg:N2} kg.";
        return RedirectToAction(nameof(Index), new { periodoId = model.PeriodoProductivoId });
    }

    // ---------- Edición ----------
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await _db.Recolecciones.FindAsync(id);
        if (r is null) return NotFound();

        var vm = new RecoleccionFormViewModel
        {
            Id = r.Id,
            LoteId = r.LoteId,
            PeriodoProductivoId = r.PeriodoProductivoId,
            ColaboradorId = r.ColaboradorId,
            Fecha = r.Fecha,
            Cajuelas = r.Cajuelas,
            Observaciones = r.Observaciones
        };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(RecoleccionFormViewModel model)
    {
        var r = await _db.Recolecciones.FindAsync(model.Id);
        if (r is null) return NotFound();

        await CargarAsync(model);
        await ValidarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        r.LoteId = model.LoteId!.Value;
        r.PeriodoProductivoId = model.PeriodoProductivoId!.Value;
        r.ColaboradorId = model.ColaboradorId!.Value;
        r.Fecha = model.Fecha.Date;
        r.Cajuelas = model.Cajuelas;
        r.PesoEstimadoKg = model.PesoEstimadoKg;
        r.Observaciones = model.Observaciones?.Trim();

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Recoleccion), r.Id.ToString(),
            $"Edición de recolección #{r.Id}: {r.Cajuelas:N2} cajuelas");

        TempData["Ok"] = "Recolección actualizada.";
        return RedirectToAction(nameof(Index), new { periodoId = model.PeriodoProductivoId });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await _db.Recolecciones.FindAsync(id);
        if (r is null) return NotFound();

        _db.Recolecciones.Remove(r);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(Recoleccion), id.ToString(),
            $"Eliminación de recolección del {r.Fecha:dd/MM/yyyy}");

        TempData["Ok"] = "Recolección eliminada.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------
    private async Task ValidarAsync(RecoleccionFormViewModel model)
    {
        if (model.LoteId is { } loteId && !await _db.Lotes.AnyAsync(l => l.Id == loteId && !l.Eliminado))
            ModelState.AddModelError(nameof(model.LoteId), "El lote seleccionado no es válido.");
        if (model.ColaboradorId is { } colId && !await _db.Colaboradores.AnyAsync(c => c.Id == colId && c.Estado == EstadoColaborador.Activo))
            ModelState.AddModelError(nameof(model.ColaboradorId), "El trabajador seleccionado no está activo.");
    }

    private async Task CargarAsync(RecoleccionFormViewModel vm)
    {
        var config = await _db.ConfiguracionesFinca.AsNoTracking().FirstOrDefaultAsync();
        vm.KilogramosPorCajuela = config?.KilogramosPorCajuela ?? 12.5m;
        vm.Lotes = await _catalogo.LotesSeleccionablesAsync();
        vm.Periodos = await _catalogo.PeriodosAsync();
        vm.Colaboradores = await _catalogo.ColaboradoresActivosAsync();
    }
}
