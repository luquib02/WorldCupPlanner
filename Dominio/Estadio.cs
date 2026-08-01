using Dominio.Excepciones;

namespace Dominio;

public class Estadio
{
    private string _nombre = string.Empty;
    private string _ciudad = string.Empty;
    private string? _descripcion;
    private int _capacidad;

    public int Id { get; set; }

    public string Nombre
    {
        get => _nombre;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 80)
                throw new DominioException("El nombre es obligatorio y debe tener entre 1 y 80 caracteres.");
            _nombre = value;
        }
    }

    public string Ciudad
    {
        get => _ciudad;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 60)
                throw new DominioException("La ciudad es obligatoria y debe tener entre 1 y 60 caracteres.");
            _ciudad = value;
        }
    }

    public string? Descripcion
    {
        get => _descripcion;
        set
        {
            if (value != null && value.Length > 400)
                throw new DominioException("La descripción no puede superar los 400 caracteres.");
            _descripcion = value;
        }
    }

    public int Capacidad
    {
        get => _capacidad;
        set
        {
            if (value < 20000)
                throw new DominioException("La capacidad debe ser mayor o igual a 20000 espectadores.");
            _capacidad = value;
        }
    }
}