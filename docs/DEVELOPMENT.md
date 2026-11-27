# Guía de desarrollo

Guía para preparar el entorno y trabajar en el proyecto. Para el panorama general, ver el [README](../README.md); para el estado real y las limitaciones, [SUMMARY.md](../SUMMARY.md).

## Requisitos

- .NET 8 SDK
- Node.js 20+ y npm
- Docker con Docker Compose v2 (PostgreSQL y Redis se ejecutan en contenedores)

## 1. Clonar y configurar

```bash
git clone https://github.com/bit-rec98/city-services-hub.git
cd city-services-hub
git switch development       # el trabajo vive en `development`; `main` solo tiene el commit inicial
cp .env.example .env
```

Completar cada valor de `.env` (ver la tabla de variables en el [README](../README.md#configuración)). Los valores de `.env.example` son marcadores de posición; `.env` está ignorado por git y no debe commitearse. Para generar secretos: `openssl rand -base64 48`.

## 2. Infraestructura

```bash
docker compose up -d postgres redis
```

Al crear el volumen por primera vez, `scripts/init-databases.sh` crea una base y un rol por servicio (`identity_db`/`identity_user`, `medical_appointments_db`/`appointments_user`, `notifications_db`/`notifications_user`) con las contraseñas del `.env`. Si cambia esas contraseñas con el volumen ya creado, hay que recrear el volumen (`docker compose down -v` borra los datos locales).

## 3. Backend

Cada servicio necesita, como mínimo, la clave JWT y su cadena de conexión. Ejemplo para Identity:

```bash
export JwtSettings__SecretKey="<el valor de JWT_SECRET_KEY>"
export ConnectionStrings__IdentityDb="Host=localhost;Port=5432;Database=identity_db;Username=identity_user;Password=<IDENTITY_DB_PASSWORD>"
ASPNETCORE_URLS=http://localhost:5001 dotnet run --project src/Services/Identity/Identity.API
```

| Servicio | Proyecto | Puerto esperado por el gateway | Cadena de conexión |
|---|---|---|---|
| Identity | `src/Services/Identity/Identity.API` | 5001 | `ConnectionStrings__IdentityDb` |
| Medical Appointments | `src/Services/MedicalAppointments/MedicalAppointments.API` | 5002 | `ConnectionStrings__MedicalAppointmentsDb` |
| Notifications | `src/Services/Notifications/Notifications.API` | 5003 | `ConnectionStrings__NotificationsDb` |
| API Gateway | `src/ApiGateway` | 5000 | — (solo `JwtSettings__SecretKey`) |

Si falta `JwtSettings__SecretKey`, o tiene menos de 32 bytes, el servicio no arranca. Swagger está disponible en `/swagger` cuando `ASPNETCORE_ENVIRONMENT=Development`.

### Migraciones de base de datos

El repositorio **no contiene migraciones de EF Core** y los servicios no aplican el esquema al iniciar, por lo que las tablas no existen en una base nueva. Para crear la primera migración de un servicio (ejemplo con Identity; requiere `dotnet-ef`):

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate \
  --project src/Services/Identity/Identity.Infrastructure \
  --startup-project src/Services/Identity/Identity.API
dotnet ef database update \
  --project src/Services/Identity/Identity.Infrastructure \
  --startup-project src/Services/Identity/Identity.API
```

Estos comandos **no se ejecutaron** al escribir esta guía. `Microsoft.EntityFrameworkCore.Design` está referenciado en Identity.Infrastructure y Notifications.Infrastructure, pero **no** en MedicalAppointments.Infrastructure, donde habría que agregarlo antes de crear su migración. Las migraciones deben revisarse y probarse antes de aplicarse en cualquier entorno compartido. Los DbContext son `IdentityDbContext`, `MedicalAppointmentsDbContext` y `NotificationsDbContext`.

## 4. Frontend

```bash
cd src/WebApp
npm ci
npm run dev        # http://localhost:5173
```

La URL del backend se define con `VITE_API_URL` (por defecto `http://localhost:5000/api/v1`, el gateway). Con `npm run build` se compila y se comprueban los tipos. No hay pruebas automatizadas del frontend.

## 5. Docker Compose completo

```bash
docker compose up -d --build
```

Falla de inmediato, indicando la variable, si falta algún valor requerido del `.env`. Puertos: frontend 3000, gateway 5000, Identity 5001, Medical Appointments 5002, PostgreSQL 5432, Redis 6379, RabbitMQ 5672/15672. Limitaciones de este modo (Notifications ausente, gateway apuntando a `localhost`, esquema sin crear): [SUMMARY.md](../SUMMARY.md).

## Pruebas

```bash
dotnet test CityServicesHub.sln
```

| Proyecto | Cubre |
|---|---|
| `Identity.Tests` | Dominio, value objects y handlers de autenticación |
| `MedicalAppointments.Tests` | Dominio y autorización por propietario de los handlers de turnos |
| `Notifications.Tests` | Dominio de notificaciones |
| `Security.Tests` | Contrato de validación JWT, CORS y usuario actual |

Las pruebas son unitarias y no necesitan PostgreSQL ni Redis. No hay pruebas de integración ni de API.

## Calidad y CI

`.github/workflows/ci.yml` se ejecuta en push y pull request a `main` y `development`. Compila con `dotnet build`, ejecuta `dotnet test`, y hace lint y build del frontend. El lint del frontend y `dotnet format` están configurados como no bloqueantes. Para reproducir localmente (`dotnet format` no se ejecutó al escribir esta guía y puede reportar cambios pendientes):

```bash
dotnet build CityServicesHub.sln --configuration Release
dotnet test CityServicesHub.sln --configuration Release
dotnet format CityServicesHub.sln --verify-no-changes
(cd src/WebApp && npm run lint && npm run build)
```

## Convenciones del backend

- Nuevas capacidades siguen el patrón existente: comando o query en `Application`, handler, validador y, si corresponde, controlador delgado en `API`.
- Las reglas de negocio y de acceso van en `Application`/`Domain`, no en los controladores.
- La identidad del usuario se obtiene de `ICurrentUserService` (token validado), nunca de datos del cliente.
- Autenticación JWT y CORS se registran con `AddCityServicesJwtAuthentication` y `AddCityServicesCors` de `BuildingBlocks/Security`; no se definen por servicio.
- Los secretos nunca van en `appsettings*.json` ni en el código.

## Resolución de problemas

| Síntoma | Causa probable |
|---|---|
| El servicio termina al arrancar con `JwtSettings:SecretKey no está configurado` | Falta `JwtSettings__SecretKey` en el entorno |
| `docker compose` informa `required variable ... is missing` | Falta esa variable en `.env` |
| `permission denied for database ...` | Se usa el rol de otro servicio en la cadena de conexión |
| Errores `relation ... does not exist` | No se aplicaron migraciones (ver arriba) |
| El gateway no alcanza a los servicios dentro de Docker | Sus destinos apuntan a `localhost` |
| Puerto en uso | `lsof -ti:5000` (Linux/macOS) y detener el proceso que lo ocupa |

Para detener la infraestructura: `docker compose down`. Con `-v` también se borran los volúmenes (datos locales).
