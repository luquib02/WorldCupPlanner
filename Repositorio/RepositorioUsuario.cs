using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly WorldCupPlannerDbContext _contexto;

    public RepositorioUsuario(WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void CrearUsuario(Usuario usuario)
    {
        if (ExisteUsuarioPorCorreo(usuario.CorreoElectronico))
            throw ExcepcionUsuarioRepositorio.UsuarioDuplicado();

        _contexto.Usuarios.Add(usuario);
        _contexto.SaveChanges();

        GuardarRoles(usuario);
        _contexto.SaveChanges();
    }

    public bool ExisteUsuario(int idUsuario)
    {
        return _contexto.Usuarios.Any(u => u.Id == idUsuario);
    }

    public bool ExisteUsuarioPorCorreo(string correo)
    {
        return _contexto.Usuarios.Any(u => u.CorreoElectronico == correo);
    }

    public Usuario ObtenerUsuarioPorId(int idUsuario)
    {
        var usuario = _contexto.Usuarios.FirstOrDefault(u => u.Id == idUsuario)
            ?? throw ExcepcionUsuarioRepositorio.UsuarioNoEncontrado();
        return CargarRoles(usuario);
    }

    public Usuario ObtenerUsuarioPorCorreo(string correo)
    {
        var usuario = _contexto.Usuarios.FirstOrDefault(u => u.CorreoElectronico == correo)
            ?? throw ExcepcionUsuarioRepositorio.UsuarioNoEncontrado();
        return CargarRoles(usuario);
    }

    public IReadOnlyList<Usuario> ObtenerListaUsuarios()
    {
        return _contexto.Usuarios.ToList()
            .Select(CargarRoles)
            .ToList()
            .AsReadOnly();
    }

    public void ModificarUsuario(int usuarioId, Usuario usuarioModificado)
    {
        var entidad = _contexto.Usuarios.FirstOrDefault(u => u.Id == usuarioId)
            ?? throw ExcepcionUsuarioRepositorio.UsuarioNoEncontrado();

        entidad.Nombre = usuarioModificado.Nombre;
        entidad.Apellido = usuarioModificado.Apellido;
        entidad.CorreoElectronico = usuarioModificado.CorreoElectronico;
        entidad.FechaNacimiento = usuarioModificado.FechaNacimiento;
        // El hash de contraseña no se toca aquí: solo se modifica vía ActualizarContrasenia.
        // (Editar el perfil reconstruye el Usuario desde un DTO sin hash, que dejaría el hash vacío.)
        GuardarRoles(usuarioId, usuarioModificado.Roles);
        _contexto.SaveChanges();
    }

    public void ActualizarContrasenia(int usuarioId, string hashDeContrasena)
    {
        var entidad = _contexto.Usuarios.FirstOrDefault(u => u.Id == usuarioId)
            ?? throw ExcepcionUsuarioRepositorio.UsuarioNoEncontrado();

        _contexto.Entry(entidad).Property(nameof(Usuario.HashDeContrasena)).CurrentValue = hashDeContrasena;
        _contexto.SaveChanges();
    }

    public void EliminarUsuario(int idUsuario)
    {
        var usuario = _contexto.Usuarios.FirstOrDefault(u => u.Id == idUsuario)
            ?? throw ExcepcionUsuarioRepositorio.UsuarioNoEncontrado();

        var roles = _contexto.UsuarioRoles.Where(r => r.UsuarioId == idUsuario).ToList();
        _contexto.UsuarioRoles.RemoveRange(roles);
        _contexto.Usuarios.Remove(usuario);
        _contexto.SaveChanges();
    }

    private Usuario CargarRoles(Usuario usuario)
    {
        var rolesEnDb = _contexto.UsuarioRoles
            .Where(r => r.UsuarioId == usuario.Id)
            .Select(r => r.Rol)
            .ToList();

        foreach (var rol in rolesEnDb)
        {
            if (Enum.TryParse<RolDeUsuario>(rol, out var rolEnum) && !usuario.Roles.Contains(rolEnum))
                usuario.AgregarRol(rolEnum);
        }

        return usuario;
    }

    private void GuardarRoles(Usuario usuario)
    {
        GuardarRoles(usuario.Id, usuario.Roles);
    }

    private void GuardarRoles(int usuarioId, IReadOnlyCollection<RolDeUsuario> roles)
    {
        var existentes = _contexto.UsuarioRoles.Where(r => r.UsuarioId == usuarioId).ToList();
        _contexto.UsuarioRoles.RemoveRange(existentes);

        foreach (var rol in roles)
        {
            _contexto.UsuarioRoles.Add(new UsuarioRolRecord
            {
                UsuarioId = usuarioId,
                Rol = rol.ToString()
            });
        }
    }
}
