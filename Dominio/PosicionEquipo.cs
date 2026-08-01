using Dominio.Excepciones;

namespace Dominio;

public class PosicionEquipo
{
    private const int _puntosVictoria = 3;
    private const int _puntosEmpate = 1;

    public Equipo Equipo { get; private set; }
    public int Puntos { get; private set; }
    public int PartidosJugados { get; private set; }
    public int Ganados { get; private set; }
    public int Empatados { get; private set; }
    public int Perdidos { get; private set; }
    public int GolesAFavor { get; private set; }
    public int GolesEnContra { get; private set; }
    public int DiferenciaGoles => GolesAFavor - GolesEnContra;

    public PosicionEquipo(Equipo equipo)
    {
        if (equipo is null)
            throw new DominioException("El equipo es obligatorio.");
        Equipo = equipo;
    }

    public void RegistrarResultado(int golesFavor, int golesContra)
    {
        ValidarGoles(golesFavor, golesContra);

        PartidosJugados++;
        GolesAFavor += golesFavor;
        GolesEnContra += golesContra;

        if (golesFavor > golesContra)
            RegistrarVictoria();
        else if (golesFavor == golesContra)
            RegistrarEmpate();
        else
            Perdidos++;
    }

    private void RegistrarVictoria()
    {
        Ganados++;
        Puntos += _puntosVictoria;
    }

    private void RegistrarEmpate()
    {
        Empatados++;
        Puntos += _puntosEmpate;
    }

    private void ValidarGoles(int golesFavor, int golesContra)
    {
        if (golesFavor < 0 || golesContra < 0)
            throw new DominioException("Los goles no pueden ser negativos.");
    }
}