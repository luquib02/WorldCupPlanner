using Dominio.Excepciones;
using Dominio.Simulacion;
using System.Collections.Generic;
using System.Linq;

namespace Sistema.Simulacion;

public static class MotorSimulacionFactory
{
    // Registro único de motores: para agregar uno nuevo basta con sumar una entrada
    // a este diccionario (OCP). ObtenerPorNombre y ObtenerNombresDisponibles derivan de aquí.
    private static readonly Dictionary<string, Func<IMotorSimulacion>> _motores = new()
    {
        [NombresMotores.Probabilistico] = () => new MotorProbabilistico(),
        [NombresMotores.AleatorioPuro] = () => new MotorAleatorioPuro()
    };

    public static IMotorSimulacion ObtenerPorNombre(string nombre)
    {
        if (_motores.TryGetValue(nombre, out Func<IMotorSimulacion>? crear))
            return crear();
        throw new DominioException($"No existe el motor de simulación '{nombre}'.");
    }

    public static List<string> ObtenerNombresDisponibles()
    {
        return _motores.Keys.ToList();
    }
}
