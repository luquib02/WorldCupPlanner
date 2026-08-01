# Agregar Nueva Entidad  y migrar a EF

Guía incremental basada en los patrones reales de `Partido`. Seguí cada paso en orden y validá antes de continuar. Usá `[NombreEntidad]` como placeholder del nombre de tu entidad (ej: `Equipo`, `Estadio`).
Luego de crear la entidad haremos la migracion a EF
No escribas codigo, porpone pasos recomendados a implementar y luego se validara  manualmente antes de implementatr

---

## Paso 1 —  Tests de la entidad (`Dominio.Tests`) - en caso de no existir la entidad

**Archivo:** `Dominio.Tests/[NombreEntidad]Test.cs`  
**Namespace:** `Dominio.Tests`

### Estructura del archivo de test

```csharp
using Dominio.Excepciones;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dominio;

namespace Dominio.Tests;

[TestClass]
[TestSubject(typeof([NombreEntidad]))]
public class [NombreEntidad]Test
{
    // Factory methods privados — crean objetos válidos con valores por defecto
    private [NombreEntidad] Crear[NombreEntidad]Valido(
        string campo = "valorPorDefecto",
        /* parámetros opcionales para cada variante */)
    {
        return new [NombreEntidad]()
        {
            Campo = campo,
            // asignar todos los campos requeridos
        };
    }

    // --- Tests del camino feliz ---
    [TestMethod]
    public void Crear[NombreEntidad]DatosValidosOK()
    {
        [NombreEntidad] entidad = Crear[NombreEntidad]Valido();

        Assert.IsNotNull(entidad.Campo);
        // verificar estado inicial esperado
    }

    // --- Tests de validación de campos obligatorios ---
    [TestMethod]
    public void Crear[NombreEntidad]CampoNuloLanzaExcepcion()
    {
        [NombreEntidad] entidad = new [NombreEntidad]();
        DominioException ex = Assert.Throws<DominioException>(
            () => entidad.Campo = null!);
        Assert.AreEqual("El campo es obligatorio.", ex.Message);
    }

    // --- Tests de reglas de negocio ---
    [TestMethod]
    public void MetodoDominio_CondicionInvalida_LanzaExcepcion()
    {
        [NombreEntidad] entidad = Crear[NombreEntidad]Valido();
        DominioException ex = Assert.Throws<DominioException>(
            () => entidad.MetodoDominio(valorInvalido));
        Assert.AreEqual("Mensaje de error esperado.", ex.Message);
    }

    [TestMethod]
    public void Validar[NombreEntidad]ValidoNoLanzaExcepcion()
    {
        [NombreEntidad] entidad = Crear[NombreEntidad]Valido();
        entidad.Validar();
    }
}
```

### Convenciones de naming de tests

| Escenario | Nombre del test |
|---|---|
| Camino feliz | `Crear[Entidad]DatosValidosOK` |
| Camino feliz variante | `Crear[Entidad][Condicion]OK` |
| Validación que falla | `Crear[Entidad][Campo]NuloLanzaExcepcion` |
| Regla de negocio que falla | `[Metodo][Condicion]LanzaExcepcion` |
| Regla de negocio OK | `[Metodo][Condicion]OK` |

### Reglas del proyecto

- Los factory methods (`CrearXValido(...)`) aceptan parámetros opcionales para cubrir variantes sin duplicar código.
- Se verifica **el mensaje exacto** de la excepción con `Assert.AreEqual(mensaje, ex.Message)`.
- Se usa `Assert.Throws<DominioException>(() => ...)` (no try/catch).
- Los tests son independientes entre sí — no se asume estado compartido.

### Validar antes de continuar

- Todos los tests pasan (`dotnet test`).
- Hay al menos un test por cada setter que valida, por cada método de dominio, y por `Validar()`.


## Paso 2 —Crear la entidad en `Dominio` en caso de que no exista

**Archivo:** `Dominio/[NombreEntidad].cs`  
**Namespace:** `Dominio`

### Estructura base

