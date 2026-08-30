using FincaNova.Web.Areas.Enfermedades.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Enfermedades;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Alertas;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Enfermedades.Controllers;

/// <summary>
/// Registro y seguimiento de enfermedades y plagas del café
/// (RF-16 a RF-19, HU-07 a HU-10).
/// </summary>
[Area("Enfermedades")]
[Authorize]
public class EnfermedadesController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;
    private readonly IAlertaService _alertas;

    public EnfermedadesController(AppDbContext db, IAuditService audit, ICatalogoService catalogo, IAlertaService alertas)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
        _alertas = alertas;
    }

    // ---------- Historial con filtros avanzados (HU-09) ----------
    [HttpGet]
    public async Task<IActionResult> Index(EnfermedadFiltroViewModel filtro)
    {
        var query = _db.RegistrosEnfermedad.AsNoTracking().AsQueryable();

        if (filtro.LoteId is { } loteId) query = query.Where(r => r.LoteId == loteId);
        if (filtro.TipoEnfermedadId is { } tipoId) query = query.Where(r => r.TipoEnfermedadId == tipoId);
        if (filtro.Estado is { } estado) query = query.Where(r => r.Estado == estado);
        if (filtro.Desde is { } d) query = query.Where(r => r.FechaDeteccion >= d);
        if (filtro.Hasta is { } h) query = query.Where(r => r.FechaDeteccion <= h);
        if (!string.IsNullOrWhiteSpace(filtro.Tratamiento))
        {
            var t = filtro.Tratamiento.Trim();
            query = query.Where(r => r.Tratamientos.Any(x =>
                x.Descripcion.Contains(t) || (x.ProductoTexto != null && x.ProductoTexto.Contains(t))));
        }

        filtro.Resultados = await query
            .OrderByDescending(r => r.FechaDeteccion).ThenByDescending(r => r.Id)
            .Select(r => new EnfermedadListItemViewModel
            {
                Id = r.Id,
                FechaDeteccion = r.FechaDeteccion,
                Lote = r.Lote.Codigo + " — " + r.Lote.Nombre,
                Enfermedad = r.TipoEnfermedad.Nombre,
                Estado = r.Estado,
                Tratamientos = r.Tratamientos.Count,
                UltimoTratamiento = r.Tratamientos
                    .OrderByDescending(x => x.FechaAplicacion)
                    .Select(x => x.Descripcion).FirstOrDefault()
            })
            .ToListAsync();

        filtro.Lotes = await _catalogo.LotesOperativosAsync();
        filtro.Tipos = await _catalogo.TiposEnfermedadAsync();
        return View(filtro);
    }

    // ---------- Detalle + tratamientos (HU-08, HU-09) ----------
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var registro = await _db.RegistrosEnfermedad.AsNoTracking()
            .Include(r => r.Lote)
            .Include(r => r.TipoEnfermedad)
            .Include(r => r.PeriodoProductivo)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (registro is null) return NotFound();

        var tratamientos = await _db.Tratamientos.AsNoTracking()
            .Where(t => t.RegistroEnfermedadId == id)
            .OrderByDescending(t => t.FechaAplicacion)
            .ToListAsync();

        var config = await _db.ConfiguracionesFinca.AsNoTracking().FirstOrDefaultAsync();
        var dias = config?.DiasVentanaRecurrencia ?? 30;
        var umbral = config?.UmbralRecurrenciaEnfermedad ?? 3;
        var desde = registro.FechaDeteccion.AddDays(-dias);
        var enVentana = await _db.RegistrosEnfermedad.CountAsync(r =>
            r.LoteId == registro.LoteId && r.TipoEnfermedadId == registro.TipoEnfermedadId
            && r.FechaDeteccion >= desde && r.FechaDeteccion <= registro.FechaDeteccion);

        return View(new EnfermedadDetalleViewModel
        {
            Registro = registro,
            Lote = $"{registro.Lote.Codigo} — {registro.Lote.Nombre}",
            Enfermedad = registro.TipoEnfermedad.Nombre,
            Periodo = registro.PeriodoProductivo?.Nombre,
            Tratamientos = tratamientos,
            DeteccionesEnVentana = enVentana,
            AlertaRecurrencia = enVentana >= umbral
        });
    }

    // ---------- Alta (HU-07) ----------
    [HttpGet]
    public async Task<IActionResult> Crear(int? loteId = null)
    {
        var vm = new EnfermedadFormViewModel
        {
            LoteId = loteId,
            PeriodoProductivoId = await _catalogo.PeriodoActivoIdAsync()
        };
        await CargarCatalogosAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(EnfermedadFormViewModel model)
    {
        await CargarCatalogosAsync(model);
        await ValidarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        var registro = new RegistroEnfermedad
        {
            LoteId = model.LoteId!.Value,
            TipoEnfermedadId = model.TipoEnfermedadId!.Value,
            PeriodoProductivoId = model.PeriodoProductivoId,
            FechaDeteccion = model.FechaDeteccion.Date,
            Estado = model.Estado,
            Sintomas = model.Sintomas?.Trim()
        };
        _db.RegistrosEnfermedad.Add(registro);
        await _db.SaveChangesAsync();

        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(RegistroEnfermedad), registro.Id.ToString(),
            $"Detección de enfermedad (tipo {registro.TipoEnfermedadId}) en el lote {registro.LoteId}");

        var detecciones = await _alertas.VerificarRecurrenciaEnfermedadAsync(
            registro.LoteId, registro.TipoEnfermedadId, registro.FechaDeteccion);

        var config = await _db.ConfiguracionesFinca.AsNoTracking().FirstOrDefaultAsync();
        if (detecciones >= (config?.UmbralRecurrenciaEnfermedad ?? 3))
            TempData["Error"] = $"Atención: esta enfermedad ya se ha detectado {detecciones} veces en este lote. Se generó una alerta.";
        else
            TempData["Ok"] = "Detección de enfermedad registrada y asociada al lote.";

        return RedirectToAction(nameof(Detalle), new { id = registro.Id });
    }

    // ---------- Edición (HU-07) ----------
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var r = await _db.RegistrosEnfermedad.FindAsync(id);
        if (r is null) return NotFound();

        var vm = new EnfermedadFormViewModel
        {
            Id = r.Id,
            LoteId = r.LoteId,
            TipoEnfermedadId = r.TipoEnfermedadId,
            PeriodoProductivoId = r.PeriodoProductivoId,
            FechaDeteccion = r.FechaDeteccion,
            Estado = r.Estado,
            Sintomas = r.Sintomas
        };
        await CargarCatalogosAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EnfermedadFormViewModel model)
    {
        var r = await _db.RegistrosEnfermedad.FindAsync(model.Id);
        if (r is null) return NotFound();

        await CargarCatalogosAsync(model);
        await ValidarAsync(model);
        if (!ModelState.IsValid)
            return View(model);

        r.LoteId = model.LoteId!.Value;
        r.TipoEnfermedadId = model.TipoEnfermedadId!.Value;
        r.PeriodoProductivoId = model.PeriodoProductivoId;
        r.FechaDeteccion = model.FechaDeteccion.Date;
        r.Estado = model.Estado;
        r.Sintomas = model.Sintomas?.Trim();

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(RegistroEnfermedad), r.Id.ToString(),
            $"Edición de la detección #{r.Id}");

        TempData["Ok"] = "Registro de enfermedad actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = r.Id });
    }

    // ---------- Cambio de estado del seguimiento ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoEnfermedad estado)
    {
        var r = await _db.RegistrosEnfermedad.FindAsync(id);
        if (r is null) return NotFound();

        r.Estado = estado;
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(RegistroEnfermedad), r.Id.ToString(),
            $"Estado del seguimiento #{r.Id} → {estado}");

        TempData["Ok"] = "Estado del seguimiento actualizado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await _db.RegistrosEnfermedad.Include(x => x.Tratamientos).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();

        _db.Tratamientos.RemoveRange(r.Tratamientos);
        _db.RegistrosEnfermedad.Remove(r);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(RegistroEnfermedad), id.ToString(),
            $"Eliminación de la detección #{id}");

        TempData["Ok"] = "Registro de enfermedad eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------
    private async Task ValidarAsync(EnfermedadFormViewModel model)
    {
        if (model.LoteId is { } loteId)
        {
            var ok = await _db.Lotes.AnyAsync(l => l.Id == loteId && !l.Eliminado && l.Estado != EstadoLote.Inactivo);
            if (!ok) ModelState.AddModelError(nameof(model.LoteId), "El lote seleccionado no existe o está inactivo.");
        }
        if (model.TipoEnfermedadId is { } tipoId)
        {
            var ok = await _db.TiposEnfermedad.AnyAsync(t => t.Id == tipoId && t.Activo);
            if (!ok) ModelState.AddModelError(nameof(model.TipoEnfermedadId), "La enfermedad seleccionada no es válida.");
        }
    }

    private async Task CargarCatalogosAsync(EnfermedadFormViewModel vm)
    {
        vm.Lotes = await _catalogo.LotesOperativosAsync();
        vm.Tipos = await _catalogo.TiposEnfermedadAsync();
        vm.Periodos = await _catalogo.PeriodosAsync();
    }
}
