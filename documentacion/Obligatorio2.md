# WorldCupPlanner 2026 — Obligatorio 2

---

Universidad ORT Uruguay  
Facultad de Ingeniería  
Licenciatura en Sistemas  
Diseño de Aplicaciones 1  
**Obligatorio 2**

Luca Bafico (297560)  
Mateo Poppolo (315354)  
Nahuel Paroldo (345530)

Fecha de entrega: 17 de Junio del 2026  
Repositorio: organización IngSoft-DA1 — WorldCupPlanner (297560_345530_315354)

---

## Índice

1. Descripción general del sistema
   - 1.1 Objetivo
   - 1.2 Estado de la implementación
   - 1.3 Bugs conocidos y decisiones de alcance
2. Diagramas de paquetes
   - 2.1 Diagrama de alto nivel
   - 2.2 Diagrama de paquetes hoja
   - 2.3 Análisis de modificabilidad
   - 2.4 Responsabilidades por paquete
3. Diagramas de clases
   - 3.1 Diagrama de dominio
   - 3.2 Diagrama de clases a nivel de servicio
   - 3.3 Diagramas de secuencia
4. Tabla de responsabilidades
   - 4.1 Clases de dominio
   - 4.2 Clases de servicio
5. Mecanismos generales del sistema
6. Aplicación de TDD y cobertura de pruebas unitarias
7. Dependencias externas
8. Uso de IA Generativa
9. Anexos

---

## 1. Descripción general del sistema

### 1.1 Objetivo

WorldCupPlanner 2026 es una aplicación que permite gestionar el Mundial de Fútbol con el nuevo formato de 48 selecciones y preparar los cruces eliminatorios mediante sorteos reproducibles. El sistema resuelve el problema de organizar un fixture válido respetando reglas de distribución por confederación, rotación de estadios y calendario de jornadas, y de gestionar luego las fases eliminatorias garantizando que no se enfrenten equipos del mismo grupo.

El sistema incluye gestión completa de usuarios con tres roles no excluyentes (Administrador, Editor y Periodista), ranking dinámico de equipos actualizado según la fórmula ELO tras cada resultado, notificaciones automáticas a periodistas cuando se carga un resultado, exportación del fixture y del log de auditoría en formatos CSV y XLSX, y motores de simulación intercambiables (Probabilístico y Aleatorio Puro) seleccionables al generar el fixture. Toda la información se persiste en una base de datos Azure SQL Edge mediante Entity Framework Core Code First.

Para la prueba de la aplicación están disponibles los siguientes usuarios:

| Rol | Email | Contraseña |
|-----|-------|------------|
| Administrador | admin@sistema.com | Admin123!@ |
| Editor | editor@sistema.com | Edit123!@ |
| Periodista | periodista@sistema.com | Perio123!@ |

Al restablecer la contraseña, el sistema la cambia a: `Sistema123!@`

### 1.2 Estado de la implementación

Se implementaron todos los requerimientos funcionales del obligatorio. La persistencia opera sobre Azure SQL Edge en Docker mediante EF Core Code First, con migraciones versionadas en el repositorio. La base de datos incluye datos de prueba completos: 2 administradores, 2 editores, 2 periodistas, 5 estadios, 48 equipos (al menos 4 con bandera en Base64), y un fixture con la fase de grupos simulada.

Funcionalidades implementadas:

- Gestión de usuarios con tres roles no excluyentes (Administrador, Editor, Periodista).
- CRUD de equipos con bandera en formato Base64 y cupos por confederación.
- CRUD de estadios con validaciones.
- Generación automática de equipos hasta completar los 48.
- Generación del fixture de la fase de grupos con motor de simulación seleccionable.
- Ranking dinámico ELO recalculado tras cada resultado manual o simulado.
- Notificaciones automáticas a periodistas al cargar un resultado.
- Generación de cruces eliminatorios (dieciseisavos a final).
- Incidencias de partido: tarjetas amarillas y rojas (diseñadas para extensión futura).
- Exportación del log y del fixture en CSV y XLSX.
- Importación de equipos desde CSV.
- Log de auditoría extendido con trazabilidad de cambios de ranking y simulaciones.

### 1.3 Bugs conocidos y decisiones de alcance

No se detectaron bugs bloqueantes. Decisiones de alcance: el ranking ELO se aplica a equipos inmediatamente al cargar el resultado, sin una fase de revisión intermedia; esto simplifica el flujo y es coherente con la letra que pide recálculo inmediato. Las notificaciones se generan para todos los usuarios con rol Periodista en el momento de aplicar el resultado, sin batching ni colas asíncronas, lo cual es suficiente para el volumen del sistema. El motor de simulación elegido al crear el fixture queda registrado en el log; si se desea cambiar el motor para un fixture existente, debe regenerarse el fixture completo.

---

## 2. Diagramas de paquetes

La aplicación se organiza en cuatro paquetes principales que reflejan la arquitectura por capas: presentación (Blazor), servicios (lógica de aplicación), dominio (entidades y reglas de negocio) y acceso a datos (repositorios EF Core). Esta separación permite que la lógica de negocio evolucione independientemente de la interfaz de usuario y de la capa de persistencia. Se adjuntan los diagramas como imágenes en los Anexos.

### 2.1 Diagrama de alto nivel

[IMAGEN: diagrama-paquetes-alto-nivel.png]

El diagrama de alto nivel muestra los cuatro paquetes principales y sus subpaquetes, junto con la dirección de las dependencias. Todos los paquetes dependen de Dominio, que no depende de nadie. Esto es deseado: las entidades y reglas de negocio no conocen ni a los servicios ni a la base de datos, lo que permite testearlas en aislamiento.

### 2.2 Diagrama de paquetes hoja

[IMAGEN: diagrama-paquetes-hoja.png]

Usando la notación UML de clasificadores anidados, los paquetes hoja del sistema y sus dependencias son los que se muestran en el diagrama adjunto.

