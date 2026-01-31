# Hub Ciudadano - Servicios Municipales

Sistema tipo Ciudadano Digital para centralizar servicios municipales: turnos médicos, gestión legislativa, recursos humanos e impuestos.

## 🏗️ Arquitectura

**Stack Tecnológico:**
- **Backend:** .NET 8 Microservicios
- **Frontend:** React 18 + Vite
- **Bases de Datos:** PostgreSQL
- **Cache/Messaging:** Redis
- **Contenedores:** Docker + Docker Compose

**Patrones de Diseño:**
- Clean Architecture
- CQRS (Command Query Responsibility Segregation)
- Event-Driven Architecture
- Repository Pattern
- Saga Pattern
- Circuit Breaker Pattern
- API Gateway Pattern

**Frontend Stack:**
- Zustand (State Management)
- React Query (Server State)
- Tailwind CSS (Styling)

## 📦 Microservicios

### 1. API Gateway
Puerto: 5000
- Enrutamiento de solicitudes
- Autenticación y autorización
- Rate limiting
- Circuit breaker

### 2. Medical Appointments Service
Puerto: 5001
- Gestión de turnos médicos
- Calendario de disponibilidad
- Notificaciones

### 3. Legislative Management Service
Puerto: 5002
- Trámites legislativos
- Seguimiento de expedientes
- Documentación

### 4. HR Management Service
Puerto: 5003
- Gestión de empleados
- Licencias y permisos
- Evaluaciones de desempeño

### 5. Tax Services
Puerto: 5004
- Gestión de impuestos
- Pagos y facturación
- Consulta de deudas

## 🚀 Inicio Rápido

### Prerrequisitos
- Docker & Docker Compose
- .NET 8 SDK
- Node.js 18+

### Desarrollo

1. Clonar el repositorio:
```bash
git clone https://github.com/bit-rec98/city-services-hub.git
cd city-services-hub
```

2. Iniciar servicios con Docker:
```bash
docker-compose up -d
```

3. Acceder a la aplicación:
- Frontend: http://localhost:3000
- API Gateway: http://localhost:5000

## 🛠️ Desarrollo Local

### Backend (.NET)
```bash
cd src/Services/[ServiceName]
dotnet restore
dotnet run
```

### Frontend (React)
```bash
cd src/Web/ClientApp
npm install
npm run dev
```

## 🏛️ Estructura del Proyecto

```
city-services-hub/
├── src/
│   ├── ApiGateway/              # API Gateway con Ocelot
│   ├── Services/
│   │   ├── MedicalAppointments/ # Microservicio de turnos médicos
│   │   ├── Legislative/         # Microservicio legislativo
│   │   ├── HumanResources/      # Microservicio de RRHH
│   │   └── TaxServices/         # Microservicio de impuestos
│   ├── Shared/
│   │   ├── Common/              # Utilidades compartidas
│   │   └── Events/              # Eventos del dominio
│   └── Web/
│       └── ClientApp/           # Aplicación React
├── docs/                        # Documentación
├── docker-compose.yml           # Orquestación de contenedores
└── README.md
```

## 📚 Documentación

- [Arquitectura](docs/ARCHITECTURE.md)
- [Guía de Desarrollo](docs/DEVELOPMENT.md)
- [API Documentation](docs/API.md)

## 🔒 Seguridad

- Autenticación JWT
- HTTPS obligatorio en producción
- Rate limiting
- CORS configurado
- Validación de entrada

## 📈 Características de Escalabilidad

- Arquitectura de microservicios
- Event-driven communication
- Redis para caché distribuido
- Circuit breaker para resiliencia
- Health checks
- Horizontal scaling con Docker

## 🧪 Testing

```bash
# Backend
dotnet test

# Frontend
npm test
```

## 📄 Licencia

MIT License