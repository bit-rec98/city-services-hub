# Guía de Desarrollo

## Requisitos

- Docker Desktop 20.10+
- .NET 8 SDK
- Node.js 18+
- PostgreSQL 15+ (o usar Docker)
- Redis 7+ (o usar Docker)

## Configuración del Entorno de Desarrollo

### 1. Clonar el Repositorio

```bash
git clone https://github.com/bit-rec98/city-services-hub.git
cd city-services-hub
```

### 2. Variables de Entorno

Cada microservicio puede usar estas variables de entorno:

```bash
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=medical_appointments_db;Username=postgres;Password=postgres123
ConnectionStrings__Redis=localhost:6379
```

### 3. Iniciar con Docker Compose

La forma más fácil de iniciar todos los servicios:

```bash
docker-compose up -d
```

Esto iniciará:
- PostgreSQL (puerto 5432)
- Redis (puerto 6379)
- API Gateway (puerto 5000)
- Medical Appointments Service (puerto 5001)
- Frontend React (puerto 3000)

### 4. Desarrollo Local

#### Backend (.NET)

Para desarrollar un microservicio específico:

```bash
cd src/Services/MedicalAppointments/MedicalAppointments.API
dotnet restore
dotnet run
```

Migrar base de datos:

```bash
dotnet ef migrations add InitialCreate --project ../MedicalAppointments.Infrastructure
dotnet ef database update --project ../MedicalAppointments.Infrastructure
```

#### Frontend (React)

```bash
cd src/Web/ClientApp
npm install
npm run dev
```

El frontend estará disponible en http://localhost:5173 (Vite dev server)

### 5. Acceder a los Servicios

- **Frontend:** http://localhost:3000
- **API Gateway:** http://localhost:5000
  - Swagger: http://localhost:5000/swagger
- **Medical Appointments:** http://localhost:5001
  - Health: http://localhost:5001/health
- **PostgreSQL:** localhost:5432
- **Redis:** localhost:6379

## Estructura de un Microservicio

Cada microservicio sigue Clean Architecture:

```
MedicalAppointments/
├── MedicalAppointments.Domain/       # Entidades, Value Objects
│   └── Entities/
│       ├── Appointment.cs
│       └── Doctor.cs
├── MedicalAppointments.Application/  # Use Cases, CQRS
│   ├── Commands/
│   │   └── CreateAppointment/
│   ├── Queries/
│   │   └── GetAppointment/
│   └── DTOs/
├── MedicalAppointments.Infrastructure/ # Implementaciones
│   ├── Data/
│   │   └── AppointmentsDbContext.cs
│   ├── Repositories/
│   │   └── Repository.cs
│   └── EventBus/
│       └── RedisEventBus.cs
└── MedicalAppointments.API/          # Controllers, Startup
    ├── Controllers/
    │   └── AppointmentsController.cs
    └── Program.cs
```

## Patrones Implementados

### CQRS

**Commands** (escritura):
```csharp
public record CreateAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public string PatientName { get; init; }
    // ...
}
```

**Queries** (lectura):
```csharp
public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDto>>;
```

### Repository Pattern

```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    public async Task<T> GetByIdAsync(Guid id) { ... }
    public async Task<T> AddAsync(T entity) { ... }
    // ...
}
```

### Event Bus

```csharp
// Publicar evento
await _eventBus.PublishAsync(new AppointmentCreatedEvent
{
    AppointmentId = appointment.Id,
    // ...
});

// Suscribirse a evento
await _eventBus.SubscribeAsync<AppointmentCreatedEvent>(async (@event) =>
{
    // Manejar evento
});
```

## Testing

### Backend

```bash
cd src/Services/MedicalAppointments
dotnet test
```

### Frontend

```bash
cd src/Web/ClientApp
npm test
```

## Base de Datos

### Migrations (Entity Framework Core)

Crear migración:
```bash
dotnet ef migrations add MigrationName --project MedicalAppointments.Infrastructure --startup-project MedicalAppointments.API
```

Aplicar migración:
```bash
dotnet ef database update --project MedicalAppointments.Infrastructure --startup-project MedicalAppointments.API
```

### Conectar a PostgreSQL

```bash
docker exec -it city-services-postgres psql -U postgres
\l                                    # Listar bases de datos
\c medical_appointments_db            # Conectar a BD
\dt                                   # Listar tablas
```

### Conectar a Redis

```bash
docker exec -it city-services-redis redis-cli
PING                                  # Test conexión
KEYS *                                # Ver todas las claves
```

## Resolución de Problemas

### Puerto ya en uso

```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5000 | xargs kill -9
```

### Limpiar Docker

```bash
docker-compose down -v           # Detener y eliminar volúmenes
docker system prune -a           # Limpiar todo
```

### Rebuild contenedores

```bash
docker-compose up -d --build
```

## API Endpoints

### Medical Appointments

#### POST /api/appointments
Crear nuevo turno médico

```json
{
  "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "patientName": "Juan Pérez",
  "patientEmail": "juan@example.com",
  "doctorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "appointmentDate": "2026-02-01T10:00:00Z",
  "notes": "Consulta general"
}
```

#### GET /api/appointments/{id}
Obtener turno por ID

## Contribuir

1. Fork del repositorio
2. Crear branch (`git checkout -b feature/nueva-funcionalidad`)
3. Commit cambios (`git commit -am 'Agregar nueva funcionalidad'`)
4. Push al branch (`git push origin feature/nueva-funcionalidad`)
5. Crear Pull Request

## Stack Tecnológico

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- MediatR (CQRS)
- FluentValidation
- Npgsql (PostgreSQL)
- StackExchange.Redis
- Ocelot (API Gateway)

### Frontend
- React 18
- Vite
- React Router v6
- TanStack Query (React Query)
- Zustand
- Tailwind CSS
- Axios

### Infraestructura
- Docker & Docker Compose
- PostgreSQL 15
- Redis 7
- Nginx

## Licencia

MIT
