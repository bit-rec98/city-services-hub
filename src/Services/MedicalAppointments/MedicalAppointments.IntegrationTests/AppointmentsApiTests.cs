using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CityServicesHub.IntegrationTests;

namespace MedicalAppointments.IntegrationTests;

/// <summary>
/// Pruebas HTTP de extremo a extremo de la API de turnos: autenticación JWT real, validación,
/// autorización por propietario y persistencia contra PostgreSQL (migraciones incluidas).
/// </summary>
[Trait("Category", "Integration")]
public class AppointmentsApiTests : IClassFixture<AppointmentsApiFixture>
{
    private const string Citizen = "Citizen";
    private const string Administrator = "Administrator";

    private readonly AppointmentsApiFixture _api;

    public AppointmentsApiTests(AppointmentsApiFixture api) => _api = api;

    // ---- helpers ----

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string path, string? token = null, object? body = null)
    {
        var request = new HttpRequestMessage(method, "/api/v1" + path);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await _api.Client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private async Task<(Guid DoctorId, Guid HealthCenterId)> NewDoctorAsync() =>
        await _api.SeedDoctorAndCenterAsync();

    private static object NewAppointment(
        Guid doctorId, Guid healthCenterId, string startTime = "10:00:00", Guid? patientId = null,
        string patientName = "Ana López", string? email = "ana@example.com", DateTime? date = null) => new
    {
        patientId,
        patientName,
        patientDocumentNumber = "12345678",
        patientEmail = email,
        doctorId,
        healthCenterId,
        specialty = 0,
        scheduledDate = (date ?? DateTime.Today.AddDays(5)).ToString("yyyy-MM-ddTHH:mm:ss"),
        startTime,
        attentionType = 0,
        reason = "Consulta"
    };

    private async Task<Guid> CreateAppointmentAsync(string token, Guid doctorId, Guid centerId, string startTime = "10:00:00")
    {
        var response = await SendAsync(HttpMethod.Post, "/appointments", token, NewAppointment(doctorId, centerId, startTime));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await ReadJsonAsync(response)).GetProperty("id").GetGuid();
    }

    // ---- authentication ----

    [Fact]
    public async Task Request_WithoutToken_IsUnauthorized()
    {
        var response = await SendAsync(HttpMethod.Get, $"/appointments/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithTamperedToken_IsUnauthorized()
    {
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);
        var tampered = token[..^4] + "AAAA";

        var response = await SendAsync(HttpMethod.Get, $"/appointments/{Guid.NewGuid()}", tampered);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---- validation ----

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequestWithMessage()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);

        var response = await SendAsync(HttpMethod.Post, "/appointments", token,
            NewAppointment(doctor, center, patientName: ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ReadJsonAsync(response)).GetProperty("message").GetString().Should().Contain("nombre");
    }

    [Fact]
    public async Task Create_WithPastDate_ReturnsBadRequest()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);

        var response = await SendAsync(HttpMethod.Post, "/appointments", token,
            NewAppointment(doctor, center, date: DateTime.Today.AddDays(-3)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithMalformedEmail_ReturnsBadRequest()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);

        var response = await SendAsync(HttpMethod.Post, "/appointments", token,
            NewAppointment(doctor, center, email: "not-an-email"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancel_WithoutReason_ReturnsBadRequest()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);
        var id = await CreateAppointmentAsync(token, doctor, center);

        var response = await SendAsync(HttpMethod.Post, $"/appointments/{id}/cancel", token, new { reason = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- creation ----

    [Fact]
    public async Task Create_TakesPatientFromTokenAndIgnoresBodyPatientId()
    {
        var (doctor, center) = await NewDoctorAsync();
        var alice = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();

        var response = await SendAsync(HttpMethod.Post, "/appointments", TestJwt.Create(alice, Citizen),
            NewAppointment(doctor, center, patientId: someoneElse));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await ReadJsonAsync(response)).GetProperty("patientId").GetGuid().Should().Be(alice);
    }

    [Fact]
    public async Task Create_AsAdministrator_CanCreateForAnotherPatient()
    {
        var (doctor, center) = await NewDoctorAsync();
        var patient = Guid.NewGuid();

        var response = await SendAsync(HttpMethod.Post, "/appointments", TestJwt.Create(Guid.NewGuid(), Administrator),
            NewAppointment(doctor, center, patientId: patient));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await ReadJsonAsync(response)).GetProperty("patientId").GetGuid().Should().Be(patient);
    }

    [Fact]
    public async Task Create_InAnAlreadyTakenSlot_ReturnsConflict()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);
        await CreateAppointmentAsync(token, doctor, center, "09:00:00");

        var response = await SendAsync(HttpMethod.Post, "/appointments", token, NewAppointment(doctor, center, "09:00:00"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithUnknownDoctor_ReturnsBadRequest()
    {
        var (_, center) = await NewDoctorAsync();

        var response = await SendAsync(HttpMethod.Post, "/appointments", TestJwt.Create(Guid.NewGuid(), Citizen),
            NewAppointment(Guid.NewGuid(), center));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---- ownership on read ----

    [Fact]
    public async Task Get_AsOwner_ReturnsAppointment()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);
        var id = await CreateAppointmentAsync(token, doctor, center);

        var response = await SendAsync(HttpMethod.Get, $"/appointments/{id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadJsonAsync(response)).GetProperty("status").GetString().Should().Be("Scheduled");
    }

    [Fact]
    public async Task Get_AsAnotherUser_ReturnsNotFound()
    {
        var (doctor, center) = await NewDoctorAsync();
        var id = await CreateAppointmentAsync(TestJwt.Create(Guid.NewGuid(), Citizen), doctor, center);

        var response = await SendAsync(HttpMethod.Get, $"/appointments/{id}", TestJwt.Create(Guid.NewGuid(), Citizen));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_AsAdministrator_ReturnsAppointment()
    {
        var (doctor, center) = await NewDoctorAsync();
        var id = await CreateAppointmentAsync(TestJwt.Create(Guid.NewGuid(), Citizen), doctor, center);

        var response = await SendAsync(HttpMethod.Get, $"/appointments/{id}", TestJwt.Create(Guid.NewGuid(), Administrator));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ---- listing ----

    [Fact]
    public async Task ListMine_ReturnsOnlyTheCallersAppointments()
    {
        var (doctor, center) = await NewDoctorAsync();
        var alice = Guid.NewGuid();
        var aliceToken = TestJwt.Create(alice, Citizen);
        var aliceAppointment = await CreateAppointmentAsync(aliceToken, doctor, center, "08:00:00");
        await CreateAppointmentAsync(TestJwt.Create(Guid.NewGuid(), Citizen), doctor, center, "08:30:00");

        var response = await SendAsync(HttpMethod.Get, "/appointments/patient/me", aliceToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = (await ReadJsonAsync(response)).EnumerateArray().ToList();
        items.Should().ContainSingle().Which.GetProperty("id").GetGuid().Should().Be(aliceAppointment);
    }

    [Fact]
    public async Task ListByPatient_ForSelf_IsOk()
    {
        var alice = Guid.NewGuid();

        var response = await SendAsync(HttpMethod.Get, $"/appointments/patient/{alice}", TestJwt.Create(alice, Citizen));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListByPatient_ForAnotherPatient_IsForbidden()
    {
        var response = await SendAsync(
            HttpMethod.Get, $"/appointments/patient/{Guid.NewGuid()}", TestJwt.Create(Guid.NewGuid(), Citizen));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListByPatient_AsAdministrator_IsOk()
    {
        var response = await SendAsync(
            HttpMethod.Get, $"/appointments/patient/{Guid.NewGuid()}", TestJwt.Create(Guid.NewGuid(), Administrator));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ---- confirm / cancel ----

    [Fact]
    public async Task Confirm_AsAnotherUser_ReturnsNotFoundAndLeavesAppointmentUntouched()
    {
        var (doctor, center) = await NewDoctorAsync();
        var ownerToken = TestJwt.Create(Guid.NewGuid(), Citizen);
        var id = await CreateAppointmentAsync(ownerToken, doctor, center);

        var response = await SendAsync(HttpMethod.Post, $"/appointments/{id}/confirm", TestJwt.Create(Guid.NewGuid(), Citizen));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var current = await ReadJsonAsync(await SendAsync(HttpMethod.Get, $"/appointments/{id}", ownerToken));
        current.GetProperty("status").GetString().Should().Be("Scheduled");
    }

    [Fact]
    public async Task Cancel_AsAnotherUser_ReturnsNotFoundAndLeavesAppointmentUntouched()
    {
        var (doctor, center) = await NewDoctorAsync();
        var ownerToken = TestJwt.Create(Guid.NewGuid(), Citizen);
        var id = await CreateAppointmentAsync(ownerToken, doctor, center);

        var response = await SendAsync(HttpMethod.Post, $"/appointments/{id}/cancel",
            TestJwt.Create(Guid.NewGuid(), Citizen), new { reason = "intruso" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var current = await ReadJsonAsync(await SendAsync(HttpMethod.Get, $"/appointments/{id}", ownerToken));
        current.GetProperty("status").GetString().Should().Be("Scheduled");
    }

    [Fact]
    public async Task OwnerCanConfirmThenCancel_AndStatusIsPersisted()
    {
        var (doctor, center) = await NewDoctorAsync();
        var token = TestJwt.Create(Guid.NewGuid(), Citizen);
        var id = await CreateAppointmentAsync(token, doctor, center);

        (await SendAsync(HttpMethod.Post, $"/appointments/{id}/confirm", token)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadJsonAsync(await SendAsync(HttpMethod.Get, $"/appointments/{id}", token)))
            .GetProperty("status").GetString().Should().Be("Confirmed");

        (await SendAsync(HttpMethod.Post, $"/appointments/{id}/cancel", token, new { reason = "ya no puedo" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var final = await ReadJsonAsync(await SendAsync(HttpMethod.Get, $"/appointments/{id}", token));
        final.GetProperty("status").GetString().Should().Be("Cancelled");
        final.GetProperty("cancellationReason").GetString().Should().Be("ya no puedo");
    }

    [Fact]
    public async Task HealthCenters_AreListedWithoutAuthentication()
    {
        await NewDoctorAsync();

        var response = await SendAsync(HttpMethod.Get, "/healthcenters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadJsonAsync(response)).GetArrayLength().Should().BeGreaterThan(0);
    }
}
