namespace Dominio;

public class TarjetaRoja : Incidencia
{
    public TarjetaRoja() { }

    public TarjetaRoja(int minutoJuego, bool esLocal)
    {
        ValidarMinuto(minutoJuego);
        MinutoJuego = minutoJuego;
        EsLocal = esLocal;
    }
}
