using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;

namespace FincaNova.Web.Areas.Finanzas.Models;

public class InsumoListItemViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoInsumo Tipo { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool BajoMinimo => StockActual <= StockMinimo;
    public bool Vencido => FechaVencimiento is { } f && f.Date < DateTime.Today;
}

public class InsumoFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre del insumo")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Tipo")]
    public TipoInsumo Tipo { get; set; } = TipoInsumo.Fertilizante;

    [Required(ErrorMessage = "Indique la unidad de medida.")]
    [StringLength(30)]
    [Display(Name = "Unidad de medida")]
    public string UnidadMedida { get; set; } = "kg";

    [Range(0, 10_000_000)]
    [Display(Name = "Existencia inicial")]
    public decimal StockInicial { get; set; }

    [Range(0, 10_000_000, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    [Display(Name = "Stock mínimo (umbral de alerta)")]
    public decimal StockMinimo { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    public bool EsEdicion => Id != 0;
    public decimal StockActual { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Tipo == TipoInsumo.Fitosanitario && FechaVencimiento is null)
            yield return new ValidationResult(
                "Para insumos fitosanitarios / químicos es obligatorio registrar la fecha de caducidad.",
                new[] { nameof(FechaVencimiento) });
    }
}

public class MovimientoFormViewModel
{
    public int InsumoId { get; set; }
    public string InsumoNombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal StockActual { get; set; }

    [Display(Name = "Tipo de movimiento")]
    public TipoMovimientoInventario Tipo { get; set; } = TipoMovimientoInventario.Entrada;

    [Range(0.0001, 10_000_000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    [Display(Name = "Cantidad")]
    public decimal Cantidad { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [StringLength(200)]
    [Display(Name = "Motivo / referencia")]
    public string? Motivo { get; set; }

    [Range(0, 1_000_000_000)]
    [Display(Name = "Costo total (opcional, solo entradas)")]
    public decimal? Costo { get; set; }
}

public class InsumoDetalleViewModel
{
    public Domain.Finanzas.Insumo Insumo { get; set; } = null!;
    public IReadOnlyList<Domain.Finanzas.MovimientoInventario> Movimientos { get; set; } = Array.Empty<Domain.Finanzas.MovimientoInventario>();
}
