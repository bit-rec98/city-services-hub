# Hub de Servicios Ciudadanos

Plataforma web para centralizar servicios municipales/provinciales, inspirada en plataformas como Ciudadano Digital de Córdoba (Argentina). Se construye con microservicios en .NET 8 y un frontend en React.

> **Estado: en desarrollo temprano.** Hoy existen el servicio de identidad, el de turnos médicos y el de notificaciones, el gateway y un frontend básico. Los demás módulos son solo planes. Ver [Estado de implementación](SUMMARY.md) para el detalle y las limitaciones conocidas.

## Funcionalidades

**Implementadas (con limitaciones, ver [SUMMARY.md](SUMMARY.md)):**

- Registro e inicio de sesión de usuarios con JWT (Identity).
- Turnos médicos: crear, consultar, listar por paciente, confirmar y cancelar; el acceso se limita al paciente titular y a administradores (Medical Appointments).
- Notificaciones por usuario: listar, contar no leídas, marcar como leídas (Notifications). El envío por canales externos (email, SMS, push) es un stub.
- API Gateway con YARP: enrutamiento, rate limiting y CORS.
- Frontend: inicio de sesión, registro, panel y página de turnos. Esta última llama a rutas que la API aún no expone (ver [SUMMARY.md](SUMMARY.md)).

**Planificadas (no existe código):** Legislative, Human Resources, Tax Management, Document Management y Citizen Portal. Recuperación de contraseña, refresco de tokens, verificación de 2FA y reprogramación de turnos tampoco están disponibles por API.

## Stack tecnológico

| Capa | Tecnologías |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API, Entity Framework Core 8 (Npgsql), MediatR, FluentValidation, Serilog, BCrypt.Net, YARP, AspNetCoreRateLimit |
| Frontend | React 19, TypeScript, Vite (`rolldown-vite`), Tailwind CSS 4, Zustand, TanStack Query, React Router 7, Axios, React Hook Form + Zod |
| Infraestructura | PostgreSQL, Redis, Docker Compose, Nginx (frontend) |

RabbitMQ está declarado en `docker-compose.yml`, pero **ningún servicio lo usa**: la abstracción de eventos existente apunta a Redis y tampoco está conectada todavía (ver [SUMMARY.md](SUMMARY.md)).

## Arquitectura

Cada microservicio sigue Clean Architecture (`API → Application → Domain`, con `Infrastructure` implementando las interfaces) y CQRS con MediatR. Cada servicio tiene su propia base de datos PostgreSQL.

```
Frontend (React) ──► API Gateway (YARP) ──► Identity API
                                       ├──► Medical Appointments API
                                       └──► Notifications API
                         PostgreSQL (una base por servicio) · Redis (caché)
```

