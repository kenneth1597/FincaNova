using System.Diagnostics;
using FincaNova.Web.Data;
using FincaNova.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    /// <summary>Panel principal. En el Hito 7 se sustituye por el dashboard de indicadores.</summary>
    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            Finca = await _db.Fincas.Select(f => f.Nombre).FirstOrDefaultAsync() ?? "Finca",
            TotalLotes = await _db.Lotes.CountAsync(l => !l.Eliminado),
            LotesActivos = await _db.Lotes.CountAsync(l => !l.Eliminado && l.Estado == Domain.EstadoLote.Activo),
            PeriodoActivo = await _db.PeriodosProductivos.Where(p => p.Activo).Select(p => p.Nombre).FirstOrDefaultAsync(),
            AlertasPendientes = await _db.Alertas.CountAsync(a => !a.Atendida)
        };
        return View(vm);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
