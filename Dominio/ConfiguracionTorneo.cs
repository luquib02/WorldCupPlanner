using Dominio.Excepciones;

namespace Dominio;

public record ConfiguracionTorneo
{
    private static readonly DateTime _fechaInicioPorDefecto = new(2026, 6, 1);
    private const int _maxPartidosPorDiaPorDefecto = 3;
    private const int _separacionEntreFechasPorDefecto = 3;

    public int SemillaFixture { get; init; }
    public int SemillaCompletar { get; init; }
    public int SemillaCrucesFase { get; init; }
    public int SemillaSimulation { get; init; }
    public DateTime FechaInicioTorneo { get; init; }
    public int MaxPartidosPorDia { get; init; }
    public int SeparacionEntreFechas { get; init; }

    public ConfiguracionTorneo(
        int semillaFixture,
        int semillaCompletar,
        int semillaCrucesFase,
        int semillaSimulation,
        DateTime fechaInicioTorneo,
        int maxPartidosPorDia,
        int separacionEntreFechas)
    {
        ValidarMaxPartidosPorDia(maxPartidosPorDia);
        ValidarSeparacionEntreFechas(separacionEntreFechas);

        SemillaFixture = semillaFixture;
        SemillaCompletar = semillaCompletar;
        SemillaCrucesFase = semillaCrucesFase;
        SemillaSimulation = semillaSimulation;
        FechaInicioTorneo = fechaInicioTorneo;
        MaxPartidosPorDia = maxPartidosPorDia;
        SeparacionEntreFechas = separacionEntreFechas;
    }

    public static ConfiguracionTorneo PorDefecto() =>
        new(0, 0, 0, 0, _fechaInicioPorDefecto, _maxPartidosPorDiaPorDefecto, _separacionEntreFechasPorDefecto);

    private static void ValidarMaxPartidosPorDia(int valor)
    {
        if (valor <= 0)
            throw new DominioException("MaxPartidosPorDia debe ser mayor a 0.");
    }

    private static void ValidarSeparacionEntreFechas(int valor)
    {
        if (valor <= 0)
            throw new DominioException("SeparacionEntreFechas debe ser mayor a 0.");
    }
}