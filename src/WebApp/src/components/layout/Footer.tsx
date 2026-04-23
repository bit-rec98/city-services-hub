import React from 'react';

export const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-gray-800 text-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          {/* Logo y descripción */}
          <div className="col-span-1 md:col-span-2">
            <div className="flex items-center space-x-2 mb-4">
              <div className="w-8 h-8 bg-primary-500 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-lg">H</span>
              </div>
              <span className="text-xl font-bold">Hub Ciudadano</span>
            </div>
            <p className="text-gray-400 text-sm">
              Portal de servicios ciudadanos que centraliza trámites y gestiones 
              municipales para facilitar la vida de los ciudadanos.
            </p>
          </div>

          {/* Enlaces útiles */}
          <div>
            <h3 className="font-semibold mb-4">Servicios</h3>
            <ul className="space-y-2 text-gray-400 text-sm">
              <li><a href="/appointments" className="hover:text-white">Turnos Médicos</a></li>
              <li><a href="/taxes" className="hover:text-white">Impuestos</a></li>
              <li><a href="/procedures" className="hover:text-white">Trámites</a></li>
              <li><a href="/documents" className="hover:text-white">Documentos</a></li>
            </ul>
          </div>

          {/* Contacto */}
          <div>
            <h3 className="font-semibold mb-4">Contacto</h3>
            <ul className="space-y-2 text-gray-400 text-sm">
              <li>📞 0800-XXX-XXXX</li>
              <li>📧 contacto@hubciudadano.gob.ar</li>
              <li>📍 Córdoba, Argentina</li>
            </ul>
          </div>
        </div>

        <div className="border-t border-gray-700 mt-8 pt-8 flex flex-col md:flex-row justify-between items-center">
          <p className="text-gray-400 text-sm">
            © {currentYear} Hub de Servicios Ciudadanos. Todos los derechos reservados.
          </p>
          <div className="flex space-x-6 mt-4 md:mt-0">
            <a href="/privacy" className="text-gray-400 hover:text-white text-sm">
              Política de Privacidad
            </a>
            <a href="/terms" className="text-gray-400 hover:text-white text-sm">
              Términos de Uso
            </a>
            <a href="/accessibility" className="text-gray-400 hover:text-white text-sm">
              Accesibilidad
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
};
