using Dominio;
using Repositorio;

namespace Sistema;

// Centraliza los efectos posteriores a un partido con resultado cargado (manual o simulado):
// recalcula el ranking ELO de ambos equipos, persiste equipos y partido, registra el cambio
// en auditoría y genera la notificación. Evita duplicar esta lógica en PartidoServicio,
// FixtureServicio y CrucesServicio.
public class ResultadoPartidoServicio
{
    private readonly IEquipoRepositorio _equipoRepositorio;
    private readonly IPartidoRepositorio _partidoRepositorio;
    private readonly LogServicio _logServicio;
    private readonly CalculadoraRankingElo _calculadora;
    private readonly NotificacionServicio _notificacionServicio;

    public ResultadoPartidoServicio(
        IEquipoRepositorio equipoRepositorio,
        IPartidoRepositorio partidoRepositorio,
        LogServicio logServicio,
        CalculadoraRankingElo calculadora,
        NotificacionServicio notificacionServicio)
    {
        _equipoRepositorio = equipoRepositorio;
        _partidoRepositorio = partidoRepositorio;
        _logServicio = logServicio;
        _calculadora = calculadora;
        _notificacionServicio = notificacionServicio;
    }

    public void AplicarResultado(Partido partido, string usuarioEmail, string descripcionResultado)
    {
        int rankingLocalAntes = partido.EquipoLocal.RankingActual;
        int rankingVisitanteAntes = partido.EquipoVisitante.RankingActual;

        ActualizarRankings(partido);

        _logServicio.Registrar(usuarioEmail, "CambioRanking",
            $"{partido.EquipoLocal.Nombre}: {rankingLocalAntes}→{partido.EquipoLocal.RankingActual} | {partido.EquipoVisitante.Nombre}: {rankingVisitanteAntes}→{partido.EquipoVisitante.RankingActual}");

        _equipoRepositorio.Actualizar(partido.EquipoLocal);
        _equipoRepositorio.Actualizar(partido.EquipoVisitante);
        _partidoRepositorio.Actualizar(partido);

        _notificacionServicio.Crear(
            $"{descripcionResultado}: {partido.EquipoLocal.Nombre} {partido.GolesLocal} - {partido.GolesVisitante} {partido.EquipoVisitante.Nombre}",
            partido.Id);
    }

    private void ActualizarRankings(Partido partido)
    {
        double resLocal     = ObtenerResultadoElo(partido.GolesLocal!.Value,     partido.GolesVisitante!.Value);
        double resVisitante = ObtenerResultadoElo(partido.GolesVisitante!.Value, partido.GolesLocal!.Value);
        int nuevoLocal      = _calculadora.CalcularNuevoRanking(partido.EquipoLocal.RankingActual,     partido.EquipoVisitante.RankingActual, resLocal,     partido.Fase);
        int nuevoVisitante  = _calculadora.CalcularNuevoRanking(partido.EquipoVisitante.RankingActual, partido.EquipoLocal.RankingActual,     resVisitante, partido.Fase);
        partido.EquipoLocal.ActualizarRankingActual(nuevoLocal);
        partido.EquipoVisitante.ActualizarRankingActual(nuevoVisitante);
    }

    private static double ObtenerResultadoElo(int propios, int rivales)
    {
        if (propios > rivales) return 1.0;
        if (propios < rivales) return 0.0;
        return 0.5;
    }
}
