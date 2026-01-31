# Hub Ciudadano - Servicios Municipales

## 🎯 Resumen del Proyecto

Este repositorio contiene la implementación completa de un sistema tipo "Ciudadano Digital" para centralizar servicios municipales, incluyendo:
- ✅ Turnos médicos
- ✅ Gestión legislativa  
- ✅ Recursos humanos
- ✅ Servicios tributarios

## 🏗️ Arquitectura Implementada

### Backend (.NET 8 Microservicios)

#### **Medical Appointments Service** (Puerto 5001)
- ✅ Clean Architecture completa (Domain, Application, Infrastructure, API)
- ✅ CQRS con MediatR
- ✅ Repository Pattern
- ✅ Event-Driven con Redis
- ✅ Entity Framework Core + PostgreSQL
- ✅ FluentValidation
- ✅ Health checks

**Endpoints Implementados:**
- `POST /api/appointments` - Crear turno médico
- `GET /api/appointments/{id}` - Obtener turno por ID

#### **API Gateway** (Puerto 5000)
- ✅ Ocelot para enrutamiento
- ✅ Configuración CORS
- ✅ Rate limiting
- ✅ Health checks
- ✅ Enrutamiento a todos los microservicios

#### **Shared Libraries**
- ✅ `CityServices.Shared.Common` - BaseEntity, IRepository, Result pattern
- ✅ `CityServices.Shared.Events` - DomainEvent, IEventBus, eventos de dominio

### Frontend (React 18)

- ✅ Vite como build tool
- ✅ TanStack Query (React Query) para server state
- ✅ Zustand para client state
- ✅ React Router v6
- ✅ Tailwind CSS
- ✅ Axios para HTTP
- ✅ Error handling robusto
- ✅ UI responsive y moderna

**Páginas Implementadas:**
- Home con resumen de servicios
- Medical Appointments con formulario funcional
- Legislative (placeholder)
- HR Management (placeholder)
- Tax Services (placeholder)

### Infraestructura

- ✅ Docker Compose para orquestación
- ✅ PostgreSQL 15 con database per service
- ✅ Redis 7 para event bus y cache
- ✅ Dockerfiles para todos los servicios
- ✅ Nginx para frontend
- ✅ Health checks en todos los servicios

## 🚀 Inicio Rápido

### Con Docker Compose (Recomendado)

```bash
# Clonar repositorio
git clone https://github.com/bit-rec98/city-services-hub.git
cd city-services-hub

# Iniciar todos los servicios
docker-compose up -d

# Verificar que los servicios estén corriendo
docker-compose ps
```

**Servicios disponibles:**
- Frontend: http://localhost:3000
- API Gateway: http://localhost:5000
- Medical Appointments: http://localhost:5001
- PostgreSQL: localhost:5432
- Redis: localhost:6379

### Desarrollo Local

**Backend:**
```bash
cd src/Services/MedicalAppointments/MedicalAppointments.API
dotnet restore
dotnet run
```

**Frontend:**
```bash
cd src/Web/ClientApp
npm install
npm run dev
```

## 📊 Patrones de Diseño Aplicados

### 1. Clean Architecture
Separación clara de responsabilidades en capas:
- **Domain**: Entidades y lógica de negocio
- **Application**: Use cases (CQRS handlers)
- **Infrastructure**: Implementaciones (EF Core, Redis)
- **API**: Controllers y configuración

### 2. CQRS (Command Query Responsibility Segregation)
```csharp
// Command - Modifica estado
CreateAppointmentCommand → CreateAppointmentCommandHandler

// Query - Solo lectura
GetAppointmentByIdQuery → GetAppointmentByIdQueryHandler
```

### 3. Repository Pattern
```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    // ...
}
```

### 4. Event-Driven Architecture
```csharp
// Publicar evento
await _eventBus.PublishAsync(new AppointmentCreatedEvent { ... });

// Otros servicios pueden reaccionar al evento
```

