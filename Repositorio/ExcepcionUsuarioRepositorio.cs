namespace Repositorio;

public class ExcepcionUsuarioRepositorio(string message) : Exception(message)
{
    public static ExcepcionUsuarioRepositorio UsuarioDuplicado()
    {
        throw new ExcepcionUsuarioRepositorio("El usuario ya existe en la base de datos.");
    }
    public static ExcepcionUsuarioRepositorio UsuarioNoEncontrado()
    {
        throw new ExcepcionUsuarioRepositorio("El usuario no existe en la base de datos.");
    }

    public static ExcepcionUsuarioRepositorio UsuarioInvalido()
    {
        throw new ExcepcionUsuarioRepositorio("El usuario es inválido, por favor verifique los datos ingresados.");
    }
}