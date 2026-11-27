# Estado de implementación

Resumen verificable del estado del proyecto. Cada afirmación se contrastó con el código de la rama `development` (compilación y pruebas del backend, lint y build del frontend). Lo que **no** se pudo verificar está indicado como tal.

## Verificado

| Comprobación | Resultado |
|---|---|
| `dotnet build CityServicesHub.sln` | Correcto (con advertencias de dependencias vulnerables, ver más abajo) |
| `dotnet test CityServicesHub.sln` | Correcto: pruebas unitarias (Identity, Medical Appointments, Notifications y Security) y de integración (Identity y Medical Appointments, con PostgreSQL en Docker) |
| `npm run lint` y `npm run build` en `src/WebApp` | Correcto |
| `docker compose config` con un `.env` completo / sin `.env` | Valida / falla indicando la variable faltante |
| Script `scripts/init-databases.sh` en un contenedor `postgres:17-alpine` | Crea un rol y una base por servicio; cada rol solo puede conectarse a su base |
| Arranque de cada servicio sin `JwtSettings__SecretKey` | Falla al iniciar |
| Appointments API sobre PostgreSQL real (migraciones al arrancar, rol de mínimo privilegio, JWT firmados con la clave de prueba) | 21 pruebas de integración automatizadas: 401 sin token o con firma alterada, validación 400, paciente tomado del token, 404/403 sobre turnos ajenos, acceso de administrador, 409 por horario ocupado, confirmar y cancelar, `patient/me`, listado público de centros |
| Identity API sobre PostgreSQL real (Redis reemplazado por caché en memoria) | 17 pruebas de integración automatizadas: registro (todos los tipos de documento, duplicados 409, contraseña débil), login con tokens y refresh token persistido, respuesta idéntica para contraseña errónea y email inexistente, bloqueo tras 5 intentos, `GET /auth/me`, restricción de `GET /auth/{id}` a administradores, 2FA (se pide el código y nunca se acepta). Con el bug del refresh token reintroducido, 3 de ellas fallan |
| Migraciones | Las tres se aplican en una base vacía; el gateway usa el destino sobrescrito por variable de entorno |

**No verificado:** `docker compose up` completo (no se construyeron las imágenes), el comportamiento con un Redis real (el registro y los códigos de verificación), Notifications en ejecución, y el flujo de extremo a extremo entre frontend, gateway y servicios. Las pruebas de integración se ejecutaron localmente con `postgres:17-alpine`; el compose y la configuración por defecto usan `postgres:15-alpine`, y su ejecución en CI aún no se ha observado.

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

- El compose no incluye Notifications (no tiene Dockerfile), y el gateway enruta `/api/v1/notifications` a `localhost:5003`, que dentro de Docker no es ese servicio.

**Seguridad**

- 2FA no está implementado: no hay verificación de códigos ni flujo para habilitarlo. Por seguridad, `LoginCommandHandler` rechaza siempre el login de un usuario con 2FA habilitado (no acepta ningún código).
- El frontend guarda `accessToken` y `refreshToken` en `localStorage` (expuestos ante XSS).
- El rate limiting del gateway usa el encabezado `X-Real-IP` enviado por el cliente.
- Dependencias con avisos de seguridad: AutoMapper 12.0.1 (3 avisos de severidad alta) y 6 vulnerabilidades en `npm audit`.
- `GET /api/v1/healthcenters` es público (datos de referencia).
- Los tokens se firman con HS256 y una clave compartida por todos los servicios.

**Frontend y API desalineados**

- `/auth/refresh`, `/auth/forgot-password` y `/auth/reset-password` no existen en la API.
- La página de turnos llama a `/doctors` y `/appointments/available-slots`, que no existen.

**Eventos**

- `RedisEventBus` no está registrado en la inyección de dependencias y ningún flujo activo publica eventos; los eventos de dominio se acumulan en las entidades pero no se despachan.

**Código duplicado o sin uso**

- `src/ApiGateway/ApiGateway/` es una copia con Ocelot fuera de la solución; la que se ejecuta es `src/ApiGateway/` (YARP).
- `src/Shared/` duplica tipos de `BuildingBlocks` y no forma parte de la solución.
- En Medical Appointments hay archivos duplicados excluidos de la compilación (`Commands/CreateAppointment`, `Commands/CancelAppointment`, `Queries/GetAppointment`, `Queries/GetAllAppointments`, `DTOs/AppointmentDto.cs`), dos `DbContext` y dos Dockerfiles.
- `scripts/init-databases.sql` no se usa: el compose monta `init-databases.sh`.

**Otras**

- `TreatWarningsAsErrors` está desactivado; el lint y la comprobación de formato en CI no bloquean el pipeline (`continue-on-error`).
- No hay pruebas automatizadas del frontend, ni de integración para Notifications ni para el gateway.
- `AuthResponseDto.ExpiresAt` está fijado a una hora, aunque la expiración del token se configura con `JwtSettings:ExpirationMinutes`.

## Documentación relacionada

- [README](README.md)
- [Arquitectura](docs/ARCHITECTURE.md)
- [API](docs/API.md)
- [Guía de desarrollo](docs/DEVELOPMENT.md)
