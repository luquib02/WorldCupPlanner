---
name: create-readme
description: >-
  Genera el README.md del proyecto WorldCupPlanner. Incluye descripción de la solución,
  tecnologías, estructura de la solución, instrucciones de ejecución y referencias a skills
  y agentes. Cumple con el requisito del Obligatorio 2 sección 3.1.
---

# Skill: Generar README.md — WorldCupPlanner

Al invocar este skill, generar un README.md completo para el proyecto.
Leer los archivos del proyecto antes de generarlo para que la información sea precisa.

---

## Qué leer antes de generar el README

1. `CLAUDE.md` — para entender las convenciones y estructura.
2. `Ejecutable/appsettings.json` — para el connection string y puertos.
3. `Repositorio/Migrations/` — para listar las migraciones existentes.
4. `.claude/skills/` — para listar los skills disponibles.
5. Cualquier archivo `Program.cs` — para el registro de dependencias.

---

## Plantilla del README a generar

```markdown
# WorldCupPlanner 2026

Aplicación web para la gestión del Mundial de Fútbol 2026.
Permite administrar equipos, estadios, fixture, simulación de resultados y seguimiento de rankings.

Proyecto académico — Diseño de Aplicaciones I, ORT Uruguay 2026.

---

## Tecnologías utilizadas

| Tecnología | Versión | Rol |
|-----------|---------|-----|
| C# / .NET | 8.0 | Lenguaje y framework base |
| Blazor Server | 8.0 | Interfaz de usuario |
| Entity Framework Core | 8.x | ORM — persistencia Code First |
| Azure SQL Edge | latest | Base de datos (contenedor Docker) |
| Docker | - | Contenedor para la base de datos |
| MSTest | 3.x | Framework de pruebas unitarias |
| Moq | 4.x | Mocking en tests de servicios |

---

## Estructura de la solución

```
WorldCupPlanner/
├── Dominio/                        # Entidades y lógica de negocio pura
│   ├── Excepciones/                # DominioException, ExcepcionUsuario
│   ├── Helpers/                    # Normalizador, FisherYates
│   ├── Equipo.cs                   # Entidad equipo (con BanderaBase64)
│   ├── Partido.cs                  # Entidad partido (con Incidencias OCP)
│   ├── Incidencia.cs               # Clase base abstracta (OCP)
│   ├── TarjetaAmarilla.cs          # Incidencia concreta
│   ├── TarjetaRoja.cs              # Incidencia concreta
│   ├── CalculadoraRankingElo.cs    # Fabricación Pura — fórmula ELO
│   └── ...
│
├── Dominio.Tests/                  # Tests unitarios del dominio
│
├── Repositorio/                    # Persistencia EF Core
│   ├── DataAccess/
│   │   ├── Configurations/         # Mapeos Fluent API por entidad
│   │   ├── Migrations/             # Migraciones Code First
│   │   ├── WorldCupPlannerDbContext.cs
│   │   ├── DesignTimeDbContextFactory.cs
│   │   └── InMemoryDbContextFactory.cs
│   ├── IEquipoRepositorio.cs       # Interfaces de repositorios
│   ├── EquipoRepositorio.cs        # Implementaciones concretas
│   └── ...
│
├── Repositorio.Tests/              # Tests de repositorios (EF In-Memory)
│
├── Sistema/                        # Servicios de aplicación (casos de uso)
│   ├── DTOs/                       # Objetos de transferencia de datos
│   ├── Interfaces/                 # Interfaces de servicios (si aplica)
│   ├── EquipoServicio.cs           # Controlador GRASP de caso de uso
│   ├── PartidoServicio.cs          # Hookea CalculadoraRankingElo
│   └── ...
│
├── Sistema.Tests/                  # Tests unitarios de servicios (con Moq)
│
├── Ejecutable/                     # Blazor Server App
│   ├── Components/Pages/           # Páginas Razor
│   ├── appsettings.json            # Connection strings y configuración
│   └── Program.cs                  # Registro de dependencias (DI)
│
└── .claude/                        # Configuración del agente IA
    ├── skills/                     # Skills del proyecto
    │   ├── grasp-solid/SKILL.md
    │   ├── csharp-tdd/SKILL.md
    │   ├── csharp-ef-migrations/SKILL.md
    │   └── create-readme/SKILL.md
    └── CLAUDE.md                   # Instrucciones principales del agente
