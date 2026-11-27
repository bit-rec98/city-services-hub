# Estado de implementación

Resumen verificable del estado del proyecto. Cada afirmación se contrastó con el código de la rama `development` (compilación y pruebas del backend, lint y build del frontend). Lo que **no** se pudo verificar está indicado como tal.

## Verificado

| Comprobación | Resultado |
|---|---|
| `dotnet build CityServicesHub.sln` | Correcto (con advertencias de dependencias vulnerables, ver más abajo) |
| `dotnet test CityServicesHub.sln` | Correcto: Identity, Medical Appointments, Notifications y Security |
| `npm run lint` y `npm run build` en `src/WebApp` | Correcto |
| `docker compose config` con un `.env` completo / sin `.env` | Valida / falla indicando la variable faltante |
| Script `scripts/init-databases.sh` en un contenedor `postgres:17-alpine` | Crea un rol y una base por servicio; cada rol solo puede conectarse a su base |
| Arranque de cada servicio sin `JwtSettings__SecretKey` | Falla al iniciar |

**No verificado:** `docker compose up` completo, servicios en ejecución contra PostgreSQL/Redis reales, el flujo de extremo a extremo entre frontend, gateway y servicios, y la autorización a nivel HTTP (solo está cubierta a nivel de handlers).

## Estado por componente

| Componente | Estado | Detalle |
|---|---|---|
| Identity API | Parcial | Registro, login, `GET /auth/me` y `GET /auth/{id}` (administradores). Sin refresco de tokens, logout, recuperación de contraseña ni verificación de 2FA. |
| Medical Appointments API | Parcial | Crear, consultar, listar por paciente, confirmar y cancelar turnos; listar centros de salud. Sin reprogramar, completar, médicos ni horarios disponibles por API. |
| Notifications API | Parcial | Listar, contar no leídas y marcar como leídas. Los senders de email, SMS y push son stubs. No está en `docker-compose.yml`. |
| API Gateway (YARP) | Parcial | Rutas y rate limiting configurados; destinos en `localhost`. |
| Frontend | Parcial | Login, registro, panel y turnos; ver incompatibilidades abajo. |
| Legislative, HR, Tax, Document Management, Citizen Portal | Planificado | No existe código. |

## Limitaciones conocidas

**Bloqueantes para ejecutar el sistema completo**

- No hay migraciones de EF Core en el repositorio ni se llama a `Migrate()`/`EnsureCreated()`: el esquema de las bases no se crea.
- Dentro de Docker, el gateway apunta a `localhost:5001/5002/5003`, que no son los servicios. Debe usar los nombres de servicio del compose.
- El compose no incluye Notifications, y el gateway enruta `/api/v1/notifications` a `localhost:5003`.

**Seguridad**

- Medical Appointments no valida sus entradas: los validadores de FluentValidation de esa API están excluidos de la compilación y no hay un comportamiento de validación registrado.
- `LoginCommandHandler` solo comprueba que se envíe un `TwoFactorCode` cuando el usuario tiene 2FA habilitado; **no verifica el código**. No existe un flujo para habilitar 2FA.
- El frontend guarda `accessToken` y `refreshToken` en `localStorage` (expuestos ante XSS).
- El rate limiting del gateway usa el encabezado `X-Real-IP` enviado por el cliente.
- Dependencias con avisos de seguridad: AutoMapper 12.0.1 (3 avisos de severidad alta) y 6 vulnerabilidades en `npm audit`.
- `GET /api/v1/healthcenters` es público (datos de referencia).
- Los tokens se firman con HS256 y una clave compartida por todos los servicios.

**Frontend y API desalineados**

- `/auth/refresh`, `/auth/forgot-password` y `/auth/reset-password` no existen en la API.
- La página de turnos llama a `/appointments/patient/me`, `/doctors` y `/appointments/available-slots`, que no existen.

**Eventos**

- `RedisEventBus` no está registrado en la inyección de dependencias y ningún flujo activo publica eventos; los eventos de dominio se acumulan en las entidades pero no se despachan.

**Código duplicado o sin uso**

- `src/ApiGateway/ApiGateway/` es una copia con Ocelot fuera de la solución; la que se ejecuta es `src/ApiGateway/` (YARP).
- `src/Shared/` duplica tipos de `BuildingBlocks` y no forma parte de la solución.
- En Medical Appointments hay archivos duplicados excluidos de la compilación (`Commands/CreateAppointment`, `Commands/CancelAppointment`, `Queries/GetAppointment`, `Queries/GetAllAppointments`, `DTOs/AppointmentDto.cs`), dos `DbContext` y dos Dockerfiles.
- `scripts/init-databases.sql` no se usa: el compose monta `init-databases.sh`.

**Otras**

- `TreatWarningsAsErrors` está desactivado; el lint y la comprobación de formato en CI no bloquean el pipeline (`continue-on-error`).
- No hay pruebas automatizadas del frontend ni pruebas de API/integración del backend.
- `AuthResponseDto.ExpiresAt` está fijado a una hora, aunque la expiración del token se configura con `JwtSettings:ExpirationMinutes`.

## Documentación relacionada

- [README](README.md)
- [Arquitectura](docs/ARCHITECTURE.md)
- [API](docs/API.md)
- [Guía de desarrollo](docs/DEVELOPMENT.md)
