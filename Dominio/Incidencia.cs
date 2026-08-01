using Dominio.Excepciones;

namespace Dominio;

public abstract class Incidencia
{
    public int Id { get; set; }
    public int MinutoJuego { get; protected set; }
    public bool EsLocal { get; protected set; }

    public Incidencia() { }

    protected static void ValidarMinuto(int minuto)
    {
        if (minuto < 0)
            throw new DominioException("El minuto de juego no puede ser negativo.");
    }
}