```

---

## Instrucciones de ejecución

### Prerrequisitos

- .NET 8 SDK instalado
- Docker Desktop corriendo
- Rider (o Visual Studio) con soporte .NET 8

### 1. Levantar la base de datos

```bash
# Iniciar el contenedor de Azure SQL Edge (si no está corriendo)
docker start <nombre-del-contenedor>

# Verificar que está corriendo
docker ps
```

### 2. Configurar el connection string

Editar `Ejecutable/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"
  }
}
```

### 3. Aplicar migraciones

```bash
# Desde la raíz de la solución
dotnet ef database update --project Repositorio --startup-project Ejecutable
```

### 4. Cargar datos de prueba (opcional)

Ejecutar el script SQL de datos de prueba:
```bash
# Conectar a SQL Server y ejecutar el script
# Scripts disponibles en: Database/datos-prueba.sql
```

### 5. Ejecutar la aplicación

```bash
dotnet run --project Ejecutable
# O desde Rider: botón Run / F5
```

La aplicación estará disponible en `https://localhost:5001` (o el puerto configurado).

### Usuario inicial de prueba

| Rol | Email | Contraseña |
|-----|-------|------------|
| Administrador | admin@worldcup.com | Admin@2026! |
| Editor | editor@worldcup.com | Editor@2026! |

---

## Ejecutar los tests

```bash
# Todos los tests
dotnet test

# Solo tests de dominio
dotnet test Dominio.Tests

# Solo tests de servicios
dotnet test Sistema.Tests

# Con reporte de cobertura (requiere dotnet-coverage o JetBrains dotCover desde Rider)
dotnet test --collect:"Code Coverage"
```

---

## Principios de diseño aplicados

Este proyecto aplica los siguientes principios, documentados en `.claude/skills/grasp-solid/SKILL.md`:

| Principio | Dónde se aplica |
|-----------|----------------|
| GRASP Experto | `Partido` calcula si está jugado; `Grupo` calcula tabla de posiciones |
| GRASP Controlador | Servicios actúan como controladores de caso de uso |
| GRASP Fabricación Pura | `CalculadoraRankingElo`, repositorios, `Normalizador` |
| GRASP Bajo Acoplamiento | Servicios dependen de interfaces de repositorios |
| GRASP Polimorfismo | Jerarquía de `Incidencia` (TarjetaAmarilla, TarjetaRoja) |
| SOLID SRP | Cada clase tiene una única responsabilidad |
| SOLID OCP | Nuevas incidencias se agregan sin modificar `Partido` |
| SOLID DIP | Inversión de dependencias mediante interfaces |

---

## Uso de IA Generativa

Este proyecto utiliza Claude Code con el archivo de instrucciones `.claude/CLAUDE.md`.

**Skills utilizados:**
- `grasp-solid` — guía de decisiones de diseño GRASP y SOLID
- `csharp-tdd` — ciclo TDD con MSTest y F.I.R.S.T
- `csharp-ef-migrations` — flujo Code First con EF Core
- `create-readme` — generación de este README

**Herramienta:** Claude Code (Anthropic) — contexto de uso: generación de código siguiendo TDD,
revisión de diseño según GRASP/SOLID, generación de migraciones EF, documentación.

Todo código generado por IA fue revisado y verificado por el equipo.

---

## Integrantes

| Nombre | Número de estudiante |
|--------|---------------------|
| [Nombre 1] | [número] |
| [Nombre 2] | [número] |
| [Nombre 3] | [número] |
```

---

## Instrucciones para el agente al usar este skill

1. Leer los archivos del proyecto listados arriba.
2. Reemplazar los valores de la plantilla con datos reales del proyecto:
   - Versiones exactas de paquetes NuGet (leer los `.csproj`).
   - Puerto real de la aplicación (leer `appsettings.json` o `launchSettings.json`).
   - Lista de migraciones existentes (leer la carpeta `Migrations/`).
   - Usuarios de prueba reales (si existen en los scripts SQL).
3. Guardar el resultado como `README.md` en la raíz de la solución.
4. **No inventar** información que no esté en los archivos del proyecto.