La validación de JWT, la política de CORS y la identidad del usuario actual están centralizadas en `src/BuildingBlocks/Security`. Más detalle en [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (el Dockerfile y CI usan Node 20)
- [Docker](https://www.docker.com/) con Docker Compose v2

## Configuración

Los secretos **no** están en el repositorio. Se leen de variables de entorno.

1. Copiar la plantilla y completar cada valor (el archivo `.env` está en `.gitignore`):

   ```bash
   cp .env.example .env
   ```

   Generar valores fuertes, por ejemplo con `openssl rand -base64 48`.

2. Variables de entorno:

   | Variable | Obligatoria | Descripción |
   |---|---|---|
   | `POSTGRES_ADMIN_USER` | No (por defecto `postgres`) | Cuenta administradora de PostgreSQL, usada solo para inicializar las bases. |
   | `POSTGRES_ADMIN_PASSWORD` | Sí | Contraseña de esa cuenta. |
   | `IDENTITY_DB_PASSWORD` | Sí | Contraseña del rol `identity_user` (dueño de `identity_db`). |
   | `APPOINTMENTS_DB_PASSWORD` | Sí | Contraseña del rol `appointments_user` (dueño de `medical_appointments_db`). |
   | `NOTIFICATIONS_DB_PASSWORD` | Sí | Contraseña del rol `notifications_user` (dueño de `notifications_db`). |
   | `JWT_SECRET_KEY` | Sí | Clave HMAC (HS256) compartida por todos los servicios, de al menos 32 bytes. |
   | `RABBITMQ_USER`, `RABBITMQ_PASSWORD` | Sí (mientras el contenedor siga en el compose) | Credenciales de RabbitMQ. |

   Los servicios se configuran con estas claves de .NET (en Docker Compose las rellena el compose a partir del `.env`):

   | Clave | Servicios | Descripción |
   |---|---|---|
   | `JwtSettings__SecretKey` | Todos | Clave de firma. Sin ella el servicio **no arranca**. |
   | `JwtSettings__Issuer`, `JwtSettings__Audience` | Todos | Ya definidos en `appsettings.json` (`CityServicesHub`). |
   | `ConnectionStrings__IdentityDb` | Identity | Cadena de conexión a PostgreSQL. |
   | `ConnectionStrings__MedicalAppointmentsDb` | Medical Appointments | Cadena de conexión a PostgreSQL. |
   | `ConnectionStrings__NotificationsDb` | Notifications | Cadena de conexión a PostgreSQL. |
   | `ConnectionStrings__Redis` | Identity, Medical Appointments | Host de Redis (por defecto `localhost:6379`). |
   | `Cors__AllowedOrigins__0`, `__1`, … | Identity, Medical Appointments | Orígenes permitidos (por defecto `http://localhost:3000` y `http://localhost:5173`). Sin orígenes no se permite ninguno; `*` se rechaza. |
   | `SendGrid__ApiKey` | Notifications | El sender de email lanza un error si está vacía; el envío real sigue siendo un stub. |

   El frontend usa `VITE_API_URL` (por defecto `http://localhost:5000/api/v1`, el gateway).

## Ejecución

### Con Docker Compose

```bash
docker compose up -d --build
```

| Componente | URL / puerto |
|---|---|
| Frontend (Nginx) | http://localhost:3000 |
| API Gateway | http://localhost:5000 |
| Identity API | http://localhost:5001 (Swagger en `/swagger`) |
| Medical Appointments API | http://localhost:5002 (Swagger en `/swagger`) |
| PostgreSQL / Redis | 5432 / 6379 |
| RabbitMQ (sin uso) | 5672 / 15672 |

> **Limitaciones actuales de este modo (el stack completo no se ha levantado con `docker compose up`):** el compose no incluye el servicio Notifications. Identity y Medical Appointments aplican sus migraciones al arrancar (`Database__MigrateOnStartup=true`), y el compose sobrescribe los destinos del gateway para usar los nombres de servicio. Ver [SUMMARY.md](SUMMARY.md).

### Desarrollo local (sin Docker para los servicios)

1. Levantar la infraestructura (requiere el `.env` del paso anterior):

   ```bash
   docker compose up -d postgres redis
   ```

2. Exportar la configuración del servicio y ejecutarlo. Ejemplo para Identity (los puertos 5001/5002/5003 son los que espera la configuración del gateway):

   ```bash
   export JwtSettings__SecretKey="<el mismo valor de JWT_SECRET_KEY>"
   export ConnectionStrings__IdentityDb="Host=localhost;Port=5432;Database=identity_db;Username=identity_user;Password=<IDENTITY_DB_PASSWORD>"
   export Database__MigrateOnStartup=true   # aplica las migraciones al arrancar (opcional; ver docs/DEVELOPMENT.md)
   ASPNETCORE_URLS=http://localhost:5001 dotnet run --project src/Services/Identity/Identity.API
   ```

   Medical Appointments se ejecuta igual con `ConnectionStrings__MedicalAppointmentsDb` y puerto 5002; Notifications, con `ConnectionStrings__NotificationsDb` y puerto 5003. El gateway (`src/ApiGateway`) usa el puerto 5000 y también requiere `JwtSettings__SecretKey`.

3. Frontend:

   ```bash
   cd src/WebApp
   npm ci
   npm run dev      # http://localhost:5173
   ```

## Pruebas

```bash
dotnet test CityServicesHub.sln          # pruebas del backend (xUnit)
cd src/WebApp && npm run lint && npm run build   # el frontend no tiene pruebas automatizadas
```

CI (`.github/workflows/ci.yml`) compila y prueba el backend, y hace lint y build del frontend en push y pull request a `main` y `development`.

## Estructura del proyecto

```
src/
├── ApiGateway/              # Gateway YARP (el subdirectorio ApiGateway/ApiGateway es una copia Ocelot sin uso)
├── BuildingBlocks/          # Código compartido: Common, EventBus, Logging, Security (+ Security.Tests)
├── Services/
│   ├── Identity/            # Registro, login, JWT
│   ├── MedicalAppointments/ # Turnos médicos y centros de salud
│   └── Notifications/       # Notificaciones de usuario
├── Shared/                  # Librerías duplicadas de BuildingBlocks, fuera de la solución
└── WebApp/                  # Frontend React
docs/                        # Arquitectura, API y guía de desarrollo
scripts/                     # Inicialización de PostgreSQL
```

## Documentación

- [Estado de implementación y limitaciones](SUMMARY.md)
- [Arquitectura](docs/ARCHITECTURE.md)
- [API](docs/API.md)
- [Guía de desarrollo](docs/DEVELOPMENT.md)

## Licencia

No hay un archivo `LICENSE` en el repositorio; la licencia está pendiente de definir.
