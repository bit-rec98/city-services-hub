import React from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export const DashboardPage: React.FC = () => {
  const { user } = useAuthStore();

  const quickActions = [
    {
      icon: '🏥',
      title: 'Nuevo Turno Médico',
      description: 'Agendá un turno en un centro de salud',
      link: '/appointments/new',
      color: 'bg-blue-500'
    },
    {
      icon: '📋',
      title: 'Consultar Impuestos',
      description: 'Verificá tu estado de cuenta',
      link: '/taxes',
      color: 'bg-green-500'
    },
    {
      icon: '📁',
      title: 'Mis Documentos',
      description: 'Accedé a tus documentos digitales',
      link: '/documents',
      color: 'bg-purple-500'
    },
    {
      icon: '🔔',
      title: 'Notificaciones',
      description: 'Revisá tus notificaciones pendientes',
      link: '/notifications',
      color: 'bg-yellow-500'
    }
  ];

  const upcomingAppointments = [
    {
      id: 1,
      specialty: 'Medicina General',
      doctor: 'Dr. Juan García',
      date: '15/02/2024',
      time: '10:30',
      location: 'Centro de Salud Nº 12'
    },
    {
      id: 2,
      specialty: 'Odontología',
      doctor: 'Dra. María López',
      date: '22/02/2024',
      time: '14:00',
      location: 'Centro de Salud Nº 5'
    }
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Bienvenida */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          ¡Hola, {user?.firstName}!
        </h1>
        <p className="text-gray-600 mt-1">
          Bienvenido/a al Hub de Servicios Ciudadanos
        </p>
      </div>

      {/* Acciones Rápidas */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {quickActions.map((action, index) => (
          <Link
            key={index}
            to={action.link}
            className="card hover:shadow-lg transition-shadow"
          >
            <div className={`w-12 h-12 ${action.color} rounded-lg flex items-center justify-center text-2xl mb-4`}>
              {action.icon}
            </div>
            <h3 className="font-semibold text-gray-900 mb-1">{action.title}</h3>
            <p className="text-sm text-gray-600">{action.description}</p>
          </Link>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Próximos Turnos */}
        <div className="card">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Próximos Turnos</h2>
            <Link to="/appointments" className="text-primary-600 text-sm hover:underline">
              Ver todos
            </Link>
          </div>

          {upcomingAppointments.length > 0 ? (
            <div className="space-y-4">
              {upcomingAppointments.map((appointment) => (
                <div 
                  key={appointment.id}
                  className="flex items-start space-x-4 p-4 bg-gray-50 rounded-lg"
                >
                  <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center text-primary-600 font-semibold text-sm">
                    {appointment.date.split('/')[0]}
                  </div>
                  <div className="flex-1">
                    <h4 className="font-medium text-gray-900">{appointment.specialty}</h4>
                    <p className="text-sm text-gray-600">{appointment.doctor}</p>
                    <p className="text-sm text-gray-500">
                      {appointment.date} - {appointment.time} | {appointment.location}
                    </p>
                  </div>
                  <button className="text-primary-600 text-sm hover:underline">
                    Ver detalle
                  </button>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <p>No tenés turnos próximos</p>
              <Link to="/appointments/new" className="text-primary-600 hover:underline mt-2 inline-block">
                Agendar un turno
              </Link>
            </div>
          )}
        </div>

        {/* Actividad Reciente */}
        <div className="card">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Actividad Reciente</h2>
          </div>

          <div className="space-y-4">
            <div className="flex items-start space-x-3 text-sm">
              <div className="w-2 h-2 bg-green-500 rounded-full mt-2"></div>
              <div>
                <p className="text-gray-900">Turno confirmado con Dr. García</p>
                <p className="text-gray-500">Hace 2 horas</p>
              </div>
            </div>
            <div className="flex items-start space-x-3 text-sm">
              <div className="w-2 h-2 bg-blue-500 rounded-full mt-2"></div>
              <div>
                <p className="text-gray-900">Documento subido exitosamente</p>
                <p className="text-gray-500">Hace 1 día</p>
              </div>
            </div>
            <div className="flex items-start space-x-3 text-sm">
              <div className="w-2 h-2 bg-yellow-500 rounded-full mt-2"></div>
              <div>
                <p className="text-gray-900">Vencimiento próximo: Impuesto inmobiliario</p>
                <p className="text-gray-500">Hace 3 días</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Banner informativo */}
      <div className="mt-8 bg-gradient-to-r from-secondary-500 to-secondary-600 rounded-xl p-6 text-white">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-lg font-semibold mb-2">
              ¿Necesitás ayuda?
            </h3>
            <p className="text-secondary-100">
              Nuestro equipo de soporte está disponible para asistirte con cualquier consulta.
            </p>
          </div>
          <a 
            href="/help" 
            className="bg-white text-secondary-600 px-6 py-2 rounded-lg font-medium hover:bg-secondary-50 transition-colors"
          >
            Centro de Ayuda
          </a>
        </div>
      </div>
    </div>
  );
};
