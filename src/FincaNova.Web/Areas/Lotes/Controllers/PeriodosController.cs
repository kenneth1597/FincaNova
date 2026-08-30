using FincaNova.Web.Areas.Lotes.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Lotes;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Lotes.Controllers;

/// <summary>
/// Administración de períodos productivos (temporadas de cosecha). Solo puede
/// haber uno activo y no se permiten fechas solapadas (RF-09, HU-40).
/// </summary>
[Area("Lotes")]
[Authorize(Roles = Roles.Gestion)]
public class PeriodosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;

    public PeriodosController(AppDbContext db, IAuditService audit, ICatalogoService catalogo)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var periodos = await _db.PeriodosProductivos.AsNoTracking()
            .OrderByDescending(p => p.FechaInicio)
            .Select(p => new PeriodoListItemViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Activo = p.Activo,
                Recolecciones = _db.Recolecciones.Count(r => r.PeriodoProductivoId == p.Id),
                Labores = _db.Labores.Count(l => l.PeriodoProductivoId == p.Id)
            })
            .ToListAsync();
        return View(periodos);
    }

    [HttpGet]
    public IActionResult Crear() => View(new PeriodoFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PeriodoFormViewModel model)
    {
        await ValidarSolapamientoAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var fincaId = await _catalogo.FincaIdAsync();
        var periodo = new PeriodoProductivo
        {
            FincaId = fincaId!.Value,
            Nombre = model.Nombre.Trim(),
            FechaInicio = model.FechaInicio.Date,
            FechaFin = model.FechaFin.Date,
            Observaciones = model.Observaciones?.Trim(),
            Activo = model.Activo
        };

        if (model.Activo)
            await DesactivarTodosAsync();

        _db.PeriodosProductivos.Add(periodo);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(PeriodoProductivo), periodo.Id.ToString(),
            $"Registro del período productivo {periodo.Nombre}");

        TempData["Ok"] = $"Período «{periodo.Nombre}» registrado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var p = await _db.PeriodosProductivos.FindAsync(id);
        if (p is null) return NotFound();

        return View(new PeriodoFormViewModel
        {
            Id = p.Id,
            Nombre = p.Nombre,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Activo = p.Activo,
            Observaciones = p.Observaciones
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(PeriodoFormViewModel model)
    {
        var p = await _db.PeriodosProductivos.FindAsync(model.Id);
        if (p is null) return NotFound();

        await ValidarSolapamientoAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        p.Nombre = model.Nombre.Trim();
        p.FechaInicio = model.FechaInicio.Date;
        p.FechaFin = model.FechaFin.Date;
        p.Observaciones = model.Observaciones?.Trim();

        if (model.Activo && !p.Activo)
        {
            await DesactivarTodosAsync();
            p.Activo = true;
        }
        else if (!model.Activo)
        {
            p.Activo = false;
        }

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(PeriodoProductivo), p.Id.ToString(),
            $"Edición del período productivo {p.Nombre}");

        TempData["Ok"] = $"Período «{p.Nombre}» actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(int id)
    {
        var p = await _db.PeriodosProductivos.FindAsync(id);
        if (p is null) return NotFound();

        await DesactivarTodosAsync();
        p.Activo = true;
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(PeriodoProductivo), p.Id.ToString(),
            $"Período productivo {p.Nombre} marcado como activo");

        TempData["Ok"] = $"«{p.Nombre}» es ahora el período productivo activo.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------
    private async Task ValidarSolapamientoAsync(PeriodoFormViewModel model)
    {
        var inicio = model.FechaInicio.Date;
        var fin = model.FechaFin.Date;

        // Dos rangos se solapan si  inicioA <= finB  y  inicioB <= finA.
        var solapa = await _db.PeriodosProductivos
            .AnyAsync(p => p.Id != model.Id && inicio <= p.FechaFin && p.FechaInicio <= fin);

        if (solapa)
            ModelState.AddModelError(nameof(model.FechaInicio),
                "Las fechas se solapan con otro período productivo existente.");
    }

    private async Task DesactivarTodosAsync()
    {
        await _db.PeriodosProductivos.Where(p => p.Activo)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Activo, false));
    }
}
