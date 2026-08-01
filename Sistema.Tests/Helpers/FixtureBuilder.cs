using Dominio;
using Dominio.Simulacion;
using System;
using System.Collections.Generic;

namespace Sistema.Tests.Helpers;

public class FixtureBuilder
{
    private static int _siguienteId = 1;
    private bool _conResultados;
    private List<Partido> _partidosEliminatorios = new List<Partido>();
    private string _nombreMotor = NombresMotores.Probabilistico;

    public FixtureBuilder ConResultadosCargados()
    {
        _conResultados = true;
        return this;
    }

    public FixtureBuilder ConPartidosEliminatorios(List<Partido> partidos)
    {
        _partidosEliminatorios = partidos;
        return this;
    }

    public FixtureBuilder ConMotor(string nombreMotor)
    {
        _nombreMotor = nombreMotor;
        return this;
    }

    public Fixture Construir()
    {
        Fixture fixture = new Fixture(42, _nombreMotor);
        string[] etiquetas = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };

        foreach (string etiqueta in etiquetas)
            fixture.AgregarGrupo(ConstruirGrupo(etiqueta));

        foreach (Partido partido in _partidosEliminatorios)
            fixture.AgregarPartidoEliminatoria(partido);

        return fixture;
    }

    // ----------------------------------------------------------------
    // Metodos estaticos para construir eliminatorios en estado jugado
    // ----------------------------------------------------------------

    public static List<Partido> CrearDieciseisavosJugados()
    {
        List<Partido> partidos = new List<Partido>();
        string[] etiquetasA = { "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8" };
        string[] etiquetasB = { "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8" };
        foreach (string etiqueta in etiquetasA)
            partidos.Add(CrearEliminatorioJugado(etiqueta, FasePartido.DieciseisavosDeFinal));
        foreach (string etiqueta in etiquetasB)
            partidos.Add(CrearEliminatorioJugado(etiqueta, FasePartido.DieciseisavosDeFinal));
        return partidos;
    }

    public static List<Partido> CrearOctavosJugados()
    {
        List<Partido> partidos = new List<Partido>();
        for (int i = 1; i <= 8; i++)
            partidos.Add(CrearEliminatorioJugado($"C{i}", FasePartido.OctavosDeFinal));
        return partidos;
    }

    public static List<Partido> CrearCuartosJugados()
    {
        List<Partido> partidos = new List<Partido>();
        for (int i = 1; i <= 4; i++)
            partidos.Add(CrearEliminatorioJugado($"D{i}", FasePartido.CuartosDeFinal));
        return partidos;
    }

    public static List<Partido> CrearSemifinalesJugadas()
    {
        return new List<Partido>
        {
            CrearEliminatorioJugado("S1", FasePartido.Semifinal),
            CrearEliminatorioJugado("S2", FasePartido.Semifinal)
        };
    }

    // ----------------------------------------------------------------
    // Privados
    // ----------------------------------------------------------------

    private Grupo ConstruirGrupo(string etiqueta)
    {
        Grupo grupo = new Grupo(etiqueta);
        Equipo e1 = new Equipo($"{etiqueta}1", Confederacion.UEFA, 1800);
        Equipo e2 = new Equipo($"{etiqueta}2", Confederacion.CONMEBOL, 1700);
        Equipo e3 = new Equipo($"{etiqueta}3", Confederacion.CAF, 1600);
        Equipo e4 = new Equipo($"{etiqueta}4", Confederacion.AFC, 1500);

        grupo.AgregarEquipo(e1);
        grupo.AgregarEquipo(e2);
        grupo.AgregarEquipo(e3);
        grupo.AgregarEquipo(e4);

        Jornada j1 = new Jornada(1, new DateTime(2026, 6, 1));
        Jornada j2 = new Jornada(2, new DateTime(2026, 6, 4));
        Jornada j3 = new Jornada(3, new DateTime(2026, 6, 7));

        j1.AgregarPartido(CrearPartido(e1, e4, etiqueta, j1.Fecha, _conResultados));
        j1.AgregarPartido(CrearPartido(e2, e3, etiqueta, j1.Fecha.AddHours(4), _conResultados));
        j2.AgregarPartido(CrearPartido(e1, e3, etiqueta, j2.Fecha, _conResultados));
        j2.AgregarPartido(CrearPartido(e2, e4, etiqueta, j2.Fecha.AddHours(4), _conResultados));
        j3.AgregarPartido(CrearPartido(e1, e2, etiqueta, j3.Fecha, _conResultados));
        j3.AgregarPartido(CrearPartido(e3, e4, etiqueta, j3.Fecha.AddHours(4), _conResultados));

        grupo.AgregarJornada(j1);
        grupo.AgregarJornada(j2);
        grupo.AgregarJornada(j3);

        return grupo;
    }

    private static Partido CrearPartido(Equipo local, Equipo visitante, string grupo, DateTime fecha, bool conResultado)
    {
        Partido partido = new Partido
        {
            Id = _siguienteId++,
            Fecha = fecha,
            Estadio = new Estadio { Nombre = "Estadio", Ciudad = "Ciudad", Capacidad = 20000 },
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = grupo,
            Fase = FasePartido.FaseDeGrupos
        };
        if (conResultado)
            partido.CargarResultado(2, 1);
        return partido;
    }

    private static Partido CrearEliminatorioJugado(string etiqueta, FasePartido fase)
    {
        Equipo local = new Equipo($"Local_{etiqueta}", Confederacion.UEFA, 1800);
        Equipo visitante = new Equipo($"Visit_{etiqueta}", Confederacion.CONMEBOL, 1500);
        Partido partido = new Partido
        {
            Id = _siguienteId++,
            Fecha = new DateTime(2026, 7, 1, 16, 0, 0),
            Estadio = new Estadio { Nombre = "Estadio", Ciudad = "Ciudad", Capacidad = 20000 },
            EquipoLocal = local,
            EquipoVisitante = visitante,
            Grupo = "X",
            Fase = fase,
            EtiquetaCruce = etiqueta
        };
        partido.CargarResultado(2, 1);
        return partido;
    }
}
