namespace Dominio;

using Excepciones;
using Helpers;
using System.Text.RegularExpressions;
using BCrypt.Net;

public class Usuario
{
    private string _nombre;
    private string _apellido;
    private string _correoElectronico;
    private DateOnly _fechaNacimiento;
    public IReadOnlyCollection<RolDeUsuario> Roles => _roles.ToList();

    private List<RolDeUsuario> _roles = new List<RolDeUsuario>();

    public int Id { get; private set; }

    public Usuario() { }
    

    public void AgregarRol(RolDeUsuario rol)
    {
        if (_roles.Contains(rol))
            throw ExcepcionUsuario.RolDuplicado();
        _roles.Add(rol);
    }

    public void QuitarRol(RolDeUsuario rol)
    {
        if (!_roles.Contains(rol))
            throw ExcepcionUsuario.RolNoExiste();
        _roles.Remove(rol);
    }
    

    public string Nombre
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw ExcepcionUsuario.NombreVacio();
            _nombre = value;
        }
        get => _nombre;
    }

    public string Apellido
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw ExcepcionUsuario.ApellidoVacio();
            _apellido = value;
        }
        get => _apellido;
    }

    public string CorreoElectronico
    {
        set
        {
            if (!EsCorreoValido(value))
                throw ExcepcionUsuario.CorreoInvalido();
            _correoElectronico = value;
        }
        get => _correoElectronico;
    }

    public DateOnly FechaNacimiento
    {
        set
        {
            if (value > DateOnly.FromDateTime(DateTime.Now))
                throw ExcepcionUsuario.FechaNacimientoFutura();
            _fechaNacimiento = value;
        }
        get => _fechaNacimiento;
    }
    

    public string HashDeContrasena { get; private set; } = string.Empty;

    public void EstablecerContrasenia(string contrasenia)
    {
        ValidadorContraseniaUsuario.Validar(contrasenia);
        HashDeContrasena = BCrypt.HashPassword(contrasenia);
    }

    public bool VerificarContrasenia(string contrasenia)
    {
        if (string.IsNullOrWhiteSpace(contrasenia))
            return false;

        return BCrypt.Verify(contrasenia, HashDeContrasena);
    }
    private static bool EsCorreoValido(string correo)
    {
        var patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(correo, patron);
    }

    
}
