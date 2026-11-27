# AGENTS.md

Guidance for AI coding agents working in this repository. Humans can use it too. It is a pointer to the project's real sources, not a replacement for them: when this file and the code disagree, the code wins, so fix this file.

## What this is

Microservices platform for municipal services: .NET 8 services (Identity, Medical Appointments, Notifications) behind a YARP gateway, plus a React frontend. Documentation is written in Spanish; keep that language when editing `README.md`, `SUMMARY.md` and `docs/`. Code identifiers are in English; user-facing messages and comments are mostly Spanish. Commit messages use Conventional Commits in English (`fix(scope): ...`, `feat(...)`, `docs: ...`).

Start with `README.md`, then `SUMMARY.md` (verified status and known limitations) and `docs/architecture/decisions/` (ADRs). Treat documentation as context, not proof: verify against the code.

## Standards

Work follows the shared engineering standards (coding, security, documentation). Their location is machine-specific, so ask the user for the path if you cannot find them. Read the quick reference first, and the full section for any risky area (authentication, authorization, data access, infrastructure). MUST-level rules are violations; SHOULD-level gaps are suggestions.

Rules that matter most here:

- Never commit secrets. They come from environment variables (`.env`, git-ignored; template in `.env.example` with placeholders only).
- Authorization is enforced on the server, per resource. User identity comes from the validated token via `ICurrentUserService`, never from request bodies or routes.
- Validate external input at the boundary. Do not weaken or disable authentication, authorization, validation or security middleware to make a test pass.
- Do not claim something works without running it. Report what was verified and what was not.
- Keep changes scoped; no unrelated refactors. Update the affected docs in the same change.
- Ask before: pushing, opening or merging PRs, deleting branches, or starting the optional suggestions in `SUMMARY.md`.

## Commands

```bash
dotnet build CityServicesHub.sln
dotnet test CityServicesHub.sln                  # unit tests only; no database or Redis needed
dotnet tool restore                              # installs the pinned dotnet-ef (.config/dotnet-tools.json)

cd src/WebApp && npm ci && npm run lint && npm run build   # the frontend has no test suite
```

Services need configuration to run; see "Configuración" in `README.md`. At minimum `JwtSettings__SecretKey` (32+ bytes; a service refuses to start without it) and the service's `ConnectionStrings__...Db`.

Migrations (always pass the context; Medical Appointments also contains an unused duplicate `AppointmentsDbContext`):

```bash
dotnet tool run dotnet-ef migrations add <Name> \
  --project src/Services/<Service>/<Service>.Infrastructure --context <Service>DbContext --output-dir Migrations
```

## Layout and conventions

- `src/Services/<Service>/` is split into `.Domain`, `.Application` (MediatR commands/queries, validators, access rules), `.Infrastructure` (EF Core, repositories) and `.API` (thin controllers), plus `.Tests` (xUnit, FluentAssertions, Moq).
- Business and access rules live in `Application`/`Domain`, not in controllers. Handlers return `Result`/`Result<T>`; controllers map error codes to HTTP statuses.
- Shared building blocks are in `src/BuildingBlocks` (`Common`, `Logging`, `Security`, `EventBus`). JWT validation and CORS are registered only through `AddCityServicesJwtAuthentication` and `AddCityServicesCors`; do not reimplement them per service.
- Each service owns its PostgreSQL database and role. Do not connect as the superuser or share databases across services.
- `src/ApiGateway/ApiGateway/` (Ocelot) and `src/Shared/` are unused leftovers outside the solution. Several files under `MedicalAppointments.Application` are excluded with `Compile Remove` in the `.csproj`; they are dead code, not a template to follow.

## Pitfalls already hit in this codebase

- **jsonb collections:** Npgsql 8 needs `EnableDynamicJson()` on the data source to read `List<T>`/`Dictionary` columns. It is configured in each `DependencyInjection.cs`; keep it when changing DB setup.
- **Client-assigned Guid keys:** EF treats a new child entity with a preset key as existing and emits an UPDATE. Declare `ValueGeneratedNever()` for such keys and avoid calling `Update()` on an already-tracked aggregate.
- **`IsInEnum()`** only works on enum-typed properties; for an `int` property it rejects every value.
- **Calendar dates:** `Appointment.ScheduledDate` is a PostgreSQL `date`; do not store it as `timestamptz`.
- **Validation in Medical Appointments** runs through a MediatR behavior that returns a failed `Result` (`Validation.Failed`), not an exception.
- Environment variable names containing `-` (YARP cluster overrides) cannot be `export`ed in bash; use `env "NAME=value" command`. Do not use `pkill -f` with a service name: it can match your own shell command.
- Docker Compose interpolation fails closed on missing `.env` values; that is intended.

## Known gaps to keep in mind

Redis is required for Identity registration, and nothing publishes domain events; 2FA is not implemented (login for users with 2FA enabled is always rejected). The frontend still calls `/auth/refresh`, `/doctors` and `/appointments/available-slots`, which do not exist. The full list is in `SUMMARY.md`.