```csharp
using Dominio.Excepciones;
namespace Dominio;

public class [NombreEntidad]
{
    // Backing fields para propiedades con validación
    private TipoCampo? _campo;

    // Propiedades sin validación: auto-property directa
    public int Id { get; set; }
    public string? CampoOpcional { get; set; }

    // Constructor vacío requerido por EF Core
    public [NombreEntidad]() { }

    // Propiedades con validación: backing field + setter que lanza DominioException
    public TipoCampo Campo
    {
        get => _campo!;
        set
        {
            if (value is null)
                throw new DominioException("El campo es obligatorio.");
            _campo = value;
        }
    }

    // Métodos de dominio (operaciones con invariantes)
    public void Validar()
    {
        if (_campo is null)
            throw new DominioException("El campo es obligatorio.");
        // repetir para cada campo requerido sin setter validado
    }
}
```

### Reglas del proyecto

- Las propiedades que **pueden ser nulas** van como auto-properties (`public string? Nombre { get; set; }`).
- Las propiedades **requeridas o con lógica** usan backing field privado `_nombreCampo` y validan en el setter.
- Todos los errores de dominio lanzan `DominioException` (nunca `ArgumentException`, `ArgumentNullException`, etc.).
- El **constructor vacío es obligatorio** para que EF Core pueda materializar objetos desde la DB.
- `Validar()` centraliza la validación completa del objeto — lo llama el servicio antes de persistir.
- Los enums se definen en archivos separados dentro de `Dominio/` (ej: `FasePartido.cs`).

### Validar antes de continuar

- Compila sin errores (`dotnet build`).
- Las validaciones del setter son correctas en lógica de negocio.
- El constructor vacío existe.

---
---

## Paso 3 — Crear la `Configuration` de EF

**Archivo:** `Repositorio/DataAccess/Configurations/[NombreEntidad]Configuration.cs`  
**Namespace:** `Repositorio.DataAccess.Configurations`

```csharp
using Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorio.DataAccess.Configurations;

public class [NombreEntidad]Configuration : IEntityTypeConfiguration<[NombreEntidad]>
{
    public void Configure(EntityTypeBuilder<[NombreEntidad]> builder)
    {
        builder.ToTable("[NombreEntidadPlural]");   // ej: "Partidos", "Equipos"

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // Propiedades simples
        builder.Property(e => e.CampoRequerido)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.CampoOpcional)
            .HasMaxLength(50)
            .IsRequired(false);

        // Enums se guardan como string
        builder.Property(e => e.PropiedadEnum)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        // Nullable int/decimal
        builder.Property(e => e.ValorNullable).IsRequired(false);

        // Relación a-uno requerida (FK shadow property — sin FK en la entidad)
        builder.HasOne(e => e.OtraEntidad)
            .WithMany()
            .HasForeignKey("OtraEntidadId")
            .OnDelete(DeleteBehavior.Restrict);

        // Relación a-uno opcional
        builder.HasOne(e => e.EntidadOpcional)
            .WithMany()
            .HasForeignKey("EntidadOpcionalId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
```

### Reglas del proyecto

- El nombre de la tabla es el **plural de la entidad** (igual que el `DbSet` en el contexto).
- Las **FK no se declaran como propiedades en la entidad de dominio** — son shadow properties configuradas en `HasForeignKey("NombreFK")`.
- Los enums se mapean con `.HasConversion<string>()` para que queden legibles en la DB.
- `DeleteBehavior.Restrict` para relaciones requeridas; `DeleteBehavior.NoAction` para auto-referencias o casos que SQL Server no permite con CASCADE.
- La clase **no necesita registrarse manualmente** — el `DbContext` usa `ApplyConfigurationsFromAssembly(...)` que la descubre automáticamente.

### Validar antes de continuar

- Compila sin errores.
- Los nombres de tabla y FK son consistentes con las demás entidades.

---

## Paso 4 — Registrar en `DbContext`

**Archivo:** `Repositorio/DataAccess/WorldCupPlannerDbContext.cs`

Agregar **solo** la propiedad `DbSet`:

```csharp
public DbSet<[NombreEntidad]> [NombreEntidadPlural] { get; set; } = null!;
```

Ejemplo de cómo queda el archivo (no tocar nada más):

```csharp
public class WorldCupPlannerDbContext : DbContext
{
    public DbSet<Estadio> Estadios { get; set; } = null!;
    public DbSet<Partido> Partidos { get; set; } = null!;
    public DbSet<Equipo> Equipos { get; set; } = null!;
    public DbSet<[NombreEntidad]> [NombreEntidadPlural] { get; set; } = null!;  // <-- nueva línea
    public DbSet<LogEntry> Logs { get; set; } = null!;
    // ...resto sin cambios
}
```

