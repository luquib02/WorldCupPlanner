using Dominio;
using Dominio.Excepciones;
using Repositorio;

namespace Sistema.Importacion.FacilidadComprension;

public class ImportacionServicioLegible : IImportacionServicio
{
    private const char SeparadorColumnas = ',';
    private const string ColumnaObligatoriaNombre = "Nombre";
    private const string ColumnaObligatoriaConfederacion = "Confederacion";
    private const string ColumnaObligatoriaRankingFIFA = "RankingFIFA";

    private readonly IEquipoRepositorio _equipoRepositorio;

    public ImportacionServicioLegible(IEquipoRepositorio equipoRepositorio)
    {
        _equipoRepositorio = equipoRepositorio;
    }

    public ResultadoImportacion Importar(string contenidoCsv)
    {
        ResultadoImportacion resultado = new ResultadoImportacion();
        List<string> lineas = SepararLineas(contenidoCsv);
        ValidarArchivoNoVacio(lineas);

        string[] encabezados = ObtenerEncabezados(lineas.First());
        ValidarEncabezadosObligatorios(encabezados);

        IEnumerable<string> lineasDeDatos = lineas.Skip(1);
        foreach (string linea in lineasDeDatos)
            ProcesarLinea(linea, encabezados, resultado);

        return resultado;
    }

    private List<string> SepararLineas(string contenidoCsv)
    {
        return contenidoCsv
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();
    }

    private void ValidarArchivoNoVacio(List<string> lineas)
    {
        if (lineas.Count < 2)
            throw new DominioException("El archivo CSV no contiene datos.");
    }

    private string[] ObtenerEncabezados(string primeraLinea)
    {
        return primeraLinea.Split(SeparadorColumnas)
            .Select(e => e.Trim())
            .ToArray();
    }

    private void ValidarEncabezadosObligatorios(string[] encabezados)
    {
        string[] columnsObligatorias = { ColumnaObligatoriaNombre, ColumnaObligatoriaConfederacion, ColumnaObligatoriaRankingFIFA };
        foreach (string columna in columnsObligatorias)
            if (!encabezados.Contains(columna))
                throw new DominioException($"El archivo CSV no contiene la columna obligatoria: {columna}");
    }

    private void ProcesarLinea(string linea, string[] encabezados, ResultadoImportacion resultado)
    {
        try
        {
            Equipo equipo = CrearEquipoDesdeLinea(linea, encabezados);
            ValidarNombreUnico(equipo.Nombre);
            _equipoRepositorio.Agregar(equipo);
            resultado.AgregarExito();
        }
        catch (Exception ex)
        {
            resultado.AgregarError(ex.Message);
        }
    }

    private Equipo CrearEquipoDesdeLinea(string linea, string[] encabezados)
    {
        string[] valores = linea.Split(SeparadorColumnas);
        string nombre = ObtenerValorColumna(valores, encabezados, ColumnaObligatoriaNombre);
        Confederacion confederacion = ParsearConfederacion(ObtenerValorColumna(valores, encabezados, ColumnaObligatoriaConfederacion));
        int rankingFIFA = ParsearRankingFIFA(ObtenerValorColumna(valores, encabezados, ColumnaObligatoriaRankingFIFA));
        return new Equipo(nombre, confederacion, rankingFIFA);
    }

    private string ObtenerValorColumna(string[] valores, string[] encabezados, string nombreColumna)
    {
        int indice = Array.IndexOf(encabezados, nombreColumna);
        return valores[indice].Trim();
    }

    private Confederacion ParsearConfederacion(string valor)
    {
        if (!Enum.TryParse(valor, out Confederacion confederacion))
            throw new DominioException($"Confederación inválida: {valor}");
        return confederacion;
    }

    private int ParsearRankingFIFA(string valor)
    {
        if (!int.TryParse(valor, out int rankingFIFA))
            throw new DominioException($"RankingFIFA inválido: {valor}");
        return rankingFIFA;
    }

    private void ValidarNombreUnico(string nombre)
    {
        if (_equipoRepositorio.ExisteNombre(nombre))
            throw new DominioException($"Ya existe un equipo con el nombre: {nombre}");
    }
}