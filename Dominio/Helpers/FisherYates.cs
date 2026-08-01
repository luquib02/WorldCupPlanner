using Dominio.Excepciones;

namespace Dominio.Helpers;

public static class FisherYates
{
    public static void Barajar<T>(List<T> lista, int semilla)
    {
        if (lista is null)
            throw new DominioException("La lista a barajar no puede ser nula.");

        Random random = new Random(semilla);
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }
}