### Reglas del proyecto

- **No** llamar a `modelBuilder.Entity<[NombreEntidad]>()` en `OnModelCreating` — la configuration se descubre automáticamente vía `ApplyConfigurationsFromAssembly`.
- El nombre del `DbSet` es el **plural de la entidad**, igual que el nombre de la tabla en la Configuration.

### Validar antes de continuar

- Compila sin errores.
- La propiedad `DbSet` aparece en el contexto.

---
--


## Paso 5 — Crear la interfaz y el repositorio concreto

### Interfaz

**Archivo:** `Repositorio/I[NombreEntidad]Repositorio.cs`  
**Namespace:** `Repositorio`

```csharp
using Dominio;

namespace Repositorio;

public interface I[NombreEntidad]Repositorio
{
    void Agregar([NombreEntidad] entidad);
    [NombreEntidad]? ObtenerPorId(int id);
    List<[NombreEntidad]> ObtenerTodos();
    void Actualizar([NombreEntidad] entidad);
    void Eliminar(int id);
    // Métodos de consulta específicos del dominio si aplica:
    // List<[NombreEntidad]> ObtenerPor[Criterio]([TipoCriterio] valor);
}
```

### Repositorio concreto

**Archivo:** `Repositorio/Repositorio[NombreEntidad].cs`  
**Namespace:** `Repositorio`

```csharp
using Dominio;
using Microsoft.EntityFrameworkCore;
using Repositorio.DataAccess;

namespace Repositorio;

public class Repositorio[NombreEntidad] : I[NombreEntidad]Repositorio
{
    private readonly WorldCupPlannerDbContext _contexto;

    public Repositorio[NombreEntidad](WorldCupPlannerDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Agregar([NombreEntidad] entidad)
    {
        try
        {
            _contexto.[NombreEntidadPlural].Add(entidad);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public [NombreEntidad]? ObtenerPorId(int id)
    {
        return _contexto.[NombreEntidadPlural]
            .Include(e => e.RelacionNavegacion)   // Include por cada relación eager
            .FirstOrDefault(e => e.Id == id);
    }

    public List<[NombreEntidad]> ObtenerTodos()
    {
        return _contexto.[NombreEntidadPlural]
            .Include(e => e.RelacionNavegacion)
            .ToList();
    }

    public void Actualizar([NombreEntidad] entidad)
    {
        bool existe = _contexto.[NombreEntidadPlural].Any(e => e.Id == entidad.Id);
        if (!existe)
            throw new InvalidOperationException("La entidad no existe.");
        try
        {
            _contexto.[NombreEntidadPlural].Update(entidad);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }

    public void Eliminar(int id)
    {
        [NombreEntidad]? entidad = _contexto.[NombreEntidadPlural].FirstOrDefault(e => e.Id == id);
        if (entidad is null)
            throw new InvalidOperationException("La entidad no existe.");
        try
        {
            _contexto.[NombreEntidadPlural].Remove(entidad);
            _contexto.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            throw new DbUpdateException(e.Message);
        }
    }
}
```

### Reglas del proyecto

- El repositorio **siempre recibe `WorldCupPlannerDbContext` por constructor** (inyección de dependencias).
- `Actualizar` y `Eliminar` verifican existencia y lanzan `InvalidOperationException` si no existe.
- Todas las mutaciones capturan `DbUpdateException` y la relanzán.
- `ObtenerPorId` devuelve **nullable** (`T?`).
- Se usan `Include(...)` para carga eager de las navegaciones que el servicio necesita.
- **El servicio depende de la interfaz**, nunca del repositorio concreto.

### Validar antes de continuar

- Compila sin errores.
- La interfaz y el repositorio concreto están en `Repositorio/` (no en subcarpetas).

---
## Paso 6 — Tests del servicio (`Sistema.Tests`)

**Archivo:** `Sistema.Tests/[NombreEntidad]ServicioTest.cs`  
**Namespace:** `Sistema.Tests`

