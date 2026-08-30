using FincaNova.Web.Areas.Labores.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Catalogos;
using FincaNova.Web.ViewSupport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Labores.Controllers;

/// <summary>
/// Cálculo automático del pago de un colaborador por período productivo,
/// sumando jornadas/horas de labores y cajuelas recolectadas (RF-15, HU-06).
/// </summary>
[Area("Labores")]
[Authorize(Roles = Roles.Gestion)]
public class PlanillaController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICatalogoService _catalogo;

    public PlanillaController(AppDbContext db, ICatalogoService catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? colaboradorId, int? periodoId)
    {
        var vm = new PlanillaViewModel
        {
            ColaboradorId = colaboradorId,
            PeriodoId = periodoId ?? await _catalogo.PeriodoActivoIdAsync(),
            ColaboradoresDisponibles = await _catalogo.ColaboradoresActivosAsync(),
            PeriodosDisponibles = await _catalogo.PeriodosAsync()
        };

        if (vm.ColaboradorId is not { } cid || vm.PeriodoId is not { } pid)
            return View(vm);

        var colaborador = await _db.Colaboradores.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cid);
        var periodo = await _db.PeriodosProductivos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pid);
        if (colaborador is null || periodo is null) return View(vm);

        vm.Calculada = true;
        vm.ColaboradorNombre = colaborador.Nombre;
        vm.PeriodoNombre = periodo.Nombre;

        vm.Labores = await _db.LaborColaboradores.AsNoTracking()
            .Where(lc => lc.ColaboradorId == cid && lc.Labor.PeriodoProductivoId == pid)
            .OrderBy(lc => lc.Labor.Fecha)
            .Select(lc => new PlanillaViewModel.LineaLabor(
                lc.Labor.Fecha,
                lc.Labor.Lote.Codigo,
                lc.Labor.TipoLabor,
                lc.Labor.Modalidad == Domain.ModalidadPago.PorHora ? "Por hora" : "Por jornada",
                lc.Cantidad,
                lc.Costo))
            .ToListAsync();

        vm.Recolecciones = await _db.Recolecciones.AsNoTracking()
            .Where(r => r.ColaboradorId == cid && r.PeriodoProductivoId == pid)
            .OrderBy(r => r.Fecha)
            .Select(r => new PlanillaViewModel.LineaRecoleccion(
                r.Fecha, r.Lote.Codigo, r.Cajuelas, colaborador.TarifaCajuela, r.Cajuelas * colaborador.TarifaCajuela))
            .ToListAsync();

        return View(vm);
    }
}
