# 🏛️ Hub de Servicios Ciudadanos

Sistema empresarial para centralización de servicios municipales/provinciales, desarrollado con arquitectura de microservicios y tecnologías modernas.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![React](https://img.shields.io/badge/React-18+-61DAFB?style=flat-square&logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178C6?style=flat-square&logo=typescript)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-336791?style=flat-square&logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker)

## 📋 Descripción

El Hub de Servicios Ciudadanos es una plataforma web que centraliza múltiples servicios gubernamentales para facilitar la gestión de trámites y consultas de los ciudadanos. Inspirado en plataformas como Ciudadano Digital de Córdoba, Argentina.

### Características principales

- 🔐 **Autenticación segura** con JWT y soporte para 2FA
- 🏥 **Gestión de turnos médicos** en centros de salud
- 📋 **Consulta de impuestos** y gestión de pagos
- 📜 **Repositorio legislativo** de ordenanzas y decretos
- 👥 **Portal de RRHH** para empleados municipales
- 📁 **Gestión documental** con firma digital
- 🔔 **Sistema de notificaciones** multicanal

## 🏗️ Arquitectura

### Arquitectura de Microservicios

```
                    ┌─────────────────┐
                    │   API Gateway   │
                    │     (YARP)      │
                    └────────┬────────┘
                             │
     ┌───────────────────────┼───────────────────────┐
     │           │           │           │           │
┌────▼────┐ ┌────▼────┐ ┌────▼────┐ ┌────▼────┐ ┌────▼────┐
│Identity │ │Medical  │ │Legisla- │ │  Tax    │ │Notifi-  │
│ Service │ │Appoint- │ │  tive   │ │Manage-  │ │cations  │
│         │ │ments    │ │ Service │ │ment     │ │Service  │
└────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘
     │           │           │           │           │
     └───────────┴───────────┼───────────┴───────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
         ┌────▼────┐    ┌────▼────┐    ┌────▼────┐
         │PostgreSQL│    │  Redis  │    │RabbitMQ │
         │  (DBs)  │    │ (Cache) │    │ (Events)│
         └─────────┘    └─────────┘    └─────────┘
```

### Clean Architecture en cada Microservicio

```
┌──────────────────────────────────────────┐
│              Presentation                │
│           (API Controllers)              │
├──────────────────────────────────────────┤
│              Application                 │
│     (Commands, Queries, DTOs)            │
├──────────────────────────────────────────┤
│                Domain                    │
│   (Entities, Value Objects, Events)      │
├──────────────────────────────────────────┤
│             Infrastructure               │
│   (Repositories, External Services)      │
└──────────────────────────────────────────┘
```

## 🛠️ Stack Tecnológico

### Backend
- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - APIs REST
- **Entity Framework Core 8.0** - ORM
- **MediatR** - CQRS y Mediator pattern
- **FluentValidation** - Validaciones
- **Serilog** - Logging estructurado
- **YARP** - Reverse proxy / API Gateway

### Frontend
- **React 18+** con TypeScript
- **Vite** - Build tool
- **Zustand** - Estado global
- **TanStack Query** - Server state
- **React Router v6** - Navegación
- **Tailwind CSS** - Estilos
- **React Hook Form + Zod** - Formularios

### Base de Datos e Infraestructura
- **PostgreSQL 15+** - Base de datos principal
- **Redis** - Caché distribuido
- **RabbitMQ** - Message broker
- **Docker & Docker Compose** - Contenedorización

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/your-org/city-services-hub.git
cd city-services-hub
```

2. **Iniciar servicios con Docker Compose**
```bash
docker-compose up -d
```

3. **Instalar dependencias del frontend**
```bash
cd src/WebApp
npm install
```

4. **Ejecutar el frontend en modo desarrollo**
```bash
npm run dev
```

5. **Acceder a la aplicación**
- Frontend: http://localhost:5173
- API Gateway: http://localhost:5000
- Swagger Identity API: http://localhost:5001/swagger
- Swagger Appointments API: http://localhost:5002/swagger
- RabbitMQ Management: http://localhost:15672

### Desarrollo Local (sin Docker)

1. **Iniciar PostgreSQL y Redis**
```bash
docker-compose up -d postgres redis
```

2. **Restaurar y ejecutar backend**
```bash
dotnet restore
dotnet run --project src/Services/Identity/Identity.API
```

3. **En otra terminal, ejecutar el frontend**
```bash
cd src/WebApp
npm run dev
```

## 📁 Estructura del Proyecto

```
/
├── src/
│   ├── ApiGateway/                    # YARP Gateway
│   ├── BuildingBlocks/                # Código compartido
│   │   ├── Common/                    # Abstracciones comunes
│   │   ├── EventBus/                  # Bus de eventos
│   │   └── Logging/                   # Serilog setup
│   ├── Services/
│   │   ├── Identity/                  # Autenticación
│   │   │   ├── Identity.API/
│   │   │   ├── Identity.Application/
│   │   │   ├── Identity.Domain/
│   │   │   ├── Identity.Infrastructure/
│   │   │   └── Identity.Tests/
│   │   ├── MedicalAppointments/       # Turnos médicos
│   │   ├── Legislative/              # Documentación legislativa
│   │   ├── HumanResources/           # RRHH
│   │   ├── TaxManagement/            # Impuestos
│   │   ├── Notifications/            # Notificaciones
│   │   ├── DocumentManagement/       # Gestión documental
│   │   └── CitizenPortal/            # Portal ciudadano
│   └── WebApp/                        # React Frontend
│       ├── src/
│       │   ├── components/
│       │   ├── pages/
│       │   ├── services/
│       │   ├── store/
│       │   ├── hooks/
│       │   └── types/
│       └── package.json
├── scripts/                           # Scripts de utilidad
├── docs/                              # Documentación
├── docker-compose.yml
├── CityServicesHub.sln
└── README.md
```

## 🔑 Variables de Entorno

### Backend (.NET)
```bash
ConnectionStrings__IdentityDb=Host=localhost;Port=5432;Database=identity_db;Username=postgres;Password=postgres
ConnectionStrings__Redis=localhost:6379
JwtSettings__SecretKey=your-secret-key-at-least-32-characters
JwtSettings__Issuer=CityServicesHub
JwtSettings__Audience=CityServicesHub
```

### Frontend
```bash
VITE_API_URL=http://localhost:5000/api/v1
```

## 📖 API Documentation

La documentación de las APIs está disponible mediante Swagger/OpenAPI:

- **Identity API**: `http://localhost:5001/swagger`
- **Medical Appointments API**: `http://localhost:5002/swagger`

### Endpoints Principales

#### Autenticación
```
POST /api/v1/auth/register    - Registro de usuario
POST /api/v1/auth/login       - Inicio de sesión
GET  /api/v1/auth/me          - Perfil del usuario actual
```

#### Turnos Médicos
```
GET  /api/v1/appointments              - Listar turnos
POST /api/v1/appointments              - Crear turno
GET  /api/v1/appointments/{id}         - Obtener turno
POST /api/v1/appointments/{id}/confirm - Confirmar turno
POST /api/v1/appointments/{id}/cancel  - Cancelar turno
```

## 🧪 Testing

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests del frontend
cd src/WebApp
npm test
```

## 🚢 Deployment

### Docker Compose (Producción)
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Kubernetes
Ver documentación en `/docs/kubernetes/`

## 🤝 Contribución

1. Fork el repositorio
2. Crear feature branch (`git checkout -b feature/nueva-funcionalidad`)
3. Commit cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push al branch (`git push origin feature/nueva-funcionalidad`)
5. Abrir Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

## 👥 Equipo

- **City Services Hub Team** - Desarrollo y mantenimiento

## 📞 Soporte

- **Email**: soporte@hubciudadano.gob.ar
- **Documentación**: [Wiki del proyecto](docs/)
- **Issues**: [GitHub Issues](issues/)

---

Desarrollado con ❤️ para los ciudadanos