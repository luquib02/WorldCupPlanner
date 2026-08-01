namespace Dominio.Excepciones;

public class ExcepcionUsuario : Exception
{
    public ExcepcionUsuario(string message) : base(message) { }

    public static ExcepcionUsuario NombreVacio()
    {
        return new ExcepcionUsuario("El nombre no puede estar vacio");
    }

    public static ExcepcionUsuario ApellidoVacio()
    {
        return new ExcepcionUsuario("El apellido no puede estar vacio");
    }

    public static ExcepcionUsuario CorreoInvalido()
    {
        return new ExcepcionUsuario("El correo electrónico es inválido");
    }

    public static ExcepcionUsuario FechaNacimientoFutura()
    {
        return new ExcepcionUsuario("La fecha de nacimiento no puede ser en el futuro");
    }

    public static ExcepcionUsuario RolNoExiste()
    {
        return new ExcepcionUsuario("El rol no existe en la lista de roles del usuario");
    }

    public static ExcepcionUsuario RolDuplicado()
    {
        return new ExcepcionUsuario("El rol ya existe en la lista de roles del usuario");
    }

    public static ExcepcionUsuario ContraseniaVacia()
    {
        return new ExcepcionUsuario("La contraseña no puede estar vacía");
    }

    public static ExcepcionUsuario ContraseniaCorta()
    {
        return new ExcepcionUsuario("La contraseña debe tener al menos 8 caracteres");
    }

    public static ExcepcionUsuario ContraseniaSinMayuscula()
    {
        return new ExcepcionUsuario("La contraseña debe incluir al menos una letra mayúscula (A-Z)");
    }

    public static ExcepcionUsuario ContraseniaSinMinuscula()
    {
        return new ExcepcionUsuario("La contraseña debe incluir al menos una letra minúscula (a-z)");
    }

    public static ExcepcionUsuario ContraseniaSinNumero()
    {
        return new ExcepcionUsuario("La contraseña debe incluir al menos un número (0-9)");
    }

    public static ExcepcionUsuario ContraseniaSinEspecial()
    {
        return new ExcepcionUsuario("La contraseña debe incluir al menos un carácter especial (ej: @, #, $, %, !, *, +, etc.)");
    }
}