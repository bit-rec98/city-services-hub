import api from './api';
import type { Appointment, CreateAppointmentRequest, HealthCenter, Doctor } from '../types';

export const appointmentsService = {
  // Obtener turnos del paciente
  getMyAppointments: async (upcomingOnly = false): Promise<Appointment[]> => {
    const response = await api.get<Appointment[]>('/appointments/patient/me', {
      params: { upcomingOnly }
    });
    return response.data;
  },

  // Obtener turno por ID
  getAppointment: async (id: string): Promise<Appointment> => {
    const response = await api.get<Appointment>(`/appointments/${id}`);
    return response.data;
  },

  // Crear turno
  createAppointment: async (data: CreateAppointmentRequest): Promise<Appointment> => {
    const response = await api.post<Appointment>('/appointments', data);
    return response.data;
  },

  // Confirmar turno
  confirmAppointment: async (id: string): Promise<void> => {
    await api.post(`/appointments/${id}/confirm`);
  },

  // Cancelar turno
  cancelAppointment: async (id: string, reason: string): Promise<void> => {
    await api.post(`/appointments/${id}/cancel`, { reason });
  },

  // Obtener centros de salud
  getHealthCenters: async (city?: string, specialty?: number): Promise<HealthCenter[]> => {
    const response = await api.get<HealthCenter[]>('/healthcenters', {
      params: { city, specialty }
    });
    return response.data;
  },

  // Obtener médicos
  getDoctors: async (specialty?: number, healthCenterId?: string): Promise<Doctor[]> => {
    const response = await api.get<Doctor[]>('/doctors', {
      params: { specialty, healthCenterId }
    });
    return response.data;
  },

  // Obtener slots disponibles
  getAvailableSlots: async (doctorId: string, healthCenterId: string, date: string) => {
    const response = await api.get('/appointments/available-slots', {
      params: { doctorId, healthCenterId, date }
    });
    return response.data;
  }
};
