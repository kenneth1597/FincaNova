using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Areas.Finanzas.Models;

public class VentaListItemViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Comprador { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public string? Lote { get; set; }
}

public class VentaFiltroViewModel
{
    public int? PeriodoId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public IReadOnlyList<VentaListItemViewModel> Resultados { get; set; } = Array.Empty<VentaListItemViewModel>();
    public decimal Total { get; set; }
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

public class VentaFormViewModel
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de la venta")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Indique el comprador.")]
    [StringLength(150)]
    [Display(Name = "Comprador")]
    public string Comprador { get; set; } = string.Empty;

    [Display(Name = "Unidad de medida")]
    public string UnidadMedida { get; set; } = "Fanega";

    [Range(0.01, 10_000_000, ErrorMessage = "La cantidad debe ser un número mayor a cero.")]
    [Display(Name = "Cantidad")]
    public decimal Cantidad { get; set; }

    [Range(0.01, 1_000_000_000, ErrorMessage = "El precio unitario debe ser un número mayor a cero.")]
    [Display(Name = "Precio unitario")]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "Lote (opcional)")]
    public int? LoteId { get; set; }

    [Display(Name = "Período productivo (opcional)")]
    public int? PeriodoProductivoId { get; set; }

    [StringLength(400)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public decimal Total => decimal.Round(Cantidad * PrecioUnitario, 2);
    public bool EsEdicion => Id != 0;

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}
