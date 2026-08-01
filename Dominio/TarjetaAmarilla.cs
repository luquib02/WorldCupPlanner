namespace Dominio;

public class TarjetaAmarilla : Incidencia
{
    public TarjetaAmarilla() { }

    public TarjetaAmarilla(int minutoJuego, bool esLocal)
    {
        ValidarMinuto(minutoJuego);
        MinutoJuego = minutoJuego;
        EsLocal = esLocal;
    }
}
