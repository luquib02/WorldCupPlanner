using Dominio;
using Dominio.Clasificacion;
using Dominio.Excepciones;
using Dominio.Helpers;
using Dominio.Simulacion;
using Repositorio;
using Sistema.DTOs;
using Sistema.Mappers;
using Sistema.Simulacion;

namespace Sistema;

public class FixtureServicio
{
    private const int _cantidadEquiposRequeridos = 48;
    private const int _cantidadMinimaEstadios = 4;
    private const int _cantidadGrupos = 12;
    private const int _cantidadBombos = 4;
    private const int _equiposPorBombo = 12;
    private const int _maxUefaPorGrupo = 2;
    private const int _maxOtrosPorGrupo = 1;
    private const string _etiquetasGrupos = "ABCDEFGHIJKL";
    private const int _horaInicio = 14;
    private const int _horasEntrePartidos = 4;

    private readonly IFixtureRepositorio _fixtureRepositorio;
    private readonly IEquipoRepositorio _equipoRepositorio;
    private readonly IRepositorioEstadio _estadioRepositorio;
    private readonly IPartidoRepositorio _partidoRepositorio;
    private readonly LogServicio _logServicio;
    private readonly SimulacionServicio _simulacionServicio;
    private readonly ResultadoPartidoServicio _resultadoServicio;

    public FixtureServicio(
        IFixtureRepositorio fixtureRepositorio,
        IEquipoRepositorio equipoRepositorio,
        IRepositorioEstadio estadioRepositorio,
        IPartidoRepositorio partidoRepositorio,
        LogServicio logServicio,
        SimulacionServicio simulacionServicio,
        ResultadoPartidoServicio resultadoServicio)
    {
        _fixtureRepositorio = fixtureRepositorio;
        _equipoRepositorio = equipoRepositorio;
        _estadioRepositorio = estadioRepositorio;
        _partidoRepositorio = partidoRepositorio;
        _logServicio = logServicio;
        _simulacionServicio = simulacionServicio;
        _resultadoServicio = resultadoServicio;
    }

    public FixtureDTO GenerarFixture(ConfiguracionTorneo configuracion, string usuarioEmail, string nombreMotor = NombresMotores.Probabilistico)
    {
        ValidarFixtureNoExiste();
        IMotorSimulacion motor = MotorSimulacionFactory.ObtenerPorNombre(nombreMotor);
        List<Equipo> equipos = ObtenerEquiposValidados();
        List<Estadio> estadios = ObtenerEstadiosValidados();

        List<Equipo> equiposOrdenados = OrdenarConSemilla(equipos, configuracion.SemillaFixture);
        List<Bombo> bombos = ArmarBombos(equiposOrdenados);
        Fixture fixture = new Fixture(configuracion.SemillaFixture, motor.Nombre);
        DistribuirEnGrupos(fixture, bombos);
        List<Estadio> estadiosOrdenados = OrdenarEstadios(estadios);
        GenerarCalendarioYPartidos(fixture, estadiosOrdenados, configuracion);

        _fixtureRepositorio.Guardar(fixture);
        _logServicio.Registrar(usuarioEmail, "GeneracionFixture",
            $"Se genero fixture con semilla {configuracion.SemillaFixture} y motor '{motor.Nombre}'. Total partidos: {fixture.ObtenerTodosLosPartidos().Count}.");

        return FixtureMapper.AFixtureDTO(fixture);
    }

    public List<string> ObtenerMotoresDisponibles()
    {
        return MotorSimulacionFactory.ObtenerNombresDisponibles();
    }

    public FixtureDTO ObtenerFixture()
    {
        return FixtureMapper.AFixtureDTO(ObtenerFixtureDominio());
    }

    public bool EstaFaseGruposCompleta()
    {
        return ObtenerFixtureDominio().EstaFaseGruposCompleta();
    }

    public List<PosicionEquipoDTO> ObtenerTablaPosiciones(string etiquetaGrupo)
    {
        Grupo grupo = ObtenerGrupoDominio(etiquetaGrupo);
        return grupo.ObtenerTablaPosiciones(new ClasificadorFifa())
            .Select(PosicionEquipoMapper.APosicionEquipoDTO)
            .ToList();
    }

