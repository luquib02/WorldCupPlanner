using Dominio.Excepciones;
using Dominio.Simulacion;

namespace Dominio;

public class Fixture
{
    private const int _maxGrupos = 12;

    private readonly List<Grupo> _grupos = new();
    private readonly List<Partido> _partidosEliminatorias = new();

    public int Id { get; set; }
    public DateTime FechaGeneracion { get; private set; }
    public IReadOnlyList<Grupo> Grupos => _grupos;
    public IReadOnlyList<Partido> PartidosEliminatorias => _partidosEliminatorias;
    public bool CrucesGenerados { get; private set; }
    public int SemillaUtilizada { get; private set; }
    public string NombreMotorSimulacion { get; private set; }

    protected Fixture() { NombreMotorSimulacion = string.Empty; }

    public Fixture(int semilla, string nombreMotorSimulacion = NombresMotores.Probabilistico)
    {
        SemillaUtilizada = semilla;
        FechaGeneracion = DateTime.Now;
        CrucesGenerados = false;
        NombreMotorSimulacion = nombreMotorSimulacion;
    }

    public void AgregarGrupo(Grupo grupo)
    {
        ValidarGrupo(grupo);
        ValidarCapacidadGrupos();
        _grupos.Add(grupo);
    }

    public void AgregarPartidoEliminatoria(Partido partido) =>
        _partidosEliminatorias.Add(partido);

    public void MarcarCrucesGenerados() => CrucesGenerados = true;

    public bool EstaFaseGruposCompleta()
    {
        if (_grupos.Count < _maxGrupos)
            return false;
        return _grupos.SelectMany(g => g.ObtenerPartidos()).All(p => p.EstaJugado());
    }

    public bool PuedeGenerarCruces() =>
        !CrucesGenerados && EstaFaseGruposCompleta();

    public List<Partido> ObtenerTodosLosPartidos()
    {
        List<Partido> resultado = _grupos.SelectMany(g => g.ObtenerPartidos()).ToList();
        resultado.AddRange(_partidosEliminatorias);
        return resultado;
    }

    private static void ValidarGrupo(Grupo grupo)
    {
        if (grupo is null)
            throw new DominioException("El grupo es obligatorio.");
    }

    private void ValidarCapacidadGrupos()
    {
        if (_grupos.Count >= _maxGrupos)
            throw new DominioException("El fixture no puede contener más de 12 grupos.");
    }
}