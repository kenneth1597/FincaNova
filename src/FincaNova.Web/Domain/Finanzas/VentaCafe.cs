using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Lotes;

namespace FincaNova.Web.Domain.Finanzas;

/// <summary>
/// Ingreso por venta de café. El total se calcula como cantidad × precio unitario
/// (RF-24, HU-20).
/// </summary>
public class VentaCafe : AuditableEntity
{
    public int? LoteId { get; set; }

    public Lote? Lote { get; set; }

    public int? PeriodoProductivoId { get; set; }

    public PeriodoProductivo? PeriodoProductivo { get; set; }

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [Required, StringLength(150)]
    public string Comprador { get; set; } = string.Empty;

    [StringLength(40)]
    public string UnidadMedida { get; set; } = "Fanega";

    [Range(0.01, 10_000_000)]
    public decimal Cantidad { get; set; }

    [Range(0.01, 1_000_000_000)]
    public decimal PrecioUnitario { get; set; }

    [Range(0, 1_000_000_000_000)]
    public decimal Total { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }
}