### 2.3 Análisis de modificabilidad

La forma de las dependencias favorece la modificabilidad porque siempre apuntan hacia adentro, desde la UI hacia el dominio. La interfaz de usuario depende de los servicios y del dominio, pero nunca al revés: si mañana cambia toda la UI a una API REST, el dominio y los servicios no se enteran.

Los servicios dependen de los repositorios únicamente a través de interfaces (`IFixtureRepositorio`, `IEquipoRepositorio`, `IRepositorioEstadio`, etc.), lo que significa que cuando se cambió de persistencia en memoria a EF Core, los servicios quedaron intactos: solo se reemplazaron las clases concretas de repositorio sin tocar ningún servicio.

La jerarquía de `IMotorSimulacion` protege a `FixtureServicio` y `SimulacionServicio` de los cambios en los motores de simulación. Agregar un motor nuevo (por ejemplo, uno basado en estadísticas históricas) requiere únicamente crear una nueva clase que implemente `IMotorSimulacion` y registrarla en `MotorSimulacionFactory`, sin modificar ningún servicio existente.

### 2.4 Responsabilidades por paquete

| Paquete | Responsabilidad |
|---------|----------------|
| Ejecutable | Aplicación Blazor. Contiene los componentes Razor de las pantallas y la inicialización del contenedor de dependencias (Program.cs). |
| Ejecutable::Components | Componentes Razor concretos: Login, Usuarios, Equipos, Estadios, Fixture Mundial, Cruces, Partidos, Logs, Importar, Exportación, Notificaciones, Perfil. |
| Sistema | Servicios de aplicación: FixtureServicio, CrucesServicio, SimulacionServicio, ServicioUsuario, ServicioSesion, ServicioRoles, EquipoServicio, EstadioServicio, PartidoServicio, LogServicio, NotificacionServicio, ExportacionServicio, ResultadoPartidoServicio. Orquestan la lógica de negocio entre dominio y repositorios. |
| Sistema::Interfaces | Contratos públicos de los servicios con abstracción: IServicioUsuario, IServicioSesion, IServicioRoles, IServicioLog, IExportacionServicio, IImportacionServicio. |
| Sistema::DTOs | Objetos de transferencia de datos entre la UI y la capa de servicios. |
| Sistema::Excepciones | ExcepcionLogServicio, ExcepcionSesion, ExcepcionServicioUsuario. |
| Sistema::Importacion | Contrato IImportacionServicio, ResultadoImportacion e ImportacionServicioLegible. |
| Repositorio | Repositorios EF Core e interfaces correspondientes. Encapsulan el acceso a la base de datos. |
| Repositorio::DataAccess | WorldCupPlannerDbContext, configuraciones Fluent API y migraciones Code First. |
| Dominio | Entidades de negocio: Usuario, Equipo, Estadio, Partido, Fixture, Grupo, Jornada, Bombo, PosicionEquipo, LogEntry, Notificacion, Incidencia, TarjetaAmarilla, TarjetaRoja, CalculadoraRankingElo, ConfiguracionTorneo, enums. |
| Dominio::Helpers | FisherYates, Normalizador, ValidadorContraseniaUsuario. Lógica pura sin estado. |
| Dominio::Excepciones | DominioException y ExcepcionUsuario. |
| Dominio::Simulacion | IMotorSimulacion, MotorProbabilistico, MotorAleatorioPuro, NombresMotores. |
| Dominio::Clasificacion | IClasificador, ClasificadorFifa. Lógica de ordenamiento para cruces. |

---

## 3. Diagramas de clases

### 3.1 Diagrama de dominio

[IMAGEN: DiagramaClasesDominio.png]

El dominio mantiene su estructura del primer obligatorio, enriquecida con nuevas entidades y atributos. `Equipo` incorpora `BanderaBase64` para almacenar la bandera del país y `RankingActual` que ahora se recalcula dinámicamente. `Partido` incorpora las colecciones `IncidenciasLocal` e `IncidenciasVisitante` de tipo `List<Incidencia>`.

`Incidencia` es una clase abstracta con el atributo compartido `MinutoJuego` y la validación de minuto en un método protegido estático. `TarjetaAmarilla` y `TarjetaRoja` heredan de ella sin agregar validaciones adicionales. La decisión de usar clase abstracta en lugar de interfaz responde a que hay estado compartido y una validación común que no debe duplicarse en cada subclase.

`CalculadoraRankingElo` es una clase de dominio sin identidad ni persistencia (Fabricación Pura) que encapsula la fórmula ELO con sus constantes: K=30, multiplicadores por fase y límites de ranking. `Notificacion` es una entidad con identidad, timestamp inmutable, estado de lectura mutable y validaciones propias en el constructor.

Los motores de simulación viven en `Dominio::Simulacion`. `IMotorSimulacion` define el contrato `GenerarMarcador(Equipo, Equipo, Random)`. `MotorProbabilistico` calcula la probabilidad de victoria a partir de la diferencia de ranking actuales. `MotorAleatorioPuro` ignora los rankings y usa solo la semilla aleatoria.

### 3.2 Diagrama de clases a nivel de servicio

[IMAGEN: DiagramaClasesServicio.png]

La capa de servicio orquesta el dominio y la persistencia siguiendo un flujo unidireccional de dependencias. Cada componente Razor recibe los servicios que necesita a través de `@inject` con la interfaz correspondiente; nunca accede a repositorios directamente. Los servicios reciben las interfaces de repositorio por constructor, y las implementaciones concretas se registran en `Program.cs` mediante el contenedor de dependencias de .NET.

El flujo completo de una operación es el siguiente:

```
Pages (Blazor)
    @inject IServicioXxx → ServicioXxxImpl (resuelto por DI en Program.cs)
         ──► IRepositorioXxx (campo privado readonly)
                  → RepositorioXxxImpl (resuelto por DI)
                       → WorldCupPlannerDbContext (EF Core, accede a Azure SQL Edge)
```