    public FixtureDTO SimularGrupo(string etiquetaGrupo, ConfiguracionTorneo configuracion, string usuarioEmail)
    {
        Fixture fixture = ObtenerFixtureDominio();
        Grupo grupo = ObtenerGrupoEnFixture(fixture, etiquetaGrupo);
        IMotorSimulacion motor = MotorSimulacionFactory.ObtenerPorNombre(fixture.NombreMotorSimulacion);
        foreach (Jornada jornada in grupo.Jornadas)
        {
            _simulacionServicio.SimularJornada(jornada, configuracion, motor);
            PersistirPartidosSimulados(jornada.Partidos, usuarioEmail);
        }
        _logServicio.Registrar(usuarioEmail, "SimulacionGrupo",
            $"Grupo {etiquetaGrupo} simulado con motor '{motor.Nombre}' y semilla {configuracion.SemillaSimulation}.");
        return FixtureMapper.AFixtureDTO(fixture);
    }

    public FixtureDTO SimularFaseDeGrupos(ConfiguracionTorneo configuracion, string usuarioEmail)
    {
        Fixture fixture = ObtenerFixtureDominio();
        IMotorSimulacion motor = MotorSimulacionFactory.ObtenerPorNombre(fixture.NombreMotorSimulacion);
        foreach (Grupo grupo in fixture.Grupos)
            foreach (Jornada jornada in grupo.Jornadas)
            {
                _simulacionServicio.SimularJornada(jornada, configuracion, motor);
                PersistirPartidosSimulados(jornada.Partidos, usuarioEmail);
            }
        _logServicio.Registrar(usuarioEmail, "SimulacionFaseGrupos",
            $"Fase de grupos simulada con motor '{motor.Nombre}' y semilla {configuracion.SemillaSimulation}.");
        return FixtureMapper.AFixtureDTO(fixture);
    }

    private Fixture ObtenerFixtureDominio()
    {
        Fixture? fixture = _fixtureRepositorio.ObtenerActual();
        if (fixture is null)
            throw new DominioException("No existe un fixture generado.");
        return fixture;
    }

    private Grupo ObtenerGrupoDominio(string etiquetaGrupo)
    {
        Fixture fixture = ObtenerFixtureDominio();
        return ObtenerGrupoEnFixture(fixture, etiquetaGrupo);
    }

    private static Grupo ObtenerGrupoEnFixture(Fixture fixture, string etiquetaGrupo)
    {
        Grupo? grupo = fixture.Grupos.FirstOrDefault(g => g.Etiqueta == etiquetaGrupo);
        if (grupo is null)
            throw new DominioException($"No existe el grupo {etiquetaGrupo}.");
        return grupo;
    }

    private void ValidarFixtureNoExiste()
    {
        if (_fixtureRepositorio.Existe())
            throw new DominioException("Ya existe un fixture generado.");
    }

    private List<Equipo> ObtenerEquiposValidados()
    {
        List<Equipo> equipos = _equipoRepositorio.ObtenerTodos();
        if (equipos.Count != _cantidadEquiposRequeridos)
            throw new DominioException("Se necesitan exactamente 48 equipos para generar el fixture.");
        return equipos;
    }

    private List<Estadio> ObtenerEstadiosValidados()
    {
        List<Estadio> estadios = _estadioRepositorio.ListarTodos();
        if (estadios.Count < _cantidadMinimaEstadios)
            throw new DominioException("Se necesitan al menos 4 estadios para generar el fixture.");
        return estadios;
    }

    private static List<Equipo> OrdenarConSemilla(List<Equipo> equipos, int semilla)
    {
        List<Equipo> ordenados = equipos.OrderByDescending(e => e.RankingFIFA).ToList();
        List<List<Equipo>> grupos = AgruparPorRanking(ordenados);
        foreach (List<Equipo> grupo in grupos)
        {
            if (grupo.Count > 1)
                FisherYates.Barajar(grupo, semilla);
        }
        return grupos.SelectMany(g => g).ToList();
    }

    private static List<List<Equipo>> AgruparPorRanking(List<Equipo> ordenados)
    {
        List<List<Equipo>> grupos = new List<List<Equipo>>();
        List<Equipo> actual = new List<Equipo>();
        int rankingActual = -1;
        foreach (Equipo equipo in ordenados)
        {
            if (equipo.RankingFIFA != rankingActual)
            {
                if (actual.Count > 0)
                    grupos.Add(actual);
                actual = new List<Equipo>();
                rankingActual = equipo.RankingFIFA;
            }
            actual.Add(equipo);
        }
        if (actual.Count > 0)
            grupos.Add(actual);
        return grupos;
    }

