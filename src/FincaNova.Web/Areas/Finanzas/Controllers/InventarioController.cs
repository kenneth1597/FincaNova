using FincaNova.Web.Areas.Finanzas.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Finanzas;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Alertas;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Finanzas.Controllers;

/// <summary>
/// Inventario de insumos: existencias, movimientos y alertas de stock mínimo
/// (RF-26, HU-22, HU-23, HU-25). Consultar existencias es visible para todos;
/// registrar movimientos e insumos requiere gestión.
/// </summary>
[Area("Finanzas")]
[Authorize]
public class InventarioController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IAlertaService _alertas;

    public InventarioController(AppDbContext db, IAuditService audit, IAlertaService alertas)
    {
        _db = db;
        _audit = audit;
        _alertas = alertas;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Insumos.AsNoTracking().Where(i => i.Activo);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim();
            query = query.Where(i => i.Nombre.Contains(t));
        }

        var filas = await query
            .OrderBy(i => i.Nombre)
            .Select(i => new InsumoListItemViewModel
            {
                Id = i.Id, Nombre = i.Nombre, Tipo = i.Tipo, UnidadMedida = i.UnidadMedida,
                StockActual = i.StockActual, StockMinimo = i.StockMinimo, FechaVencimiento = i.FechaVencimiento
            })
            .ToListAsync();

        ViewBag.Query = q;
        return View(filas);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var insumo = await _db.Insumos.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (insumo is null) return NotFound();

        var movimientos = await _db.MovimientosInventario.AsNoTracking()
            .Where(m => m.InsumoId == id)
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .ToListAsync();

        return View(new InsumoDetalleViewModel { Insumo = insumo, Movimientos = movimientos });
    }

    // ---------- Insumo ----------
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public IActionResult Crear() => View(new InsumoFormViewModel());

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(InsumoFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var insumo = new Insumo
        {
            Nombre = model.Nombre.Trim(),
            Tipo = model.Tipo,
            UnidadMedida = model.UnidadMedida.Trim(),
            StockActual = model.StockInicial,
            StockMinimo = model.StockMinimo,
            FechaVencimiento = model.FechaVencimiento,
            Activo = true
        };
        _db.Insumos.Add(insumo);
        await _db.SaveChangesAsync();

        if (model.StockInicial > 0)
        {
            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                InsumoId = insumo.Id, Tipo = TipoMovimientoInventario.Entrada,
                Cantidad = model.StockInicial, Fecha = DateTime.Today, Motivo = "Existencia inicial"
            });
            await _db.SaveChangesAsync();
        }

        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Insumo), insumo.Id.ToString(),
            $"Alta de insumo {insumo.Nombre}");
        await _alertas.VerificarStockMinimoAsync(insumo.Id);

        TempData["Ok"] = $"Insumo «{insumo.Nombre}» registrado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public async Task<IActionResult> Editar(int id)
    {
        var i = await _db.Insumos.FindAsync(id);
        if (i is null) return NotFound();

        return View(new InsumoFormViewModel
        {
            Id = i.Id, Nombre = i.Nombre, Tipo = i.Tipo, UnidadMedida = i.UnidadMedida,
            StockMinimo = i.StockMinimo, FechaVencimiento = i.FechaVencimiento, StockActual = i.StockActual
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(InsumoFormViewModel model)
    {
        var i = await _db.Insumos.FindAsync(model.Id);
        if (i is null) return NotFound();

        model.StockActual = i.StockActual;
        if (!ModelState.IsValid)
            return View(model);

        i.Nombre = model.Nombre.Trim();
        i.Tipo = model.Tipo;
        i.UnidadMedida = model.UnidadMedida.Trim();
        i.StockMinimo = model.StockMinimo;
        i.FechaVencimiento = model.FechaVencimiento;
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Insumo), i.Id.ToString(), $"Edición del insumo {i.Nombre}");
        await _alertas.VerificarStockMinimoAsync(i.Id);

        TempData["Ok"] = "Insumo actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = i.Id });
    }

    // ---------- Movimientos (HU-22) ----------
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public async Task<IActionResult> Movimiento(int insumoId)
    {
        var i = await _db.Insumos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == insumoId);
        if (i is null) return NotFound();

        return View(new MovimientoFormViewModel
        {
            InsumoId = i.Id, InsumoNombre = i.Nombre, UnidadMedida = i.UnidadMedida, StockActual = i.StockActual
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Movimiento(MovimientoFormViewModel model)
    {
        var i = await _db.Insumos.FindAsync(model.InsumoId);
        if (i is null) return NotFound();

        model.InsumoNombre = i.Nombre;
        model.UnidadMedida = i.UnidadMedida;
        model.StockActual = i.StockActual;

        if (model.Tipo == TipoMovimientoInventario.Salida && model.Cantidad > i.StockActual)
            ModelState.AddModelError(nameof(model.Cantidad),
                $"No hay existencias suficientes. Disponible: {i.StockActual:N2} {i.UnidadMedida}.");

        if (!ModelState.IsValid)
            return View(model);

        i.StockActual = model.Tipo switch
        {
            TipoMovimientoInventario.Entrada => i.StockActual + model.Cantidad,
            TipoMovimientoInventario.Salida => i.StockActual - model.Cantidad,
            TipoMovimientoInventario.Ajuste => model.Cantidad,
            _ => i.StockActual
        };

        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            InsumoId = i.Id, Tipo = model.Tipo, Cantidad = model.Cantidad,
            Fecha = model.Fecha.Date, Motivo = model.Motivo?.Trim(), Costo = model.Costo
        });
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Insumo), i.Id.ToString(),
            $"Movimiento de inventario ({model.Tipo}) de {model.Cantidad:N2} en {i.Nombre}");
        await _alertas.VerificarStockMinimoAsync(i.Id);

        TempData["Ok"] = "Movimiento de inventario registrado. Inventario actualizado exitosamente.";
        return RedirectToAction(nameof(Detalle), new { id = i.Id });
    }
}