Las colaboraciones entre servicios son las siguientes:

```
ServicioUsuario       ──► IRepositorioUsuario
                      ──► IServicioSesion
                      ──► IServicioLog

ServicioSesion        ──► [mantiene asociación con Usuario en memoria de sesión]

ServicioRoles         ──► IServicioSesion
                      ──► IServicioUsuario

LogServicio           ──► IRepositorioLog

EquipoServicio        ──► IEquipoRepositorio
                      ──► LogServicio

EstadioServicio       ──► IRepositorioEstadio
                      ──► LogServicio

FixtureServicio       ──► IFixtureRepositorio
                      ──► IEquipoRepositorio
                      ──► IRepositorioEstadio
                      ──► IPartidoRepositorio
                      ──► LogServicio
                      ──► SimulacionServicio
                      ──► ResultadoPartidoServicio

PartidoServicio       ──► IPartidoRepositorio
                      ──► IRepositorioEstadio
                      ──► LogServicio
                      ──► ResultadoPartidoServicio

CrucesServicio        ──► IFixtureRepositorio
                      ──► IPartidoRepositorio
                      ──► IRepositorioEstadio
                      ──► LogServicio
                      ──► SimulacionServicio
                      ──► ResultadoPartidoServicio

SimulacionServicio    ──► [sin repositorios propios; recibe IMotorSimulacion por parámetro]

ResultadoPartidoServicio ──► IEquipoRepositorio
                         ──► IPartidoRepositorio
                         ──► LogServicio
                         ──► CalculadoraRankingElo  (Fabricación Pura del dominio)
                         ──► NotificacionServicio

NotificacionServicio  ──► IRepositorioNotificacion
                      ──► IRepositorioUsuario
                      ──► IServicioSesion
                      ──► LogServicio

ExportacionServicio   ──► IRepositorioLog
                      ──► IFixtureRepositorio
                      ──► LogServicio

ImportacionServicioLegible ──► IEquipoRepositorio
```

Este diseño aplica el principio DIP (SOLID-D): todos los servicios dependen de abstracciones, no de implementaciones concretas. Esto permitió reemplazar los repositorios en memoria de la primera entrega por los repositorios EF Core sin modificar ningún servicio. La dependencia siempre apunta desde el módulo de más alto nivel (servicios) hacia abstracciones (interfaces de repositorio), y desde las interfaces hacia las implementaciones concretas, que son un detalle de infraestructura registrado en `Program.cs`.

Los servicios actúan como **GRASP Controller** de sus respectivos casos de uso: reciben eventos desde la UI, validan precondiciones y orquestan el dominio y la persistencia. El patrón **Information Expert** se aplica asignando la responsabilidad de cada cálculo a la clase que posee los datos: `ResultadoPartidoServicio` es el experto en aplicar resultados porque tiene acceso tanto al partido como a `CalculadoraRankingElo` y a los repositorios de equipos.

El principio de **Low Coupling** se mantiene porque la dependencia entre capas siempre pasa por una interfaz: ni los servicios conocen las clases concretas de repositorio, ni los repositorios conocen los servicios. El único acoplamiento concreto entre servicios (por ejemplo, `FixtureServicio` usando `SimulacionServicio` directamente en lugar de una interfaz) es un punto documentado de mejora potencial.

### 3.3 Diagramas de secuencia

#### 3.3.1 Simulación de grupo con MotorProbabilístico

Este diagrama muestra el flujo desde que el editor presiona "Simular grupo" hasta que el ranking de ambos equipos queda actualizado y los periodistas reciben su notificación.

[IMAGEN: secuencia-simular-grupo.png]

```
FixtureMundial (Page)
    │
    ├──► FixtureServicio.SimularGrupo(grupoId, config, emailUsuario)
    │         │
    │         ├──► IFixtureRepositorio.ObtenerActual() ──► fixture
    │         │
    │         ├──► MotorSimulacionFactory.ObtenerPorNombre("Probabilistico")
    │         │         └──► MotorProbabilistico (implementa IMotorSimulacion)
    │         │
    │         ├── [por cada jornada del grupo]
    │         │     │
    │         │     └──► SimulacionServicio.SimularJornada(jornada, config, motor)
    │         │               │
    │         │               ├── [por cada partido de la jornada]
    │         │               │     │
    │         │               │     ├──► MotorProbabilistico.GenerarMarcador(local, visitante, random)
    │         │               │     │         └──► (golesLocal, golesVisitante)
    │         │               │     │
    │         │               │     ├──► partido.CargarResultado(golesLocal, golesVisitante)
    │         │               │     │
    │         │               │     └──► ResultadoPartidoServicio.AplicarResultado(partido, email, detalle)
    │         │               │               │
    │         │               │               ├──► IEquipoRepositorio.ObtenerPorId(localId) ──► equipoLocal
    │         │               │               ├──► IEquipoRepositorio.ObtenerPorId(visitanteId) ──► equipoVisitante
    │         │               │               │
    │         │               │               ├──► CalculadoraRankingElo.CalcularNuevoRanking(...)
    │         │               │               │         └──► nuevoRankingLocal, nuevoRankingVisitante
    │         │               │               │
    │         │               │               ├──► equipoLocal.ActualizarRankingActual(nuevoRankingLocal)
    │         │               │               ├──► equipoVisitante.ActualizarRankingActual(nuevoRankingVisitante)
    │         │               │               ├──► IEquipoRepositorio.Actualizar(equipoLocal)
    │         │               │               ├──► IEquipoRepositorio.Actualizar(equipoVisitante)
    │         │               │               │
    │         │               │               ├──► NotificacionServicio.Crear(mensaje, periodistas)
    │         │               │               └──► LogServicio.Registrar("CambioRanking", email, detalle)
    │         │
    │         ├──► IFixtureRepositorio.Guardar(fixture)
    │         └──► LogServicio.Registrar("SimularGrupo", email, detalle)
```

