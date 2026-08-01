using Dominio;
using Dominio.Excepciones;
using Dominio.Helpers;
using Dominio.Simulacion;
using Repositorio;
using Sistema.DTOs;
using Sistema.Mappers;
using Sistema.Simulacion;

namespace Sistema;

public class CrucesServicio
{
    private readonly IFixtureRepositorio _fixtureRepositorio;
    private readonly IPartidoRepositorio _partidoRepositorio;
    private readonly IRepositorioEstadio _estadioRepositorio;
    private readonly LogServicio _logServicio;
    private readonly SimulacionServicio _simulacionServicio;
    private readonly ResultadoPartidoServicio _resultadoServicio;

    public CrucesServicio(
        IFixtureRepositorio fixtureRepositorio,
        IPartidoRepositorio partidoRepositorio,
        IRepositorioEstadio estadioRepositorio,
        LogServicio logServicio,
        SimulacionServicio simulacionServicio,
        ResultadoPartidoServicio resultadoServicio)
    {
        _fixtureRepositorio = fixtureRepositorio;
        _partidoRepositorio = partidoRepositorio;
        _estadioRepositorio = estadioRepositorio;
        _logServicio = logServicio;
        _simulacionServicio = simulacionServicio;
        _resultadoServicio = resultadoServicio;
    }

    public FixtureDTO SimularFase(FasePartido fase, ConfiguracionTorneo configuracion, string usuarioEmail)
    {
        Fixture fixture = ObtenerFixture();
        IMotorSimulacion motor = MotorSimulacionFactory.ObtenerPorNombre(fixture.NombreMotorSimulacion);
        List<Partido> partidos = fixture.PartidosEliminatorias
            .Where(p => p.Fase == fase && !p.EstaJugado())
            .ToList();
        foreach (Partido partido in partidos)
        {
            _simulacionServicio.SimularPartido(partido, configuracion, motor);
            if (partido.EstaJugado())
                _resultadoServicio.AplicarResultado(partido, usuarioEmail, "Resultado simulado");
        }
        _logServicio.Registrar(usuarioEmail, "SimulacionFase",
            $"Fase {fase} simulada con motor '{motor.Nombre}' y semilla {configuracion.SemillaSimulation}.");
        return FixtureMapper.AFixtureDTO(fixture);
    }

    public void GenerarDieciseisavos(ConfiguracionTorneo configuracion, string usuarioEmail)
    {
        Fixture fixture = ObtenerFixture();
        ValidarFaseGruposCompleta(fixture);
        ValidarCrucesNoGenerados(fixture);

        List<Estadio> estadios = ObtenerEstadiosOrdenados();
        List<(PosicionEquipo pos, string grupo)> tablas = CalcularTodasLasTablas(fixture, configuracion.SemillaCrucesFase);

        List<(PosicionEquipo pos, string grupo)> primeros = tablas.Where((_, i) => i % 4 == 0).ToList();
        List<(PosicionEquipo pos, string grupo)> segundos = tablas.Where((_, i) => i % 4 == 1).ToList();
        List<(PosicionEquipo pos, string grupo)> terceros = tablas.Where((_, i) => i % 4 == 2).ToList();

        List<(PosicionEquipo pos, string grupo)> primerosOrdenados = OrdenarGlobal(primeros, configuracion.SemillaCrucesFase);
        List<(PosicionEquipo pos, string grupo)> segundosOrdenados = OrdenarGlobal(segundos, configuracion.SemillaCrucesFase);
        List<(PosicionEquipo pos, string grupo)> mejoresTerceros = OrdenarGlobal(terceros, configuracion.SemillaCrucesFase).Take(8).ToList();

        DateTime fechaBase = CalcularFechaDieciseisavos(fixture);

        List<Partido> todos = new List<Partido>();
        todos.AddRange(EmparejarBloqueA(primerosOrdenados, mejoresTerceros, configuracion.SemillaCrucesFase));
        todos.AddRange(EmparejarBloqueB14(primerosOrdenados, segundosOrdenados, configuracion.SemillaCrucesFase));
        todos.AddRange(EmparejarBloqueB58(segundosOrdenados, configuracion.SemillaCrucesFase));

        AsignarFechaYEstadio(todos, fechaBase, estadios);
        PersistirYAgregar(fixture, todos);
        fixture.MarcarCrucesGenerados();

        _logServicio.Registrar(usuarioEmail, "GeneracionCruces",
            $"Se generaron dieciseisavos con semilla {configuracion.SemillaCrucesFase}. Partidos: {todos.Count}.");
    }

