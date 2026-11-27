# Documentación de la API

Esta guía describe las rutas implementadas hoy. La fuente de verdad es el código de los controladores y Swagger (disponible solo en `Development`). Lo planificado y no implementado se indica como tal. Estado general: [SUMMARY.md](../SUMMARY.md).

## Información general

| | |
|---|---|
| Versión | `v1`, incluida en la ruta: `/api/v1/...` |
| Contenido | `application/json` |
| Autenticación | JWT Bearer (HS256), emitido por Identity. Cabecera `Authorization: Bearer <token>` |
| Base URL por el gateway | `http://localhost:5000` |
| Base URL directa (Docker Compose) | Identity `http://localhost:5001`, Medical Appointments `http://localhost:5002` |
| Swagger (solo `Development`) | `/swagger` en cada servicio |
| Health check | `GET /health` en cada servicio y en el gateway |

> **Nota sobre el gateway:** sus rutas están definidas, pero sus destinos apuntan a `localhost`, así que solo funcionan cuando los servicios corren fuera de Docker en los puertos 5001 (Identity), 5002 (Medical Appointments) y 5003 (Notifications). Ver [SUMMARY.md](../SUMMARY.md).

### Rutas del gateway

El gateway reenvía sin reescribir la ruta:

| Ruta | Servicio |
|---|---|
| `/api/v1/auth/**` | Identity |
| `/api/v1/users/**` | Identity (hoy no existen endpoints bajo esta ruta) |
| `/api/v1/appointments/**`, `/api/v1/healthcenters/**` | Medical Appointments |
| `/api/v1/notifications/**` | Notifications |

Límites por cliente: 10 solicitudes por segundo y 100 por minuto en general, y 5 por minuto en rutas que terminan en `/auth/login`. Al excederlos responde `429`.

### Token

El token contiene los claims `sub` (id de usuario), `email`, `jti`, `iat` y un claim `role` por cada rol. Se valida emisor (`CityServicesHub`), audiencia (`CityServicesHub`), expiración y firma, y solo se acepta HS256. Expira a los `JwtSettings:ExpirationMinutes` (60 por defecto). Roles disponibles: `Citizen`, `MunicipalEmployee`, `HealthWorker`, `TaxAgent`, `HRManager`, `LegislativeStaff`, `Administrator`, `SuperAdmin`.

### Errores

Las respuestas de error de los controladores tienen la forma `{ "message": "..." }`. En Identity, las excepciones no controladas pasan por un middleware que responde `{ "message": "...", "code": "...", "errors": { "campo": ["..."] } }` (`code` y `errors` pueden faltar). Los errores internos nunca devuelven detalles de la implementación.

| Código | Cuándo |
|---|---|
| 400 | Datos inválidos o regla de negocio incumplida |
| 401 | Falta el token, o es inválido o expiró |
| 403 | Autenticado pero sin permiso |
| 404 | El recurso no existe, **o no pertenece al usuario** (ver Medical Appointments) |
| 409 | Conflicto (email o documento ya registrados, horario ocupado) |
| 429 | Rate limit excedido |
| 500 | Error inesperado |

## Identity — `/api/v1/auth`

### `POST /api/v1/auth/register` (anónimo)

Registra un usuario.

```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan.perez@example.com",
  "documentNumber": "12345678",
  "documentType": 0,
  "password": "<contraseña>",
  "confirmPassword": "<contraseña>"
}
```

- `documentType`: `0` DNI, `1` CUIT, `2` CUIL, `3` Passport, `4` ForeignId.
- `documentNumber`: 7 u 8 dígitos. `email`: formato válido, máximo 256 caracteres. Nombre y apellido: obligatorios, máximo 100.
- `password`: mínimo 8 caracteres, con mayúscula, minúscula, número y carácter especial; `confirmPassword` debe coincidir.
- Respuestas: `201` con el usuario (`UserDto`), `400` datos inválidos, `409` email o documento ya registrados.

### `POST /api/v1/auth/login` (anónimo)

```json
{ "email": "juan.perez@example.com", "password": "<contraseña>", "twoFactorCode": null }
```

- `200`: `{ "user": UserDto, "accessToken": "...", "refreshToken": "...", "expiresAt": "..." }`.
- `200` con `{ "requiresTwoFactor": true, "message": "..." }` si el usuario tiene 2FA habilitado y no envió código. **El código aún no se verifica** (ver SUMMARY.md).
- `401` credenciales inválidas, cuenta bloqueada o inactiva. `400` datos inválidos.

### `GET /api/v1/auth/me` (requiere token)

Devuelve el `UserDto` del usuario autenticado. `401` sin token; `404` si el usuario ya no existe.

### `GET /api/v1/auth/{id}` (rol `Administrator` o `SuperAdmin`)

Devuelve un usuario por id. `403` para otros roles; `404` si no existe.

`UserDto`: `id`, `firstName`, `lastName`, `fullName`, `email`, `documentNumber`, `documentType`, `status`, `emailVerified`, `twoFactorEnabled`, `roles`, `lastLoginAt`, `createdAt`.

