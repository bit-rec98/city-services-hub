-- Create databases for each microservice
CREATE DATABASE medical_appointments_db;
CREATE DATABASE legislative_db;
CREATE DATABASE hr_db;
CREATE DATABASE tax_services_db;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE medical_appointments_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE legislative_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE hr_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE tax_services_db TO postgres;
