namespace Sistema.Importacion;

public interface IImportacionServicio
{
    ResultadoImportacion Importar(string contenidoCsv);
}