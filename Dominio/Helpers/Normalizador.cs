using System.Globalization;
using System.Text;

namespace Dominio.Helpers;

public static class Normalizador
{
    public static string Normalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        string sinTildes = RemoverTildes(texto);
        string procesado = ProcesarCaracteres(sinTildes);
        return procesado.Trim();
    }

    private static string RemoverTildes(string texto)
    {
        string normalizado = texto.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();
        foreach (char c in normalizado)
        {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string ProcesarCaracteres(string texto)
    {
        StringBuilder sb = new StringBuilder();
        bool ultimoFueEspacio = false;
        foreach (char c in texto.ToLower())
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                ultimoFueEspacio = false;
            }
            else if (!ultimoFueEspacio)
            {
                sb.Append(' ');
                ultimoFueEspacio = true;
            }
        }
        return sb.ToString();
    }
}