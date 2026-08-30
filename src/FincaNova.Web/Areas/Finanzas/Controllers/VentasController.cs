using FincaNova.Web.Areas.Finanzas.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Finanzas;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Finanzas.Controllers;

/// <summary>Registro de las ventas de café (RF-24, HU-20).</summary>
[Area("Finanzas")]
[Authorize(Roles = Roles.Gestion)]
public class VentasController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;

    public VentasController(AppDbContext db, IAuditService audit, ICatalogoService catalogo)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(VentaFiltroViewModel filtro)
    {
        var query = _db.VentasCafe.AsNoTracking().AsQueryable();
        if (filtro.PeriodoId is { } p) query = query.Where(v => v.PeriodoProductivoId == p);
        if (filtro.Desde is { } d) query = query.Where(v => v.Fecha >= d);
        if (filtro.Hasta is { } h) query = query.Where(v => v.Fecha <= h);

        filtro.Resultados = await query
            .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id)
            .Select(v => new VentaListItemViewModel
            {
                Id = v.Id, Fecha = v.Fecha, Comprador = v.Comprador,
                Cantidad = v.Cantidad, UnidadMedida = v.UnidadMedida,
                PrecioUnitario = v.PrecioUnitario, Total = v.Total,
                Lote = v.Lote != null ? v.Lote.Codigo : null
            })
            .ToListAsync();
        filtro.Total = filtro.Resultados.Sum(v => v.Total);
        filtro.Periodos = await _catalogo.PeriodosAsync();
        return View(filtro);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var vm = new VentaFormViewModel { PeriodoProductivoId = await _catalogo.PeriodoActivoIdAsync() };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(VentaFormViewModel model)
    {
        await CargarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var venta = new VentaCafe
        {
            Fecha = model.Fecha.Date,
            Comprador = model.Comprador.Trim(),
            UnidadMedida = model.UnidadMedida.Trim(),
            Cantidad = model.Cantidad,
            PrecioUnitario = model.PrecioUnitario,
            Total = model.Total,
            LoteId = model.LoteId,
            PeriodoProductivoId = model.PeriodoProductivoId,
            Observaciones = model.Observaciones?.Trim()
        };
        _db.VentasCafe.Add(venta);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(VentaCafe), venta.Id.ToString(),
            $"Venta de café a {venta.Comprador} por {venta.Total:N2}");

        TempData["Ok"] = $"Venta registrada. Total: {model.Total:N2}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var v = await _db.VentasCafe.FindAsync(id);
        if (v is null) return NotFound();

        var vm = new VentaFormViewModel
        {
            Id = v.Id, Fecha = v.Fecha, Comprador = v.Comprador, UnidadMedida = v.UnidadMedida,
            Cantidad = v.Cantidad, PrecioUnitario = v.PrecioUnitario,
            LoteId = v.LoteId, PeriodoProductivoId = v.PeriodoProductivoId, Observaciones = v.Observaciones
        };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(VentaFormViewModel model)
    {
        var v = await _db.VentasCafe.FindAsync(model.Id);
        if (v is null) return NotFound();

        await CargarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        v.Fecha = model.Fecha.Date;
        v.Comprador = model.Comprador.Trim();
        v.UnidadMedida = model.UnidadMedida.Trim();
        v.Cantidad = model.Cantidad;
        v.PrecioUnitario = model.PrecioUnitario;
        v.Total = model.Total;
        v.LoteId = model.LoteId;
        v.PeriodoProductivoId = model.PeriodoProductivoId;
        v.Observaciones = model.Observaciones?.Trim();
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(VentaCafe), v.Id.ToString(), $"Edición de la venta #{v.Id}");
        TempData["Ok"] = "Venta actualizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var v = await _db.VentasCafe.FindAsync(id);
        if (v is null) return NotFound();

        _db.VentasCafe.Remove(v);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(VentaCafe), id.ToString(), $"Eliminación de la venta #{id}");
        TempData["Ok"] = "Venta eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarAsync(VentaFormViewModel vm)
    {
        vm.Lotes = await _catalogo.LotesSeleccionablesAsync();
        vm.Periodos = await _catalogo.PeriodosAsync();
    }
}
