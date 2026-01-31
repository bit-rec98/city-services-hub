FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy shared projects
COPY ["src/Shared/Common/CityServices.Shared.Common/CityServices.Shared.Common.csproj", "Shared/Common/"]
COPY ["src/Shared/Events/CityServices.Shared.Events/CityServices.Shared.Events.csproj", "Shared/Events/"]

# Copy service projects
COPY ["src/Services/MedicalAppointments/MedicalAppointments.Domain/MedicalAppointments.Domain.csproj", "Services/MedicalAppointments/Domain/"]
COPY ["src/Services/MedicalAppointments/MedicalAppointments.Application/MedicalAppointments.Application.csproj", "Services/MedicalAppointments/Application/"]
COPY ["src/Services/MedicalAppointments/MedicalAppointments.Infrastructure/MedicalAppointments.Infrastructure.csproj", "Services/MedicalAppointments/Infrastructure/"]
COPY ["src/Services/MedicalAppointments/MedicalAppointments.API/MedicalAppointments.API.csproj", "Services/MedicalAppointments/API/"]

# Restore dependencies
RUN dotnet restore "Services/MedicalAppointments/API/MedicalAppointments.API.csproj"

# Copy everything else
COPY src/Shared/Common/ Shared/Common/
COPY src/Shared/Events/ Shared/Events/
COPY src/Services/MedicalAppointments/ Services/MedicalAppointments/

# Build
WORKDIR "/src/Services/MedicalAppointments/API"
RUN dotnet build "MedicalAppointments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MedicalAppointments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MedicalAppointments.API.dll"]