**No implementado:** refresco de tokens, logout, recuperación y cambio de contraseña, habilitar 2FA.

## Medical Appointments — `/api/v1/appointments`

Todos los endpoints de turnos requieren token. La identidad del paciente **se toma siempre del token**, nunca del cuerpo ni de la ruta. Un turno solo lo puede consultar o modificar su paciente titular o un usuario `Administrator`/`SuperAdmin`.

### `POST /api/v1/appointments`

```json
{
  "patientName": "Juan Pérez",
  "patientDocumentNumber": "12345678",
  "patientPhoneNumber": null,
  "patientEmail": "juan.perez@example.com",
  "doctorId": "00000000-0000-0000-0000-000000000000",
  "healthCenterId": "00000000-0000-0000-0000-000000000000",
  "specialty": 0,
  "scheduledDate": "2026-12-15T00:00:00",
  "startTime": "10:00:00",
  "durationMinutes": null,
  "attentionType": 0,
  "reason": "Consulta general"
}
```

- `patientId` es opcional y **se ignora**, salvo para `Administrator`/`SuperAdmin`, que pueden crear turnos a nombre de otro paciente.
- `specialty`: entero del enum `MedicalSpecialty` (`0` GeneralMedicine, `1` Pediatrics, `2` Cardiology, …; ver `MedicalEnums.cs`).
- `attentionType`: `0` InPerson, `1` Telemedicine, `2` HomeVisit. `durationMinutes` omitido usa la duración por defecto del médico.
- Respuestas: `201` con el turno (`AppointmentDto`), `400` médico o centro inexistente u otro error de negocio, `409` el médico ya tiene un turno en ese horario.
- **Sin validación de entrada todavía:** los validadores de este servicio no están activos (ver SUMMARY.md).

### `GET /api/v1/appointments/{id}`

`200` con el turno. `404` si no existe **o si pertenece a otro paciente** (no se revela su existencia).

### `GET /api/v1/appointments/patient/{patientId}?upcomingOnly=false`

Turnos del paciente indicado. `200`; `403` si `patientId` no es el del usuario autenticado y este no es administrador. No existe el alias `patient/me`.

### `POST /api/v1/appointments/{id}/confirm`

Confirma un turno en estado `Scheduled`. `200` `{ "message": "..." }`; `400` si el estado no lo permite; `404` si no existe o es ajeno.

### `POST /api/v1/appointments/{id}/cancel`

Cuerpo: `{ "reason": "Motivo de la cancelación" }`. `200` `{ "message": "..." }`; `400` si el turno no puede cancelarse; `404` si no existe o es ajeno.

`AppointmentDto`: `id`, `patientId`, `patientName`, `patientDocumentNumber`, `patientPhoneNumber`, `patientEmail`, `doctorId`, `doctorName`, `specialty`, `healthCenterId`, `healthCenterName`, `scheduledDate`, `startTime` (`HH:mm`), `endTime`, `durationMinutes`, `status`, `attentionType`, `reason`, `notes`, `cancellationReason`, `createdAt`, `confirmedAt`, `cancelledAt`, `completedAt`. `status` es uno de `Scheduled`, `Confirmed`, `InProgress`, `Completed`, `Cancelled`, `NoShow`, `Rescheduled`.

### `GET /api/v1/healthcenters?city=&specialty=` (anónimo)

Lista centros de salud (`id`, `name`, `code`, `address`, `city`, `province`, `phoneNumber`, `email`, `latitude`, `longitude`, `isActive`, `specialties`).

**No implementado:** reprogramar, completar, marcar ausencia, médicos y horarios disponibles por API.

## Notifications — `/api/v1/notifications`

Todos los endpoints requieren token y operan sobre el usuario autenticado. Este servicio no está en `docker-compose.yml`.

| Método y ruta | Descripción | Respuestas |
|---|---|---|
| `GET /api/v1/notifications?unreadOnly=false&page=1&pageSize=20` | Notificaciones del usuario | `200` |
| `GET /api/v1/notifications/unread-count` | Cantidad de no leídas | `200` |
| `POST /api/v1/notifications/{id}/read` | Marca una como leída | `204`, `403`, `404` |
| `POST /api/v1/notifications/read-all` | Marca todas como leídas | `204` |

El envío por email, SMS y push es un stub; hoy no se envía nada externo.

## Eventos de dominio

Los turnos generan eventos de dominio (`AppointmentCreatedEvent`, `AppointmentConfirmedEvent`, `AppointmentCancelledEvent`, …), pero **hoy no se publican en ningún bus**: `RedisEventBus` no está registrado y ningún flujo activo lo invoca.

## Ejemplo con cURL

```bash
# 1. Iniciar sesión y guardar el token (requiere jq)
TOKEN=$(curl -s -X POST http://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"juan.perez@example.com","password":"<contraseña>"}' | jq -r .accessToken)

# 2. Consultar un turno propio
curl -H "Authorization: Bearer $TOKEN" http://localhost:5002/api/v1/appointments/<id-del-turno>
```