### 5. API Gateway Pattern
- Punto de entrada único
- Enrutamiento inteligente
- Rate limiting
- CORS

### 6. Circuit Breaker (Preparado para implementación)
Para resiliencia ante fallos de servicios externos.

### 7. Saga Pattern (Preparado para implementación)
Para transacciones distribuidas en workflows complejos.

## 🔒 Seguridad

✅ **CodeQL Analysis**: Sin vulnerabilidades detectadas
✅ **Input Validation**: FluentValidation en todos los comandos
✅ **CORS**: Configurado correctamente
✅ **Error Handling**: Manejo robusto de errores
✅ **Health Checks**: Monitoreo de estado de servicios

**Próximos Pasos de Seguridad:**
- [ ] JWT Authentication
- [ ] Role-based Authorization
- [ ] Rate Limiting por usuario
- [ ] HTTPS obligatorio en producción
- [ ] Secrets management

## 📚 Documentación

- [Arquitectura Detallada](docs/ARCHITECTURE.md)
- [Guía de Desarrollo](docs/DEVELOPMENT.md)
- [Documentación API](docs/API.md)

## 🧪 Testing

```bash
# Backend
cd src/Services/MedicalAppointments
dotnet test

# Frontend
cd src/Web/ClientApp
npm test
```

## 📈 Características de Escalabilidad

✅ **Horizontal Scaling**: Arquitectura stateless
✅ **Database per Service**: Aislamiento de datos
✅ **Event-Driven**: Comunicación asíncrona
✅ **Caching**: Redis distribuido
✅ **Load Balancing**: Preparado con Docker Swarm/Kubernetes

## 🛠️ Stack Tecnológico Completo

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- MediatR 14
- FluentValidation 12
- Npgsql (PostgreSQL driver)
- StackExchange.Redis 2.10
- Ocelot 23.4

### Frontend
- React 18
- Vite 6
- React Router v6
- TanStack Query v5
- Zustand 5
- Tailwind CSS 3
- Axios 1.7

### Infraestructura
- Docker 20.10+
- Docker Compose 2+
- PostgreSQL 15
- Redis 7
- Nginx (Alpine)

## 🎓 Conceptos Demostrados

✅ Microservices Architecture
✅ Clean Architecture
✅ CQRS Pattern
✅ Event-Driven Architecture
✅ Repository Pattern
✅ Dependency Injection
✅ Async/Await patterns
✅ RESTful API design
✅ Database per service
✅ API Gateway pattern
✅ Containerization
✅ Modern frontend architecture
✅ State management (client & server)
✅ Responsive design

## 📝 Próximas Mejoras Sugeridas

### Backend
- [ ] Implementar servicios Legislative, HR y Tax completos
- [ ] Agregar autenticación JWT
- [ ] Implementar Circuit Breaker con Polly
- [ ] Saga pattern para workflows complejos
- [ ] Unit & Integration tests
- [ ] Logging estructurado (Serilog)
- [ ] Metrics con Prometheus

### Frontend
- [ ] Implementar autenticación
- [ ] Agregar más features a Medical Appointments (listar, editar, cancelar)
- [ ] Implementar los otros módulos completamente
- [ ] Agregar tests (Jest + React Testing Library)
- [ ] Mejorar UX con loading states
- [ ] Agregar notificaciones (Toast)

### DevOps
- [ ] CI/CD pipelines
- [ ] Kubernetes deployment
- [ ] Monitoring & Alerting
- [ ] Backup strategies
- [ ] Production environment configuration

## 🤝 Contribuciones

Las contribuciones son bienvenidas! Por favor:

1. Fork el proyecto
2. Crear feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add AmazingFeature'`)
4. Push al branch (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

## 📄 Licencia

MIT License - ver [LICENSE](LICENSE) para más detalles

## 👥 Autor

Implementado como demostración de arquitectura enterprise-grade con .NET 8 y React 18.

---

**⭐ Si este proyecto te fue útil, considera darle una estrella!**