    public void GenerarOctavos(string usuarioEmail)
    {
        Fixture fixture = ObtenerFixture();
        ValidarFaseCompleta(fixture, FasePartido.DieciseisavosDeFinal, "Los dieciseisavos no estan completos.");
        ValidarFaseNoGenerada(fixture, FasePartido.OctavosDeFinal, "Los octavos ya fueron generados.");

        List<Estadio> estadios = ObtenerEstadiosOrdenados();
        List<Partido> dieciseisavos = ObtenerPartidosDeFase(fixture, FasePartido.DieciseisavosDeFinal);
        DateTime fechaBase = CalcularFechaPosterior(dieciseisavos);
        List<Partido> octavos = new List<Partido>();

        for (int i = 0; i < 8; i++)
        {
            Partido a = BuscarPorEtiqueta(dieciseisavos, $"A{i + 1}");
            Partido b = BuscarPorEtiqueta(dieciseisavos, $"B{i + 1}");
            octavos.Add(CrearPartidoSucesor(a, b, FasePartido.OctavosDeFinal, $"C{i + 1}"));
        }

        AsignarFechaYEstadio(octavos, fechaBase, estadios);
        PersistirYAgregar(fixture, octavos);
        _logServicio.Registrar(usuarioEmail, "GeneracionCruces", "Se generaron octavos.");
    }

    public void GenerarCuartos(string usuarioEmail)
    {
        Fixture fixture = ObtenerFixture();
        ValidarFaseCompleta(fixture, FasePartido.OctavosDeFinal, "Los octavos no estan completos.");
        ValidarFaseNoGenerada(fixture, FasePartido.CuartosDeFinal, "Los cuartos ya fueron generados.");

        List<Estadio> estadios = ObtenerEstadiosOrdenados();
        List<Partido> octavos = ObtenerPartidosDeFase(fixture, FasePartido.OctavosDeFinal);
        DateTime fechaBase = CalcularFechaPosterior(octavos);
        List<Partido> cuartos = new List<Partido>();

        for (int i = 0; i < 4; i++)
        {
            Partido c1 = BuscarPorEtiqueta(octavos, $"C{i * 2 + 1}");
            Partido c2 = BuscarPorEtiqueta(octavos, $"C{i * 2 + 2}");
            cuartos.Add(CrearPartidoSucesor(c1, c2, FasePartido.CuartosDeFinal, $"D{i + 1}"));
        }

        AsignarFechaYEstadio(cuartos, fechaBase, estadios);
        PersistirYAgregar(fixture, cuartos);
        _logServicio.Registrar(usuarioEmail, "GeneracionCruces", "Se generaron cuartos.");
    }

    public void GenerarSemifinales(string usuarioEmail)
    {
        Fixture fixture = ObtenerFixture();
        ValidarFaseCompleta(fixture, FasePartido.CuartosDeFinal, "Los cuartos no estan completos.");
        ValidarFaseNoGenerada(fixture, FasePartido.Semifinal, "Las semifinales ya fueron generadas.");

        List<Estadio> estadios = ObtenerEstadiosOrdenados();
        List<Partido> cuartos = ObtenerPartidosDeFase(fixture, FasePartido.CuartosDeFinal);
        DateTime fechaBase = CalcularFechaPosterior(cuartos);
        List<Partido> semis = new List<Partido>();

        for (int i = 0; i < 2; i++)
        {
            Partido d1 = BuscarPorEtiqueta(cuartos, $"D{i * 2 + 1}");
            Partido d2 = BuscarPorEtiqueta(cuartos, $"D{i * 2 + 2}");
            semis.Add(CrearPartidoSucesor(d1, d2, FasePartido.Semifinal, $"S{i + 1}"));
        }

        AsignarFechaYEstadio(semis, fechaBase, estadios);
        PersistirYAgregar(fixture, semis);
        _logServicio.Registrar(usuarioEmail, "GeneracionCruces", "Se generaron semifinales.");
    }