El punto de variación protegida en este flujo es `IMotorSimulacion`: `SimulacionServicio` nunca conoce si el motor es `MotorProbabilistico` o `MotorAleatorioPuro`; solo llama a `GenerarMarcador`. Esto aplica el patrón **Protected Variations** de GRASP y el principio **OCP** de SOLID.

#### 3.3.2 Gestión de Usuarios — Creación de usuario

Este diagrama muestra el flujo de alta de un nuevo usuario desde la pantalla de administración.

[IMAGEN: secuencia-crear-usuario.png]

```
Usuarios (Page)
    │
    ├──► IServicioUsuario.CrearUsuario(dto, contrasenia)
    │         │
    │         ├──► IServicioSesion.TienePermiso(RolDeUsuario.Administrador) ──► true
    │         │
    │         ├──► new Usuario(dto.Nombre, dto.Apellido, dto.CorreoElectronico, dto.FechaNacimiento)
    │         │         ├── ValidarNombre(dto.Nombre)          [ExcepcionUsuario si inválido]
    │         │         ├── ValidarApellido(dto.Apellido)      [ExcepcionUsuario si inválido]
    │         │         └── ValidarEmail(dto.CorreoElectronico) [ExcepcionUsuario si inválido]
    │         │
    │         ├──► usuario.SetearHashContrasenia(BCrypt.HashPassword(contrasenia))
    │         │         └── ValidadorContraseniaUsuario.Validar(contrasenia)
    │         │
    │         ├──► IRepositorioUsuario.CrearUsuario(usuario)
    │         │         └── [persiste en WorldCupPlannerDbContext]
    │         │
    │         └──► IServicioLog.Registrar("CrearUsuario", emailAdmin, detalle)
```

El servicio `ServicioUsuario` actúa como **GRASP Controller**: recibe el evento de la UI, valida que el ejecutor tiene permiso de administrador delegando en `IServicioSesion`, instancia el dominio que valida sus propias invariantes, y coordina la persistencia y la auditoría a través de las abstracciones correspondientes.

---

## 4. Tabla de responsabilidades

La tabla siguiente describe las responsabilidades de las clases de dominio y de servicio. Las clases de UI no se incluyen porque, salvo composición de pantallas, no implementan lógica de negocio.

### 4.1 Clases de dominio

| Clase | Responsabilidad | Justificación |
|-------|----------------|---------------|
| Usuario | Validar y mantener datos personales (nombre, apellido, email, fecha de nacimiento, contraseña, roles). Hashear y verificar contraseña. Agregar y quitar roles. | El usuario es el único que tiene toda la información para validar (formato de email, política de contraseña, rol único). |
| Equipo | Validar nombre (1-60), confederación, ranking FIFA (300-2500). Actualizar ranking actual. Establecer bandera Base64. | Encapsula las reglas del equipo. Garantiza que el ranking actual esté siempre en rango válido. |
| Estadio | Validar nombre (1-80), ciudad (1-60), descripción (0-400), capacidad (≥20000). | Información completa del estadio centralizada y autovalidada. |
| Partido | Cargar resultado, decidir vencedor y perdedor, exponer si está jugado, mantener colecciones de incidencias, referencias a partidos origen para fases eliminatorias. | Las reglas sobre quién gana pertenecen al propio partido. Las incidencias son responsabilidad del partido que las contiene. |
| Incidencia | Clase abstracta. Mantener el minuto de juego y validarlo. Define el contrato para incidencias concretas. | Centraliza la validación del minuto y el estado compartido, evitando duplicación en TarjetaAmarilla y TarjetaRoja. El uso de clase abstracta (en lugar de interfaz) se debe al estado compartido. |
| TarjetaAmarilla | Representar una tarjeta amarilla en un minuto dado. | Subclase concreta de Incidencia. No tiene validaciones adicionales. Diseñada para ser sustituible donde se espera Incidencia (LSP). |
| TarjetaRoja | Representar una tarjeta roja en un minuto dado. | Igual que TarjetaAmarilla; ambas permiten que Partido no sepa el tipo concreto de incidencia (OCP). |
| CalculadoraRankingElo | Calcular el nuevo ranking de un equipo tras un resultado aplicando la fórmula ELO con K=30, multiplicadores por fase y clamping 300-2500. | Fabricación Pura: responsabilidad única que no pertenece a ninguna entidad del dominio. Centraliza constantes y fórmula; testeable directamente sin mocks. |
| Notificacion | Representar una notificación para un periodista con mensaje, partido asociado, timestamp inmutable y estado de lectura mutable. Validar mensaje en el constructor. | Entidad con identidad propia. Es el experto en su propio estado de lectura: MarcarComoLeida() vive en la entidad, no en el servicio. |
| Fixture | Raíz agregada del torneo. Sabe si la fase de grupos está completa, si se pueden generar cruces, expone todos los partidos. Estado de transición unidireccional para CrucesGenerados. | Facilita el requerimiento de que, una vez generados los cruces, no se puedan editar los partidos anteriores. |
| Grupo | Mantener hasta 4 equipos y hasta 3 jornadas. Calcular tabla de posiciones. | La tabla se calcula a partir de los partidos que el propio grupo contiene (Experto en Información). |
| Jornada | Agrupar los 2 partidos de una misma fecha del grupo. | Permite validar fechas por jornada y estructurar el calendario. |
| Bombo | Mantener hasta 12 equipos únicos para el sorteo. Validar capacidad e identidad. | Entidad de soporte para el algoritmo de fixture; encapsula el requerimiento de 4 bombos de 12. |
| PosicionEquipo | Acumular puntos, partidos jugados, ganados, empatados, perdidos, goles a favor y en contra a partir de cada resultado. | La lógica de acumulación pertenece a la propia fila de la tabla (Experto en Información). |
| LogEntry | Representar una entrada inmutable del log con usuario, acción y detalle. Validar en el constructor. | Las entradas de log son inmutables por naturaleza; el constructor garantiza esto. |
| ConfiguracionTorneo | Record con las 4 semillas y parámetros del calendario. | Agrupa los parámetros configurables en un solo tipo inmutable, reduce la cantidad de parámetros en las firmas de métodos. |
| IMotorSimulacion | Contrato para generación de marcadores: `GenerarMarcador(Equipo, Equipo, Random)`. | Punto de variación protegida (GRASP). Permite agregar nuevos motores sin tocar los servicios. |
| MotorProbabilistico | Calcular el marcador de un partido basándose en la diferencia de ranking actual entre los equipos. | Implementa IMotorSimulacion. Encapsula el algoritmo probabilístico con sus constantes. |
| MotorAleatorioPuro | Calcular el marcador ignorando los rankings, basándose solo en la semilla aleatoria. | Implementa IMotorSimulacion. Alternativa para simulaciones sin sesgo por ranking. |
| FisherYates (helper) | Barajar una lista de forma determinística usando System.Random(semilla). | Algoritmo reutilizable sin estado, testeable en aislamiento. |
| Normalizador (helper) | Normalizar strings para comparaciones (minúsculas, sin tildes, no alfanuméricos a espacio). | Lógica pura sin estado; se reutiliza en rotación de estadios e importación. |
| ValidadorContraseniaUsuario (helper) | Validar que una contraseña cumple los requisitos de longitud, mayúsculas, minúsculas, números y caracteres especiales. | Responsabilidad única extraída para que Usuario no acumule lógica de validación de strings. |

