namespace Sistema.Excepciones;

public class ExcepcionLogServicio : Exception
{
    public ExcepcionLogServicio(string mensaje) : base(mensaje) { }

    public static ExcepcionLogServicio EmailInvalido() =>
        new ExcepcionLogServicio("El email del log es inválido.");

    public static ExcepcionLogServicio AccionVacia() =>
        new ExcepcionLogServicio("La acción del log es obligatoria.");

    public static ExcepcionLogServicio DetalleVacio() =>
        new ExcepcionLogServicio("El detalle del log es obligatorio.");
}