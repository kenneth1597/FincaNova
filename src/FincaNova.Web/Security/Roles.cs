namespace FincaNova.Web.Security;

/// <summary>
/// Roles del sistema (RF-04, HU-46). Se usan como constantes en los atributos
/// <c>[Authorize(Roles = ...)]</c> de los controladores de cada módulo.
/// </summary>
public static class Roles
{
    /// <summary>Control total: gestión de usuarios, configuración y todos los módulos.</summary>
    public const string Administrador = "Administrador";

    /// <summary>Propietario de la finca: consulta todo y genera reportes; no administra usuarios.</summary>
    public const string Caficultor = "Caficultor";

    /// <summary>Personal de campo: registra labores, recolección y enfermedades.</summary>
    public const string Trabajador = "Trabajador";

    public static readonly string[] Todos = { Administrador, Caficultor, Trabajador };

    /// <summary>Roles con acceso de gestión (crear/editar/eliminar) sobre la mayoría de módulos.</summary>
    public const string Gestion = Administrador + "," + Caficultor;
}
