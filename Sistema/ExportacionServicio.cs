using ClosedXML.Excel;
using Dominio;
using Dominio.Excepciones;
using Repositorio;
using Sistema.Interfaces;

namespace Sistema;

public class ExportacionServicio : IExportacionServicio
{
    private readonly IRepositorioLog _repositorioLog;
    private readonly IFixtureRepositorio _fixtureRepositorio;
    private readonly LogServicio _logServicio;

    public ExportacionServicio(
        IRepositorioLog repositorioLog,
        IFixtureRepositorio fixtureRepositorio,
        LogServicio logServicio)
    {
        _repositorioLog = repositorioLog;
        _fixtureRepositorio = fixtureRepositorio;
        _logServicio = logServicio;
    }

    public byte[] ExportarLogsCsv(DateTime desde, DateTime hasta, string usuarioEmail)
    {
        List<LogEntry> logs = _repositorioLog.ObtenerPorRango(desde, hasta.AddDays(1).AddSeconds(-1));
        string contenido = GenerarCsvLogs(logs);
        byte[] resultado = System.Text.Encoding.UTF8.GetBytes(contenido);
        _logServicio.Registrar(usuarioEmail, "ExportacionLogs",
            $"Exportacion CSV de logs desde {desde:yyyy-MM-dd} hasta {hasta:yyyy-MM-dd}. Registros: {logs.Count}.");
        return resultado;
    }

    public byte[] ExportarLogsXlsx(DateTime desde, DateTime hasta, string usuarioEmail)
    {
        List<LogEntry> logs = _repositorioLog.ObtenerPorRango(desde, hasta.AddDays(1).AddSeconds(-1));
        byte[] resultado = GenerarXlsxLogs(logs);
        _logServicio.Registrar(usuarioEmail, "ExportacionLogs",
            $"Exportacion XLSX de logs desde {desde:yyyy-MM-dd} hasta {hasta:yyyy-MM-dd}. Registros: {logs.Count}.");
        return resultado;
    }

    public byte[] ExportarFixtureCsv(string usuarioEmail)
    {
        Fixture fixture = ObtenerFixtureOLanzarExcepcion();
        List<Partido> partidos = fixture.ObtenerTodosLosPartidos();
        string contenido = GenerarCsvFixture(partidos);
        byte[] resultado = System.Text.Encoding.UTF8.GetBytes(contenido);
        _logServicio.Registrar(usuarioEmail, "ExportacionFixture",
            $"Exportacion CSV de fixture. Partidos: {partidos.Count}.");
        return resultado;
    }

    public byte[] ExportarFixtureXlsx(string usuarioEmail)
    {
        Fixture fixture = ObtenerFixtureOLanzarExcepcion();
        List<Partido> partidos = fixture.ObtenerTodosLosPartidos();
        byte[] resultado = GenerarXlsxFixture(partidos);
        _logServicio.Registrar(usuarioEmail, "ExportacionFixture",
            $"Exportacion XLSX de fixture. Partidos: {partidos.Count}.");
        return resultado;
    }

    private Fixture ObtenerFixtureOLanzarExcepcion()
    {
        Fixture? fixture = _fixtureRepositorio.ObtenerActual();
        if (fixture == null)
            throw new DominioException("No existe un fixture generado.");
        return fixture;
    }

    private string GenerarCsvLogs(List<LogEntry> logs)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Timestamp,Usuario,Accion,Detalle");
        foreach (LogEntry log in logs)
            sb.AppendLine($"{log.Timestamp:yyyy-MM-ddTHH:mm:ss},{log.UsuarioEmail},{log.Accion},{log.Detalle}");
        return sb.ToString();
    }

    private byte[] GenerarXlsxLogs(List<LogEntry> logs)
    {
        using XLWorkbook workbook = new XLWorkbook();
        IXLWorksheet hoja = workbook.Worksheets.Add("Logs");

        hoja.Cell(1, 1).Value = "Timestamp";
        hoja.Cell(1, 2).Value = "Usuario";
        hoja.Cell(1, 3).Value = "Accion";
        hoja.Cell(1, 4).Value = "Detalle";

        for (int i = 0; i < logs.Count; i++)
        {
            hoja.Cell(i + 2, 1).Value = logs[i].Timestamp.ToString("yyyy-MM-ddTHH:mm:ss");
            hoja.Cell(i + 2, 2).Value = logs[i].UsuarioEmail;
            hoja.Cell(i + 2, 3).Value = logs[i].Accion.ToString();
            hoja.Cell(i + 2, 4).Value = logs[i].Detalle;
        }

        using System.IO.MemoryStream stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private string GenerarCsvFixture(List<Partido> partidos)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Fase,Grupo,Fecha,Estadio,EquipoLocal,EquipoVisitante,GolesLocal,GolesVisitante");
        foreach (Partido partido in partidos)
            sb.AppendLine($"{partido.Fase}," +
                          $"{partido.Grupo}," +
                          $"{partido.Fecha:yyyy-MM-ddTHH:mm:ss}," +
                          $"{partido.Estadio?.Nombre ?? "-"}," +
                          $"{partido.EquipoLocal?.Nombre ?? "-"}," +
                          $"{partido.EquipoVisitante?.Nombre ?? "-"}," +
                          $"{partido.GolesLocal?.ToString() ?? "-"}," +
                          $"{partido.GolesVisitante?.ToString() ?? "-"}");
        return sb.ToString();
    }

    private byte[] GenerarXlsxFixture(List<Partido> partidos)
    {
        using XLWorkbook workbook = new XLWorkbook();
        IXLWorksheet hoja = workbook.Worksheets.Add("Fixture");

        hoja.Cell(1, 1).Value = "Fase";
        hoja.Cell(1, 2).Value = "Grupo";
        hoja.Cell(1, 3).Value = "Fecha";
        hoja.Cell(1, 4).Value = "Estadio";
        hoja.Cell(1, 5).Value = "Equipo Local";
        hoja.Cell(1, 6).Value = "Equipo Visitante";
        hoja.Cell(1, 7).Value = "Goles Local";
        hoja.Cell(1, 8).Value = "Goles Visitante";

        for (int i = 0; i < partidos.Count; i++)
        {
            Partido partido = partidos[i];
            hoja.Cell(i + 2, 1).Value = partido.Fase.ToString();
            hoja.Cell(i + 2, 2).Value = partido.Grupo;
            hoja.Cell(i + 2, 3).Value = partido.Fecha.ToString("yyyy-MM-ddTHH:mm:ss");
            hoja.Cell(i + 2, 4).Value = partido.Estadio?.Nombre ?? "-";
            hoja.Cell(i + 2, 5).Value = partido.EquipoLocal?.Nombre ?? "-";
            hoja.Cell(i + 2, 6).Value = partido.EquipoVisitante?.Nombre ?? "-";
            hoja.Cell(i + 2, 7).Value = partido.GolesLocal?.ToString() ?? "-";
            hoja.Cell(i + 2, 8).Value = partido.GolesVisitante?.ToString() ?? "-";
        }

        using System.IO.MemoryStream stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}