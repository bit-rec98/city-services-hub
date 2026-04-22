import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Home from './pages/Home';
import Dashboard from './pages/Dashboard';
import MedicalAppointments from './pages/MedicalAppointments';
import Legislative from './pages/Legislative';
import HumanResources from './pages/HumanResources';
import TaxServices from './pages/TaxServices';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="min-h-screen bg-gray-50">
          {/* Navigation */}
          <nav className="bg-blue-600 text-white shadow-lg">
            <div className="container mx-auto px-4">
              <div className="flex items-center justify-between h-16">
                <Link to="/" className="text-xl font-bold">
                  Hub Ciudadano
                </Link>
                <div className="flex space-x-4">
                  <Link to="/dashboard" className="hover:bg-blue-700 px-3 py-2 rounded">
                    Dashboard
                  </Link>
                  <Link to="/medical" className="hover:bg-blue-700 px-3 py-2 rounded">
                    Turnos Médicos
                  </Link>
                  <Link to="/legislative" className="hover:bg-blue-700 px-3 py-2 rounded">
                    Legislativo
                  </Link>
                  <Link to="/hr" className="hover:bg-blue-700 px-3 py-2 rounded">
                    RRHH
                  </Link>
                  <Link to="/tax" className="hover:bg-blue-700 px-3 py-2 rounded">
                    Impuestos
                  </Link>
                </div>
              </div>
            </div>
          </nav>

          {/* Main Content */}
          <main className="container mx-auto px-4 py-8">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/medical" element={<MedicalAppointments />} />
              <Route path="/legislative" element={<Legislative />} />
              <Route path="/hr" element={<HumanResources />} />
              <Route path="/tax" element={<TaxServices />} />
            </Routes>
          </main>

          {/* Footer */}
          <footer className="bg-gray-800 text-white mt-12 py-6">
            <div className="container mx-auto px-4 text-center">
              <p>&copy; 2026 Hub Ciudadano - Servicios Municipales</p>
            </div>
          </footer>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
