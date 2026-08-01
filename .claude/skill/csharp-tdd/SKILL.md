````md
---
name: csharp-test-design
description: >-
  Diseña y recomienda tests unitarios en C# con MSTest para WorldCupPlanner.
  Analiza una clase existente o una clase de test en construcción y propone
  escenarios de prueba siguiendo F.I.R.S.T., patrones reales del proyecto,
  cobertura cercana al 100% y convenciones existentes.
  Prioriza sugerir tests antes que escribir implementaciones completas.
---

# Skill: Diseño y recomendación de tests — MSTest + F.I.R.S.T.

## Objetivo

Ayudar a construir o completar clases de test existentes recomendando escenarios
de prueba útiles, evitando tests redundantes y manteniendo consistencia con el
resto del proyecto.

### Regla principal

NO empezar escribiendo tests automáticamente.

Primero analizar qué comportamientos vale la pena testear.

---

# Entrada esperada

La skill puede recibir cualquiera de estos contextos:

- una clase de producción;
- una clase de test ya comenzada;
- un método puntual;
- un diff o cambio reciente.

Ejemplos:

```text
ayudame a completar EquipoTest
````

```text
qué tests faltan para PartidoServicio
```

```text
revisá esta clase y recomendá cobertura
```

```text
estoy empezando CalculadoraRankingEloTest
```

---

# Paso 1 — Analizar el caso especifico y contexto 

Antes de escribir o sugerir código responder:

## 1. ¿Qué tipo de clase es y que funcioinalidad cumple?

Elegir una:

* Dominio
* Servicio
* Repositorio
* Factory
* Adaptador
* DTO
* Clase EF
* Fabricación Pura

---

## 2. ¿Qué comportamiento observable tiene?

Listar únicamente:

* validaciones;
* cambios de estado;
* cálculos;
* efectos secundarios;
* excepciones;
* persistencia.

No testear implementación interna.

---

# Paso 2 — Recomendar escenarios (SIN generar código todavía)

Para cada método generar esta tabla:

| Método | Prioridad | Escenario        | Tipo              |
| ------ | --------- | ---------------- | ----------------- |
| Crear  | Alta      | Datos válidos    | Happy Path        |
| Crear  | Alta      | Nombre vacío     | Validación        |
| Crear  | Media     | Longitud máxima  | Caso borde        |
| Crear  | Alta      | Persiste cambios | Efecto secundario |

## Prioridades

### Alta

* reglas de negocio;
* validaciones;
* persistencia;
* efectos visibles.

### Media

* límites;
* bordes;
* integración controlada.

### Baja

* getters;
* setters;
* código trivial.

---

# Paso 3 — Validar contra F.I.R.S.T.

Cada test recomendado debe justificar cómo cumple cada principio.

## Fast

Los tests deben ejecutarse rápido.

Reglas:

* nunca usar SQL Server real;
* nunca levantar Docker;
* usar `InMemoryDbContextFactory`;
* usar mocks para dependencias.

Ejemplo del proyecto:

```csharp
var contexto = new InMemoryDbContextFactory()
    .CreateDbContext();
```

---

## Isolated

Cada test debe poder ejecutarse solo.

Reglas:

* un escenario por test;
* sin dependencias entre tests;
* usar `[TestInitialize]`.

---

## Repeatable

Siempre debe dar el mismo resultado.

Evitar:

```csharp
DateTime.Now
Random
Guid.NewGuid()
```

Preferir:

* reloj inyectado;
* valores fijos;
* semilla explícita.

---

## Self-validating

El test debe indicar claramente si pasó.

Siempre:

```csharp
Assert.AreEqual(valorEsperado,
                resultado,
                "Mensaje descriptivo");
```

Nunca dejar asserts ambiguos.

---

## Timely

El test debe acompañar el cambio.

Preguntar:

* ¿este test detectaría una regresión futura?
* ¿el comportamiento quedó documentado?

Eliminar tests que no agreguen valor.

---

# Paso 4 — Buscar patrones existentes del proyecto

Antes de generar código:

1. Buscar tests similares ya existentes.
2. Copiar estructura.
3. Mantener:

* naming;
* Arrange / Act / Assert;
* estilo de mocks;
* setup;
* factories.

## Regla

Preferir consistencia antes que creatividad.

Ejemplo:

Existe:

```text
UsuarioServicioTest
```

Nuevo:

```text
NotificacionServicioTest
```

→ reutilizar estructura.

---

# Paso 5 — Cobertura recomendada

Objetivo:

Cobertura cercana al 100 % del comportamiento observable.

## Dominio

Intentar incluir:

* constructor válido;
* validación;
* transición de estado;
* excepción.

Ejemplo:

```text
Crear_NombreValido_CreaEntidad
```

```text
EstablecerBandera_StringVacio_LanzaExcepcion
```

---

## Servicio

Intentar incluir:

* camino feliz;
* dependencia falla;
* Verify();
* efectos secundarios.

Ejemplo:

```text
CargarResultado_PartidoExistente_ActualizaRanking
```

---

## Repositorio

Intentar incluir:

* guardar;
* recuperar;
* actualizar;
* eliminar.

Usar:

```csharp
InMemoryDbContextFactory
```

No crear contexto manual salvo excepción.

---

# Generación opcional de código

Solo después de aprobar escenarios.

Formato esperado:

## Tests recomendados

* Crear_NombreValido_CreaEntidad
* Crear_NombreVacio_LanzaExcepcion
* Actualizar_CambioValido_Persiste

## Código sugerido

(MSTest)

---

# Naming de tests

Formato:

```text
[Metodo]_[Escenario]_[ResultadoEsperado]
```

Ejemplos:

Happy path:

```text
CargarResultado_GolesValidos_GuardaResultado
```

Validación:

```text
AgregarEquipo_NombreVacio_LanzaExcepcion
```

Caso borde:

```text
CalcularRanking_ResultadoSuperaMaximo_RetornaMaximo
```

Efecto secundario:

```text
CargarResultado_VictoriaLocal_ActualizaRanking
```

---

# Checklist antes de terminar

* [ ] ¿Cada test valida comportamiento observable?
* [ ] ¿Se evitó infraestructura real?
* [ ] ¿Se siguió F.I.R.S.T.?
* [ ] ¿Existe un caso borde?
* [ ] ¿Hay asserts descriptivos?
* [ ] ¿Se reutilizó patrón del proyecto?
* [ ] ¿Se evitó duplicación?
* [ ] ¿La cobertura agrega valor real?

```
```