    public void GenerarTercerPuestoYFinal(string usuarioEmail)
    {
        Fixture fixture = ObtenerFixture();
        ValidarFaseCompleta(fixture, FasePartido.Semifinal, "Las semifinales no estan completas.");
        ValidarFaseNoGenerada(fixture, FasePartido.Final, "La final ya fue generada.");

        List<Estadio> estadios = ObtenerEstadiosOrdenados();
        List<Partido> semis = ObtenerPartidosDeFase(fixture, FasePartido.Semifinal);
        DateTime fechaBase = CalcularFechaPosterior(semis);

        Partido s1 = BuscarPorEtiqueta(semis, "S1");
        Partido s2 = BuscarPorEtiqueta(semis, "S2");

        Partido tercerPuesto = new Partido
        {
            Fecha = fechaBase,
            Estadio = estadios[0],
            EquipoLocal = s1.ObtenerPerdedor(),
            EquipoVisitante = s2.ObtenerPerdedor(),
            Grupo = "X",
            Fase = FasePartido.TercerPuesto,
            EtiquetaCruce = "TP",
            PartidoOrigenLocal = s1,
            PartidoOrigenVisitante = s2
        };

        Partido final = new Partido
        {
            Fecha = fechaBase.AddDays(2),
            Estadio = estadios.Count > 1 ? estadios[1] : estadios[0],
            EquipoLocal = s1.ObtenerVencedor(),
            EquipoVisitante = s2.ObtenerVencedor(),
            Grupo = "X",
            Fase = FasePartido.Final,
            EtiquetaCruce = "F",
            PartidoOrigenLocal = s1,
            PartidoOrigenVisitante = s2
        };

        PersistirYAgregar(fixture, new List<Partido> { tercerPuesto, final });
        _logServicio.Registrar(usuarioEmail, "GeneracionCruces", "Se generaron tercer puesto y final.");
    }
    

    private Fixture ObtenerFixture()
    {
        Fixture? fixture = _fixtureRepositorio.ObtenerActual();
        if (fixture is null)
            throw new DominioException("No existe un fixture generado.");
        return fixture;
    }

    private static void ValidarFaseGruposCompleta(Fixture fixture)
    {
        if (!fixture.EstaFaseGruposCompleta())
            throw new DominioException("La fase de grupos no esta completa.");
    }

    private static void ValidarCrucesNoGenerados(Fixture fixture)
    {
        if (fixture.CrucesGenerados)
            throw new DominioException("Los cruces ya fueron generados.");
    }

    private static void ValidarFaseCompleta(Fixture fixture, FasePartido fase, string mensaje)
    {
        List<Partido> partidos = ObtenerPartidosDeFase(fixture, fase);
        if (partidos.Count == 0 || partidos.Any(p => !p.EstaJugado()))
            throw new DominioException(mensaje);
    }

    private static void ValidarFaseNoGenerada(Fixture fixture, FasePartido fase, string mensaje)
    {
        if (fixture.PartidosEliminatorias.Any(p => p.Fase == fase))
            throw new DominioException(mensaje);
    }

    private static List<Partido> ObtenerPartidosDeFase(Fixture fixture, FasePartido fase)
    {
        return fixture.PartidosEliminatorias.Where(p => p.Fase == fase).ToList();
    }

    private static Partido BuscarPorEtiqueta(List<Partido> partidos, string etiqueta)
    {
        Partido? p = partidos.FirstOrDefault(x => x.EtiquetaCruce == etiqueta);
        if (p is null)
            throw new DominioException($"No se encontro el partido {etiqueta}.");
        return p;
    }

