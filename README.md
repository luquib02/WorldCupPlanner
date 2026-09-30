# WorldCupPlanner 2026

Aplicación web para gestionar la fase de grupos y los cruces eliminatorios del
Mundial de Fútbol 2026 (48 equipos, 12 grupos de 4). Permite administrar usuarios,
equipos, estadios y partidos; generar el fixture de la fase de grupos con sorteos
deterministas; simular resultados con motores de simulación intercambiables;
recalcular el ranking de los equipos tras cada partido; notificar a
los periodistas cuando se carga un resultado; exportar el fixture y el log de
auditoría en CSV/XLSX; y mantener trazabilidad completa de las acciones del sistema.

Proyecto académico — Diseño de Aplicaciones I, ORT Uruguay 2026 (Obligatorio 1 y 2).

---

## Tecnologías utilizadas

| Tecnología | Versión | Rol |
|-----------|---------|-----|
| C# / .NET | 8.0 | Lenguaje y framework base |
| Blazor Server | net8.0 | Interfaz de usuario (componentes Razor interactivos) |
| Entity Framework Core | 8.0.10 (SqlServer, InMemory, Design) | ORM — persistencia Code First |
| Azure SQL Edge / SQL Server | contenedor Docker | Base de datos relacional |
| ClosedXML | 0.105.0 | Generación de archivos XLSX (exportación de fixture y logs) |
| BCrypt.Net-Next | 4.1.0 | Hash de contraseñas de usuario |
| MSTest (TestFramework/TestAdapter) | 4.2.2 | Framework de pruebas unitarias |
| Microsoft.NET.Test.Sdk | 18.5.1 | SDK de ejecución de tests |
| Moq | 4.20.72 | Mocking de interfaces en tests de servicios |
| JetBrains.Annotations | 2025.2.4 | Anotaciones `[TestSubject]` para navegación en Rider |

---

## Estructura de la solución

```
WorldCupPlanner/
├── Dominio/                          # Entidades y reglas de negocio puras
│   ├── Excepciones/                  # DominioException y especializaciones
│   ├── Helpers/                      # FisherYates, Normalizador
│   ├── Simulacion/                   # IMotorSimulacion, MotorProbabilistico, MotorAleatorioPuro
│   ├── Clasificacion/                # ClasificadorFifa (tabla de posiciones)
│   ├── Equipo.cs                     # Con BanderaBase64
│   ├── Partido.cs                    # Con colección de Incidencias (OCP)
│   ├── Incidencia.cs                 # Clase abstracta base (TPH en EF)
│   ├── TarjetaAmarilla.cs / TarjetaRoja.cs
│   ├── CalculadoraRankingElo.cs      # Fabricación Pura — fórmula ELO
│   ├── Notificacion.cs
│   ├── Fixture.cs / Grupo.cs / Jornada.cs / Bombo.cs
│   ├── Usuario.cs / RolDeUsuario.cs
│   └── LogEntry.cs
│
├── Dominio.Tests/                    # Tests unitarios del dominio (MSTest)
│
├── Repositorio/                      # Persistencia EF Core (Code First)
│   ├── DataAccess/
│   │   ├── Configurations/           # IEntityTypeConfiguration por entidad
│   │   ├── WorldCupPlannerDbContext.cs
│   │   ├── DesignTimeDbContextFactory.cs   # Contexto para `dotnet ef` (CLI)
│   │   └── InMemoryDbContextFactory.cs     # Contexto In-Memory para tests
│   ├── Migrations/                   # Migraciones Code First
│   ├── I[Entidad]Repositorio.cs      # Interfaces de repositorios
│   └── [Entidad]Repositorio.cs       # Implementaciones EF
│
├── Repositorio.Tests/                # Tests de repositorios (EF InMemory)
│
├── Sistema/                          # Servicios de aplicación (casos de uso)
│   ├── DTOs/                         # Objetos de transferencia hacia la UI
│   ├── Mappers/                      # Entidad ↔ DTO
│   ├── Interfaces/                   # IExportacionServicio, IServicioUsuario, etc.
│   ├── Servicios/                    # ServicioRoles
│   ├── Simulacion/                   # MotorSimulacionFactory
│   ├── Importacion/                  # IImportacionServicio + implementaciones
│   ├── EquipoServicio.cs / EstadioServicio.cs / PartidoServicio.cs
│   ├── FixtureServicio.cs / CruceServicio.cs / SimulacionServicio.cs
│   ├── NotificacionServicio.cs / ExportacionServicio.cs / LogServicio.cs
│   └── ServicioUsuario.cs / ServicioSesion.cs
│
├── Sistema.Tests/                    # Tests de servicios (con Moq)
│
├── Ejecutable/                       # Blazor Server App
│   ├── Components/Pages/             # Login, Equipos, Estadios, Fixture Mundial, Cruces,
│   │                                  # Notificaciones, Exportacion, Usuarios, Log, Importar
│   ├── Components/Layout/            # NavMenu y layout principal
│   ├── appsettings.json              # Connection string
│   └── Program.cs                    # Registro de dependencias (DI) y seed inicial
│
├── reportes/                         # Reportes de diseño por feature (no se pushean)
│
└── .claude/                          # Configuración del agente de IA
    ├── CLAUDE.md                     # Instrucciones principales del agente
    └── skills/
        ├── grasp-solid/SKILL.md
        ├── csharp-tdd/SKILL.md
        ├── csharp-ef-migrations/SKILL.md
        ├── create-readme/SKILL.md
        └── documentacion/SKILL.md
```

---

## Instrucciones de ejecución

### Prerrequisitos

