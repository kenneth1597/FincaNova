using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;

namespace FincaNova.Web.Areas.Labores.Models;

/// <summary>Fila del listado de colaboradores (HU-05).</summary>
public class ColaboradorListItemViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public decimal TarifaJornada { get; set; }
    public EstadoColaborador Estado { get; set; }
    public int Labores { get; set; }
}

/// <summary>Formulario de alta y edición de colaborador (HU-04).</summary>
public class ColaboradorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(30)]
    [Display(Name = "Identificación (cédula)")]
    public string Identificacion { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El teléfono no es válido.")]
    [StringLength(30)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [StringLength(60)]
    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }

    [Range(0, 10_000_000, ErrorMessage = "La tarifa no puede ser negativa.")]
    [Display(Name = "Tarifa por jornada")]
    public decimal TarifaJornada { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "La tarifa no puede ser negativa.")]
    [Display(Name = "Tarifa por hora")]
    public decimal TarifaHora { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "La tarifa no puede ser negativa.")]
    [Display(Name = "Tarifa por cajuela")]
    public decimal TarifaCajuela { get; set; }

    public bool EsEdicion => Id != 0;
}

/// <summary>Detalle de un colaborador con su historial de labores (HU-05).</summary>
public class ColaboradorDetalleViewModel
{
    public Domain.Labores.Colaborador Colaborador { get; set; } = null!;

    public record ParticipacionLabor(DateTime Fecha, string Lote, string TipoLabor, decimal Cantidad, decimal Costo);

    public IReadOnlyList<ParticipacionLabor> Historial { get; set; } = Array.Empty<ParticipacionLabor>();
    public decimal TotalAcumulado { get; set; }
}