```csharp
using Dominio;
using Dominio.Excepciones;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repositorio;
using Sistema;

namespace Sistema.Tests;

[TestClass]
public class [NombreEntidad]ServicioTest
{
    private Mock<I[NombreEntidad]Repositorio> _repoMock;
    private Mock<IRepositorioLog> _logRepoMock;
    private [NombreEntidad]Servicio _servicio;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<I[NombreEntidad]Repositorio>();
        _logRepoMock = new Mock<IRepositorioLog>();
        LogServicio logServicio = new LogServicio(_logRepoMock.Object);
        _servicio = new [NombreEntidad]Servicio(_repoMock.Object, logServicio);
    }

    private [NombreEntidad] Crear[NombreEntidad]()
    {
        return new [NombreEntidad]
        {
            // valores válidos
        };
    }

    // --- Crear ---
    [TestMethod]
    public void Crear[NombreEntidad]ValidoOK()
    {
        [NombreEntidad] entidad = Crear[NombreEntidad]();

        _servicio.Crear(entidad, "admin@test.com");

        _repoMock.Verify(r => r.Agregar(entidad), Times.Once);
    }

    [TestMethod]
    public void Crear[NombreEntidad]RegistraLogOK()
    {
        [NombreEntidad] entidad = Crear[NombreEntidad]();

        _servicio.Crear(entidad, "admin@test.com");

        _logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(
            l => l.Accion == "Alta[NombreEntidad]"
        )), Times.Once);
    }

    // --- Modificar ---
    [TestMethod]
    public void Modificar[Campo]ValidoOK()
    {
        [NombreEntidad] entidad = Crear[NombreEntidad]();
        entidad.Id = 1;
        _repoMock.Setup(r => r.ObtenerPorId(1)).Returns(entidad);

        _servicio.Modificar[Campo](1, nuevoValor, "admin@test.com");

        Assert.AreEqual(nuevoValor, entidad.Campo);
        _repoMock.Verify(r => r.Actualizar(entidad), Times.Once);
    }

    // --- Not found ---
    [TestMethod]
    public void Modificar[NombreEntidad]InexistenteLanzaExcepcion()
    {
        _repoMock.Setup(r => r.ObtenerPorId(99)).Returns(([NombreEntidad]?)null);

        DominioException ex = Assert.Throws<DominioException>(
            () => _servicio.Modificar[Campo](99, nuevoValor, "admin@test.com"));
        Assert.AreEqual("La entidad no existe.", ex.Message);
    }

    // --- Listar ---
    [TestMethod]
    public void ListarTodosOK()
    {
        List<[NombreEntidad]> lista = new List<[NombreEntidad]> { Crear[NombreEntidad](), Crear[NombreEntidad]() };
        _repoMock.Setup(r => r.ObtenerTodos()).Returns(lista);

        Assert.AreEqual(2, _servicio.ListarTodos().Count);
    }
}
```

### Reglas del proyecto

- `[TestInitialize]` crea todos los mocks y el servicio — no se comparte estado entre tests.
- `LogServicio` se instancia **real** con el `_logRepoMock` — no se mockea el servicio de log.
- Se usa `_repoMock.Setup(...).Returns(...)` para stubbear consultas.
- Se usa `_repoMock.Verify(..., Times.Once)` para verificar que el repositorio fue llamado.
- `_logRepoMock.Verify(r => r.Agregar(It.Is<LogEntry>(l => l.Accion == "...")))` para verificar logs.
- Los factory methods privados crean objetos de dominio válidos para los tests.

### Validar antes de continuar

- Todos los tests pasan.
- Hay tests para: crear (verifica repo + log), modificar (camino feliz + not-found), listar.

---
## Paso 7  — Crear el servicio en `Sistema`

**Archivo:** `Sistema/[NombreEntidad]Servicio.cs`  
**Namespace:** `Sistema`