### 4.2 Clases de servicio

| Clase | Responsabilidad | Justificación |
|-------|----------------|---------------|
| ServicioUsuario | Iniciar y cerrar sesión. CRUD de usuarios. Asignar y quitar roles. Modificar y resetear contraseñas. | Orquesta repositorio, sesión y log. La verificación de permisos vive acá porque depende del estado de sesión. |
| ServicioSesion | Mantener al usuario actualmente logueado, validar sesión iniciada, cerrar sesión, actualizar datos del usuario en sesión. | Separar el estado de sesión evita que ServicioUsuario sea singleton. La sesión tiene su propio ciclo de vida. |
| ServicioRoles | Exponer qué roles tiene el usuario actual, verificar si es Administrador, Editor o Periodista. Asignar y quitar roles por nombre. | Centraliza la lógica de autorización para que las páginas no accedan directamente al dominio de usuario. |
| LogServicio | Crear LogEntry con timestamp del momento y persistirlo. | Centraliza la generación de timestamps y la persistencia de la auditoría. |
| EstadioServicio | CRUD de estadios validando unicidad de nombre y reglas. Registrar auditoría en cada operación. | Orquesta repositorio y log; las validaciones propias del estadio viven en la entidad. |
| EquipoServicio | CRUD de equipos respetando cupos por confederación y unicidad de nombre. Generar equipos automáticos. Establecer bandera. Registrar auditoría. | Las reglas que cruzan más de un equipo (cupos) viven en el servicio; las reglas individuales viven en la entidad. |
| PartidoServicio | Editar partido (fecha, estadio, resultado) respetando que el fixture no haya generado cruces. Delegar cálculo de ELO a ResultadoPartidoServicio. | Controller GRASP: coordina la edición respetando el estado del fixture. |
| FixtureServicio | Validar precondiciones, ordenar equipos, armar bombos, distribuir en grupos, generar calendario, coordinar simulación con el motor elegido. | Orquestador del algoritmo más complejo. Coordina múltiples repositorios y servicios colaboradores. |
| CrucesServicio | Calcular orden final de grupos, seleccionar mejores terceros, generar las rondas eliminatorias respetando que no se enfrenten equipos del mismo grupo. | Orquesta el armado de eliminatorias respetando reglas de la letra. |
| SimulacionServicio | Simular un partido, una jornada o una fase completa delegando la generación del marcador al motor recibido por parámetro. | Aísla la lógica de orquestación de la simulación, desacoplada del motor concreto. |
| ResultadoPartidoServicio | Aplicar el resultado de un partido: actualizar ranking ELO de ambos equipos, generar notificaciones para periodistas, registrar auditoría. | Controller GRASP del caso de uso "aplicar resultado": concentra las colaboraciones necesarias (CalculadoraRankingElo, NotificacionServicio, repositorios). |
| NotificacionServicio | Crear notificaciones para todos los usuarios Periodistas, marcar como leídas, obtener no leídas y todas del usuario actual. | Encapsula el ciclo de vida de notificaciones, con auditoría de lecturas. |
| ExportacionServicio | Exportar el log de auditoría y el fixture en formatos CSV y XLSX para un rango de fechas dado. Registrar auditoría de descarga. | Fabricación Pura: responsabilidad de exportación separada para no sobrecargar LogServicio ni FixtureServicio. |
| ImportacionServicioLegible | Parsear CSV de equipos, validar y crear los equipos faltantes. Devolver ResultadoImportacion con éxitos y errores. | Implementa IImportacionServicio. Elegida por facilidad de comprensión (nombres explícitos, métodos pequeños). |

---

## 5. Mecanismos generales del sistema

### 5.1 Interacción entre la interfaz de usuario y el dominio

La aplicación es un Blazor Server. Cada componente Razor recibe los servicios que necesita a través de `@inject` con la interfaz correspondiente y llama únicamente a esos servicios: nunca accede a repositorios ni al dominio directamente. Por ejemplo, la página de Equipos inyecta `EquipoServicio` (o su interfaz) y llama a `Agregar`, `Actualizar`, `Eliminar`, `ObtenerTodos`; nunca llama a `IEquipoRepositorio.Agregar` directamente.

