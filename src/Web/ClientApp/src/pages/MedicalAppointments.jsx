import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

function MedicalAppointments() {
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState(null);
  const [formData, setFormData] = useState({
    patientId: '',
    patientName: '',
    patientEmail: '',
    doctorId: '',
    appointmentDate: '',
    notes: ''
  });

  const queryClient = useQueryClient();

  const createAppointment = useMutation({
    mutationFn: (data) => axios.post(`${API_URL}/medical/appointments`, data),
    onSuccess: () => {
      setShowForm(false);
      setError(null);
      setFormData({
        patientId: '',
        patientName: '',
        patientEmail: '',
        doctorId: '',
        appointmentDate: '',
        notes: ''
      });
      // Invalidate and refetch appointments list
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      alert('¡Turno creado exitosamente!');
    },
    onError: (error) => {
      const errorMessage = error.response?.data?.error || 
                          error.response?.data?.errors || 
                          'Error al crear el turno. Por favor, intente nuevamente.';
      setError(errorMessage);
    }
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    createAppointment.mutate(formData);
  };

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Turnos Médicos
        </h1>
        <p className="text-gray-600">
          Sistema de gestión de citas médicas
        </p>
      </div>

      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <button
          onClick={() => {
            setShowForm(!showForm);
            setError(null);
          }}
          className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition"
        >
          {showForm ? 'Cancelar' : 'Nuevo Turno'}
        </button>

        {error && (
          <div className="mt-4 bg-red-50 border-l-4 border-red-500 p-4">
            <p className="text-red-700">
              {typeof error === 'string' ? error : JSON.stringify(error)}
            </p>
          </div>
        )}

        {showForm && (
          <form onSubmit={handleSubmit} className="mt-6 space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nombre del Paciente
              </label>
              <input
                type="text"
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={formData.patientName}
                onChange={(e) => setFormData({ ...formData, patientName: e.target.value })}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                type="email"
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={formData.patientEmail}
                onChange={(e) => setFormData({ ...formData, patientEmail: e.target.value })}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Fecha y Hora del Turno
              </label>
              <input
                type="datetime-local"
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                value={formData.appointmentDate}
                onChange={(e) => setFormData({ ...formData, appointmentDate: e.target.value })}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Notas
              </label>
              <textarea
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows="3"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              />
            </div>

            <button
              type="submit"
              disabled={createAppointment.isPending}
              className="w-full bg-green-600 text-white px-6 py-2 rounded-lg hover:bg-green-700 transition disabled:bg-gray-400"
            >
              {createAppointment.isPending ? 'Guardando...' : 'Guardar Turno'}
            </button>
          </form>
        )}
      </div>

      <div className="bg-blue-50 border-l-4 border-blue-500 p-4">
        <p className="text-blue-700">
          <strong>Microservicio:</strong> Medical Appointments Service (Puerto 5001)
        </p>
        <p className="text-blue-600 text-sm mt-2">
          Arquitectura: Clean Architecture + CQRS + Event-Driven
        </p>
      </div>
    </div>
  );
}

export default MedicalAppointments;
