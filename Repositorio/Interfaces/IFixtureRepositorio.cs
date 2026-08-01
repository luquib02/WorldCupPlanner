using Dominio;

namespace Repositorio;

public interface IFixtureRepositorio
{
    void Guardar(Fixture fixture);
    Fixture? ObtenerActual();
    bool Existe();
}