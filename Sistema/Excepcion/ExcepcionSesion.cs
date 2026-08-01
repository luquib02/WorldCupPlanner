namespace Sistema;

public class ExcepcionSesion : Exception 
{
    private ExcepcionSesion(string message) : base(message) { }
    public static ExcepcionSesion NoEstaLogueado()
    {
        throw new ExcepcionSesion("El usuario no inició sesión");
    }

    public static ExcepcionSesion UsuarioYaLogueado()
    {
        throw new ExcepcionSesion("El usuario ya tiene la sesión iniciada");
    }

    public static ExcepcionSesion SesionNoIniciada()
    {
        throw new ExcepcionSesion("No hay ningún usuario con sesión abierta.");
    }

    public static ExcepcionSesion SinPermiso()
    {
        throw new ExcepcionSesion("El usuario no tiene permisos los permisos necesarios.");
    }
}