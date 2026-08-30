using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Controllers;

/// <summary>
/// Panel de alertas automáticas del sistema: enfermedades recurrentes (HU-10) y,
/// más adelante, stock mínimo de insumos (HU-25).
/// </summary>
[Authorize(Roles = Roles.Gestion)]
public class AlertasController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public AlertasController(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> Index(bool incluirAtendidas = false)
    {
        var query = _db.Alertas.AsNoTracking().AsQueryable();
        if (!incluirAtendidas)
            query = query.Where(a => !a.Atendida);

        var alertas = await query
            .OrderByDescending(a => a.FechaGeneracion)
            .ToListAsync();

        ViewBag.IncluirAtendidas = incluirAtendidas;
        return View(alertas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarAtendida(int id)
    {
        var alerta = await _db.Alertas.FindAsync(id);
        if (alerta is null) return NotFound();

        alerta.Atendida = true;
        alerta.FechaAtencion = DateTime.UtcNow;
        alerta.AtendidaPor = User.Identity?.Name;
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Domain.Alertas.Alerta), id.ToString(),
            "Alerta marcada como atendida");

        TempData["Ok"] = "Alerta marcada como atendida.";
        return RedirectToAction(nameof(Index));
    }
}
