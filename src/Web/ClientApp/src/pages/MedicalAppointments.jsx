import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const STATUS_LABELS = {
  Scheduled: 'Programado',
  Confirmed: 'Confirmado',
  Cancelled: 'Cancelado',
  Completed: 'Completado',
  NoShow: 'No se presentó',
};

const STATUS_CLASSES = {
  Scheduled: 'bg-blue-100 text-blue-800',
  Confirmed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
  Completed: 'bg-gray-100 text-gray-800',
  NoShow: 'bg-yellow-100 text-yellow-800',
};

function StatusBadge({ status }) {
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_CLASSES[status] ?? 'bg-gray-100 text-gray-800'}`}>
      {STATUS_LABELS[status] ?? status}
    </span>
  );
}

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

  const { data: appointments, isLoading, isError } = useQuery({
    queryKey: ['appointments'],
    queryFn: () =>
      axios.get(`${API_URL}/medical/appointments`).then((res) => res.data),
  });

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

  const cancelAppointment = useMutation({
    mutationFn: ({ id, reason }) =>
      axios.put(`${API_URL}/medical/appointments/${id}/cancel`, { cancellationReason: reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
    onError: (error) => {
      const msg = error.response?.data?.error || 'Error al cancelar el turno.';
      alert(msg);
    },
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    createAppointment.mutate(formData);
  };

  const handleCancel = (id) => {
    const reason = window.prompt('Motivo de cancelación (opcional):');
    if (reason === null) return; // user pressed Cancel in the dialog
    cancelAppointment.mutate({ id, reason: reason.trim() });
  };

  return (
    <div className="max-w-5xl mx-auto">
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

      {/* Appointments list */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Turnos Registrados</h2>

        {isLoading && (
          <p className="text-gray-500 text-sm">Cargando turnos...</p>
        )}

        {isError && (
          <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4">
            <p className="text-yellow-700 text-sm">
              No se pudo conectar con el servicio. Asegúrese de que el backend esté en ejecución.
            </p>
          </div>
        )}

        {!isLoading && !isError && appointments?.length === 0 && (
          <p className="text-gray-500 text-sm">No hay turnos registrados aún.</p>
        )}

        {!isLoading && !isError && appointments?.length > 0 && (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 uppercase tracking-wider">Paciente</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 uppercase tracking-wider">Email</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 uppercase tracking-wider">Fecha</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 uppercase tracking-wider">Estado</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 uppercase tracking-wider">Acciones</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-100">
                {appointments.map((appt) => (
                  <tr key={appt.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-medium text-gray-900">{appt.patientName}</td>
                    <td className="px-4 py-3 text-gray-600">{appt.patientEmail}</td>
                    <td className="px-4 py-3 text-gray-600">
                      {new Date(appt.appointmentDate).toLocaleString('es-AR', {
                        dateStyle: 'short',
                        timeStyle: 'short',
                      })}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={appt.status} />
                    </td>
                    <td className="px-4 py-3">
                      {appt.status !== 'Cancelled' && appt.status !== 'Completed' && (
                        <button
                          onClick={() => handleCancel(appt.id)}
                          disabled={cancelAppointment.isPending}
                          className="text-red-600 hover:text-red-800 text-xs font-medium disabled:opacity-50"
                        >
                          Cancelar
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
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
