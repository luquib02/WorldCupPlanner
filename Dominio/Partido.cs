using Dominio.Excepciones;
namespace Dominio;
public class Partido
{
    public List<Incidencia> Incidencias { get; private set; } = new();

    public IReadOnlyList<Incidencia> IncidenciasLocal =>
        Incidencias.Where(i => i.EsLocal).ToList();

    public IReadOnlyList<Incidencia> IncidenciasVisitante =>
        Incidencias.Where(i => !i.EsLocal).ToList();

    private static readonly string[] GruposValidos =
        { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "X" };

    private Estadio? _estadio;
    private Equipo? _equipoLocal;
    private Equipo? _equipoVisitante;
    private string _grupo = "X";

    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public FasePartido Fase { get; set; }
    public string? EtiquetaCruce { get; set; }
    public int? GolesLocal { get; private set; }
    public int? GolesVisitante { get; private set; }
    public int? GolesPenalesLocal { get; private set; }
    public int? GolesPenalesVisitante { get; private set; }
    public Partido? PartidoOrigenLocal { get; set; }
    public Partido? PartidoOrigenVisitante { get; set; }
    
    public Partido() { } 

    public Estadio Estadio
    {
        get => _estadio!;
        set
        {
            if (value is null)
                throw new DominioException("El estadio es obligatorio.");
            _estadio = value;
        }
    }

    public Equipo EquipoLocal
    {
        get => _equipoLocal!;
        set
        {
            ValidarEquipo(value, "El equipo local es obligatorio.");
            ValidarEquiposDistintos(value, _equipoVisitante);
            _equipoLocal = value;
        }
    }

    public Equipo EquipoVisitante
    {
        get => _equipoVisitante!;
        set
        {
            ValidarEquipo(value, "El equipo visitante es obligatorio.");
            ValidarEquiposDistintos(value, _equipoLocal);
            _equipoVisitante = value;
        }
    }

    public string Grupo
    {
        get => _grupo;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !GruposValidos.Contains(value))
                throw new DominioException("El grupo debe ser una letra entre A y L.");
            _grupo = value;
        }
    }

    public void AgregarIncidencia(Incidencia incidencia)
    {
        if (incidencia is null)
            throw new DominioException("La incidencia no puede ser nula.");
        Incidencias.Add(incidencia);
    }

    public void CargarResultado(int golesLocal, int golesVisitante)
    {
        if (golesLocal < 0 || golesVisitante < 0)
            throw new DominioException("Los goles no pueden ser negativos.");
        GolesLocal = golesLocal;
        GolesVisitante = golesVisitante;
    }

    public void CargarPenales(int golesLocal, int golesVisitante)
    {
        if (golesLocal < 0 || golesVisitante < 0)
            throw new DominioException("Los penales no pueden ser negativos.");
        if (golesLocal == golesVisitante)
            throw new DominioException("Los penales no pueden terminar empatados.");
        GolesPenalesLocal = golesLocal;
        GolesPenalesVisitante = golesVisitante;
    }

    public bool EstaJugado() => GolesLocal.HasValue && GolesVisitante.HasValue;

    public bool EsEmpate()
    {
        if (!EstaJugado())
            throw new DominioException("El partido no ha sido jugado.");
        return GolesLocal == GolesVisitante;
    }

    public Equipo ObtenerVencedor()
    {
        if (!EstaJugado())
            throw new DominioException("El partido no ha sido jugado.");
        if (GolesLocal > GolesVisitante)
            return EquipoLocal;
        if (GolesLocal < GolesVisitante)
            return EquipoVisitante;
        if (GolesPenalesLocal.HasValue && GolesPenalesVisitante.HasValue)
            return GolesPenalesLocal > GolesPenalesVisitante ? EquipoLocal : EquipoVisitante;

        throw new DominioException("El partido terminó empatado.");
    }

    public Equipo ObtenerPerdedor()
    {
        if (!EstaJugado())
            throw new DominioException("El partido no ha sido jugado.");
        if (GolesLocal > GolesVisitante)
            return EquipoVisitante;
        if (GolesLocal < GolesVisitante)
            return EquipoLocal;
        if (GolesPenalesLocal.HasValue && GolesPenalesVisitante.HasValue)
            return GolesPenalesLocal < GolesPenalesVisitante ? EquipoLocal : EquipoVisitante;

        throw new DominioException("El partido terminó empatado.");
    }

    public void Validar()
    {
        if (_estadio is null)
            throw new DominioException("El estadio es obligatorio.");
        if (_equipoLocal is null)
            throw new DominioException("El equipo local es obligatorio.");
        if (_equipoVisitante is null)
            throw new DominioException("El equipo visitante es obligatorio.");
    }
    
    private static void ValidarEquipo(Equipo? equipo, string mensaje)
    {
        if (equipo is null)
            throw new DominioException(mensaje);
    }

    private static void ValidarEquiposDistintos(Equipo? nuevo, Equipo? existente)
    {
        if (existente != null && existente == nuevo)
            throw new DominioException("El equipo local y visitante no pueden ser el mismo.");
    }
}   
