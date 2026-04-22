import { Link } from 'react-router-dom';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';

// ---------------------------------------------------------------------------
// Mock data – replace with real API calls once list endpoints are available
// ---------------------------------------------------------------------------
const mockKpis = {
  totalAppointments: 142,
  pending: 38,
  confirmed: 87,
  cancelled: 17,
};

const mockChartData = [
  { status: 'Pendiente', count: 38, color: '#F59E0B' },
  { status: 'Confirmado', count: 87, color: '#10B981' },
  { status: 'Cancelado', count: 17, color: '#EF4444' },
];

const mockRecentActivity = [
  { id: 1, type: 'Turno Médico', patient: 'María García', date: '2026-04-22 10:30', status: 'Confirmado' },
  { id: 2, type: 'Turno Médico', patient: 'Carlos López', date: '2026-04-22 11:00', status: 'Pendiente' },
  { id: 3, type: 'Turno Médico', patient: 'Ana Martínez', date: '2026-04-22 11:30', status: 'Cancelado' },
  { id: 4, type: 'Turno Médico', patient: 'Luis Rodríguez', date: '2026-04-22 12:00', status: 'Confirmado' },
  { id: 5, type: 'Turno Médico', patient: 'Laura Sánchez', date: '2026-04-22 12:30', status: 'Pendiente' },
  { id: 6, type: 'Turno Médico', patient: 'Diego Fernández', date: '2026-04-22 13:00', status: 'Confirmado' },
  { id: 7, type: 'Turno Médico', patient: 'Valeria Torres', date: '2026-04-21 09:00', status: 'Confirmado' },
];

const mockServices = [
  { name: 'Turnos Médicos', icon: '🏥', link: '/medical', status: 'Activo', color: 'green', port: 5001 },
  { name: 'Legislativo', icon: '📜', link: '/legislative', status: 'En desarrollo', color: 'yellow', port: 5002 },
  { name: 'Recursos Humanos', icon: '👥', link: '/hr', status: 'En desarrollo', color: 'yellow', port: 5003 },
  { name: 'Servicios Tributarios', icon: '💰', link: '/tax', status: 'En desarrollo', color: 'yellow', port: 5004 },
];

// ---------------------------------------------------------------------------
// Sub-components
// ---------------------------------------------------------------------------
function KpiCard({ label, value, color }) {
  const colorMap = {
    blue: 'bg-blue-50 border-blue-500 text-blue-700',
    yellow: 'bg-yellow-50 border-yellow-500 text-yellow-700',
    green: 'bg-green-50 border-green-500 text-green-700',
    red: 'bg-red-50 border-red-500 text-red-700',
  };
  return (
    <div className={`rounded-lg border-l-4 p-5 shadow-sm ${colorMap[color]}`}>
      <p className="text-sm font-medium uppercase tracking-wide opacity-75">{label}</p>
      <p className="mt-2 text-4xl font-bold">{value}</p>
    </div>
  );
}

function StatusBadge({ status }) {
  const map = {
    Confirmado: 'bg-green-100 text-green-800',
    Pendiente: 'bg-yellow-100 text-yellow-800',
    Cancelado: 'bg-red-100 text-red-800',
  };
  return (
    <span className={`inline-block rounded-full px-2 py-0.5 text-xs font-semibold ${map[status] ?? 'bg-gray-100 text-gray-800'}`}>
      {status}
    </span>
  );
}

function ServiceStatusCard({ name, icon, link, status, color, port }) {
  const badgeMap = {
    green: 'bg-green-100 text-green-800',
    yellow: 'bg-yellow-100 text-yellow-800',
    red: 'bg-red-100 text-red-800',
  };
  return (
    <Link
      to={link}
      className="bg-white rounded-lg shadow-sm p-5 flex flex-col gap-3 hover:shadow-md transition-shadow"
    >
      <div className="flex items-center justify-between">
        <span className="text-3xl">{icon}</span>
        <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${badgeMap[color]}`}>
          {status}
        </span>
      </div>
      <p className="font-semibold text-gray-900">{name}</p>
      <p className="text-xs text-gray-400">Puerto {port}</p>
    </Link>
  );
}

// ---------------------------------------------------------------------------
// Main component
// ---------------------------------------------------------------------------
function Dashboard() {
  return (
    <div className="max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Dashboard Analítico</h1>
        <p className="mt-1 text-gray-500">
          Resumen de actividad de servicios municipales
          <span className="ml-2 rounded bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-800">
            Datos de ejemplo – integración con API pendiente
          </span>
        </p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <KpiCard label="Total de Turnos" value={mockKpis.totalAppointments} color="blue" />
        <KpiCard label="Pendientes" value={mockKpis.pending} color="yellow" />
        <KpiCard label="Confirmados" value={mockKpis.confirmed} color="green" />
        <KpiCard label="Cancelados" value={mockKpis.cancelled} color="red" />
      </div>

      {/* Chart + Recent Activity */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Bar Chart */}
        <div className="bg-white rounded-lg shadow-sm p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Turnos por Estado
          </h2>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={mockChartData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="status" tick={{ fontSize: 13 }} />
              <YAxis tick={{ fontSize: 13 }} allowDecimals={false} />
              <Tooltip />
              <Bar dataKey="count" name="Turnos" radius={[4, 4, 0, 0]}>
                {mockChartData.map((entry) => (
                  <Cell key={entry.status} fill={entry.color} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Recent Activity */}
        <div className="bg-white rounded-lg shadow-sm p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Actividad Reciente
          </h2>
          <ul className="divide-y divide-gray-100">
            {mockRecentActivity.map((item) => (
              <li key={item.id} className="py-3 flex items-center justify-between gap-4">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium text-gray-900">{item.patient}</p>
                  <p className="text-xs text-gray-400">{item.type} · {item.date}</p>
                </div>
                <StatusBadge status={item.status} />
              </li>
            ))}
          </ul>
        </div>
      </div>

      {/* Per-service status */}
      <div>
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Estado de Microservicios</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
          {mockServices.map((svc) => (
            <ServiceStatusCard key={svc.name} {...svc} />
          ))}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