    private List<Estadio> ObtenerEstadiosOrdenados()
    {
        List<Estadio> estadios = _estadioRepositorio.ListarTodos();
        if (estadios.Count == 0)
            throw new DominioException("No hay estadios disponibles.");
        return estadios.OrderBy(e => Normalizador.Normalizar(e.Nombre)).ToList();
    }

    private static DateTime CalcularFechaDieciseisavos(Fixture fixture)
    {
        DateTime ultima = fixture.Grupos
            .SelectMany(g => g.Jornadas)
            .Max(j => j.Fecha);
        return ultima.AddDays(7).Date.AddHours(16);
    }

    private static DateTime CalcularFechaPosterior(List<Partido> partidos)
    {
        return partidos.Max(p => p.Fecha).AddDays(7).Date.AddHours(16);
    }

    private static List<(PosicionEquipo pos, string grupo)> CalcularTodasLasTablas(Fixture fixture, int semilla)
    {
        List<(PosicionEquipo pos, string grupo)> resultado = new List<(PosicionEquipo pos, string grupo)>();
        foreach (Grupo grupo in fixture.Grupos)
        {
            List<PosicionEquipo> posiciones = grupo.ObtenerTablaPosiciones();
            foreach (Partido partido in grupo.ObtenerPartidos())
            {
                if (!partido.EstaJugado()) continue;
                PosicionEquipo local = posiciones.First(p => p.Equipo == partido.EquipoLocal);
                PosicionEquipo visitante = posiciones.First(p => p.Equipo == partido.EquipoVisitante);
                local.RegistrarResultado(partido.GolesLocal!.Value, partido.GolesVisitante!.Value);
                visitante.RegistrarResultado(partido.GolesVisitante!.Value, partido.GolesLocal!.Value);
            }
            List<(PosicionEquipo, string)> conGrupo = posiciones
                .Select(p => (p, grupo.Etiqueta))
                .ToList();
            OrdenarTabla(conGrupo, semilla);
            resultado.AddRange(conGrupo);
        }
        return resultado;
    }

    private static void OrdenarTabla(List<(PosicionEquipo pos, string grupo)> tabla, int semilla)
    {
        FisherYates.Barajar(tabla, semilla);
        tabla.Sort((a, b) =>
        {
            int c = b.pos.Puntos.CompareTo(a.pos.Puntos);
            if (c != 0) return c;
            c = b.pos.DiferenciaGoles.CompareTo(a.pos.DiferenciaGoles);
            if (c != 0) return c;
            return b.pos.GolesAFavor.CompareTo(a.pos.GolesAFavor);
        });
    }

    private static List<(PosicionEquipo pos, string grupo)> OrdenarGlobal(
        List<(PosicionEquipo pos, string grupo)> lista, int semilla)
    {
        List<(PosicionEquipo pos, string grupo)> copia = new List<(PosicionEquipo, string)>(lista);
        FisherYates.Barajar(copia, semilla);
        copia.Sort((a, b) =>
        {
            int c = b.pos.Puntos.CompareTo(a.pos.Puntos);
            if (c != 0) return c;
            c = b.pos.DiferenciaGoles.CompareTo(a.pos.DiferenciaGoles);
            if (c != 0) return c;
            return b.pos.GolesAFavor.CompareTo(a.pos.GolesAFavor);
        });
        return copia;
    }

    private List<Partido> EmparejarBloqueA(
        List<(PosicionEquipo pos, string grupo)> primeros,
        List<(PosicionEquipo pos, string grupo)> terceros,
        int semilla)
    {
        List<(PosicionEquipo pos, string grupo)> p = primeros.Take(8).ToList();
        List<(PosicionEquipo pos, string grupo)> t = new List<(PosicionEquipo, string)>(terceros);
        FisherYates.Barajar(p, semilla);
        FisherYates.Barajar(t, semilla);
        return Emparejar(p, t, "A");
    }

