namespace Sistema.Interfaces;

public interface IExportacionServicio
{
    byte[] ExportarLogsCsv(DateTime desde, DateTime hasta, string usuarioEmail);
    byte[] ExportarLogsXlsx(DateTime desde, DateTime hasta, string usuarioEmail);
    byte[] ExportarFixtureCsv(string usuarioEmail);
    byte[] ExportarFixtureXlsx(string usuarioEmail);
}