Cuando una operación requiere conocer al usuario actual (por ejemplo para auditar quién genera el fixture o quién carga un resultado), el componente no lo pasa como parámetro: el servicio consulta a `IServicioSesion.ObtenerUsuarioActual()`. Esto evita pasar el usuario en cada llamada y mantiene el flujo simple en la UI.

### 5.2 Persistencia con Entity Framework Core

Todos los datos se persisten en Azure SQL Edge corriendo en un contenedor Docker. Se usa EF Core 8 Code First: cada entidad tiene su clase de configuración Fluent API en `Repositorio/DataAccess/Configurations/`, y `WorldCupPlannerDbContext.OnModelCreating` las aplica automáticamente con `ApplyConfigurationsFromAssembly`.

Las jerarquías de herencia (`Incidencia` → `TarjetaAmarilla`, `TarjetaRoja`) se persisten con Table Per Hierarchy (TPH) usando un discriminador de tipo string. Las FK de relaciones entre entidades se gestionan como shadow properties para no exponer enteros de FK en el dominio. Los repositorios reciben el `WorldCupPlannerDbContext` por constructor (DIP) y envuelven las mutaciones en `try/catch(DbUpdateException)` para convertir errores de BD en excepciones de la capa de repositorio.

Para los tests de repositorios se usa `EF Core In-Memory Database` con un nombre de base de datos único por test (Guid), garantizando que cada test empiece con un estado limpio. La base de datos Docker solo se usa en producción.

### 5.3 Manejo de errores y excepciones

La estrategia de excepciones está jerarquizada por capa:

- **DominioException**: raíz del dominio; lanzada por entidades al violar invariantes (capacidad de estadio, ranking fuera de rango, nombre vacío).
- **ExcepcionUsuario**: especialización para errores del usuario (contraseña corta, sin mayúscula, sin carácter especial). Usa el patrón de métodos estáticos factory para centralizar mensajes.
- **ExcepcionSesion, ExcepcionServicioUsuario, ExcepcionLogServicio**: errores de la capa de servicio (SesionNoIniciada, ContraseniaIncorrecta, SinPermiso, AccionVacia).
- **ExcepcionUsuarioRepositorio**: errores de acceso a datos (UsuarioDuplicado, UsuarioNoEncontrado).

Los servicios no envuelven excepciones del dominio en try/catch innecesarios: si la entidad lanza, la excepción sube directamente a la UI, que la captura y muestra el mensaje al usuario.

### 5.4 Uso de polimorfismo

El polimorfismo se aplica en cuatro lugares principales:

- **Interfaces de repositorio**: cada implementación EF Core (`EquipoRepositorio`, `RepositorioEstadio`, etc.) implementa su interfaz correspondiente. Los servicios reciben la interfaz, nunca la clase concreta. Esto permite usar mocks de Moq en los tests de servicio sin levantar la base de datos.
- **Interfaces de servicio**: `IServicioUsuario`, `IServicioSesion`, `IServicioRoles`, `IServicioLog`, `IExportacionServicio`, `IImportacionServicio` permiten inyectar implementaciones distintas o mocks en los tests.
- **IMotorSimulacion**: `FixtureServicio` y `SimulacionServicio` reciben y usan `IMotorSimulacion` sin conocer si el motor es `MotorProbabilistico` o `MotorAleatorioPuro`. La selección del motor es responsabilidad de `MotorSimulacionFactory` basándose en el nombre elegido por el usuario.
- **Incidencia**: `Partido` mantiene `List<Incidencia>` y acepta cualquier subclase vía `AgregarIncidencia(Incidencia incidencia, bool esLocal)`. Agregar `CambioJugador` o `IntervencionVAR` mañana requiere solo crear la nueva clase; `Partido.cs` no se modifica.

### 5.5 GRASP: patrones aplicados

**Controller**: Los servicios actúan como Controladores de caso de uso. Tres ejemplos concretos: (1) `PartidoServicio.CargarResultado` recibe el evento desde la UI, delega la validación del resultado al dominio, delega el cálculo ELO a `ResultadoPartidoServicio` y delega la auditoría a `LogServicio`. (2) `FixtureServicio.GenerarFixture` orquesta repositorios, dominio y log sin ejecutar él mismo ningún algoritmo. (3) `ServicioUsuario.CrearUsuario` verifica permisos en `IServicioSesion`, instancia y valida la entidad, persiste, y registra en el log.

**Information Expert**: La responsabilidad se asigna a la clase que tiene la información necesaria. `Partido` sabe si está jugado (`EstaJugado`). `Grupo` calcula su tabla de posiciones porque contiene los equipos y jornadas. `Notificacion.MarcarComoLeida()` modifica su propio estado: la entidad es experta en sí misma. `CalculadoraRankingElo` es la experta en la fórmula ELO porque concentra todas sus constantes y la lógica de clamping.

**Pure Fabrication**: Tres Fabricaciones Puras en el sistema: `CalculadoraRankingElo` (responsabilidad de cálculo que no corresponde a ninguna entidad), `MotorSimulacionFactory` (fábrica de motores que desacopla la elección del motor de su uso), y los repositorios EF Core (responsabilidad de persistencia que tampoco es del dominio).

**Protected Variations**: `IMotorSimulacion` es el punto de variación protegido para los algoritmos de simulación. `IImportacionServicio` protege a los consumidores del algoritmo de importación. La clase abstracta `Incidencia` protege a `Partido` de nuevos tipos de incidencia que la letra anticipa explícitamente.

**Low Coupling / High Cohesion**: La dependencia siempre apunta hacia la abstracción (interfaz), nunca hacia la implementación. `FixtureServicio` no conoce `EquipoRepositorio` sino `IEquipoRepositorio`. Cada servicio tiene una cohesión alta porque agrupa solo las operaciones de su caso de uso. `ResultadoPartidoServicio` existe precisamente para extraer la responsabilidad de aplicar resultados de `PartidoServicio` y `SimulacionServicio`, manteniendo ambos cohesivos.

### 5.6 SOLID: principios aplicados

