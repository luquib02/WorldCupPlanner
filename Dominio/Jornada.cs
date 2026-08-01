using Dominio.Excepciones;

namespace Dominio;

public class Jornada
{
    private const int _numeroMinimo = 1;
    private const int _numeroMaximo = 3;
    public int Id { get; set; }
    public int Numero { get; private set; }
    public DateTime Fecha { get; private set; }
    public List<Partido> Partidos { get; private set; }

    protected Jornada() { Partidos = new List<Partido>(); }

    public Jornada(int numero, DateTime fecha)
    {
        ValidarNumero(numero);
        Numero = numero;
        Fecha = fecha;
        Partidos = new List<Partido>();
    }
    public void AgregarPartido(Partido partido)
    {
        ValidarPartidoNoNulo(partido);
        ValidarPartidoNoDuplicado(partido);
        Partidos.Add(partido);
    }
    private void ValidarNumero(int numero)
    {
        if (numero < _numeroMinimo || numero > _numeroMaximo)
            throw new DominioException("El número de jornada debe estar entre 1 y 3.");
    }
    private void ValidarPartidoNoNulo(Partido partido)
    {
        if (partido == null)
        {
            throw new DominioException("El partido es obligatorio.");
        }
    }

    private void ValidarPartidoNoDuplicado(Partido partido)
    {
        if (Partidos.Contains(partido))
        {
            throw new DominioException("El partido ya está en la jornada.");
        }
    }
}