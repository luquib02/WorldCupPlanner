using Dominio.Excepciones;

namespace Dominio.Helpers;

public static class ValidadorContraseniaUsuario
{
    public static void Validar(string contrasenia)
    {
        if (string.IsNullOrWhiteSpace(contrasenia))
            throw ExcepcionUsuario.ContraseniaVacia();

        if (contrasenia.Length < 8)
            throw ExcepcionUsuario.ContraseniaCorta();

        if (!contrasenia.Any(char.IsUpper))
            throw ExcepcionUsuario.ContraseniaSinMayuscula();

        if (!contrasenia.Any(char.IsLower))
            throw ExcepcionUsuario.ContraseniaSinMinuscula();

        if (!contrasenia.Any(char.IsDigit))
            throw ExcepcionUsuario.ContraseniaSinNumero();

        if (!contrasenia.Any(c => !char.IsLetterOrDigit(c)))
            throw ExcepcionUsuario.ContraseniaSinEspecial();
    }
}
