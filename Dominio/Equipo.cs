using Dominio.Excepciones;

namespace Dominio;

public class Equipo
{
    private const int _nombreMaxLength = 60;
    private const int _rankingMin = 300;
    private const int _rankingMax = 2500;

    public int Id { get; set; }
    public string Nombre { get; private set; } = string.Empty;
    public Confederacion Confederacion { get; private set; }
    public int RankingFIFA { get; private set; }
    public int RankingActual { get; private set; }
    public string? BanderaBase64 { get; private set; }

    public Equipo(string nombre, Confederacion confederacion, int rankingFIFA)
    {
        ValidarNombre(nombre);
        ValidarConfederacion(confederacion);
        ValidarRanking(rankingFIFA);
        Nombre = nombre;
        Confederacion = confederacion;
        RankingFIFA = rankingFIFA;
        RankingActual = rankingFIFA;
    }

    protected Equipo() { }  

    public void ActualizarRankingActual(int ranking)
    {
        ValidarRanking(ranking);
        RankingActual = ranking;
    }

    public void EstablecerBandera(string banderaBase64)
    {
        if (string.IsNullOrWhiteSpace(banderaBase64))
            throw new DominioException("La bandera no puede estar vacía.");
        BanderaBase64 = banderaBase64;
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > _nombreMaxLength)
            throw new DominioException("El nombre es obligatorio y debe tener entre 1 y 60 caracteres.");
    }

    private static void ValidarRanking(int ranking)
    {
        if (ranking < _rankingMin || ranking > _rankingMax)
            throw new DominioException("El RankingFIFA debe estar entre 300 y 2500.");
    }

    private static void ValidarConfederacion(Confederacion confederacion)
    {
        if (!Enum.IsDefined(typeof(Confederacion), confederacion))
            throw new DominioException("La confederacion no es valida.");
    }
}