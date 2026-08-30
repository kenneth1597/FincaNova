using FincaNova.Web.Areas.Lotes.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Lotes;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Catalogos;
using FincaNova.Web.ViewSupport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Lotes.Controllers;

/// <summary>
/// Gestión de lotes y micro lotes: el registro central del sistema
/// (RF-06 a RF-10, HU-30 a HU-39 y HU-41).
/// </summary>
[Area("Lotes")]
[Authorize]
public class LotesController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICatalogoService _catalogo;

    public LotesController(AppDbContext db, IAuditService audit, ICatalogoService catalogo)
    {
        _db = db;
        _audit = audit;
        _catalogo = catalogo;
    }

    // ---------- Listado + panel por estado (HU-31, HU-32, HU-39) ----------
    [HttpGet]
    public async Task<IActionResult> Index(LoteFiltroViewModel filtro)
    {
        var baseQuery = _db.Lotes.AsNoTracking().Where(l => !l.Eliminado);

        filtro.Conteos = await baseQuery
            .GroupBy(l => l.Estado)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var query = baseQuery;
        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var t = filtro.Q.Trim();
            query = query.Where(l => l.Codigo.Contains(t) || l.Nombre.Contains(t));
        }
        if (filtro.Tipo is { } tipo) query = query.Where(l => l.Tipo == tipo);
        if (filtro.Estado is { } estado) query = query.Where(l => l.Estado == estado);

        filtro.Resultados = await query
            .OrderBy(l => l.Codigo)
            .Select(l => new LoteListItemViewModel
            {
                Id = l.Id,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                Tipo = l.Tipo,
                LotePadre = l.LotePadre != null ? l.LotePadre.Codigo : null,
                AreaHectareas = l.AreaHectareas,
                VariedadCafe = l.VariedadCafe,
                AnioSiembra = l.AnioSiembra,
                Estado = l.Estado,
                MicroLotes = l.MicroLotes.Count(m => !m.Eliminado)
            })
            .ToListAsync();

        return View(filtro);
    }

    // ---------- Detalle + bitácora del lote (HU-31, HU-41) ----------
    [HttpGet]
    public async Task<IActionResult> Detalle(int id, DateTime? desde, DateTime? hasta)
    {
        var lote = await _db.Lotes.AsNoTracking()
            .Include(l => l.LotePadre)
            .FirstOrDefaultAsync(l => l.Id == id && !l.Eliminado);
        if (lote is null) return NotFound();

        var microLotes = await _db.Lotes.AsNoTracking()
            .Where(l => l.LotePadreId == id && !l.Eliminado)
            .OrderBy(l => l.Codigo)
            .Select(l => new LoteListItemViewModel
            {
                Id = l.Id, Codigo = l.Codigo, Nombre = l.Nombre, Tipo = l.Tipo,
                AreaHectareas = l.AreaHectareas, VariedadCafe = l.VariedadCafe,
                AnioSiembra = l.AnioSiembra, Estado = l.Estado
            })
            .ToListAsync();

        var vm = new LoteDetalleViewModel
        {
            Lote = lote,
            MicroLotes = microLotes,
            Desde = desde,
            Hasta = hasta,
            Bitacora = await ConstruirBitacoraAsync(id, desde, hasta)
        };
        return View(vm);
    }

    // ---------- Alta (HU-30, HU-36) ----------
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public async Task<IActionResult> Crear(TipoLote tipo = TipoLote.Lote, int? padreId = null)
    {
        var vm = new LoteFormViewModel
        {
            Tipo = tipo,
            LotePadreId = padreId,
            LotesPadre = await LotesPadreAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(LoteFormViewModel model)
    {
        model.LotesPadre = await LotesPadreAsync();
        var fincaId = await _catalogo.FincaIdAsync();

        await ValidarComunAsync(model, fincaId);

        if (!ModelState.IsValid)
            return View(model);

        var lote = new Lote
        {
            FincaId = fincaId!.Value,
            Codigo = model.Codigo.Trim(),
            Nombre = model.Nombre.Trim(),
            Tipo = model.Tipo,
            LotePadreId = model.Tipo == TipoLote.MicroLote ? model.LotePadreId : null,
            AreaHectareas = model.AreaHectareas,
            VariedadCafe = model.VariedadCafe?.Trim(),
            AnioSiembra = model.AnioSiembra,
            Ubicacion = model.Ubicacion?.Trim(),
            Estado = EstadoLote.Activo,
            FechaUltimoCambioEstado = DateTime.UtcNow,
            EstadoCambiadoPor = User.Identity?.Name,
            Observaciones = model.Observaciones?.Trim()
        };

        _db.Lotes.Add(lote);
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(Lote), lote.Id.ToString(),
            $"Registro de {(lote.Tipo == TipoLote.MicroLote ? "micro lote" : "lote")} {lote.Codigo}",
            valorNuevo: new { lote.Codigo, lote.Nombre, lote.AreaHectareas, lote.VariedadCafe });

        TempData["Ok"] = $"{(lote.Tipo == TipoLote.MicroLote ? "Micro lote" : "Lote")} «{lote.Codigo}» registrado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = lote.Id });
    }

    // ---------- Edición (HU-33, HU-37) ----------
    [HttpGet]
    [Authorize(Roles = Roles.Gestion)]
    public async Task<IActionResult> Editar(int id)
    {
        var lote = await _db.Lotes.FirstOrDefaultAsync(l => l.Id == id && !l.Eliminado);
        if (lote is null) return NotFound();

        return View(new LoteFormViewModel
        {
            Id = lote.Id,
            Codigo = lote.Codigo,
            Nombre = lote.Nombre,
            Tipo = lote.Tipo,
            LotePadreId = lote.LotePadreId,
            AreaHectareas = lote.AreaHectareas,
            VariedadCafe = lote.VariedadCafe,
            AnioSiembra = lote.AnioSiembra,
            Ubicacion = lote.Ubicacion,
            Estado = lote.Estado,
            Observaciones = lote.Observaciones,
            LotesPadre = await LotesPadreAsync(exceptoId: lote.Id)
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(LoteFormViewModel model)
    {
        var lote = await _db.Lotes.FirstOrDefaultAsync(l => l.Id == model.Id && !l.Eliminado);
        if (lote is null) return NotFound();

        model.LotesPadre = await LotesPadreAsync(exceptoId: lote.Id);
        model.Codigo = lote.Codigo; // el código es la llave histórica y no se edita (HU-37 esc. 2)
        model.Tipo = lote.Tipo;

        if (model.AreaHectareas < 0)
            ModelState.AddModelError(nameof(model.AreaHectareas), "El área no puede ser negativa.");

        if (!ModelState.IsValid)
            return View(model);

        var antes = new { lote.Nombre, lote.AreaHectareas, lote.VariedadCafe, lote.AnioSiembra, lote.Ubicacion };

        lote.Nombre = model.Nombre.Trim();
        lote.AreaHectareas = model.AreaHectareas;
        lote.VariedadCafe = model.VariedadCafe?.Trim();
        lote.AnioSiembra = model.AnioSiembra;
        lote.Ubicacion = model.Ubicacion?.Trim();
        lote.Observaciones = model.Observaciones?.Trim();

        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Lote), lote.Id.ToString(),
            $"Edición del lote {lote.Codigo}", valorAnterior: antes,
            valorNuevo: new { lote.Nombre, lote.AreaHectareas, lote.VariedadCafe, lote.AnioSiembra, lote.Ubicacion });

        TempData["Ok"] = $"Lote «{lote.Codigo}» actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = lote.Id });
    }

    // ---------- Cambio de estado (HU-34) ----------
    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, EstadoLote estado)
    {
        var lote = await _db.Lotes.FirstOrDefaultAsync(l => l.Id == id && !l.Eliminado);
        if (lote is null) return NotFound();

        if (!Enum.IsDefined(estado))
        {
            TempData["Error"] = "Debe seleccionar un estado válido.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var anterior = lote.Estado;
        if (anterior == estado)
        {
            TempData["Error"] = "El lote ya se encuentra en ese estado.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        lote.Estado = estado;
        lote.FechaUltimoCambioEstado = DateTime.UtcNow;
        lote.EstadoCambiadoPor = User.Identity?.Name;
        await _db.SaveChangesAsync();

        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(Lote), lote.Id.ToString(),
            $"Cambio de estado del lote {lote.Codigo}: {Ui.EstadoLoteTexto(anterior)} → {Ui.EstadoLoteTexto(estado)}");

        TempData["Ok"] = $"El lote «{lote.Codigo}» pasó a estado «{Ui.EstadoLoteTexto(estado)}».";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    // ---------- Desactivar / eliminar (HU-35, HU-38) ----------
    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var lote = await _db.Lotes.FirstOrDefaultAsync(l => l.Id == id && !l.Eliminado);
        if (lote is null) return NotFound();

        lote.Estado = EstadoLote.Inactivo;
        lote.FechaUltimoCambioEstado = DateTime.UtcNow;
        lote.EstadoCambiadoPor = User.Identity?.Name;
        await _db.SaveChangesAsync();

        await _audit.RegistrarAsync(AccionAuditoria.Inactivar, nameof(Lote), lote.Id.ToString(),
            $"Inactivación del lote {lote.Codigo}; su historial se conserva");

        TempData["Ok"] = $"El lote «{lote.Codigo}» fue inactivado. Su historial se conservó.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Gestion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var lote = await _db.Lotes.FirstOrDefaultAsync(l => l.Id == id && !l.Eliminado);
        if (lote is null) return NotFound();

        if (await TieneRegistrosRelacionadosAsync(id))
        {
            TempData["Error"] = "El lote tiene actividades, cosechas o gastos asociados. " +
                                "Use «Inactivar» para conservar su historial.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        lote.Eliminado = true;
        lote.Estado = EstadoLote.Inactivo;
        await _db.SaveChangesAsync();
        await _audit.RegistrarAsync(AccionAuditoria.Eliminar, nameof(Lote), lote.Id.ToString(),
            $"Eliminación del lote {lote.Codigo} (sin registros asociados)");

        TempData["Ok"] = $"El lote «{lote.Codigo}» fue eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------
    private async Task ValidarComunAsync(LoteFormViewModel model, int? fincaId)
    {
        if (fincaId is null)
        {
            ModelState.AddModelError(string.Empty, "No hay una finca configurada.");
            return;
        }

        var codigo = model.Codigo.Trim();
        var existe = await _db.Lotes.AnyAsync(l => l.FincaId == fincaId && l.Codigo == codigo && l.Id != model.Id);
        if (existe)
            ModelState.AddModelError(nameof(model.Codigo), "El código de lote ya se encuentra registrado.");

        if (model.Tipo == TipoLote.MicroLote && model.LotePadreId is null)
            ModelState.AddModelError(nameof(model.LotePadreId), "Debe indicar el lote al que pertenece el micro lote.");

        if (model.AreaHectareas < 0)
            ModelState.AddModelError(nameof(model.AreaHectareas), "El área no puede ser negativa.");
    }

    private async Task<IEnumerable<(int Id, string Texto)>> LotesPadreAsync(int? exceptoId = null)
    {
        var lotes = await _db.Lotes.AsNoTracking()
            .Where(l => l.Tipo == TipoLote.Lote && !l.Eliminado && l.Id != exceptoId)
            .OrderBy(l => l.Codigo)
            .Select(l => new { l.Id, l.Codigo, l.Nombre })
            .ToListAsync();
        return lotes.Select(l => (l.Id, $"{l.Codigo} — {l.Nombre}")).ToList();
    }

    private async Task<bool> TieneRegistrosRelacionadosAsync(int loteId)
        => await _db.Labores.AnyAsync(x => x.LoteId == loteId)
        || await _db.RegistrosEnfermedad.AnyAsync(x => x.LoteId == loteId)
        || await _db.Recolecciones.AnyAsync(x => x.LoteId == loteId)
        || await _db.RegistrosProduccion.AnyAsync(x => x.LoteId == loteId)
        || await _db.Gastos.AnyAsync(x => x.LoteId == loteId)
        || await _db.Lotes.AnyAsync(x => x.LotePadreId == loteId && !x.Eliminado);

    private async Task<IReadOnlyList<BitacoraItemViewModel>> ConstruirBitacoraAsync(int loteId, DateTime? desde, DateTime? hasta)
    {
        var hastaExcl = hasta?.Date.AddDays(1);
        var items = new List<BitacoraItemViewModel>();

        var labores = await _db.Labores.AsNoTracking()
            .Where(l => l.LoteId == loteId
                && (desde == null || l.Fecha >= desde) && (hastaExcl == null || l.Fecha < hastaExcl))
            .Select(l => new { l.Fecha, l.TipoLabor, l.CostoCalculado })
            .ToListAsync();
        items.AddRange(labores.Select(l => new BitacoraItemViewModel
        {
            Fecha = l.Fecha, Tipo = "Labor", Icono = "bi-tools", Color = "primary",
            Descripcion = l.TipoLabor, Detalle = l.CostoCalculado > 0 ? $"Costo: {l.CostoCalculado:N2}" : null
        }));

        var enfermedades = await _db.RegistrosEnfermedad.AsNoTracking()
            .Where(r => r.LoteId == loteId
                && (desde == null || r.FechaDeteccion >= desde) && (hastaExcl == null || r.FechaDeteccion < hastaExcl))
            .Select(r => new { r.FechaDeteccion, Nombre = r.TipoEnfermedad.Nombre })
            .ToListAsync();
        items.AddRange(enfermedades.Select(e => new BitacoraItemViewModel
        {
            Fecha = e.FechaDeteccion, Tipo = "Enfermedad", Icono = "bi-bug", Color = "danger",
            Descripcion = $"Detección: {e.Nombre}"
        }));

        var recolecciones = await _db.Recolecciones.AsNoTracking()
            .Where(r => r.LoteId == loteId
                && (desde == null || r.Fecha >= desde) && (hastaExcl == null || r.Fecha < hastaExcl))
            .Select(r => new { r.Fecha, r.Cajuelas })
            .ToListAsync();
        items.AddRange(recolecciones.Select(r => new BitacoraItemViewModel
        {
            Fecha = r.Fecha, Tipo = "Recolección", Icono = "bi-basket", Color = "success",
            Descripcion = $"{r.Cajuelas:N2} cajuelas recolectadas"
        }));

        var produccion = await _db.RegistrosProduccion.AsNoTracking()
            .Where(r => r.LoteId == loteId
                && (desde == null || r.Fecha >= desde) && (hastaExcl == null || r.Fecha < hastaExcl))
            .Select(r => new { r.Fecha, r.Etapa, r.PesoKg })
            .ToListAsync();
        items.AddRange(produccion.Select(p => new BitacoraItemViewModel
        {
            Fecha = p.Fecha, Tipo = "Producción", Icono = "bi-box-seam", Color = "success",
            Descripcion = $"{p.Etapa}: {p.PesoKg:N2} kg"
        }));

        var gastos = await _db.Gastos.AsNoTracking()
            .Where(g => g.LoteId == loteId
                && (desde == null || g.Fecha >= desde) && (hastaExcl == null || g.Fecha < hastaExcl))
            .Select(g => new { g.Fecha, g.Categoria, g.Monto, g.Descripcion })
            .ToListAsync();
        items.AddRange(gastos.Select(g => new BitacoraItemViewModel
        {
            Fecha = g.Fecha, Tipo = "Gasto", Icono = "bi-cash-coin", Color = "warning",
            Descripcion = $"{g.Categoria}: {g.Monto:N2}", Detalle = g.Descripcion
        }));

        return items.OrderByDescending(i => i.Fecha).ToList();
    }
}
