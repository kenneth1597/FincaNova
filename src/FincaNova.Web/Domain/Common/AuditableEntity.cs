namespace FincaNova.Web.Domain.Common;

/// <summary>
/// Base para las entidades del negocio. Aporta identificador y campos de
/// trazabilidad (RQNF-015) que el <c>AppDbContext</c> completa automáticamente.
/// </summary>
public abstract class AuditableEntity
{
    public int Id { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? CreadoPor { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? ModificadoPor { get; set; }
}
