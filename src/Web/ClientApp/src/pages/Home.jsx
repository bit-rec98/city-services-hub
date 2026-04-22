function Home() {
  return (
    <div className="max-w-7xl mx-auto">
      <div className="text-center mb-12">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          Bienvenido al Hub Ciudadano
        </h1>
        <p className="text-xl text-gray-600">
          Sistema centralizado de servicios municipales
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <ServiceCard
          title="Turnos Médicos"
          description="Gestión de citas médicas y consultas"
          icon="🏥"
          link="/medical"
        />
        <ServiceCard
          title="Gestión Legislativa"
          description="Trámites y expedientes legislativos"
          icon="📜"
          link="/legislative"
        />
        <ServiceCard
          title="Recursos Humanos"
          description="Gestión de empleados y permisos"
          icon="👥"
          link="/hr"
        />
        <ServiceCard
          title="Servicios Tributarios"
          description="Impuestos y pagos municipales"
          icon="💰"
          link="/tax"
        />
      </div>

      <div className="mt-8">
        <a
          href="/dashboard"
          className="flex items-center justify-between bg-blue-600 text-white rounded-lg shadow-md px-8 py-6 hover:bg-blue-700 transition-colors"
        >
          <div>
            <h3 className="text-xl font-bold mb-1">📊 Ver Dashboard Analítico</h3>
            <p className="text-blue-100 text-sm">
              Resumen de actividad, KPIs y estado de todos los microservicios
            </p>
          </div>
          <span className="text-3xl">→</span>
        </a>
      </div>

      <div className="mt-8 bg-white rounded-lg shadow-md p-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          Arquitectura del Sistema
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <FeatureCard title="Microservicios" description=".NET 8 con Clean Architecture y CQRS" />
          <FeatureCard title="Frontend Moderno" description="React 18 + Zustand + React Query + Tailwind" />
          <FeatureCard title="Infraestructura" description="PostgreSQL + Redis + Docker" />
        </div>
      </div>
    </div>
  );
}

function ServiceCard({ title, description, icon, link }) {
  return (
    <a
      href={link}
      className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow cursor-pointer"
    >
      <div className="text-4xl mb-4">{icon}</div>
      <h3 className="text-xl font-bold text-gray-900 mb-2">{title}</h3>
      <p className="text-gray-600">{description}</p>
    </a>
  );
}

function FeatureCard({ title, description }) {
  return (
    <div className="text-center">
      <h3 className="text-lg font-semibold text-gray-900 mb-2">{title}</h3>
      <p className="text-gray-600">{description}</p>
    </div>
  );
}

export default Home;
