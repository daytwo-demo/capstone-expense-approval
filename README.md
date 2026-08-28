# ExpenseApproval.Api

API para la gestión de solicitudes de gasto interno y sus pasos de aprobación.

## Requisitos

- .NET 10 SDK
- PostgreSQL 16 accesible por red

## Base de datos

Necesita un PostgreSQL local escuchando en `localhost:5432`, base
`expenses`, usuario `expenses`, password `Expenses!2026` (ver la cadena de
conexión en `Program.cs`).

## Ejecutar

```bash
dotnet run
```

La API queda escuchando en `http://+:8080`. El esquema de base de datos se crea automáticamente al arrancar (`EnsureCreated`).

## Endpoints

### Solicitudes de gasto (`/api/expenses`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/expenses` | Lista las solicitudes, ordenadas por fecha de creación |
| GET | `/api/expenses/{id}` | Obtiene una solicitud puntual |
| POST | `/api/expenses` | Crea una solicitud (siempre arranca en `Pending`) |
| PUT | `/api/expenses/{id}` | Reemplaza los campos editables de una solicitud |
| DELETE | `/api/expenses/{id}` | Elimina una solicitud |

Body de ejemplo para `POST /api/expenses`:

```json
{
  "concept": "Pasajes aéreos - visita a cliente",
  "amount": 450.00,
  "requestedBy": "mfernandez"
}
```

Body de ejemplo para `PUT /api/expenses/{id}`:

```json
{
  "concept": "Pasajes aéreos - visita a cliente",
  "amount": 450.00,
  "requestedBy": "mfernandez",
  "status": "Approved"
}
```

### Pasos de aprobación (`/api/expenses/{id}/steps`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/expenses/{id}/steps` | Lista los pasos de aprobación de una solicitud, ordenados por fecha de decisión |
| POST | `/api/expenses/{id}/steps` | Agrega un paso de aprobación a una solicitud |

Body de ejemplo para `POST /api/expenses/{id}/steps`:

```json
{
  "approverName": "jrodriguez",
  "decision": "Approved",
  "comment": "Dentro del presupuesto del área"
}
```

## Configuración hardcodeada (a propósito)

`Program.cs` tiene dos constantes fijas en el código:

- `MaxResultsPerPage`: límite de resultados en `GET /api/expenses`.
- `ExternalApiKey`: crear una solicitud (`POST /api/expenses`) simula
  que la app necesita notificar a un sistema externo, y para eso
  necesita esta clave. Si queda vacía, `POST /api/expenses` responde
  `500` con `"Falta ExternalApiKey: no se puede notificar al sistema
  externo."`.

## Endpoint de carga

`GET /api/carga/{n}` calcula el n-ésimo número de Fibonacci de forma
recursiva, sin memoización, para consumir CPU real de forma controlada.
Útil para pruebas de carga. Devuelve el resultado y el tiempo transcurrido
en milisegundos.

```bash
curl http://localhost:8080/api/carga/35
```

Valores de `n` entre 38 y 42 tardan varios segundos reales en responder.

## Probar rápido

```bash
# Crear una solicitud
curl -X POST http://localhost:8080/api/expenses \
  -H "Content-Type: application/json" \
  -d '{"concept":"Pasajes aéreos - visita a cliente","amount":450.00,"requestedBy":"mfernandez"}'

# Listar solicitudes
curl http://localhost:8080/api/expenses

# Agregar un paso de aprobación
curl -X POST http://localhost:8080/api/expenses/{id}/steps \
  -H "Content-Type: application/json" \
  -d '{"approverName":"jrodriguez","decision":"Approved","comment":"Dentro del presupuesto del área"}'

# Ver los pasos de una solicitud
curl http://localhost:8080/api/expenses/{id}/steps
```
