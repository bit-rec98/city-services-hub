# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/Services/MedicalAppointments/MedicalAppointments.API/MedicalAppointments.API.csproj", "src/Services/MedicalAppointments/MedicalAppointments.API/"]
COPY ["src/Services/MedicalAppointments/MedicalAppointments.Application/MedicalAppointments.Application.csproj", "src/Services/MedicalAppointments/MedicalAppointments.Application/"]
COPY ["src/Services/MedicalAppointments/MedicalAppointments.Domain/MedicalAppointments.Domain.csproj", "src/Services/MedicalAppointments/MedicalAppointments.Domain/"]
COPY ["src/Services/MedicalAppointments/MedicalAppointments.Infrastructure/MedicalAppointments.Infrastructure.csproj", "src/Services/MedicalAppointments/MedicalAppointments.Infrastructure/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/Logging/Logging.csproj", "src/BuildingBlocks/Logging/"]

# Restaurar dependencias
RUN dotnet restore "src/Services/MedicalAppointments/MedicalAppointments.API/MedicalAppointments.API.csproj"

# Copiar código fuente
COPY . .

# Build
WORKDIR "/src/src/Services/MedicalAppointments/MedicalAppointments.API"
RUN dotnet build "MedicalAppointments.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MedicalAppointments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "MedicalAppointments.API.dll"]
