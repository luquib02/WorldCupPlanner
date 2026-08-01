using Dominio;

namespace Sistema.DTOs;

public class UsuarioDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string CorreoElectronico { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public List<RolDeUsuario> Roles { get; set; } = new();

    public Usuario convertirAUsuario()
    {
        var usuario = new Usuario()
        {
            Nombre = this.Nombre,
            Apellido = this.Apellido,
            CorreoElectronico = this.CorreoElectronico,
            FechaNacimiento = this.FechaNacimiento
        };
        foreach (var rol in this.Roles)
        {
            usuario.AgregarRol(rol);
        }
        return usuario;
    }

    public static UsuarioDTO convertirAUsuarioDTO(Usuario usuario)
    {
        return new UsuarioDTO()
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            CorreoElectronico = usuario.CorreoElectronico,
            FechaNacimiento = usuario.FechaNacimiento,
            Roles = usuario.Roles.ToList()
        };
    }
}