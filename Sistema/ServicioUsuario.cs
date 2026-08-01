using Dominio;
using Repositorio;
using Sistema.Interfaces;
using System.Linq;
using Sistema.DTOs;

namespace Sistema;

public class ServicioUsuario : IServicioUsuario
{
    private IRepositorioUsuario _repositorioUsuario;
    private IServicioSesion _servicioSesion;
    private IServicioLog _servicioLog;
    private const string CONTRASENIA_DEFECTO = "Sistema123!@";

    public ServicioUsuario(IRepositorioUsuario repositorioUsuario, IServicioSesion servicioSesion, IServicioLog servicioLog)
    {
        _repositorioUsuario = repositorioUsuario;
        _servicioSesion = servicioSesion;
        _servicioLog = servicioLog;
        
    }

    public void IniciarSesion(string correoElectronico, string contrasenia)
    {
        Usuario usuario = _repositorioUsuario.ObtenerUsuarioPorCorreo(correoElectronico);
        if (usuario.VerificarContrasenia(contrasenia))
        {
            _servicioSesion.IniciarSesion(usuario);
        }
        else
        {
            throw ExcepcionServicioUsuario.ContraseniaIncorrecta();
        }
    }
    
    public void CerrarSesion()
    {
        _servicioSesion.CerrarSesion();
    }
    public void CrearUsuario(UsuarioDTO usuarioDto, string contrasenia)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        var usuario = usuarioDto.convertirAUsuario();
        usuario.EstablecerContrasenia(contrasenia);
        _repositorioUsuario.CrearUsuario(usuario);
        _servicioLog.Registrar(ObtenerUsuarioSesion.CorreoElectronico, "Alta de Usuario", "Usuario creado: " + usuario.CorreoElectronico);
    }

    public void ModificarUsuario(int usuarioId, UsuarioDTO usuarioModificado)
    {
        var usuario = usuarioModificado.convertirAUsuario();
        _repositorioUsuario.ModificarUsuario(usuarioId, usuario);
        _servicioLog.Registrar(ObtenerUsuarioSesion.CorreoElectronico, "Modificacion de Usuario",
            $"Usuario modificado: Id={usuarioId}.");
    }
    
    public Usuario ObtenerUsuarioSesion => _servicioSesion.ObtenerUsuarioActual();

    public void EliminarUsuario(int usuarioId)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        _repositorioUsuario.EliminarUsuario(usuarioId);
        _servicioLog.Registrar(ObtenerUsuarioSesion.CorreoElectronico, "Baja de Usuario",
            $"Usuario eliminado: Id={usuarioId}.");
    }

    public IReadOnlyList<UsuarioDTO> ObtenerListaUsuarios()
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        return _repositorioUsuario.ObtenerListaUsuarios().Select(u => UsuarioDTO.convertirAUsuarioDTO(u)).ToList();
    }

    public IReadOnlyList<RolDeUsuario>? ObtenerListaRoles()
    {
        return _servicioSesion.UsuarioActual?.Roles.ToList();
    }
    public IReadOnlyList<RolDeUsuario> ObtenerListaRoles(int usuarioId)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        return _repositorioUsuario.ObtenerUsuarioPorId(usuarioId).Roles.ToList();
    }

    public void AsignarRol(int usuarioId, RolDeUsuario rol)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        Usuario usuario = _repositorioUsuario.ObtenerUsuarioPorId(usuarioId);
        usuario.AgregarRol(rol);
        _repositorioUsuario.ModificarUsuario(usuarioId, usuario);
        _servicioLog.Registrar(ObtenerUsuarioSesion.CorreoElectronico, "Asignacion de Rol",
            $"Rol {rol} asignado al usuario Id={usuarioId}.");
        if (ObtenerUsuarioSesion.Id == usuarioId)
            _servicioSesion.ActualizarUsuarioActual(usuario);
    }
    public void QuitarRol(int usuarioId, RolDeUsuario rol)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        Usuario usuario = _repositorioUsuario.ObtenerUsuarioPorId(usuarioId);
        usuario.QuitarRol(rol);
        _repositorioUsuario.ModificarUsuario(usuarioId, usuario);
        _servicioLog.Registrar(ObtenerUsuarioSesion.CorreoElectronico, "Quita de Rol",
            $"Rol {rol} quitado al usuario Id={usuarioId}.");
        if (ObtenerUsuarioSesion.Id == usuarioId)
            _servicioSesion.ActualizarUsuarioActual(usuario);
    }

    public void ModificarContrasenia(string contraseniaActual, string contraseniaNueva)
    {
        Usuario usuarioActual = ObtenerUsuarioSesion;
        if (!usuarioActual.VerificarContrasenia(contraseniaActual))
            throw ExcepcionServicioUsuario.ContraseniaIncorrecta();
        usuarioActual.EstablecerContrasenia(contraseniaNueva);
        _repositorioUsuario.ActualizarContrasenia(usuarioActual.Id, usuarioActual.HashDeContrasena);
        _servicioLog.Registrar(usuarioActual.CorreoElectronico, "Cambio de Contrasena",
            "El usuario cambió su contraseña.");
    }
    public UsuarioDTO ObtenerUsuario(int idUsuario)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        var usuario = _repositorioUsuario.ObtenerUsuarioPorId(idUsuario);
        return UsuarioDTO.convertirAUsuarioDTO(usuario);
    }

    public void GenerarContraseniaDefecto(int usuarioId)
    {
        _servicioSesion.TienePermiso(RolDeUsuario.AdministradorDelSistema);
        Usuario usuario = _repositorioUsuario.ObtenerUsuarioPorId(usuarioId);
        usuario.EstablecerContrasenia(CONTRASENIA_DEFECTO);
        _repositorioUsuario.ActualizarContrasenia(usuarioId, usuario.HashDeContrasena);
        _servicioLog.Registrar(ObtenerUsuarioSesion.CorreoElectronico, "Reseteo de Contrasena",
            $"Se reseteó la contraseña del usuario Id={usuarioId}.");
    }
}
