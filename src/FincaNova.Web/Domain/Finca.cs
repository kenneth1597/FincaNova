using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Lotes;

namespace FincaNova.Web.Domain;

/// <summary>
/// Finca cafetalera. En la versión 1 existe un único registro (Café Chaperno);
/// la relación se mantiene en el modelo para habilitar multi-finca más adelante.
/// </summary>
public class Finca : AuditableEntity
{
    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Ubicacion { get; set; }

    [StringLength(150)]
    public string? Propietario { get; set; }

    [StringLength(30)]
    public string? Telefono { get; set; }

    public ConfiguracionFinca? Configuracion { get; set; }

    public ICollection<Lote> Lotes { get; set; } = new List<Lote>();

    public ICollection<PeriodoProductivo> PeriodosProductivos { get; set; } = new List<PeriodoProductivo>();
}

/// <summary>
/// Parámetros de negocio configurables por finca: reglas de conversión y umbrales
/// que usan los módulos de producción, enfermedades e inventario.
/// </summary>
public class ConfiguracionFinca : AuditableEntity
{
    public int FincaId { get; set; }

    public Finca Finca { get; set; } = null!;

    /// <summary>Kilogramos equivalentes a una cajuela recolectada (HU-13).</summary>
    [Range(0.1, 1000)]
    public decimal KilogramosPorCajuela { get; set; } = 12.5m;

    /// <summary>Símbolo de moneda para reportes y pantallas (colón por defecto).</summary>
    [StringLength(5)]
    public string SimboloMoneda { get; set; } = "₡";

    /// <summary>Repeticiones de una misma enfermedad en un lote que disparan alerta (HU-10, RF-19).</summary>
    [Range(2, 20)]
    public int UmbralRecurrenciaEnfermedad { get; set; } = 3;

    /// <summary>Ventana en días para evaluar la recurrencia de enfermedades.</summary>
    [Range(1, 365)]
    public int DiasVentanaRecurrencia { get; set; } = 30;

    /// <summary>Tamaño máximo permitido para comprobantes adjuntos, en MB (HU-52).</summary>
    [Range(1, 25)]
    public int TamanoMaximoAdjuntoMb { get; set; } = 5;
}
