namespace FincaNova.Web.Domain;

/// <summary>Tipo de unidad productiva.</summary>
public enum TipoLote
{
    Lote = 1,
    MicroLote = 2
}

/// <summary>Estado operativo de un lote o micro lote (HU-34, HU-38, HU-39).</summary>
public enum EstadoLote
{
    Activo = 1,
    Inactivo = 2,
    EnProduccion = 3,
    EnDescanso = 4
}

/// <summary>Estado de un colaborador de la finca.</summary>
public enum EstadoColaborador
{
    Activo = 1,
    Inactivo = 2
}

/// <summary>Forma en que se le paga a un colaborador por una labor.</summary>
public enum ModalidadPago
{
    PorJornada = 1,
    PorHora = 2,
    PorCajuela = 3
}

/// <summary>Estado del seguimiento de una enfermedad detectada (HU-07..10).</summary>
public enum EstadoEnfermedad
{
    Detectada = 1,
    EnTratamiento = 2,
    Controlada = 3
}

/// <summary>Etapa del beneficiado del café (HU-11..15).</summary>
public enum EtapaProduccion
{
    Recoleccion = 1,
    CafeSeco = 2,
    CafeProcesado = 3
}

/// <summary>Naturaleza de un movimiento financiero.</summary>
public enum TipoMovimientoFinanciero
{
    Ingreso = 1,
    Gasto = 2
}

/// <summary>Categoría de un gasto de la finca (HU-19).</summary>
public enum CategoriaGasto
{
    Insumos = 1,
    ManoDeObra = 2,
    Herramientas = 3,
    Transporte = 4,
    Servicios = 5,
    Otros = 6
}

/// <summary>Tipo de insumo del inventario (HU-22).</summary>
public enum TipoInsumo
{
    Fertilizante = 1,
    Fitosanitario = 2,
    Herramienta = 3,
    MaterialEmpaque = 4,
    Otro = 5
}

/// <summary>Dirección de un movimiento de inventario (HU-22, HU-24).</summary>
public enum TipoMovimientoInventario
{
    Entrada = 1,
    Salida = 2,
    Ajuste = 3
}

/// <summary>Origen de una alerta generada por el sistema.</summary>
public enum TipoAlerta
{
    EnfermedadRecurrente = 1,
    StockMinimo = 2
}

/// <summary>Acción registrada en la bitácora de auditoría (RQNF-013, HU-18).</summary>
public enum AccionAuditoria
{
    InicioSesion = 1,
    CierreSesion = 2,
    Crear = 3,
    Modificar = 4,
    Eliminar = 5,
    Inactivar = 6,
    Activar = 7
}
