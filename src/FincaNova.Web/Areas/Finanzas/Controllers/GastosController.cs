using FincaNova.Web.Areas.Finanzas.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Finanzas;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Archivos;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Finanzas.Controllers;

/// <summary>
/// Registro de gastos de la finca con comprobante adjunto (RF-23, HU-19, HU-52).
/// </summary>
[Area("Finanzas")]
[Authorize(Roles = Roles.Gestion)]
public class GastosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;
    private readonly IFileStorageService _archivos;

    public GastosController(AppDbContext db, IAuditService audit, ICatalogoService catalogo, IFileStorageService archivos)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
        _archivos = archivos;
    }

    [HttpGet]
    public async Task<IActionResult> Index(GastoFiltroViewModel filtro)
    {
        var query = _db.Gastos.AsNoTracking().AsQueryable();
        if (filtro.LoteId is { } l) query = query.Where(g => g.LoteId == l);
        if (filtro.PeriodoId is { } p) query = query.Where(g => g.PeriodoProductivoId == p);
        if (filtro.Categoria is { } c) query = query.Where(g => g.Categoria == c);
        if (filtro.Desde is { } d) query = query.Where(g => g.Fecha >= d);
        if (filtro.Hasta is { } h) query = query.Where(g => g.Fecha <= h);

        filtro.Resultados = await query
            .OrderByDescending(g => g.Fecha).ThenByDescending(g => g.Id)
            .Select(g => new GastoListItemViewModel
            {
                Id = g.Id,
                Fecha = g.Fecha,
                Categoria = g.Categoria,
                Descripcion = g.Descripcion,
                Lote = g.Lote != null ? g.Lote.Codigo : null,
                Monto = g.Monto,
                TieneComprobante = g.ComprobanteArchivo != null
            })
            .ToListAsync();
        filtro.Total = filtro.Resultados.Sum(g => g.Monto);
        filtro.Lotes = await _catalogo.LotesSeleccionablesAsync();
        filtro.Periodos = await _catalogo.PeriodosAsync();
        return View(filtro);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var vm = new GastoFormViewModel { PeriodoProductivoId = await _catalogo.PeriodoActivoIdAsync() };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Crear(GastoFormViewModel model)
    {
        await CargarAsync(model);
        var archivo = await GuardarComprobanteAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var gasto = new Gasto
        {
            Fecha = model.Fecha.Date,
            Categoria = model.Categoria,
            Monto = model.Monto,
            Descripcion = model.Descripcion.Trim(),
            Proveedor = model.Proveedor?.Trim(),
            LoteId = model.LoteId,
            PeriodoProductivoId = model.PeriodoProductivoId,
            ComprobanteArchivo = archivo
        };
        _db.Gastos.Add(gasto);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Gasto), gasto.Id.ToString(),
            $"Registro de gasto ({model.Categoria}) por {model.Monto:N2}");

        TempData["Ok"] = "Gasto registrado y asociado al historial.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var g = await _db.Gastos.FindAsync(id);
        if (g is null) return NotFound();

        var vm = new GastoFormViewModel
        {
            Id = g.Id, Fecha = g.Fecha, Categoria = g.Categoria, Monto = g.Monto,
            Descripcion = g.Descripcion, Proveedor = g.Proveedor,
            LoteId = g.LoteId, PeriodoProductivoId = g.PeriodoProductivoId,
            ComprobanteActual = g.ComprobanteArchivo
        };
        await CargarAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Editar(GastoFormViewModel model)
    {
        var g = await _db.Gastos.FindAsync(model.Id);
        if (g is null) return NotFound();

        model.ComprobanteActual = g.ComprobanteArchivo;
        await CargarAsync(model);
        var nuevoArchivo = await GuardarComprobanteAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        g.Fecha = model.Fecha.Date;
        g.Categoria = model.Categoria;
        g.Monto = model.Monto;
        g.Descripcion = model.Descripcion.Trim();
        g.Proveedor = model.Proveedor?.Trim();
        g.LoteId = model.LoteId;
        g.PeriodoProductivoId = model.PeriodoProductivoId;
        if (nuevoArchivo is not null)
        {
            _archivos.Eliminar(g.ComprobanteArchivo);
            g.ComprobanteArchivo = nuevoArchivo;
        }

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Gasto), g.Id.ToString(), $"Edición del gasto #{g.Id}");
        TempData["Ok"] = "Gasto actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var g = await _db.Gastos.FindAsync(id);
        if (g is null) return NotFound();

        _archivos.Eliminar(g.ComprobanteArchivo);
        _db.Gastos.Remove(g);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(Gasto), id.ToString(), $"Eliminación del gasto #{id}");
        TempData["Ok"] = "Gasto eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Comprobante(int id)
    {
        var g = await _db.Gastos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (g?.ComprobanteArchivo is null) return NotFound();

        var ruta = _archivos.RutaFisica(g.ComprobanteArchivo);
        if (ruta is null) return NotFound();

        var stream = System.IO.File.OpenRead(ruta);
        return File(stream, LocalFileStorageService.ContentType(g.ComprobanteArchivo));
    }

    // ---------- helpers ----------
    private async Task<string?> GuardarComprobanteAsync(GastoFormViewModel model)
    {
        if (model.Comprobante is null || model.Comprobante.Length == 0)
            return null;
        try
        {
            return await _archivos.GuardarComprobanteAsync(model.Comprobante);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Comprobante), ex.Message);
            return null;
        }
    }

    private async Task CargarAsync(GastoFormViewModel vm)
    {
        vm.Lotes = await _catalogo.LotesSeleccionablesAsync();
        vm.Periodos = await _catalogo.PeriodosAsync();
    }
}