- .NET 8 SDK
- Docker Desktop corriendo (para la base de datos)
- Rider o Visual Studio con soporte .NET 8

### 1. Levantar la base de datos

El proyecto espera un contenedor de SQL Server / Azure SQL Edge accesible en
`127.0.0.1,1433` con usuario `sa`. Ejemplo de creación del contenedor (ajustar
la contraseña para que coincida con `appsettings.json`):

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Mundial2026!" \
  -p 1433:1433 --name worldcupplanner-db -d mcr.microsoft.com/azure-sql-edge
```

### 2. Connection string

Ya configurado en `Ejecutable/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1,1433;Database=WorldCupPlanner;User Id=sa;Password=Mundial2026!;TrustServerCertificate=True;"
}
```

Si se usa otra contraseña o host, modificar este valor antes de ejecutar.

### 3. Migraciones

**No es necesario ejecutar `dotnet ef database update` manualmente**: `Program.cs`
llama a `dbContext.Database.Migrate()` al iniciar la aplicación, por lo que las
migraciones pendientes se aplican automáticamente contra la base configurada.

Migraciones existentes (`Repositorio/Migrations/`):

| # | Migración |
|---|-----------|
| 1 | `Inicial` |
| 2 | `AgregarEquipoPartidoLog` |
| 3 | `AgregoBanderaAEquipo` |
| 4 | `AgregarNotificaciones` |
| 5 | `AgregoIncidencias` |
| 6 | `AgregarUsuarios` |
| 7 | `AgregarFixtureGrupoJornada` |
| 8 | `EliminarShadowFKsIncidencias` |
| 9 | `AgregarConstraintsYAuditoria` |

### 4. Datos de prueba

> Mediante gestión se entregó dos ZIP con scripts SQL, uno para insertar las tablas vacías, y el otro con datos cargados de equipos, usuarios, logs, para simular un entorno real.

Lo único que se carga automáticamente al iniciar (si no existe ya un
Administrador) son tres usuarios semilla (ver tabla de credenciales abajo).

### 5. Ejecutar la aplicación

```bash
dotnet run --project Ejecutable
# o desde Rider: botón Run
```

La aplicación queda disponible en `http://localhost:5002` (HTTP) o
`https://localhost:7049` (HTTPS), según `launchSettings.json`.

### Usuarios de prueba (seed automático en `Program.cs`)

| Rol | Email | Contraseña |
|-----|-------|------------|
| Administrador del Sistema | admin@sistema.com | Admin123!@ |
| Editor | editor@sistema.com | Edit123!@ |
| Periodista | periodista@sistema.com | Periodista123!@ |

---

## Ejecutar los tests

```bash
dotnet test                    # todos los proyectos de test
dotnet test Dominio.Tests      # solo dominio
dotnet test Sistema.Tests      # solo servicios (con Moq)
dotnet test Repositorio.Tests  # solo repositorios (EF InMemory)
```

Los tests de `Repositorio.Tests` y de servicios que requieren un `DbContext`
usan `InMemoryDbContextFactory` (`Microsoft.EntityFrameworkCore.InMemory`),
nunca el contenedor de Docker.

---

## Principios de diseño aplicados

Documentados en detalle en `.claude/skills/grasp-solid/SKILL.md` y en
`reportes/`. Resumen:

| Principio | Dónde se aplica |
|-----------|----------------|
| GRASP Experto en Información | `Partido.EstaJugado()`, `Equipo.ActualizarRankingActual()`, `Grupo` calcula su tabla de posiciones |
| GRASP Controlador | `PartidoServicio`, `FixtureServicio`, `CruceServicio` orquestan casos de uso sin contener lógica de negocio propia |
| GRASP Fabricación Pura | `CalculadoraRankingElo`, `Normalizador`, `FisherYates`, los repositorios EF |
| GRASP Bajo Acoplamiento | Servicios dependen de interfaces de repositorio (`IPartidoRepositorio`, etc.), nunca de la clase EF concreta |
| GRASP Polimorfismo / Variaciones Protegidas | Jerarquía `Incidencia` → `TarjetaAmarilla`/`TarjetaRoja`; `IMotorSimulacion` → `MotorProbabilistico`/`MotorAleatorioPuro` |
| SOLID SRP | `CalculadoraRankingElo` solo calcula ranking; `LogServicio` solo audita |
| SOLID OCP | Agregar un nuevo tipo de incidencia o un nuevo motor de simulación no requiere modificar `Partido` ni `FixtureServicio` |
| SOLID DIP | `Ejecutable/Program.cs` conecta interfaces con implementaciones EF vía inyección de dependencias |

---

## Uso de IA Generativa

Este proyecto utilizó Claude Code en su desarrollo para el análisis de diseÑo, borrador de documentación, generación de tests y algunos CRUDs del proyecto respetando TDD.

**Skills del repositorio (`.claude/skills/`):**

- `grasp-solid` — guía de decisiones de diseño GRASP y SOLID usada antes de crear interfaces o clases nuevas.
- `csharp-tdd` — ciclo RED-GREEN-REFACTOR con MSTest, principio F.I.R.S.T y una fase previa de propuesta de diseño de la clase antes de escribir el primer test.
- `csharp-ef-migrations` — guía para conectar una entidad a EF Core con validación (`dotnet build`) antes de generar o aplicar migraciones.
- `create-readme` — generación de este mismo README.md.
- `documentacion` — generación de los reportes de justificación de diseño por feature, en `reportes/`.

Todo el código generado con asistencia de IA fue revisado y verificado por el equipo antes de integrarlo.

---

## Integrantes

Luca Bafico, Nahuel Paroldo y Mateo Poppolo