**SRP (Single Responsibility Principle)**: Cada clase tiene un único motivo de cambio. `CalculadoraRankingElo` cambia solo si cambia la fórmula ELO. `LogServicio` cambia solo si cambia la estructura del log. `ExportacionServicio` cambia solo si cambia el formato de exportación. `Incidencia` cambia solo si cambia la estructura de una incidencia (el minuto); `TarjetaAmarilla` y `TarjetaRoja` no tienen ningún motivo propio de cambio más allá de identificarse como tipo.

**OCP (Open/Closed Principle)**: `Partido.AgregarIncidencia(Incidencia incidencia, bool esLocal)` acepta cualquier subclase futura de `Incidencia` sin modificar `Partido.cs`. La letra anticipa explícitamente nuevos tipos (cambios, VAR); la jerarquía está diseñada para eso. De la misma forma, agregar un nuevo motor de simulación requiere solo crear la clase que implementa `IMotorSimulacion` y registrarla en `MotorSimulacionFactory`.

**LSP (Liskov Substitution Principle)**: `MotorProbabilistico` y `MotorAleatorioPuro` son sustituibles donde se espera `IMotorSimulacion` sin que `SimulacionServicio` necesite conocer cuál es. `TarjetaAmarilla` y `TarjetaRoja` son sustituibles donde se espera `Incidencia`. Ninguna subclase lanza excepciones no declaradas en el contrato de la clase base ni altera el comportamiento esperado.

**DIP (Dependency Inversion Principle)**: `ServicioUsuario` recibe `IRepositorioUsuario`, `IServicioSesion` e `IServicioLog` por constructor; nunca instancia `RepositorioUsuario` directamente. `FixtureServicio` recibe cuatro interfaces de repositorio más `LogServicio` concreto (punto documentado de mejora). La inyección de las implementaciones concretas ocurre en `Program.cs`, que es el único lugar del sistema que conoce tanto las interfaces como sus implementaciones. Esto permitió que los tests de servicio inyecten mocks de Moq sin tocar ni una línea de código de producción.

---

## 6. Aplicación de TDD y cobertura de pruebas unitarias

Todas las clases (sin contar UI) se construyeron con TDD usando MSTest. La estrategia varía según la capa:

- **Tests de dominio**: las entidades no tienen dependencias externas; sus tests son directos, sin mocks, y cubren todas las reglas del constructor, los métodos públicos y las excepciones esperadas. La cobertura de dominio es cercana al 100%.
- **Tests de servicios**: los servicios se testean con mocks de Moq para sus repositorios e interfaces de servicio colaboradoras. El setup de `[TestInitialize]` crea los mocks y el servicio bajo prueba, garantizando que cada test empiece sin estado previo. Se verifica tanto el comportamiento observable (el ranking fue actualizado, la notificación fue creada) como los efectos en los mocks (el repositorio fue llamado una vez).
- **Tests de repositorios**: se usa `EF Core In-Memory Database Provider` con un nombre único por test. Esto hace los tests F.I.R.S.T (Fast, Independent, Repeatable, Self-Validating, Timely) sin necesidad de la base de datos Docker.

El principio F.I.R.S.T es el criterio de calidad de los tests del proyecto. Un test que falla al cambiar código de producción señala exactamente qué se rompió, reduciendo el tiempo de diagnóstico. Cuando se migró la persistencia de memoria a EF Core, los tests de servicio con mocks garantizaron que ninguna regla de negocio se rompió durante la migración: todos los tests de servicio pasaron sin modificación porque los mocks siguieron funcionando igual.

La cobertura se midió con JetBrains dotCover. El reporte detallado, abierto a nivel de clase, se entrega en el Anexo.

Resumen de cobertura por capa:

- **Dominio**: cercana al 100% en todas las entidades. Las entidades puras son las más fáciles de cubrir porque no tienen dependencias.
- **Dominio::Simulacion**: 100%. Los motores son funciones determinísticas con semilla; los tests cubren todas las ramas.
- **Helpers**: 100%. Funciones puras, los tests cubren todas las ramas.
- **Servicios**: cercana al 100%, con caídas puntuales en ramas de error infrecuentes.
- **Repositorios**: alta cobertura con In-Memory; las ramas de excepción de DbUpdateException se cubren parcialmente.

---

## 7. Dependencias externas

| Nombre | Versión | Uso |
|--------|---------|-----|
| Microsoft.NET.Sdk.Web | 8.0 | Framework base de .NET 8. |
| Blazor Server | 8.0 | Tecnología de interfaz exigida por la letra. |
| Microsoft.EntityFrameworkCore | 8.x | ORM para persistencia Code First. |
| Microsoft.EntityFrameworkCore.SqlServer | 8.x | Proveedor SQL Server para Azure SQL Edge. |
| Microsoft.EntityFrameworkCore.InMemory | 8.x | Base de datos en memoria para tests. |
| Microsoft.EntityFrameworkCore.Tools | 8.x | Herramientas CLI para generar migraciones. |
| MSTest | 3.x | Framework de pruebas unitarias. |
| Moq | 4.x | Mocking de interfaces en tests de servicios. |
| BCrypt.Net-Next | 4.x | Hash de contraseñas con algoritmo BCrypt. |
| ClosedXML | 0.102.x | Generación de archivos XLSX para exportación. |
| JetBrains.Annotations | última | Anotaciones [TestSubject] para navegación en Rider. |

---

## 8. Uso de IA Generativa

El agente utilizado durante el desarrollo del proyecto es Claude Code (Anthropic). El uso incluyó generación de código siguiendo TDD, propuesta de diseño según GRASP/SOLID, implementación de migraciones EF Core y generación de documentación. Todo el código generado fue revisado y ajustado por el equipo antes de integrarse al proyecto.

### 8.1 Skill creada: create-readme

**Ruta en el repositorio**: `.claude/skills/create-readme/SKILL.md`

