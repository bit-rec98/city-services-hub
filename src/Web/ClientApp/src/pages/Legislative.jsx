function Legislative() {
  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-3xl font-bold text-gray-900 mb-4">
        Gestión Legislativa
      </h1>
      <div className="bg-white rounded-lg shadow-md p-8">
        <p className="text-gray-600 mb-4">
          Módulo para la gestión de trámites y expedientes legislativos.
        </p>
        <div className="bg-blue-50 border-l-4 border-blue-500 p-4">
          <p className="text-blue-700">
            <strong>Microservicio:</strong> Legislative Management Service (Puerto 5002)
          </p>
        </div>
      </div>
    </div>
  );
}

export default Legislative;
