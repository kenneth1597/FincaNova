using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;

namespace FincaNova.Web.Domain.Finanzas;

/// <summary>
/// Insumo del inventario de la finca (fertilizantes, fitosanitarios, herramientas).
/// Genera alerta cuando el stock cae bajo el mínimo (RF-26, HU-22, HU-23, HU-25).
/// </summary>
public class Insumo : AuditableEntity
{
    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    public TipoInsumo Tipo { get; set; } = TipoInsumo.Otro;

    [Required, StringLength(30)]
    public string UnidadMedida { get; set; } = "unidad";

    [Range(0, 10_000_000)]
    public decimal StockActual { get; set; }

    [Range(0, 10_000_000)]
    public decimal StockMinimo { get; set; }

    /// <summary>Obligatoria para insumos fitosanitarios / químicos (HU-22 esc. 2).</summary>
    [DataType(DataType.Date)]
    public DateTime? FechaVencimiento { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();

    public bool BajoStockMinimo => StockActual <= StockMinimo;
}

/// <summary>Entrada, salida o ajuste de existencias de un insumo (HU-22, HU-24).</summary>
public class MovimientoInventario : AuditableEntity
{
    public int InsumoId { get; set; }

    public Insumo Insumo { get; set; } = null!;

    public TipoMovimientoInventario Tipo { get; set; }

    [Range(0.0001, 10_000_000)]
    public decimal Cantidad { get; set; }

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [StringLength(200)]
    public string? Motivo { get; set; }

    /// <summary>Costo total de la entrada, si aplica (para valuar el inventario).</summary>
    [Range(0, 1_000_000_000)]
    public decimal? Costo { get; set; }
}
