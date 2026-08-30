using FincaNova.Web.Areas.Labores.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Labores;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using FincaNova.Web.ViewSupport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Labores.Controllers;

/// <summary>
/// Registro de labores agrícolas. Cualquier usuario autenticado puede registrar
/// (Trabajador incluido, HU-01); eliminar queda restringido a gestión
/// (RF-11 a RF-13, RF-15, HU-01 a HU-03).
/// </summary>
[Area("Labores")]
[Authorize]
public class LaboresController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;

    public LaboresController(AppDbContext db, IAuditService audit, ICatalogoService catalogo)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
    }

    // ---------- Listado ----------
    [HttpGet]
    public async Task<IActionResult> Index(LaborFiltroViewModel filtro)
    {
        var query = _db.Labores.AsNoTracking().AsQueryable();

        if (filtro.LoteId is { } loteId) query = query.Where(l => l.LoteId == loteId);
        if (filtro.PeriodoId is { } perId) query = query.Where(l => l.PeriodoProductivoId == perId);
        if (filtro.Desde is { } d) query = query.Where(l => l.Fecha >= d);
        if (filtro.Hasta is { } h) query = query.Where(l => l.Fecha < h.AddDays(1));

        filtro.Resultados = await query
            .OrderByDescending(l => l.Fecha).ThenByDescending(l => l.Id)
            .Select(l => new LaborListItemViewModel
            {
                Id = l.Id,
                Fecha = l.Fecha,
                Lote = l.Lote.Codigo + " — " + l.Lote.Nombre,
                TipoLabor = l.TipoLabor,
                Modalidad = l.Modalidad,
                Cantidad = l.Modalidad == ModalidadPago.PorHora ? l.DuracionHoras : l.Jornadas,
                Colaboradores = l.Colaboradores.Count,
                CostoCalculado = l.CostoCalculado
            })
            .ToListAsync();

        filtro.TotalCosto = filtro.Resultados.Sum(r => r.CostoCalculado);
        filtro.Lotes = await _catalogo.LotesSeleccionablesAsync();
        filtro.Periodos = await _catalogo.PeriodosAsync();
        return View(filtro);
    }

    // ---------- Detalle ----------
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var labor = await _db.Labores.AsNoTracking()
            .Include(l => l.Lote)
            .Include(l => l.PeriodoProductivo)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (labor is null) return NotFound();

        var desglose = await _db.LaborColaboradores.AsNoTracking()
            .Where(lc => lc.LaborId == id)
            .Select(lc => new LaborDetalleViewModel.Linea(lc.Colaborador.Nombre, lc.Cantidad, lc.Costo))
            .ToListAsync();

        return View(new LaborDetalleViewModel
        {
            Labor = labor,
            Lote = $"{labor.Lote.Codigo} — {labor.Lote.Nombre}",
            Periodo = labor.PeriodoProductivo?.Nombre,
            Desglose = desglose
        });
    }

    // ---------- Alta ----------
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var vm = new LaborFormViewModel
        {
            PeriodoProductivoId = await _catalogo.PeriodoActivoIdAsync()
        };
        await CargarCatalogosAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(LaborFormViewModel model)
    {
        await CargarCatalogosAsync(model);
        await ValidarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var colaboradores = await _db.Colaboradores
            .Where(c => model.ColaboradorIds.Contains(c.Id))
            .ToListAsync();

        var labor = new Labor
        {
            LoteId = model.LoteId!.Value,
            PeriodoProductivoId = model.PeriodoProductivoId,
            TipoLabor = model.TipoLabor.Trim(),
            Fecha = model.Fecha.Date,
            Modalidad = model.Modalidad,
            DuracionHoras = model.Modalidad == ModalidadPago.PorHora ? model.Cantidad : 0,
            Jornadas = model.Modalidad == ModalidadPago.PorHora ? 0 : model.Cantidad,
            CostoUnitario = model.CostoUnitario,
            Observaciones = model.Observaciones?.Trim()
        };
        AplicarCalculo(labor, model, colaboradores);

        _db.Labores.Add(labor);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Labor), labor.Id.ToString(),
            $"Registro de labor «{labor.TipoLabor}» en el lote {labor.LoteId}; costo {labor.CostoCalculado:N2}");

        TempData["Ok"] = $"Labor «{labor.TipoLabor}» registrada. Costo calculado: {labor.CostoCalculado:N2}.";
        return RedirectToAction(nameof(Detalle), new { id = labor.Id });
    }

    // ---------- Edición ----------
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var labor = await _db.Labores.Include(l => l.Colaboradores).FirstOrDefaultAsync(l => l.Id == id);
        if (labor is null) return NotFound();

        var vm = new LaborFormViewModel
        {
            Id = labor.Id,
            TipoLabor = labor.TipoLabor,
            LoteId = labor.LoteId,
            PeriodoProductivoId = labor.PeriodoProductivoId,
            Fecha = labor.Fecha,
            Modalidad = labor.Modalidad,
            Cantidad = labor.Modalidad == ModalidadPago.PorHora ? labor.DuracionHoras : labor.Jornadas,
            CostoUnitario = labor.CostoUnitario,
            Observaciones = labor.Observaciones,
            ColaboradorIds = labor.Colaboradores.Select(c => c.ColaboradorId).ToList()
        };
        await CargarCatalogosAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(LaborFormViewModel model)
    {
        var labor = await _db.Labores.Include(l => l.Colaboradores).FirstOrDefaultAsync(l => l.Id == model.Id);
        if (labor is null) return NotFound();

        await CargarCatalogosAsync(model);
        await ValidarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var colaboradores = await _db.Colaboradores
            .Where(c => model.ColaboradorIds.Contains(c.Id))
            .ToListAsync();

        labor.LoteId = model.LoteId!.Value;
        labor.PeriodoProductivoId = model.PeriodoProductivoId;
        labor.TipoLabor = model.TipoLabor.Trim();
        labor.Fecha = model.Fecha.Date;
        labor.Modalidad = model.Modalidad;
        labor.DuracionHoras = model.Modalidad == ModalidadPago.PorHora ? model.Cantidad : 0;
        labor.Jornadas = model.Modalidad == ModalidadPago.PorHora ? 0 : model.Cantidad;
        labor.CostoUnitario = model.CostoUnitario;
        labor.Observaciones = model.Observaciones?.Trim();

        _db.LaborColaboradores.RemoveRange(labor.Colaboradores);
        labor.Colaboradores.Clear();
        AplicarCalculo(labor, model, colaboradores);

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Labor), labor.Id.ToString(),
            $"Edición de labor «{labor.TipoLabor}»; costo {labor.CostoCalculado:N2}");

        TempData["Ok"] = $"Labor actualizada. Costo calculado: {labor.CostoCalculado:N2}.";
        return RedirectToAction(nameof(Detalle), new { id = labor.Id });
    }

    // ---------- Eliminación ----------
    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var labor = await _db.Labores.Include(l => l.Colaboradores).FirstOrDefaultAsync(l => l.Id == id);
        if (labor is null) return NotFound();

        _db.LaborColaboradores.RemoveRange(labor.Colaboradores);
        _db.Labores.Remove(labor);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(Labor), id.ToString(),
            $"Eliminación de labor «{labor.TipoLabor}» del {labor.Fecha:dd/MM/yyyy}");

        TempData["Ok"] = "Labor eliminada.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- cálculo del costo (RF-15, HU-02) ----------
    private static void AplicarCalculo(Labor labor, LaborFormViewModel model, List<Colaborador> colaboradores)
    {
        var cantidad = model.Cantidad;
        decimal total = 0m;

        foreach (var c in colaboradores)
        {
            var tarifa = model.CostoUnitario > 0
                ? model.CostoUnitario
                : (model.Modalidad == ModalidadPago.PorHora ? c.TarifaHora : c.TarifaJornada);

            var costo = decimal.Round(cantidad * tarifa, 2);
            total += costo;

            labor.Colaboradores.Add(new LaborColaborador
            {
                ColaboradorId = c.Id,
                Cantidad = cantidad,
                Costo = costo
            });
        }

        labor.CostoCalculado = total;
    }

    // ---------- helpers ----------
    private async Task ValidarAsync(LaborFormViewModel model)
    {
        if (model.LoteId is { } loteId)
        {
            var loteOk = await _db.Lotes.AnyAsync(l => l.Id == loteId && !l.Eliminado);
            if (!loteOk) ModelState.AddModelError(nameof(model.LoteId), "El lote seleccionado no es válido.");
        }

        if (model.ColaboradorIds.Count > 0)
        {
            var validos = await _db.Colaboradores
                .CountAsync(c => model.ColaboradorIds.Contains(c.Id) && c.Estado == EstadoColaborador.Activo);
            if (validos != model.ColaboradorIds.Distinct().Count())
                ModelState.AddModelError(nameof(model.ColaboradorIds),
                    "Uno o más colaboradores seleccionados no están activos.");
        }
    }

    private async Task CargarCatalogosAsync(LaborFormViewModel vm)
    {
        vm.Lotes = await _catalogo.LotesSeleccionablesAsync();
        vm.Periodos = await _catalogo.PeriodosAsync();
        vm.Colaboradores = await _db.Colaboradores.AsNoTracking()
            .Where(c => c.Estado == EstadoColaborador.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new LaborFormViewModel.ColaboradorOpcion(
                c.Id, c.Nombre, c.Identificacion, c.TarifaJornada, c.TarifaHora))
            .ToListAsync();
    }
}
