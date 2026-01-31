# Arquitectura del Sistema

## Visión General

Hub Ciudadano es un sistema de microservicios diseñado siguiendo principios de Clean Architecture, CQRS y Event-Driven Architecture para proporcionar servicios municipales de alta disponibilidad y escalabilidad.

## Principios Arquitectónicos

### 1. Clean Architecture

Cada microservicio sigue Clean Architecture con las siguientes capas:

```
├── Domain/              # Entidades, Value Objects, Reglas de negocio
├── Application/         # Use Cases, CQRS Handlers, Interfaces
├── Infrastructure/      # Implementaciones, DB, External Services
└── API/                 # Controllers, Middleware, Configuration
```

**Beneficios:**
- Independencia del framework
- Testabilidad
- Independencia de la UI
- Independencia de la base de datos
- Independencia de cualquier agencia externa

### 2. CQRS (Command Query Responsibility Segregation)

Separación de operaciones de lectura y escritura:

**Commands (Escritura):**
- Modifican el estado del sistema
- Validan reglas de negocio
- Publican eventos de dominio
- Ejemplo: `CreateAppointmentCommand`, `UpdateTaxPaymentCommand`

**Queries (Lectura):**
- Solo leen datos
- Optimizadas para rendimiento
- Pueden usar vistas materializadas
- Ejemplo: `GetAppointmentByIdQuery`, `GetTaxesByUserQuery`

### 3. Event-Driven Architecture

**Eventos de Dominio:**
- Comunicación asíncrona entre microservicios
- Desacoplamiento
- Eventual consistency
- Procesamiento distribuido

**Flujo de Eventos:**
```
Service A -> Evento -> Message Broker (Redis) -> Service B
```

**Ejemplos de Eventos:**
- `AppointmentCreated`
- `TaxPaymentCompleted`
- `EmployeeHired`
- `LegislativeProcessStarted`

## Patrones Implementados

### 1. Repository Pattern

Abstracción del acceso a datos:

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

### 2. Saga Pattern

Transacciones distribuidas para mantener consistencia:

**Ejemplo:** Proceso de pago de impuestos
1. Validar impuesto
2. Procesar pago
3. Actualizar registro
4. Enviar notificación
5. Compensación en caso de fallo

### 3. Circuit Breaker Pattern

Resiliencia ante fallos de servicios:

```
Estados: Closed -> Open -> Half-Open -> Closed
```

- **Closed:** Operación normal
- **Open:** Servicio caído, respuesta rápida de error
- **Half-Open:** Intentando recuperación

### 4. API Gateway Pattern

Punto de entrada único:
- Enrutamiento
- Autenticación/Autorización
- Rate Limiting
- Agregación de respuestas
- Transformación de protocolos

## Microservicios

### Medical Appointments Service

**Responsabilidades:**
- Gestión de turnos médicos
- Calendario de disponibilidad
- Notificaciones de recordatorio
- Cancelación y reprogramación

**Tecnologías:**
- .NET 8
- PostgreSQL
- Redis Cache
- MediatR (CQRS)

**Endpoints principales:**
- POST /api/appointments
- GET /api/appointments/{id}
- PUT /api/appointments/{id}
- DELETE /api/appointments/{id}
- GET /api/appointments/available

### Legislative Management Service

**Responsabilidades:**
- Trámites y expedientes
- Seguimiento de procesos
- Gestión de documentos
- Aprobaciones y firmas

**Tecnologías:**
- .NET 8
- PostgreSQL
- Document Storage
- Event Sourcing

### HR Management Service

**Responsabilidades:**
- Gestión de empleados
- Licencias y permisos
- Evaluaciones
- Nómina

**Tecnologías:**
- .NET 8
- PostgreSQL
- Redis
- Saga Pattern para workflows

### Tax Services

**Responsabilidades:**
- Gestión de impuestos
- Procesamiento de pagos
- Consulta de deudas
- Generación de comprobantes

**Tecnologías:**
- .NET 8
- PostgreSQL
- Redis
- Circuit Breaker

## Bases de Datos

### PostgreSQL

Cada microservicio tiene su propia base de datos (Database per Service):

- `medical_appointments_db`
- `legislative_db`
- `hr_db`
- `tax_services_db`

**Ventajas:**
- Aislamiento de datos
- Escalabilidad independiente
- Tecnología específica por servicio

### Redis

**Usos:**
- Cache distribuido
- Message broker
- Session storage
- Rate limiting

## Comunicación

### Síncrona
- HTTP/REST entre frontend y gateway
- gRPC entre servicios (opcional)

### Asíncrona
- Event Bus con Redis Pub/Sub
- Message Queue para procesamiento en background

## Seguridad

### Autenticación
- JWT Tokens
- OAuth 2.0 / OpenID Connect
- Refresh tokens

### Autorización
- Role-based (RBAC)
- Policy-based
- Claims-based

### Protección
- HTTPS obligatorio
- CORS configurado
- Rate limiting
- Input validation
- SQL injection prevention
- XSS protection

## Escalabilidad

### Horizontal Scaling
- Múltiples instancias de cada servicio
- Load balancing
- Stateless services

### Vertical Scaling
- Recursos por contenedor
- Database optimization
- Cache strategies

### Performance
- Redis caching
- Database indexing
- Connection pooling
- Async/await patterns
- Lazy loading

## Monitoreo y Observabilidad

### Health Checks
- Liveness probes
- Readiness probes
- Startup probes

### Logging
- Structured logging
- Centralized logs
- Log levels

### Metrics
- Request rate
- Response time
- Error rate
- Resource usage

### Tracing
- Distributed tracing
- Correlation IDs
- Request flow

## DevOps

### CI/CD
- Automated builds
- Automated tests
- Automated deployment

### Infrastructure as Code
- Docker
- Docker Compose
- Kubernetes (opcional)

### Environments
- Development
- Staging
- Production

## Diagrama de Arquitectura

```
┌─────────────────┐
│   React App     │
│  (Port 3000)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   API Gateway   │◄──── JWT Auth
│  (Port 5000)    │
└────────┬────────┘
         │
    ┌────┴────┬─────────┬─────────┐
    ▼         ▼         ▼         ▼
┌────────┐┌────────┐┌────────┐┌────────┐
│Medical ││Legisla-││   HR   ││  Tax   │
│Appoint ││tive    ││Mgmt    ││Services│
│:5001   ││:5002   ││:5003   ││:5004   │
└───┬────┘└───┬────┘└───┬────┘└───┬────┘
    │         │         │         │
    └─────────┴────┬────┴─────────┘
                   ▼
            ┌──────────────┐
            │  Redis Bus   │
            │  (Events)    │
            └──────────────┘
                   │
         ┌─────────┴─────────┐
         ▼                   ▼
    ┌─────────┐         ┌─────────┐
    │PostgreSQL│         │  Redis  │
    │Databases│         │  Cache  │
    └─────────┘         └─────────┘
```

## Flujo de Datos

### Crear Turno Médico

1. Usuario envía petición desde React
2. API Gateway valida token y enruta
3. Medical Service recibe comando
4. Valida reglas de negocio
5. Persiste en PostgreSQL
6. Publica evento `AppointmentCreated`
7. Otros servicios reaccionan al evento
8. Retorna respuesta al cliente

## Consideraciones de Producción

### Alta Disponibilidad
- Réplicas de servicios
- Database replication
- Redis cluster
- Load balancer

### Disaster Recovery
- Backups automáticos
- Recovery procedures
- Failover strategies

### Compliance
- GDPR / Data protection
- Audit logs
- Data encryption
- Access control