Este skill fue creado para el Obligatorio 2 como skill nuevo que cumple el requerimiento de generar el README.md del proyecto. Al invocarse con `/create-readme`, el agente lee los archivos del proyecto (CLAUDE.md, appsettings.json, carpeta de migraciones, .csproj de cada proyecto, Program.cs) y genera un README.md con:

- Descripción de la solución.
- Tabla de tecnologías con versiones reales leídas desde los archivos del proyecto.
- Estructura de la solución con los directorios principales y su propósito.
- Instrucciones de ejecución paso a paso (levantar Docker, aplicar migraciones, ejecutar la aplicación).
- Tabla de usuarios de prueba.
- Sección de principios de diseño aplicados.
- Sección de uso de IA y skills disponibles.

El skill instruyó explícitamente al agente a no inventar información que no esté en los archivos del proyecto, lo que garantizó que el README generado refleje el estado real del repositorio. El README.md resultante se ubica en la raíz del repositorio.

### 8.2 Skill: csharp-ef-migrations

**Ruta en el repositorio**: `.claude/skills/csharp-ef-migrations/SKILL.md`

Este skill guía el proceso de persistir una entidad de dominio con EF Core Code First. Se utilizó en el proyecto para implementar la persistencia de `Incidencia` (con la jerarquía TPH de TarjetaAmarilla y TarjetaRoja), `Notificacion` y `UsuarioRolRecord`.

El skill define un flujo de 6 pasos que previene errores comunes al trabajar con migraciones:

1. Verificar que `dotnet build` compila antes de cualquier operación EF.
2. Confirmar que los tests de dominio están en verde.
3. Revisar si ya existe la `Configuration` Fluent API.
4. Agregar el `DbSet` en el contexto.
5. Volver a verificar `dotnet build`.
6. Generar y aplicar la migración con el comando exacto: `dotnet ef migrations add Xxx --project Repositorio --startup-project Ejecutable`.

Al seguir este flujo para `Incidencia`, el skill orientó la decisión de usar Table Per Hierarchy (TPH) con discriminador de string en lugar de Table Per Type (TPT), porque TPH es más eficiente en consultas cuando el número de subtipos es pequeño y la tabla no se fragmenta mucho. El skill referenció `PartidoConfiguration.cs` como ejemplo y el agente replicó el patrón con `IncidenciaConfiguration.cs` incluyendo el discriminador `HasDiscriminator<string>("Tipo")`.

### 8.3 Skill: csharp-tdd

**Ruta en el repositorio**: `.claude/skills/csharp-tdd/SKILL.md`

Este skill define el ciclo RED-GREEN-REFACTOR con el principio F.I.R.S.T para el proyecto. Incluye una **Fase 0** que exige proponer el diseño de la clase antes del primer test, lo que evitó reescribir tests a mitad de implementación.

Dos casos concretos donde la Fase 0 orientó decisiones de diseño:

- Para `Incidencia`: la propuesta analizó si debía ser interfaz o clase abstracta. Al tener estado compartido (`MinutoJuego`, `EsLocal`) y una validación compartida (`ValidarMinuto`), la Fase 0 concluyó que debía ser clase abstracta, no interfaz. Usar una interfaz hubiera forzado a duplicar `ValidarMinuto` en cada subclase, violando DRY.
- Para `CalculadoraRankingElo`: la Fase 0 determinó que no necesita interfaz porque la letra no anticipa variación en la fórmula ELO. Crear `ICalculadoraRankingElo` hubiera sido abstracción especulativa. La clase se testea directamente sin mocks, lo que simplifica el setup de los tests de servicio que la usan.

El skill también define la convención de naming de tests (`[Metodo]_[Escenario]_[ResultadoEsperado]`) y exige un mensaje descriptivo en cada `Assert` para que los fallos sean legibles sin inspección manual.

### 8.4 Archivo de instrucciones CLAUDE.md

**Ruta en el repositorio**: `CLAUDE.md`

El archivo CLAUDE.md define las instrucciones principales que el equipo configuró para el agente. Sus instrucciones principales son:

- **Flujo /solution**: Al recibir un requerimiento como argumento, el agente debe ejecutar un flujo de 6 fases: leer el requerimiento en la letra, analizar dependencias con features de otros compañeros, estudiar el proyecto (máximo 8 archivos relevantes), leer los skills de diseño GRASP/SOLID y TDD, proponer la solución técnica con tablas de archivos a modificar y orden de implementación TDD, y preguntar si guardar la propuesta como archivo en `reportes/`.
- **Restricción crítica**: el skill /solution no modifica ningún archivo de código fuente; solo propone y espera aprobación del desarrollador antes de ejecutar cualquier implementación.
- **Límite de archivos**: la exploración del proyecto está limitada a 8 archivos por ejecución para mantener la velocidad y el foco del análisis.

Este archivo garantiza que el agente siga el proceso de diseño antes que la implementación, alineado con la metodología TDD y GRASP/SOLID del curso.

---

## 9. Anexos

Los diagramas se adjuntan como imágenes independientes en el archivo entregado y están referenciados en las secciones correspondientes del documento.

**Diagrama de clases del dominio** (`DiagramaClasesDominio.puml` / imagen adjunta): muestra las entidades, sus relaciones de herencia y composición, las clases abstractas `Incidencia`, `CalculadoraRankingElo` y la jerarquía de motores de simulación.

**Diagrama de clases del servicio** (`DiagramaClasesServicio.puml` / imagen adjunta): muestra las 5 zonas del sistema — páginas Blazor, interfaces de servicio, implementaciones de servicio con DTOs y excepciones, interfaces de repositorio, e implementaciones de repositorio con el DbContext.

**Diagramas de secuencia**: los diagramas textuales de la Sección 3.3 se complementan con las imágenes adjuntas de los diagramas UML formales.

**Reporte de cobertura**: reporte JetBrains dotCover abierto a nivel de clase, adjunto en el archivo entregado.
