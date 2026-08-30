using FincaNova.Web.Domain;

namespace FincaNova.Web.ViewSupport;

/// <summary>Textos y colores para mostrar enums del dominio en las vistas.</summary>
public static class Ui
{
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
}
