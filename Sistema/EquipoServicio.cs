using Dominio;
using Dominio.Excepciones;
using Repositorio;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema;

public class EquipoServicio
{
    private readonly IEquipoRepositorio _equipoRepositorio;
    private readonly LogServicio _logServicio;

    private static readonly Dictionary<Confederacion, int> _cuposPorConfederacion = new Dictionary<Confederacion, int>
    {
        { Confederacion.UEFA, 16 },
        { Confederacion.CONMEBOL, 7 },
        { Confederacion.CONCACAF, 7 },
        { Confederacion.CAF, 9 },
        { Confederacion.AFC, 8 },
        { Confederacion.OFC, 1 }
    };

    public EquipoServicio(IEquipoRepositorio equipoRepositorio, LogServicio logServicio)
    {
        _equipoRepositorio = equipoRepositorio;
        _logServicio = logServicio;
    }

    public void Agregar(EquipoDTO equipoDTO, string usuarioEmail)
    {
        Equipo equipo = EquipoMapper.AEquipo(equipoDTO);
        ValidarNombreUnico(equipo.Nombre);
        ValidarCupoConfederacion(equipo.Confederacion);
        _equipoRepositorio.Agregar(equipo);
        _logServicio.Registrar(usuarioEmail, "AltaEquipo",
            $"Se creo el equipo '{equipo.Nombre}'.");
    }

    public void Actualizar(EquipoDTO equipoDTO, string usuarioEmail)
    {
        Equipo equipo = EquipoMapper.AEquipo(equipoDTO);
        ValidarExistencia(equipo.Id);
        ValidarNombreUnicoParaModificacion(equipo.Nombre, equipo.Id);
        _equipoRepositorio.Actualizar(equipo);
        _logServicio.Registrar(usuarioEmail, "EdicionEquipo",
            $"Se modifico el equipo Id={equipo.Id}.");
    }

    public void Eliminar(int id, string usuarioEmail)
    {
        Equipo existente = ValidarExistencia(id);
        _equipoRepositorio.Eliminar(id);
        _logServicio.Registrar(usuarioEmail, "EliminacionEquipo",
            $"Se elimino el equipo '{existente.Nombre}'.");
    }

    public EquipoDTO ObtenerPorId(int id) => EquipoMapper.AEquipoDTO(ValidarExistencia(id));

    public List<EquipoDTO> ObtenerTodos() =>
        _equipoRepositorio.ObtenerTodos().Select(EquipoMapper.AEquipoDTO).ToList();

    private Equipo ValidarExistencia(int id)
    {
        Equipo? equipo = _equipoRepositorio.ObtenerPorId(id);
        if (equipo is null)
            throw new DominioException($"No se encontro el equipo con id {id}.");
        return equipo;
    }

    private void ValidarNombreUnico(string nombre)
    {
        if (_equipoRepositorio.ExisteNombre(nombre))
            throw new DominioException("Ya existe un equipo con ese nombre.");
    }

    private void ValidarNombreUnicoParaModificacion(string nombre, int idActual)
    {
        Equipo? duplicado = _equipoRepositorio.ObtenerTodos().FirstOrDefault(e => e.Nombre == nombre);
        if (duplicado is not null && duplicado.Id != idActual)
            throw new DominioException("Ya existe un equipo con ese nombre.");
    }

    private void ValidarCupoConfederacion(Confederacion confederacion)
    {
        int cupoMaximo = _cuposPorConfederacion[confederacion];
        int cupoActual = _equipoRepositorio.ContarPorConfederacion(confederacion);
        if (cupoActual >= cupoMaximo)
            throw new DominioException($"Se ha alcanzado el cupo maximo para la confederacion {confederacion}.");
    }
}