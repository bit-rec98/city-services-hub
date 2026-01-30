import React from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { appointmentsService } from '../services/appointmentsService';
import { AppointmentStatus } from '../types';

const getStatusColor = (status: string) => {
  const colors: Record<string, string> = {
    [AppointmentStatus.Scheduled]: 'bg-blue-100 text-blue-800',
    [AppointmentStatus.Confirmed]: 'bg-green-100 text-green-800',
    [AppointmentStatus.Completed]: 'bg-gray-100 text-gray-800',
    [AppointmentStatus.Cancelled]: 'bg-red-100 text-red-800',
    [AppointmentStatus.NoShow]: 'bg-yellow-100 text-yellow-800',
    [AppointmentStatus.InProgress]: 'bg-purple-100 text-purple-800',
    [AppointmentStatus.Rescheduled]: 'bg-orange-100 text-orange-800',
  };
  return colors[status] || 'bg-gray-100 text-gray-800';
};

const getStatusLabel = (status: string) => {
  const labels: Record<string, string> = {
    [AppointmentStatus.Scheduled]: 'Agendado',
    [AppointmentStatus.Confirmed]: 'Confirmado',
    [AppointmentStatus.Completed]: 'Completado',
    [AppointmentStatus.Cancelled]: 'Cancelado',
    [AppointmentStatus.NoShow]: 'No se presentó',
    [AppointmentStatus.InProgress]: 'En atención',
    [AppointmentStatus.Rescheduled]: 'Reprogramado',
  };
  return labels[status] || status;
};

export const AppointmentsPage: React.FC = () => {
  const [showUpcomingOnly, setShowUpcomingOnly] = React.useState(true);

  const { data: appointments, isLoading, error } = useQuery({
    queryKey: ['appointments', showUpcomingOnly],
    queryFn: () => appointmentsService.getMyAppointments(showUpcomingOnly),
  });

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Mis Turnos Médicos</h1>
          <p className="text-gray-600 mt-1">Gestioná tus turnos en centros de salud</p>
        </div>
        <Link to="/appointments/new" className="btn-primary">
          + Nuevo Turno
        </Link>
      </div>

      {/* Filtros */}
      <div className="card mb-6">
        <div className="flex items-center space-x-4">
          <span className="text-sm font-medium text-gray-700">Mostrar:</span>
          <button
            onClick={() => setShowUpcomingOnly(true)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              showUpcomingOnly 
                ? 'bg-primary-100 text-primary-700' 
                : 'text-gray-600 hover:bg-gray-100'
            }`}
          >
            Próximos
          </button>
          <button
            onClick={() => setShowUpcomingOnly(false)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              !showUpcomingOnly 
                ? 'bg-primary-100 text-primary-700' 
                : 'text-gray-600 hover:bg-gray-100'
            }`}
          >
            Todos
          </button>
        </div>
      </div>

      {/* Lista de turnos */}
      {isLoading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Cargando turnos...</p>
        </div>
      ) : error ? (
        <div className="text-center py-12">
          <div className="text-red-500 text-5xl mb-4">⚠️</div>
          <p className="text-gray-600">Error al cargar los turnos</p>
        </div>
      ) : appointments && appointments.length > 0 ? (
        <div className="space-y-4">
          {appointments.map((appointment) => (
            <div key={appointment.id} className="card hover:shadow-md transition-shadow">
              <div className="flex items-start justify-between">
                <div className="flex items-start space-x-4">
                  <div className="w-16 h-16 bg-primary-100 rounded-lg flex flex-col items-center justify-center text-primary-700">
                    <span className="text-lg font-bold">
                      {new Date(appointment.scheduledDate).getDate()}
                    </span>
                    <span className="text-xs">
                      {new Date(appointment.scheduledDate).toLocaleDateString('es-AR', { month: 'short' })}
                    </span>
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900 text-lg">
                      {appointment.specialty}
                    </h3>
                    <p className="text-gray-600">{appointment.doctorName}</p>
                    <p className="text-sm text-gray-500 mt-1">
                      🏥 {appointment.healthCenterName}
                    </p>
                    <p className="text-sm text-gray-500">
                      🕒 {appointment.startTime} - {appointment.endTime}
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <span className={`px-3 py-1 rounded-full text-xs font-medium ${getStatusColor(appointment.status)}`}>
                    {getStatusLabel(appointment.status)}
                  </span>
                  <div className="mt-4 space-x-2">
                    <Link 
                      to={`/appointments/${appointment.id}`}
                      className="text-primary-600 text-sm hover:underline"
                    >
                      Ver detalle
                    </Link>
                    {(appointment.status === AppointmentStatus.Scheduled || 
                      appointment.status === AppointmentStatus.Confirmed) && (
                      <button className="text-red-600 text-sm hover:underline">
                        Cancelar
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-12 card">
          <div className="text-5xl mb-4">📅</div>
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            No tenés turnos {showUpcomingOnly ? 'próximos' : ''}
          </h3>
          <p className="text-gray-600 mb-6">
            Agendá un turno en alguno de nuestros centros de salud
          </p>
          <Link to="/appointments/new" className="btn-primary">
            Agendar Turno
          </Link>
        </div>
      )}
    </div>
  );
};
