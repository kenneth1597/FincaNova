using System.Security.Claims;
using System.Text.Json;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Auditoria;

namespace FincaNova.Web.Services.Auditoria;

/// <inheritdoc />
public class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task RegistrarAsync(
        AccionAuditoria accion,
        string entidad,
        string? entidadId = null,
        string? descripcion = null,
        object? valorAnterior = null,
        object? valorNuevo = null,
        CancellationToken cancellationToken = default)
    {
        var user = _http.HttpContext?.User;

        var log = new AuditLog
        {
            FechaHora = DateTime.UtcNow,
            UsuarioId = user?.FindFirstValue(ClaimTypes.NameIdentifier),
            Usuario = user?.Identity?.Name ?? "sistema",
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            DireccionIp = _http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            Descripcion = descripcion,
            ValorAnterior = valorAnterior is null ? null : JsonSerializer.Serialize(valorAnterior, JsonOptions),
            ValorNuevo = valorNuevo is null ? null : JsonSerializer.Serialize(valorNuevo, JsonOptions)
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
