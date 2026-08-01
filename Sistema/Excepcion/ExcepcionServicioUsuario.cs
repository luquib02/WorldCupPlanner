namespace Sistema;

public class ExcepcionServicioUsuario(string message) : Exception(message)
{
    public static ExcepcionServicioUsuario ContraseniaIncorrecta()
    {
        throw new ExcepcionServicioUsuario("Contraseña incorrecta.");
    }
    
}