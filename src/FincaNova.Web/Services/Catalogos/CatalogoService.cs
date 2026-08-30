using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Services.Catalogos;

/// <summary>
/// Consultas de apoyo reutilizadas por los distintos módulos para poblar
/// selectores (lotes disponibles, períodos, colaboradores).
/// </summary>
public interface ICatalogoService
{
    Task<int?> FincaIdAsync(CancellationToken ct = default);

    /// <summary>Lotes y micro lotes que pueden recibir operaciones (no inactivos ni eliminados, HU-38 esc. 2).</summary>
    Task<IReadOnlyList<(int Id, string Texto)>> LotesSeleccionablesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<(int Id, string Texto)>> PeriodosAsync(CancellationToken ct = default);

    Task<int?> PeriodoActivoIdAsync(CancellationToken ct = default);
}

public class CatalogoService : ICatalogoService
{
    private readonly AppDbContext _db;

    public CatalogoService(AppDbContext db) => _db = db;

    public async Task<int?> FincaIdAsync(CancellationToken ct = default)
        => await _db.Fincas.OrderBy(f => f.Id).Select(f => (int?)f.Id).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<(int Id, string Texto)>> LotesSeleccionablesAsync(CancellationToken ct = default)
    {
        var lotes = await _db.Lotes.AsNoTracking()
            .Where(l => !l.Eliminado && (l.Estado == EstadoLote.Activo || l.Estado == EstadoLote.EnProduccion))
            .OrderBy(l => l.Codigo)
            .Select(l => new { l.Id, l.Codigo, l.Nombre })
            .ToListAsync(ct);
        return lotes.Select(l => (l.Id, $"{l.Codigo} — {l.Nombre}")).ToList();
    }

    public async Task<IReadOnlyList<(int Id, string Texto)>> PeriodosAsync(CancellationToken ct = default)
    {
        var periodos = await _db.PeriodosProductivos.AsNoTracking()
            .OrderByDescending(p => p.FechaInicio)
            .Select(p => new { p.Id, p.Nombre, p.Activo })
            .ToListAsync(ct);
        return periodos.Select(p => (p.Id, p.Activo ? $"{p.Nombre} (activo)" : p.Nombre)).ToList();
    }

    public async Task<int?> PeriodoActivoIdAsync(CancellationToken ct = default)
        => await _db.PeriodosProductivos.Where(p => p.Activo).Select(p => (int?)p.Id).FirstOrDefaultAsync(ct);
}
