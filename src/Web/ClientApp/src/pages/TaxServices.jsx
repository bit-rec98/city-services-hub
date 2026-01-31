function TaxServices() {
  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-4">
        Servicios Tributarios
      </h1>
      <div className="bg-white rounded-lg shadow-md p-8">
        <p className="text-gray-600 mb-4">
          Módulo para la gestión de impuestos, pagos y consulta de deudas.
        </p>
        <div className="bg-blue-50 border-l-4 border-blue-500 p-4">
          <p className="text-blue-700">
            <strong>Microservicio:</strong> Tax Services (Puerto 5004)
          </p>
          <p className="text-blue-600 text-sm mt-2">
            Implementa: Circuit Breaker Pattern para resiliencia
          </p>
        </div>
      </div>
    </div>
  );
}

export default TaxServices;
