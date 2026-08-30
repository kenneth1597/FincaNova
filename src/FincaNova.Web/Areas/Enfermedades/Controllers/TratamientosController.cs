using FincaNova.Web.Areas.Enfermedades.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Enfermedades;
using FincaNova.Web.Domain.Finanzas;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Alertas;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Enfermedades.Controllers;

/// <summary>
/// Registro de tratamientos aplicados sobre un brote activo (RF-17, HU-08).
/// Opcionalmente descuenta el insumo utilizado del inventario (HU-24).
/// </summary>
[Area("Enfermedades")]
[Authorize]
public class TratamientosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;
    private readonly IAlertaService _alertas;

    public TratamientosController(AppDbContext db, IAuditService audit, ICatalogoService catalogo, IAlertaService alertas)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
        _alertas = alertas;
    }

    [HttpGet]
    public async Task<IActionResult> Crear(int registroId)
    {
        var registro = await _db.RegistrosEnfermedad.AsNoTracking()
            .Include(r => r.Lote).Include(r => r.TipoEnfermedad)
            .FirstOrDefaultAsync(r => r.Id == registroId);
        if (registro is null) return NotFound();

        return View(new TratamientoFormViewModel
        {
            RegistroEnfermedadId = registro.Id,
            Enfermedad = registro.TipoEnfermedad.Nombre,
            Lote = $"{registro.Lote.Codigo} — {registro.Lote.Nombre}",
            Insumos = await _catalogo.InsumosActivosAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(TratamientoFormViewModel model)
    {
        var registro = await _db.RegistrosEnfermedad
            .Include(r => r.Lote).Include(r => r.TipoEnfermedad)
            .FirstOrDefaultAsync(r => r.Id == model.RegistroEnfermedadId);
        if (registro is null) return NotFound();

        model.Enfermedad = registro.TipoEnfermedad.Nombre;
        model.Lote = $"{registro.Lote.Codigo} — {registro.Lote.Nombre}";
        model.Insumos = await _catalogo.InsumosActivosAsync();

        Insumo? insumo = null;
        if (model.InsumoId is { } insumoId)
        {
            insumo = await _db.Insumos.FirstOrDefaultAsync(i => i.Id == insumoId && i.Activo);
            if (insumo is null)
                ModelState.AddModelError(nameof(model.InsumoId), "El insumo seleccionado no es válido.");
            else if (model.CantidadInsumo <= 0)
                ModelState.AddModelError(nameof(model.CantidadInsumo), "Indique la cantidad usada del inventario.");
            else if (model.CantidadInsumo > insumo.StockActual)
                ModelState.AddModelError(nameof(model.CantidadInsumo),
                    $"No es posible registrar la labor: inventario insuficiente. Existencias actuales: {insumo.StockActual:N2} {insumo.UnidadMedida}.");
        }

        if (!ModelState.IsValid)
            return View(model);

        _db.Tratamientos.Add(new Tratamiento
        {
            RegistroEnfermedadId = registro.Id,
            Descripcion = model.Descripcion.Trim(),
            ProductoTexto = insumo?.Nombre ?? model.Producto?.Trim(),
            InsumoId = insumo?.Id,
            CantidadInsumoUsada = insumo is null ? 0 : model.CantidadInsumo,
            Dosis = model.Dosis?.Trim(),
            FechaAplicacion = model.FechaAplicacion.Date,
            ResultadoObservado = model.ResultadoObservado?.Trim()
        });

        if (insumo is not null)
        {
            insumo.StockActual -= model.CantidadInsumo;
            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                InsumoId = insumo.Id,
                Tipo = TipoMovimientoInventario.Salida,
                Cantidad = model.CantidadInsumo,
                Fecha = model.FechaAplicacion.Date,
                Motivo = $"Tratamiento fitosanitario · detección #{registro.Id}"
            });
        }

        if (registro.Estado == EstadoEnfermedad.Detectada)
            registro.Estado = EstadoEnfermedad.EnTratamiento;

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Tratamiento), registro.Id.ToString(),
            $"Tratamiento aplicado a la detección #{registro.Id}: {model.Descripcion}");
        if (insumo is not null)
            await _alertas.VerificarStockMinimoAsync(insumo.Id);

        TempData["Ok"] = insumo is not null
            ? $"Tratamiento registrado. Se descontaron {model.CantidadInsumo:N2} {insumo.UnidadMedida} de «{insumo.Nombre}»."
            : "Tratamiento registrado y asociado a la enfermedad.";
        return RedirectToAction("Detalle", "Enfermedades", new { id = registro.Id });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var t = await _db.Tratamientos.FindAsync(id);
        if (t is null) return NotFound();

        var registroId = t.RegistroEnfermedadId;
        _db.Tratamientos.Remove(t);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(Tratamiento), registroId.ToString(),
            $"Eliminación de un tratamiento de la detección #{registroId}");

        TempData["Ok"] = "Tratamiento eliminado.";
        return RedirectToAction("Detalle", "Enfermedades", new { id = registroId });
    }
}
