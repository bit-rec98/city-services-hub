# API Documentation

## API Gateway

Base URL: `http://localhost:5000`

El API Gateway actúa como punto de entrada único para todos los microservicios. Utiliza Ocelot para enrutamiento y gestión de solicitudes.

## Rutas

### Medical Appointments Service

Base: `/medical`

#### POST /medical/appointments
Crear un nuevo turno médico

**Request:**
```json
{
  "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "patientName": "Juan Pérez",
  "patientEmail": "juan.perez@example.com",
  "doctorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "appointmentDate": "2026-02-15T10:00:00Z",
  "notes": "Consulta general"
}
```

**Response (201 Created):**
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "patientName": "Juan Pérez",
  "patientEmail": "juan.perez@example.com",
  "doctorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "doctorName": "Dr. María García",
  "specialty": "Medicina General",
  "appointmentDate": "2026-02-15T10:00:00Z",
  "duration": "00:30:00",
  "status": "Scheduled",
  "notes": "Consulta general",
  "createdAt": "2026-01-30T20:00:00Z"
}
```

#### GET /medical/appointments/{id}
Obtener un turno médico por ID

**Response (200 OK):**
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "patientName": "Juan Pérez",
  "doctorName": "Dr. María García",
  "appointmentDate": "2026-02-15T10:00:00Z",
  "status": "Scheduled"
}
```

**Error (404 Not Found):**
```json
{
  "error": "Appointment not found"
}
```

### Legislative Management Service

Base: `/legislative`

#### Endpoints (To be implemented)
- GET /legislative/processes
- POST /legislative/processes
- GET /legislative/processes/{id}
- PUT /legislative/processes/{id}

### HR Management Service

Base: `/hr`

#### Endpoints (To be implemented)
- GET /hr/employees
- POST /hr/employees
- GET /hr/employees/{id}
- PUT /hr/employees/{id}
- POST /hr/leaves
- GET /hr/evaluations

### Tax Services

Base: `/tax`

#### Endpoints (To be implemented)
- GET /tax/taxes
- GET /tax/taxes/{id}
- POST /tax/payments
- GET /tax/debts/{citizenId}

## Health Checks

Todos los servicios exponen un endpoint de health check:

- API Gateway: `GET http://localhost:5000/health`
- Medical Appointments: `GET http://localhost:5001/health`
- Legislative: `GET http://localhost:5002/health`
- HR Management: `GET http://localhost:5003/health`
- Tax Services: `GET http://localhost:5004/health`

**Response:**
```json
{
  "status": "healthy",
  "service": "medical-appointments"
}
```

## Autenticación y Autorización

### JWT Authentication (To be implemented)

Todas las peticiones (excepto health checks) requerirán un token JWT válido.

**Header:**
```
Authorization: Bearer <token>
```

**Token Payload:**
```json
{
  "sub": "user-id",
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "role": "citizen",
  "exp": 1672531199
}
```

## Rate Limiting

El API Gateway implementa rate limiting para prevenir abuso:

- **Límite:** 100 requests por minuto por IP
- **Respuesta cuando se excede:**
  - Status Code: 429 Too Many Requests
  - Header: `X-RateLimit-Remaining: 0`

## CORS

El API Gateway está configurado con CORS permisivo en desarrollo:

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE
Access-Control-Allow-Headers: *
```

## Códigos de Estado HTTP

- **200 OK:** Solicitud exitosa
- **201 Created:** Recurso creado exitosamente
- **400 Bad Request:** Datos de entrada inválidos
- **401 Unauthorized:** No autenticado
- **403 Forbidden:** No autorizado
- **404 Not Found:** Recurso no encontrado
- **429 Too Many Requests:** Rate limit excedido
- **500 Internal Server Error:** Error del servidor

## Eventos del Dominio

Los microservicios publican eventos de dominio a través de Redis Pub/Sub:

### AppointmentCreatedEvent
```json
{
  "eventId": "uuid",
  "occurredOn": "2026-01-30T20:00:00Z",
  "eventType": "AppointmentCreatedEvent",
  "appointmentId": "uuid",
  "patientId": "uuid",
  "doctorId": "uuid",
  "appointmentDate": "2026-02-15T10:00:00Z",
  "specialty": "Medicina General"
}
```

### TaxPaymentCompletedEvent
```json
{
  "eventId": "uuid",
  "occurredOn": "2026-01-30T20:00:00Z",
  "eventType": "TaxPaymentCompletedEvent",
  "paymentId": "uuid",
  "citizenId": "uuid",
  "amount": 1500.00,
  "taxType": "Impuesto Municipal"
}
```

## Swagger/OpenAPI

Cada microservicio expone documentación Swagger en desarrollo:

- Medical Appointments: http://localhost:5001/swagger
- Legislative: http://localhost:5002/swagger
- HR Management: http://localhost:5003/swagger
- Tax Services: http://localhost:5004/swagger

## Ejemplos de Uso

### JavaScript/Axios

```javascript
import axios from 'axios';

const API_URL = 'http://localhost:5000';

// Crear turno médico
const createAppointment = async (data) => {
  try {
    const response = await axios.post(`${API_URL}/medical/appointments`, data);
    return response.data;
  } catch (error) {
    console.error('Error creating appointment:', error.response.data);
    throw error;
  }
};

// Obtener turno médico
const getAppointment = async (id) => {
  try {
    const response = await axios.get(`${API_URL}/medical/appointments/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching appointment:', error.response.data);
    throw error;
  }
};
```

### cURL

```bash
# Crear turno médico
curl -X POST http://localhost:5000/medical/appointments \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "patientName": "Juan Pérez",
    "patientEmail": "juan@example.com",
    "doctorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "appointmentDate": "2026-02-15T10:00:00Z",
    "notes": "Consulta general"
  }'

# Obtener turno médico
curl http://localhost:5000/medical/appointments/7c9e6679-7425-40de-944b-e07fc1f90ae7
```

## Validaciones

### CreateAppointmentCommand

- `patientId`: Required, debe ser un GUID válido
- `patientName`: Required, máximo 200 caracteres
- `patientEmail`: Required, debe ser un email válido
- `doctorId`: Required, debe ser un GUID válido
- `appointmentDate`: Required, debe ser una fecha futura
- `notes`: Optional, sin límite de caracteres

**Error de validación (400 Bad Request):**
```json
{
  "errors": {
    "PatientEmail": ["Invalid email format"],
    "AppointmentDate": ["Appointment date must be in the future"]
  }
}
```
