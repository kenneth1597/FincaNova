using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Lotes;

namespace FincaNova.Web.Domain.Finanzas;

/// <summary>
/// Salida de dinero de la finca: compra de insumos, planillas, herramientas, etc.
/// Puede llevar adjunto el comprobante digital (RF-23, HU-19, HU-52).
/// </summary>
public class Gasto : AuditableEntity
{
    /// <summary>Lote al que se imputa el gasto. Opcional: hay gastos generales de finca.</summary>
    public int? LoteId { get; set; }

    public Lote? Lote { get; set; }

    public int? PeriodoProductivoId { get; set; }

    public PeriodoProductivo? PeriodoProductivo { get; set; }

    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.Otros;

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    /// <summary>Debe ser mayor que cero (HU-19 esc. 3).</summary>
    [Range(0.01, 1_000_000_000)]
    public decimal Monto { get; set; }

    [Required, StringLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Proveedor { get; set; }

    /// <summary>Ruta relativa del comprobante almacenado en el servidor (PDF/JPG/PNG ≤ 5 MB).</summary>
    [StringLength(260)]
    public string? ComprobanteArchivo { get; set; }
}
