using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;

namespace FincaNova.Web.Areas.Labores.Models;

/// <summary>Fila del listado de labores (HU-01, HU-03).</summary>
public class LaborListItemViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Lote { get; set; } = string.Empty;
    public string TipoLabor { get; set; } = string.Empty;
    public ModalidadPago Modalidad { get; set; }
    public decimal Cantidad { get; set; }
    public int Colaboradores { get; set; }
    public decimal CostoCalculado { get; set; }
}

/// <summary>Filtros del listado de labores.</summary>
public class LaborFiltroViewModel
{
    public int? LoteId { get; set; }
    public int? PeriodoId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public IReadOnlyList<LaborListItemViewModel> Resultados { get; set; } = Array.Empty<LaborListItemViewModel>();
    public decimal TotalCosto { get; set; }
    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Formulario de registro y edición de una labor agrícola (HU-01, HU-02, HU-03).</summary>
public class LaborFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El tipo de labor es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Tipo de labor")]
    public string TipoLabor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un lote.")]
    [Display(Name = "Lote / micro lote")]
    public int? LoteId { get; set; }

    [Display(Name = "Período productivo")]
    public int? PeriodoProductivoId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Display(Name = "Modalidad de pago")]
    public ModalidadPago Modalidad { get; set; } = ModalidadPago.PorJornada;

    [Range(0, 1000, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    [Display(Name = "Cantidad (jornadas u horas por colaborador)")]
    public decimal Cantidad { get; set; } = 1;

    [Range(0, 10_000_000, ErrorMessage = "La tarifa no puede ser negativa.")]
    [Display(Name = "Tarifa (opcional: reemplaza la del colaborador)")]
    public decimal CostoUnitario { get; set; }

    [StringLength(600)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Colaboradores")]
    public List<int> ColaboradorIds { get; set; } = new();

    public bool EsEdicion => Id != 0;

    // catálogos
    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
    public IReadOnlyList<ColaboradorOpcion> Colaboradores { get; set; } = Array.Empty<ColaboradorOpcion>();

    public record ColaboradorOpcion(int Id, string Nombre, string Identificacion, decimal TarifaJornada, decimal TarifaHora);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Cantidad <= 0)
            yield return new ValidationResult("La duración debe ser mayor que cero.", new[] { nameof(Cantidad) });

        if (ColaboradorIds is null || ColaboradorIds.Count == 0)
            yield return new ValidationResult("Debe asignar al menos un colaborador.", new[] { nameof(ColaboradorIds) });
    }
}

/// <summary>Detalle de una labor con el desglose por colaborador.</summary>
public class LaborDetalleViewModel
{
    public Domain.Labores.Labor Labor { get; set; } = null!;
    public string Lote { get; set; } = string.Empty;
    public string? Periodo { get; set; }

    public record Linea(string Colaborador, decimal Cantidad, decimal Costo);
    public IReadOnlyList<Linea> Desglose { get; set; } = Array.Empty<Linea>();
}