```csharp
using Dominio;
using Dominio.Excepciones;
using Repositorio;

namespace Sistema;

public class [NombreEntidad]Servicio
{
    private readonly I[NombreEntidad]Repositorio _[nombreEntidad]Repositorio;
    private readonly LogServicio _logServicio;

    public [NombreEntidad]Servicio(I[NombreEntidad]Repositorio repositorio, LogServicio logServicio)
    {
        _[nombreEntidad]Repositorio = repositorio;
        _logServicio = logServicio;
    }

    public [NombreEntidad] Crear([NombreEntidad] entidad, string usuarioEmail)
    {
        entidad.Validar();
        _[nombreEntidad]Repositorio.Agregar(entidad);
        _logServicio.Registrar(usuarioEmail, "Alta[NombreEntidad]",
            $"Se creo [NombreEntidad] Id={entidad.Id}.");
        return entidad;
    }

    public void Modificar[Campo](int id, TipoCampo nuevoValor, string usuarioEmail)
    {
        [NombreEntidad] entidad = ValidarExistencia(id);
        entidad.Campo = nuevoValor;
        _[nombreEntidad]Repositorio.Actualizar(entidad);
        _logServicio.Registrar(usuarioEmail, "Modificacion[NombreEntidad]",
            $"Se modifico [campo] de [NombreEntidad] Id={id}.");
    }

    public [NombreEntidad] ObtenerPorId(int id) => ValidarExistencia(id);

    public List<[NombreEntidad]> ListarTodos() => _[nombreEntidad]Repositorio.ObtenerTodos();

    private [NombreEntidad] ValidarExistencia(int id)
    {
        [NombreEntidad]? entidad = _[nombreEntidad]Repositorio.ObtenerPorId(id);
        if (entidad is null)
            throw new DominioException("La entidad no existe.");
        return entidad;
    }
}
```

### Reglas del proyecto

- El servicio **inyecta la interfaz** del repositorio (no el concreto).
- Siempre se llama `entidad.Validar()` antes de `Agregar`.
- Cada mutación llama a `_logServicio.Registrar(email, accion, descripcion)`.
- La acción del log sigue el patrón `"Alta[NombreEntidad]"` / `"Modificacion[NombreEntidad]"`.
- El helper privado `ValidarExistencia(int id)` evita duplicar la lógica de not-found.
- Los errores de dominio (entidad no encontrada, reglas rotas) lanzan `DominioException`.

### Validar antes de continuar

- Compila sin errores.
- El servicio no referencia `WorldCupPlannerDbContext` ni el repositorio concreto directamente.

-

## Paso 8 — Generar y aplicar la migración EF

> Ejecutar desde la raíz de la solución o desde el proyecto `Repositorio`.

```bash
# 1. Generar la migración
dotnet ef migrations add Agregar[NombreEntidad] --project Repositorio --startup-project WebApi

# 2. Revisar el archivo generado en Repositorio/DataAccess/Migrations/
#    Verificar que Up() crea la tabla correcta y Down() la elimina.

# 3. Aplicar la migración a la base de datos
dotnet ef database update --project Repositorio --startup-project WebApi
```

### Qué verificar en el archivo de migración generado

- La tabla se llama `[NombreEntidadPlural]` (ej: `"Partidos"`).
- Todas las columnas requeridas tienen `nullable: false`.
- Las FK están presentes con los nombres correctos (ej: `"EstadioId"`).
- Los enums están como `varchar` / `nvarchar`, no como `int`.
- `Down()` hace `migrationBuilder.DropTable("[NombreEntidadPlural]")`.

### Si la migración falla al aplicar

- Verificar que el modelo compiló correctamente.
- Verificar que no hay conflictos con `DeleteBehavior` (SQL Server restringe múltiples CASCADE paths).
- Revisar que las FK en la configuration coinciden con el modelo EF generado.

### Validar al terminar

- `dotnet ef database update` termina sin errores.
- La tabla existe en la DB con el esquema correcto.
- `dotnet test` sigue en verde (todos los proyectos de test).

---

## Resumen de archivos a crear/modificar

| Archivo | Acción |
|---|---|
| `Dominio/[NombreEntidad].cs` | Crear |
| `Dominio.Tests/[NombreEntidad]Test.cs` | Crear |
| `Repositorio/DataAccess/Configurations/[NombreEntidad]Configuration.cs` | Crear |
| `Repositorio/DataAccess/WorldCupPlannerDbContext.cs` | Modificar (agregar `DbSet`) |
| `Repositorio/I[NombreEntidad]Repositorio.cs` | Crear |
| `Repositorio/Repositorio[NombreEntidad].cs` | Crear |
| `Sistema/[NombreEntidad]Servicio.cs` | Crear |
| `Sistema.Tests/[NombreEntidad]ServicioTest.cs` | Crear |
| `Repositorio/DataAccess/Migrations/` | Generado automáticamente por EF |
