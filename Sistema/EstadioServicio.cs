using Dominio;
using Dominio.Excepciones;
using Repositorio;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema;

public class EstadioServicio
{
    private readonly IRepositorioEstadio _estadioRepositorio;
    private readonly LogServicio _logServicio;

    public EstadioServicio(IRepositorioEstadio estadioRepositorio, LogServicio logServicio)
    {
        _estadioRepositorio = estadioRepositorio;
        _logServicio = logServicio;
    }

    public EstadioDTO Crear(EstadioDTO estadioDTO, string usuarioEmail)
    {
        Estadio estadio = EstadioMapper.AEstadio(estadioDTO);
        ValidarNombreUnico(estadio.Nombre);
        _estadioRepositorio.Agregar(estadio);
        _logServicio.Registrar(usuarioEmail, "AltaEstadio",
            $"Se creo el estadio '{estadio.Nombre}'.");
        return EstadioMapper.AEstadioDTO(estadio);
    }

    public void Modificar(EstadioDTO estadioDTO, string usuarioEmail)
    {
        Estadio estadio = EstadioMapper.AEstadio(estadioDTO);
        ValidarExistencia(estadio.Id);
        ValidarNombreUnicoParaModificacion(estadio.Nombre, estadio.Id);
        _estadioRepositorio.Modificar(estadio);
        _logServicio.Registrar(usuarioEmail, "EdicionEstadio",
            $"Se modifico el estadio Id={estadio.Id}.");
    }

    public void Eliminar(int id, string usuarioEmail)
    {
        Estadio existente = ValidarExistencia(id);
        _estadioRepositorio.Eliminar(id);
        _logServicio.Registrar(usuarioEmail, "EliminacionEstadio",
            $"Se elimino el estadio '{existente.Nombre}'.");
    }

    public EstadioDTO ObtenerPorId(int id) => EstadioMapper.AEstadioDTO(ValidarExistencia(id));

    public List<EstadioDTO> ListarTodos() =>
        _estadioRepositorio.ListarTodos().Select(EstadioMapper.AEstadioDTO).ToList();

    private Estadio ValidarExistencia(int id)
    {
        Estadio? estadio = _estadioRepositorio.BuscarPorId(id);
        if (estadio is null)
            throw new DominioException("El estadio no existe.");
        return estadio;
    }

    private void ValidarNombreUnico(string nombre)
    {
        if (_estadioRepositorio.BuscarPorNombre(nombre) is not null)
            throw new DominioException($"Ya existe un estadio con el nombre '{nombre}'.");
    }

    private void ValidarNombreUnicoParaModificacion(string nombre, int idActual)
    {
        Estadio? duplicado = _estadioRepositorio.BuscarPorNombre(nombre);
        if (duplicado is not null && duplicado.Id != idActual)
            throw new DominioException($"Ya existe un estadio con el nombre '{nombre}'.");
    }
}
