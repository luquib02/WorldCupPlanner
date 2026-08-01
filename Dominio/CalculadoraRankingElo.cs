namespace Dominio;

public class CalculadoraRankingElo
{
    private const int K = 30;
    private const double MultiplicadorGrupos = 1.0;
    private const double MultiplicadorEliminatorias = 1.5;
    private const int RankingMinimo = 300;
    private const int RankingMaximo = 2500;

    public int CalcularNuevoRanking(int rankingActual, int rankingOponente, double resultado, FasePartido fase)
    {
        double probabilidad = CalcularProbabilidad(rankingActual, rankingOponente);
        double multiplicador = ObtenerMultiplicadorFase(fase);
        double delta = K * (resultado - probabilidad) * multiplicador;
        int nuevoRanking = (int)Math.Round(rankingActual + delta, MidpointRounding.AwayFromZero);
        return Math.Clamp(nuevoRanking, RankingMinimo, RankingMaximo);
    }

    private static double CalcularProbabilidad(int rankingActual, int rankingOponente)
    {
        return 1.0 / (1.0 + Math.Pow(10, (rankingOponente - rankingActual) / 1000.0));
    }

    private static double ObtenerMultiplicadorFase(FasePartido fase)
    {
        return fase == FasePartido.FaseDeGrupos ? MultiplicadorGrupos : MultiplicadorEliminatorias;
    }
}
