namespace Sistema.Importacion;

public class ResultadoImportacion
{
    public int Exitosos { get; private set; }
    public List<string> Errores { get; private set; }

    public ResultadoImportacion()
    {
        Errores = new List<string>();
    }

    public void AgregarExito() => Exitosos++;
    public void AgregarError(string error) => Errores.Add(error);
    public bool TieneErrores => Errores.Count > 0;
}