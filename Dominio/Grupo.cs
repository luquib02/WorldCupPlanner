using Dominio.Clasificacion;
using Dominio.Excepciones;

namespace Dominio;

public class Grupo
{
    public int Id { get; set; }
    public string Etiqueta { get; private set; }
    public List<Equipo> Equipos { get; private set; }
    public List<Jornada> Jornadas { get; private set; }

    protected Grupo()
    {
        Etiqueta = string.Empty;
        Equipos = new List<Equipo>();
        Jornadas = new List<Jornada>();
    }

    public Grupo(string etiqueta)
    {
        ValidarEtiqueta(etiqueta);
        Etiqueta = etiqueta;
        Equipos = new List<Equipo>();
        Jornadas = new List<Jornada>();
    }
    
    private void ValidarEtiqueta(string etiqueta)
    {
        if (etiqueta == null || etiqueta.Length != 1 || etiqueta[0] < 'A' || etiqueta[0] > 'L')
            throw new DominioException("La etiqueta del grupo debe ser una letra entre A y L.");
    }
    public void AgregarEquipo(Equipo equipo)
    {
        ValidarEquipoNoNulo(equipo);
        ValidarEquipoNoDuplicado(equipo);
        ValidarLimiteEquipos();
        Equipos.Add(equipo);
    }

    private void ValidarEquipoNoNulo(Equipo equipo)
    {
        if (equipo == null)
        {
            throw new DominioException("El equipo es obligatorio.");
        }
    }

    private void ValidarEquipoNoDuplicado(Equipo equipo)
    {
        if (Equipos.Contains(equipo))
        {
            throw new DominioException("El equipo ya está en el grupo.");
        }
    }
    private const int _maximoEquipos = 4;
    private void ValidarLimiteEquipos()
    {
        if (Equipos.Count >= _maximoEquipos)
        {
            throw new DominioException("El grupo no puede contener más de 4 equipos.");
        }
    }   
    private const int _maximoJornadas = 3;
    public void AgregarJornada(Jornada jornada)
    {
        ValidarLimiteJornadas();
        Jornadas.Add(jornada);
    }
    private void ValidarLimiteJornadas()
    {
        if (Jornadas.Count >= _maximoJornadas)
            throw new DominioException("El grupo no puede contener más de 3 jornadas.");
    }
    public int ContieneEquiposMismaConfederacion(Confederacion confederacion)
    {
        return Equipos.Count(e => e.Confederacion == confederacion);
    }

    public List<Partido> ObtenerPartidos()
    {
        List<Partido> partidos = new List<Partido>();
        foreach (Jornada jornada in Jornadas)
            partidos.AddRange(jornada.Partidos);
        return partidos;
    }

    public List<PosicionEquipo> ObtenerTablaPosiciones()
    {
        List<PosicionEquipo> tabla = new List<PosicionEquipo>();
        foreach (Equipo equipo in Equipos)
            tabla.Add(new PosicionEquipo(equipo));
        return tabla;
    }

    public List<PosicionEquipo> ObtenerTablaPosiciones(IClasificador clasificador)
    {
        List<PosicionEquipo> tabla = ObtenerTablaPosiciones();
        foreach (Partido partido in ObtenerPartidos())
        {
            if (!partido.EstaJugado())
                continue;
            PosicionEquipo local = tabla.First(p => p.Equipo == partido.EquipoLocal);
            PosicionEquipo visitante = tabla.First(p => p.Equipo == partido.EquipoVisitante);
            local.RegistrarResultado(partido.GolesLocal!.Value, partido.GolesVisitante!.Value);
            visitante.RegistrarResultado(partido.GolesVisitante!.Value, partido.GolesLocal!.Value);
        }
        return clasificador.Ordenar(tabla);
    }
}