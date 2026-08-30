using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Alertas;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Services.Alertas;

/// <summary>
/// Genera alertas automáticas del sistema. Por ahora, recurrencia de una misma
/// enfermedad en un lote (RF-19, HU-10). El Hito 6 añade las de stock mínimo.
/// </summary>
public interface IAlertaService
{
    /// <summary>
    /// Revisa si el tipo de enfermedad indicado se ha detectado en el lote al menos
    /// <c>UmbralRecurrenciaEnfermedad</c> veces dentro de <c>DiasVentanaRecurrencia</c>
    /// y, de ser así, genera una alerta (evitando duplicados sin atender).
    /// Devuelve la cantidad de detecciones consideradas.
    /// </summary>
    Task<int> VerificarRecurrenciaEnfermedadAsync(int loteId, int tipoEnfermedadId, DateTime referencia, CancellationToken ct = default);
}

public class AlertaService : IAlertaService
{
    private readonly AppDbContext _db;

    public AlertaService(AppDbContext db) => _db = db;

    public async Task<int> VerificarRecurrenciaEnfermedadAsync(int loteId, int tipoEnfermedadId, DateTime referencia, CancellationToken ct = default)
    {
        var config = await _db.ConfiguracionesFinca.AsNoTracking().FirstOrDefaultAsync(ct);
        var umbral = config?.UmbralRecurrenciaEnfermedad ?? 3;
        var dias = config?.DiasVentanaRecurrencia ?? 30;

        var desde = referencia.Date.AddDays(-dias);
        var detecciones = await _db.RegistrosEnfermedad.AsNoTracking()
            .CountAsync(r => r.LoteId == loteId
                && r.TipoEnfermedadId == tipoEnfermedadId
                && r.FechaDeteccion >= desde
                && r.FechaDeteccion <= referencia.Date, ct);

        if (detecciones < umbral)
            return detecciones;

        var yaExiste = await _db.Alertas.AnyAsync(a =>
            a.Tipo == TipoAlerta.EnfermedadRecurrente
            && a.ReferenciaId == loteId
            && a.ReferenciaSecundariaId == tipoEnfermedadId
            && !a.Atendida, ct);
        if (yaExiste)
            return detecciones;

        var lote = await _db.Lotes.AsNoTracking().Where(l => l.Id == loteId)
            .Select(l => l.Codigo + " — " + l.Nombre).FirstOrDefaultAsync(ct);
        var enfermedad = await _db.TiposEnfermedad.AsNoTracking().Where(t => t.Id == tipoEnfermedadId)
            .Select(t => t.Nombre).FirstOrDefaultAsync(ct);

        _db.Alertas.Add(new Alerta
        {
            Tipo = TipoAlerta.EnfermedadRecurrente,
            ReferenciaId = loteId,
            ReferenciaSecundariaId = tipoEnfermedadId,
            Mensaje = $"«{enfermedad}» se ha detectado {detecciones} veces en el lote {lote} " +
                      $"en los últimos {dias} días. Revise el manejo fitosanitario.",
            FechaGeneracion = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        return detecciones;
    }
}
