using FincaNova.Web.Domain;

namespace FincaNova.Web.ViewSupport;

/// <summary>Textos y colores para mostrar enums del dominio en las vistas.</summary>
public static class Ui
{
    /// <summary>Símbolo de moneda de la finca (colón costarricense).</summary>
    public const string SimboloMoneda = "₡";

    /// <summary>Formatea un monto con el símbolo de moneda: <c>₡50 000,00</c>.</summary>
    public static string Money(decimal monto) => $"{SimboloMoneda}{monto:N2}";


    public static string EstadoLoteTexto(EstadoLote e) => e switch
    {
        EstadoLote.Activo => "Activo",
        EstadoLote.Inactivo => "Inactivo",
        EstadoLote.EnProduccion => "En producción",
        EstadoLote.EnDescanso => "En descanso",
        _ => e.ToString()
    };

    public static string EstadoLoteColor(EstadoLote e) => e switch
    {
        EstadoLote.Activo => "success",
        EstadoLote.EnProduccion => "primary",
        EstadoLote.EnDescanso => "warning",
        EstadoLote.Inactivo => "secondary",
        _ => "secondary"
    };

    public static string TipoLoteTexto(TipoLote t) => t == TipoLote.MicroLote ? "Micro lote" : "Lote";

    public static string EstadoColaboradorTexto(EstadoColaborador e) =>
        e == EstadoColaborador.Activo ? "Activo" : "Inactivo";

    public static string EstadoColaboradorColor(EstadoColaborador e) =>
        e == EstadoColaborador.Activo ? "success" : "secondary";

    public static string ModalidadPagoTexto(ModalidadPago m) => m switch
    {
        ModalidadPago.PorJornada => "Por jornada",
        ModalidadPago.PorHora => "Por hora",
        ModalidadPago.PorCajuela => "Por cajuela",
        _ => m.ToString()
    };

    public static string EstadoEnfermedadTexto(EstadoEnfermedad e) => e switch
    {
        EstadoEnfermedad.Detectada => "Detectada",
        EstadoEnfermedad.EnTratamiento => "En tratamiento",
        EstadoEnfermedad.Controlada => "Controlada",
        _ => e.ToString()
    };

    public static string EstadoEnfermedadColor(EstadoEnfermedad e) => e switch
    {
        EstadoEnfermedad.Detectada => "danger",
        EstadoEnfermedad.EnTratamiento => "warning",
        EstadoEnfermedad.Controlada => "success",
        _ => "secondary"
    };

    public static string EtapaProduccionTexto(EtapaProduccion e) => e switch
    {
        EtapaProduccion.Recoleccion => "Café recolectado",
        EtapaProduccion.CafeSeco => "Café seco",
        EtapaProduccion.CafeProcesado => "Café procesado (sin cáscara)",
        _ => e.ToString()
    };

    /// <summary>Formatea un peso en kilogramos: <c>1 234,50 kg</c>.</summary>
    public static string Kg(decimal kg) => $"{kg:N2} kg";

    public static string CategoriaGastoTexto(CategoriaGasto c) => c switch
    {
        CategoriaGasto.Insumos => "Insumos",
        CategoriaGasto.ManoDeObra => "Mano de obra",
        CategoriaGasto.Herramientas => "Herramientas",
        CategoriaGasto.Transporte => "Transporte",
        CategoriaGasto.Servicios => "Servicios",
        CategoriaGasto.Otros => "Otros",
        _ => c.ToString()
    };

    public static string TipoInsumoTexto(TipoInsumo t) => t switch
    {
        TipoInsumo.Fertilizante => "Fertilizante",
        TipoInsumo.Fitosanitario => "Fitosanitario / químico",
        TipoInsumo.Herramienta => "Herramienta",
        TipoInsumo.MaterialEmpaque => "Material de empaque",
        TipoInsumo.Otro => "Otro",
        _ => t.ToString()
    };

    public static string TipoMovimientoTexto(TipoMovimientoInventario t) => t switch
    {
        TipoMovimientoInventario.Entrada => "Entrada",
        TipoMovimientoInventario.Salida => "Salida",
        TipoMovimientoInventario.Ajuste => "Ajuste",
        _ => t.ToString()
    };
}
