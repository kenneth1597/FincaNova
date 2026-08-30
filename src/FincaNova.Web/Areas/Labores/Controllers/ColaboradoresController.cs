using FincaNova.Web.Areas.Labores.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Labores;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Labores.Controllers;

/// <summary>
/// Administración de colaboradores de la finca (RF-14, HU-04, HU-05).
/// </summary>
[Area("Labores")]
[Authorize(Roles = Roles.Gestion)]
public class ColaboradoresController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public ColaboradoresController(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, bool soloActivos = true)
    {
        var query = _db.Colaboradores.AsNoTracking().AsQueryable();

        if (soloActivos)
            query = query.Where(c => c.Estado == EstadoColaborador.Activo);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim();
            query = query.Where(c => c.Nombre.Contains(t) || c.Identificacion.Contains(t)
                || (c.Cargo != null && c.Cargo.Contains(t)));
        }

        var filas = await query
            .OrderBy(c => c.Nombre)
            .Select(c => new ColaboradorListItemViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Identificacion = c.Identificacion,
                Telefono = c.Telefono,
                Cargo = c.Cargo,
                TarifaJornada = c.TarifaJornada,
                Estado = c.Estado,
                Labores = c.Labores.Count
            })
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.SoloActivos = soloActivos;
        return View(filas);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var colaborador = await _db.Colaboradores.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (colaborador is null) return NotFound();

        var historial = await _db.LaborColaboradores.AsNoTracking()
            .Where(lc => lc.ColaboradorId == id)
            .OrderByDescending(lc => lc.Labor.Fecha)
            .Select(lc => new ColaboradorDetalleViewModel.ParticipacionLabor(
                lc.Labor.Fecha, lc.Labor.Lote.Codigo, lc.Labor.TipoLabor, lc.Cantidad, lc.Costo))
            .ToListAsync();

        return View(new ColaboradorDetalleViewModel
        {
            Colaborador = colaborador,
            Historial = historial,
            TotalAcumulado = historial.Sum(h => h.Costo)
        });
    }

    [HttpGet]
    public IActionResult Crear() => View(new ColaboradorFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ColaboradorFormViewModel model)
    {
        await ValidarIdentificacionAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var colaborador = new Colaborador
        {
            Nombre = model.Nombre.Trim(),
            Identificacion = model.Identificacion.Trim(),
            Telefono = model.Telefono?.Trim(),
            Cargo = model.Cargo?.Trim(),
            TarifaJornada = model.TarifaJornada,
            TarifaHora = model.TarifaHora,
            TarifaCajuela = model.TarifaCajuela,
            Estado = EstadoColaborador.Activo
        };
        _db.Colaboradores.Add(colaborador);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Colaborador), colaborador.Id.ToString(),
            $"Alta de colaborador {colaborador.Nombre} ({colaborador.Identificacion})");

        TempData["Ok"] = $"Colaborador «{colaborador.Nombre}» registrado y disponible para asignar a labores.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var c = await _db.Colaboradores.FindAsync(id);
        if (c is null) return NotFound();

        return View(new ColaboradorFormViewModel
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Identificacion = c.Identificacion,
            Telefono = c.Telefono,
            Cargo = c.Cargo,
            TarifaJornada = c.TarifaJornada,
            TarifaHora = c.TarifaHora,
            TarifaCajuela = c.TarifaCajuela
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ColaboradorFormViewModel model)
    {
        var c = await _db.Colaboradores.FindAsync(model.Id);
        if (c is null) return NotFound();

        await ValidarIdentificacionAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        c.Nombre = model.Nombre.Trim();
        c.Identificacion = model.Identificacion.Trim();
        c.Telefono = model.Telefono?.Trim();
        c.Cargo = model.Cargo?.Trim();
        c.TarifaJornada = model.TarifaJornada;
        c.TarifaHora = model.TarifaHora;
        c.TarifaCajuela = model.TarifaCajuela;

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Colaborador), c.Id.ToString(),
            $"Edición del colaborador {c.Nombre}");

        TempData["Ok"] = $"Colaborador «{c.Nombre}» actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var c = await _db.Colaboradores.FindAsync(id);
        if (c is null) return NotFound();

        c.Estado = c.Estado == EstadoColaborador.Activo ? EstadoColaborador.Inactivo : EstadoColaborador.Activo;
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(
            c.Estado == EstadoColaborador.Activo ? AccionAuditoria.Activar : AccionAuditoria.Inactivar,
            nameof(Colaborador), c.Id.ToString(),
            $"{(c.Estado == EstadoColaborador.Activo ? "Reactivación" : "Inactivación")} del colaborador {c.Nombre}");

        TempData["Ok"] = c.Estado == EstadoColaborador.Activo
            ? $"«{c.Nombre}» quedó activo."
            : $"«{c.Nombre}» quedó inactivo; ya no aparece para asignar a nuevas labores.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarIdentificacionAsync(ColaboradorFormViewModel model)
    {
        var ident = model.Identificacion.Trim();
        var dup = await _db.Colaboradores.AnyAsync(c => c.Identificacion == ident && c.Id != model.Id);
        if (dup)
            ModelState.AddModelError(nameof(model.Identificacion),
                "Ya existe un colaborador con esa identificación.");
    }
}
