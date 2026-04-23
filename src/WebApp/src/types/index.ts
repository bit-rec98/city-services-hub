// Tipos para autenticación
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  documentNumber: string;
  documentType: DocumentType;
  phoneNumber?: string;
  status: UserStatus;
  emailVerified: boolean;
  twoFactorEnabled: boolean;
  roles: string[];
  lastLoginAt?: string;
  createdAt: string;
}

export enum DocumentType {
  DNI = 0,
  CUIT = 1,
  CUIL = 2,
  Passport = 3,
  ForeignId = 4
}

export enum UserStatus {
  Pending = 0,
  Active = 1,
  Suspended = 2,
  Blocked = 3,
  Inactive = 4
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode?: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  documentNumber: string;
  documentType: DocumentType;
  password: string;
  confirmPassword: string;
}

// Tipos para turnos médicos
export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  patientDocumentNumber: string;
  patientPhoneNumber?: string;
  patientEmail?: string;
  doctorId: string;
  doctorName: string;
  specialty: string;
  healthCenterId: string;
  healthCenterName: string;
  scheduledDate: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  status: AppointmentStatus;
  attentionType: string;
  reason?: string;
  notes?: string;
  cancellationReason?: string;
  createdAt: string;
  confirmedAt?: string;
  cancelledAt?: string;
  completedAt?: string;
}

export enum AppointmentStatus {
  Scheduled = 'Scheduled',
  Confirmed = 'Confirmed',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  NoShow = 'NoShow',
  Rescheduled = 'Rescheduled'
}

export interface HealthCenter {
  id: string;
  name: string;
  code: string;
  address: string;
  city: string;
  province: string;
  phoneNumber?: string;
  email?: string;
  latitude?: number;
  longitude?: number;
  isActive: boolean;
  specialties: string[];
}

export interface Doctor {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  licenseNumber: string;
  primarySpecialty: string;
  email?: string;
  phoneNumber?: string;
  defaultAppointmentDuration: number;
  isActive: boolean;
  secondarySpecialties: string[];
}

export interface CreateAppointmentRequest {
  patientId: string;
  patientName: string;
  patientDocumentNumber: string;
  patientPhoneNumber?: string;
  patientEmail?: string;
  doctorId: string;
  healthCenterId: string;
  specialty: number;
  scheduledDate: string;
  startTime: string;
  durationMinutes?: number;
  attentionType?: number;
  reason?: string;
}

// Tipos para respuestas de API
export interface ApiResponse<T> {
  data?: T;
  message?: string;
  errors?: Record<string, string[]>;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
