using Dominio.Excepciones;

namespace Dominio;

public class Bombo
{
    private const int _numeroMin = 1;
    private const int _numeroMax = 4;
    private const int _capacidadMax = 12;

    private readonly List<Equipo> _equipos = new();

    public int Numero { get; private set; }
    public IReadOnlyList<Equipo> Equipos => _equipos;

    public Bombo(int numero)
    {
        ValidarNumero(numero);
        Numero = numero;
    }

    public void Agregar(Equipo equipo)
    {
        ValidarEquipo(equipo);
        ValidarNoDuplicado(equipo);
        ValidarCapacidad();
        _equipos.Add(equipo);
    }

    public Equipo ObtenerEquipo(int indice)
    {
        if (indice < 0 || indice >= _equipos.Count)
            throw new DominioException("Índice fuera de rango.");
        return _equipos[indice];
    }

    public int Cantidad() => _equipos.Count;

    private void ValidarNumero(int numero)
    {
        if (numero < _numeroMin || numero > _numeroMax)
            throw new DominioException("El número de bombo debe estar entre 1 y 4.");
    }

    private void ValidarEquipo(Equipo equipo)
    {
        if (equipo is null)
            throw new DominioException("El equipo es obligatorio.");
    }

    private void ValidarNoDuplicado(Equipo equipo)
    {
        if (_equipos.Contains(equipo))
            throw new DominioException("El equipo ya está en el bombo.");
    }

    private void ValidarCapacidad()
    {
        if (_equipos.Count >= _capacidadMax)
            throw new DominioException("El bombo no puede contener más de 12 equipos.");
    }
}