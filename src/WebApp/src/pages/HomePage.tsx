import React from 'react';
import { Link } from 'react-router-dom';

export const HomePage: React.FC = () => {
  const services = [
    {
      icon: '🏥',
      title: 'Turnos Médicos',
      description: 'Agendá turnos en centros de salud municipales',
      link: '/appointments',
      color: 'bg-blue-50 border-blue-200'
    },
    {
      icon: '📋',
      title: 'Gestión Impositiva',
      description: 'Consultá deudas y realizá pagos de impuestos',
      link: '/taxes',
      color: 'bg-green-50 border-green-200'
    },
    {
      icon: '📜',
      title: 'Normativas',
      description: 'Accedé a ordenanzas, decretos y resoluciones',
      link: '/legislative',
      color: 'bg-purple-50 border-purple-200'
    },
    {
      icon: '📁',
      title: 'Documentos',
      description: 'Gestioná tus documentos y trámites',
      link: '/documents',
      color: 'bg-orange-50 border-orange-200'
    },
    {
      icon: '👤',
      title: 'RRHH',
      description: 'Portal de empleados municipales',
      link: '/hr',
      color: 'bg-pink-50 border-pink-200'
    },
    {
      icon: '🔔',
      title: 'Notificaciones',
      description: 'Mantente informado sobre tus trámites',
      link: '/notifications',
      color: 'bg-yellow-50 border-yellow-200'
    }
  ];

  return (
    <div>
      {/* Hero Section */}
      <section className="bg-gradient-to-br from-primary-600 to-primary-800 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
          <div className="text-center">
            <h1 className="text-4xl md:text-5xl font-bold mb-6">
              Hub de Servicios Ciudadanos
            </h1>
            <p className="text-xl text-primary-100 mb-8 max-w-2xl mx-auto">
              Todos los servicios municipales en un solo lugar. 
              Simplificamos tus trámites para que puedas enfocarte en lo importante.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link 
                to="/register" 
                className="bg-white text-primary-700 px-8 py-3 rounded-lg font-semibold hover:bg-primary-50 transition-colors"
              >
                Crear Cuenta
              </Link>
              <Link 
                to="/login" 
                className="border-2 border-white text-white px-8 py-3 rounded-lg font-semibold hover:bg-white hover:text-primary-700 transition-colors"
              >
                Iniciar Sesión
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Servicios */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              Nuestros Servicios
            </h2>
            <p className="text-gray-600 max-w-2xl mx-auto">
              Accedé a todos los servicios municipales de forma rápida y segura
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {services.map((service, index) => (
              <Link 
                key={index}
                to={service.link}
                className={`p-6 rounded-xl border ${service.color} hover:shadow-lg transition-all duration-300 block`}
              >
                <div className="text-4xl mb-4">{service.icon}</div>
                <h3 className="text-xl font-semibold text-gray-900 mb-2">
                  {service.title}
                </h3>
                <p className="text-gray-600">
                  {service.description}
                </p>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Estadísticas */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">50K+</div>
              <div className="text-gray-600">Ciudadanos Registrados</div>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">100+</div>
              <div className="text-gray-600">Centros de Salud</div>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">1M+</div>
              <div className="text-gray-600">Turnos Gestionados</div>
            </div>
            <div className="text-center">
              <div className="text-4xl font-bold text-primary-600 mb-2">24/7</div>
              <div className="text-gray-600">Disponibilidad</div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Final */}
      <section className="py-16 bg-secondary-600 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold mb-4">
            ¿Listo para simplificar tus trámites?
          </h2>
          <p className="text-secondary-100 mb-8 max-w-xl mx-auto">
            Registrate ahora y comenzá a utilizar todos los servicios del Hub Ciudadano
          </p>
          <Link 
            to="/register" 
            className="bg-white text-secondary-700 px-8 py-3 rounded-lg font-semibold hover:bg-secondary-50 transition-colors inline-block"
          >
            Registrarme Gratis
          </Link>
        </div>
      </section>
    </div>
  );
};