    private static List<Bombo> ArmarBombos(List<Equipo> equiposOrdenados)
    {
        List<Bombo> bombos = new List<Bombo>();
        for (int b = 0; b < _cantidadBombos; b++)
        {
            Bombo bombo = new Bombo(b + 1);
            for (int i = 0; i < _equiposPorBombo; i++)
                bombo.Agregar(equiposOrdenados[b * _equiposPorBombo + i]);
            bombos.Add(bombo);
        }
        return bombos;
    }

    private static void DistribuirEnGrupos(Fixture fixture, List<Bombo> bombos)
    {
        List<Grupo> grupos = new List<Grupo>();
        for (int i = 0; i < _cantidadGrupos; i++)
            grupos.Add(new Grupo(_etiquetasGrupos[i].ToString()));

        foreach (Bombo bombo in bombos)
            AsignarBomboAGrupos(bombo, grupos);

        foreach (Grupo g in grupos)
            fixture.AgregarGrupo(g);
    }

    private static void AsignarBomboAGrupos(Bombo bombo, List<Grupo> grupos)
    {
        List<Equipo> pendientes = bombo.Equipos.ToList();
        for (int i = 0; i < grupos.Count; i++)
        {
            Equipo equipo = SeleccionarEquipoParaGrupo(grupos[i], pendientes);
            grupos[i].AgregarEquipo(equipo);
            pendientes.Remove(equipo);
        }
    }

    private static Equipo SeleccionarEquipoParaGrupo(Grupo grupo, List<Equipo> pendientes)
    {
        foreach (Equipo equipo in pendientes)
        {
            if (CumpleReglasDeConfederacion(grupo, equipo))
                return equipo;
        }
        return pendientes[0];
    }

    private static bool CumpleReglasDeConfederacion(Grupo grupo, Equipo equipo)
    {
        int yaEnGrupo = grupo.ContieneEquiposMismaConfederacion(equipo.Confederacion);
        int limite = equipo.Confederacion == Confederacion.UEFA
            ? _maxUefaPorGrupo
            : _maxOtrosPorGrupo;
        return yaEnGrupo < limite;
    }

    private static List<Estadio> OrdenarEstadios(List<Estadio> estadios)
    {
        return estadios.OrderBy(e => Normalizador.Normalizar(e.Nombre)).ToList();
    }

    private void GenerarCalendarioYPartidos(Fixture fixture, List<Estadio> estadios, ConfiguracionTorneo configuracion)
    {
        int[][] patron = new int[][]
        {
            new int[] { 0, 3, 1, 2 },
            new int[] { 0, 2, 1, 3 },
            new int[] { 0, 1, 2, 3 }
        };

        int contadorPartido = 0;
        for (int g = 0; g < fixture.Grupos.Count; g++)
        {
            Grupo grupo = fixture.Grupos[g];
            List<Equipo> equipos = grupo.Equipos.ToList();
            for (int j = 0; j < 3; j++)
            {
                DateTime fechaJornada = configuracion.FechaInicioTorneo
                    .AddDays(j * configuracion.SeparacionEntreFechas);
                Jornada jornada = new Jornada(j + 1, fechaJornada);

                for (int p = 0; p < 2; p++)
                {
                    int localIdx = patron[j][p * 2];
                    int visitanteIdx = patron[j][p * 2 + 1];
                    DateTime horaPartido = CalcularHoraPartido(fechaJornada, contadorPartido, configuracion);

                    Partido partido = new Partido
                    {
                        Fecha = horaPartido,
                        Estadio = estadios[contadorPartido % estadios.Count],
                        EquipoLocal = equipos[localIdx],
                        EquipoVisitante = equipos[visitanteIdx],
                        Grupo = grupo.Etiqueta,
                        Fase = FasePartido.FaseDeGrupos
                    };
                    _partidoRepositorio.Agregar(partido);
                    jornada.AgregarPartido(partido);
                    contadorPartido++;
                }
                grupo.AgregarJornada(jornada);
            }
        }
    }

    private static DateTime CalcularHoraPartido(DateTime fechaJornada, int contadorPartido, ConfiguracionTorneo configuracion)
    {
        int partidoEnElDia = contadorPartido % configuracion.MaxPartidosPorDia;
        return fechaJornada
            .Date
            .AddHours(_horaInicio + partidoEnElDia * _horasEntrePartidos);
    }

    private void PersistirPartidosSimulados(IEnumerable<Partido> partidos, string usuarioEmail)
    {
        foreach (Partido partido in partidos.Where(p => p.EstaJugado()))
            _resultadoServicio.AplicarResultado(partido, usuarioEmail, "Resultado simulado");
    }
}