using FincaNova.Web.Areas.Enfermedades.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Enfermedades;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Enfermedades.Controllers;

/// <summary>
/// Registro de tratamientos aplicados sobre un brote activo (RF-17, HU-08).
/// </summary>
[Area("Enfermedades")]
[Authorize]
public class TratamientosController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;

    public TratamientosController(AppDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
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
            Lote = $"{registro.Lote.Codigo} — {registro.Lote.Nombre}"
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

        if (!ModelState.IsValid)
            return View(model);

        _db.Tratamientos.Add(new Tratamiento
        {
            RegistroEnfermedadId = registro.Id,
            Descripcion = model.Descripcion.Trim(),
            ProductoTexto = model.Producto?.Trim(),
            Dosis = model.Dosis?.Trim(),
            FechaAplicacion = model.FechaAplicacion.Date,
            ResultadoObservado = model.ResultadoObservado?.Trim()
        });

        // Al aplicar un tratamiento, el seguimiento pasa a "En tratamiento" si estaba solo detectada.
        if (registro.Estado == EstadoEnfermedad.Detectada)
            registro.Estado = EstadoEnfermedad.EnTratamiento;

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Tratamiento), registro.Id.ToString(),
            $"Tratamiento aplicado a la detección #{registro.Id}: {model.Descripcion}");

        TempData["Ok"] = "Tratamiento registrado y asociado a la enfermedad.";
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
