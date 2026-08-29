using FincaNova.Web.Areas.Seguridad.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Models;
using FincaNova.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Seguridad.Controllers;

/// <summary>
/// Visor de solo lectura de la bitácora de auditoría. No expone edición ni
/// borrado: la información es inmodificable (RQNF-013, RQNF-015, HU-18).
/// </summary>
[Area("Seguridad")]
[Authorize(Roles = Roles.Administrador)]
public class AuditoriaController : Controller
{
    private const int TamanoPagina = 25;
    private readonly AppDbContext _db;

    public AuditoriaController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(AuditoriaFiltroViewModel filtro)
    {
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (filtro.Desde is { } desde)
            query = query.Where(a => a.FechaHora >= desde.Date);
        if (filtro.Hasta is { } hasta)
            query = query.Where(a => a.FechaHora < hasta.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(filtro.Usuario))
            query = query.Where(a => a.Usuario != null && a.Usuario.Contains(filtro.Usuario));
        if (!string.IsNullOrWhiteSpace(filtro.Entidad))
            query = query.Where(a => a.Entidad == filtro.Entidad);
        if (filtro.Accion is { } accion)
            query = query.Where(a => a.Accion == accion);

        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((pagina - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .ToListAsync();

        filtro.Pagina = pagina;
        filtro.Resultados = new PagedResult<Domain.Auditoria.AuditLog>
        {
            Items = items,
            Pagina = pagina,
            TamanoPagina = TamanoPagina,
            TotalRegistros = total
        };
        filtro.EntidadesDisponibles = await _db.AuditLogs
            .Select(a => a.Entidad).Distinct().OrderBy(e => e).ToListAsync();

        return View(filtro);
    }
}
