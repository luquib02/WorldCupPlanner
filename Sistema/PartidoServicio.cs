using Dominio;
using Dominio.Excepciones;
using Repositorio;
using Sistema.DTOs;
using Sistema.Mappers;

namespace Sistema;

public class PartidoServicio
{
    private readonly IPartidoRepositorio _partidoRepositorio;
    private readonly IRepositorioEstadio _estadioRepositorio;
    private readonly LogServicio _logServicio;
    private readonly ResultadoPartidoServicio _resultadoServicio;

    public PartidoServicio(IPartidoRepositorio partidoRepositorio, IRepositorioEstadio estadioRepositorio,
        LogServicio logServicio, ResultadoPartidoServicio resultadoServicio)
    {
        _partidoRepositorio = partidoRepositorio;
        _estadioRepositorio = estadioRepositorio;
        _logServicio = logServicio;
        _resultadoServicio = resultadoServicio;
    }

    public Partido Crear(Partido partido, string usuarioEmail)
    {
        partido.Validar();
        _partidoRepositorio.Agregar(partido);
        _logServicio.Registrar(usuarioEmail, "AltaPartido",
            $"Se creo el partido Id={partido.Id}.");
        return partido;
    }

    public void ModificarFecha(int id, DateTime nuevaFecha, string usuarioEmail)
    {
        Partido partido = ValidarExistencia(id);
        ValidarEdicionPermitida(partido);
        partido.Fecha = nuevaFecha;
        _partidoRepositorio.Actualizar(partido);
        _logServicio.Registrar(usuarioEmail, "ModificacionPartido",
            $"Se modifico la fecha del partido Id={id}.");
    }

    public void ModificarEstadio(int idPartido, int idEstadio, string usuarioEmail)
    {
        Partido partido = ValidarExistencia(idPartido);
        ValidarEdicionPermitida(partido);
        partido.Estadio = ValidarExistenciaEstadio(idEstadio);
        _partidoRepositorio.Actualizar(partido);
        _logServicio.Registrar(usuarioEmail, "ModificacionPartido",
            $"Se modifico el estadio del partido Id={idPartido}.");
    }

    public void CargarResultado(int id, int golesLocal, int golesVisitante, string usuarioEmail)
    {
        Partido partido = ValidarExistencia(id);
        ValidarEdicionPermitida(partido);
        partido.CargarResultado(golesLocal, golesVisitante);
        _resultadoServicio.AplicarResultado(partido, usuarioEmail, "Resultado cargado");
        _logServicio.Registrar(usuarioEmail, "ModificacionPartido",
            $"Se cargo resultado del partido Id={id}: {golesLocal}-{golesVisitante}.");
    }

    public PartidoDTO ObtenerPorId(int id) => PartidoMapper.APartidoDTO(ValidarExistencia(id));

    public List<PartidoDTO> ListarTodos() =>
        _partidoRepositorio.ObtenerTodos().Select(PartidoMapper.APartidoDTO).ToList();

    public List<PartidoDTO> ListarPorGrupo(string grupo) =>
        _partidoRepositorio.ObtenerPorGrupo(grupo).Select(PartidoMapper.APartidoDTO).ToList();

    public List<PartidoDTO> ListarPorFase(FasePartido fase) =>
        _partidoRepositorio.ObtenerPorFase(fase).Select(PartidoMapper.APartidoDTO).ToList();

    private Partido ValidarExistencia(int id)
    {
        Partido? partido = _partidoRepositorio.ObtenerPorId(id);
        if (partido is null)
            throw new DominioException("El partido no existe.");
        return partido;
    }

    private Estadio ValidarExistenciaEstadio(int id)
    {
        Estadio? estadio = _estadioRepositorio.BuscarPorId(id);
        if (estadio is null)
            throw new DominioException("El estadio no existe.");
        return estadio;
    }

    private void ValidarEdicionPermitida(Partido partido)
    {
        if (partido.Fase != FasePartido.FaseDeGrupos) return;
        if (_partidoRepositorio.ExistenPartidosFueraDeGrupos())
            throw new DominioException("No se pueden editar partidos de fase de grupos una vez generados los cruces.");
    }
}
