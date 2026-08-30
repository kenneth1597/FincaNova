namespace FincaNova.Web.ViewSupport;

/// <summary>
/// Expresiones regulares reutilizables para validar campos. Se usan tanto en el
/// servidor (<c>[RegularExpression]</c>) como en el cliente (jQuery validation),
/// por lo que solo utilizan clases de caracteres explícitas compatibles con
/// JavaScript (sin <c>\p{L}</c>).
/// </summary>
public static class Validaciones
{
    /// <summary>Nombres de personas: letras (incluye tildes y ñ), espacios y . ' -</summary>
    public const string Nombre = @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ][A-Za-zÁÉÍÓÚÜÑáéíóúüñ .'\-]*$";
    public const string NombreMsg = "Solo se permiten letras, espacios y los signos . ' -";

    /// <summary>Cédula / identificación: dígitos y guiones (ej. 1-1234-5678 o 112340567).</summary>
    public const string Identificacion = @"^[0-9]+([\- ][0-9]+)*$";
    public const string IdentificacionMsg = "La identificación solo puede contener números y guiones.";

    /// <summary>Teléfono: dígitos y los signos + - ( ) y espacios.</summary>
    public const string Telefono = @"^[0-9+\-() ]{7,20}$";
    public const string TelefonoMsg = "El teléfono solo puede contener números y los signos + - ( ).";

    /// <summary>Código de lote / micro lote: letras, números, guion y guion bajo.</summary>
    public const string Codigo = @"^[A-Za-z0-9\-_]{1,30}$";
    public const string CodigoMsg = "El código solo admite letras, números, guion y guion bajo (sin espacios).";
}
