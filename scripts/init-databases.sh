#!/bin/bash
# Crea una base de datos y un rol dedicado (mínimo privilegio) por microservicio.
# Se ejecuta una sola vez, al inicializar el volumen de PostgreSQL.
# Las contraseñas llegan por variables de entorno (ver .env.example); nunca se escriben aquí.

set -e
set -u

# create_service_database <base_de_datos> <rol> <contraseña>
create_service_database() {
    local database=$1
    local role=$2
    local password=$3

    echo "  Creating role '$role' and database '$database'"
    # La contraseña se pasa como variable de psql (:'service_password') para que se cite
    # como literal y no se concatene en el SQL.
    psql -v ON_ERROR_STOP=1 \
         -v service_password="$password" \
         --username "$POSTGRES_USER" --dbname postgres <<-EOSQL
        CREATE ROLE $role LOGIN PASSWORD :'service_password';
        CREATE DATABASE $database OWNER $role;
        REVOKE ALL ON DATABASE $database FROM PUBLIC;
EOSQL
}

create_service_database identity_db identity_user "$IDENTITY_DB_PASSWORD"
create_service_database medical_appointments_db appointments_user "$APPOINTMENTS_DB_PASSWORD"
create_service_database notifications_db notifications_user "$NOTIFICATIONS_DB_PASSWORD"

echo "Service databases created"