    private List<Partido> EmparejarBloqueB14(
        List<(PosicionEquipo pos, string grupo)> primeros,
        List<(PosicionEquipo pos, string grupo)> segundos,
        int semilla)
    {
        List<(PosicionEquipo pos, string grupo)> p = primeros.Skip(8).ToList();
        List<(PosicionEquipo pos, string grupo)> s = segundos.Skip(8).ToList();
        FisherYates.Barajar(p, semilla);
        FisherYates.Barajar(s, semilla);
        return Emparejar(p, s, "B", 1);
    }

    private List<Partido> EmparejarBloqueB58(
        List<(PosicionEquipo pos, string grupo)> segundos,
        int semilla)
    {
        List<(PosicionEquipo pos, string grupo)> s = segundos.Take(8).ToList();
        FisherYates.Barajar(s, semilla);
        return EmparejarEntreSi(s, "B", 5);
    }

    private List<Partido> Emparejar(
        List<(PosicionEquipo pos, string grupo)> listaA,
        List<(PosicionEquipo pos, string grupo)> listaB,
        string prefijo,
        int inicio = 1)
    {
        List<(PosicionEquipo pos, string grupo)> disponibles = new List<(PosicionEquipo, string)>(listaB);
        List<Partido> partidos = new List<Partido>();
        int n = inicio;
        foreach ((PosicionEquipo pos, string grupo) a in listaA)
        {
            (PosicionEquipo pos, string grupo) oponente = disponibles
                .FirstOrDefault(d => d.grupo != a.grupo);
            if (oponente == default) oponente = disponibles[0];
            disponibles.Remove(oponente);
            partidos.Add(CrearPartidoEliminatorio(a.pos.Equipo, oponente.pos.Equipo, $"{prefijo}{n}"));
            n++;
        }
        return partidos;
    }

    private List<Partido> EmparejarEntreSi(
        List<(PosicionEquipo pos, string grupo)> lista,
        string prefijo,
        int inicio)
    {
        List<(PosicionEquipo pos, string grupo)> pendientes = new List<(PosicionEquipo, string)>(lista);
        List<Partido> partidos = new List<Partido>();
        int n = inicio;
        while (pendientes.Count > 0)
        {
            (PosicionEquipo pos, string grupo) primero = pendientes[0];
            pendientes.RemoveAt(0);
            (PosicionEquipo pos, string grupo) oponente = pendientes
                .FirstOrDefault(d => d.grupo != primero.grupo);
            if (oponente == default) oponente = pendientes[0];
            pendientes.Remove(oponente);
            partidos.Add(CrearPartidoEliminatorio(primero.pos.Equipo, oponente.pos.Equipo, $"{prefijo}{n}"));
            n++;
        }
        return partidos;
    }

    // El estadio y la fecha definitivos se asignan luego en AsignarFechaYEstadio.
    private Partido CrearPartidoEliminatorio(Equipo local, Equipo visitante, string etiqueta)
    {
        return new Partido
        {
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "X",
            Fase = FasePartido.DieciseisavosDeFinal,
            EtiquetaCruce = etiqueta
        };
    }

    // El estadio y la fecha definitivos se asignan luego en AsignarFechaYEstadio.
    private Partido CrearPartidoSucesor(Partido a, Partido b, FasePartido fase, string etiqueta)
    {
        return new Partido
        {
            EquipoLocal = a.ObtenerVencedor(),
            EquipoVisitante = b.ObtenerVencedor(),
            Grupo = "X",
            Fase = fase,
            EtiquetaCruce = etiqueta,
            PartidoOrigenLocal = a,
            PartidoOrigenVisitante = b
        };
    }

    private static void AsignarFechaYEstadio(List<Partido> partidos, DateTime fechaBase, List<Estadio> estadios)
    {
        for (int i = 0; i < partidos.Count; i++)
        {
            partidos[i].Fecha = fechaBase.AddHours(i * 4);
            partidos[i].Estadio = estadios[i % estadios.Count];
        }
    }

    private void PersistirYAgregar(Fixture fixture, List<Partido> partidos)
    {
        foreach (Partido p in partidos)
        {
            _partidoRepositorio.Agregar(p);
            fixture.AgregarPartidoEliminatoria(p);
        }
    }

}