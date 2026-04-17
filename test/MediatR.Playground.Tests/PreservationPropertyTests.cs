using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FakeAuth.Service;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.Playground.Tests;

/// <summary>
/// Preservation Property Tests (Property 2).
///
/// These tests capture the EXISTING behavior of the unfixed code for non-bug-condition inputs.
/// They must PASS on unfixed code — confirming the baseline behavior that fixes must preserve.
///
/// **Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7**
/// </summary>
public class PreservationPropertyTests
    : IClassFixture<UnfixedWebApplicationFactory>,
      IClassFixture<PlaygroundWebApplicationFactory>
{
    private readonly UnfixedWebApplicationFactory _unfixedFactory;
    private readonly HttpClient _unfixedClient;
    private readonly HttpClient _fixedAuthClient;

    public PreservationPropertyTests(
        UnfixedWebApplicationFactory unfixedFactory,
        PlaygroundWebApplicationFactory fixedAuthFactory)
    {
        _unfixedFactory = unfixedFactory;
        _unfixedClient = unfixedFactory.CreateClient();
        _fixedAuthClient = fixedAuthFactory.CreateClient();
    }

    #region Preservation A — Random Auth When Toggle Absent

    /// <summary>
    /// Preservation A: When no FakeAuth:AlwaysAuthorize config exists,
    /// AuthService.OperationAlowed() produces both true and false results
    /// over many calls (statistical property confirming randomness).
    ///
    /// Property-based: for N calls (N >= 50), NOT all results are true
    /// AND NOT all results are false.
    ///
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Fact]
    public void PreservationA_AuthService_Produces_Random_Results_When_Toggle_Absent()
    {
        // Resolve the original AuthService from the unfixed factory's DI container
        using var scope = _unfixedFactory.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        const int callCount = 100;
        int trueCount = 0;
        int falseCount = 0;

        for (int i = 0; i < callCount; i++)
        {
            var response = authService.OperationAlowed();
            if (response.IsSuccess)
                trueCount++;
            else
                falseCount++;
        }

        // With random Bool() over 100 calls, the probability of all-true or all-false
        // is astronomically low (2^-100). This confirms randomness is preserved.
        Assert.True(
            trueCount > 0,
            $"All {callCount} calls returned IsSuccess=false — expected some true results. " +
            $"True: {trueCount}, False: {falseCount}");

        Assert.True(
            falseCount > 0,
            $"All {callCount} calls returned IsSuccess=true — expected some false results. " +
            $"True: {trueCount}, False: {falseCount}");
    }

    /// <summary>
    /// Preservation A (Statistical Property): Repeated batches of AuthService calls
    /// consistently produce a mix of true and false results, confirming the random
    /// behavior is stable and not a fluke.
    ///
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Fact]
    public void PreservationA_AuthService_Randomness_Is_Consistent_Across_Batches()
    {
        const int batchCount = 5;
        const int callsPerBatch = 50;

        for (int batch = 0; batch < batchCount; batch++)
        {
            using var scope = _unfixedFactory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            int trueCount = 0;
            int falseCount = 0;

            for (int i = 0; i < callsPerBatch; i++)
            {
                var response = authService.OperationAlowed();
                if (response.IsSuccess)
                    trueCount++;
                else
                    falseCount++;
            }

            Assert.True(
                trueCount > 0 && falseCount > 0,
                $"Batch {batch}: Expected mix of true/false over {callsPerBatch} calls, " +
                $"got {trueCount} true and {falseCount} false");
        }
    }

    #endregion

    #region Preservation B — Empty Entity List

    /// <summary>
    /// Preservation B: GET /TransactionRequests/SampleEntity with no entities
    /// returns HTTP 200 with an empty JSON array [].
    ///
    /// Uses a fresh, isolated WebApplicationFactory with a unique in-memory database
    /// to ensure no entities from other tests pollute the results.
    ///
    /// **Validates: Requirements 3.2**
    /// </summary>
    [Fact]
    public async Task PreservationB_GetAllEntities_Returns_Empty_Array_When_No_Entities()
    {
        // Create an isolated factory with a unique DB name so no other test data leaks in
        await using var isolatedFactory = new IsolatedDbWebApplicationFactory();
        var client = isolatedFactory.CreateClient();

        var response = await client.GetAsync("/TransactionRequests/SampleEntity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(0, json.GetArrayLength());
    }

    #endregion

    #region Preservation C — Default Result for Missing Entity

    /// <summary>
    /// Preservation C: GET /TransactionRequests/SampleEntity/{random-guid}
    /// returns HTTP 200 with a default/empty entity result
    /// (id = Guid.Empty, description = "", eventTime = default).
    ///
    /// Property-based: for multiple random GUIDs, the result is always the default entity.
    ///
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Fact]
    public async Task PreservationC_MissingEntity_Returns_Default_Result()
    {
        var client = _unfixedFactory.CreateClient();

        // Test with multiple random GUIDs to confirm the property holds broadly
        var randomGuids = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();

        foreach (var guid in randomGuids)
        {
            var response = await client.GetAsync($"/TransactionRequests/SampleEntity/{guid}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            var id = json.GetProperty("id").GetGuid();
            var description = json.GetProperty("description").GetString();
            var eventTime = json.GetProperty("eventTime").GetDateTime();

            Assert.Equal(Guid.Empty, id);
            Assert.True(
                string.IsNullOrEmpty(description),
                $"Expected empty description for missing GUID {guid}, got '{description}'");
            Assert.Equal(default(DateTime), eventTime);
        }
    }

    #endregion

    #region Preservation D — Exception Handling

    /// <summary>
    /// Preservation D: POST /Exceptions/SampleCommandWithIOException with deterministic auth
    /// returns an HTTP 200 response. The MediatR InvalidOperationExceptionHandler catches the
    /// exception and returns SampleCommandComplete with Id = Guid.Empty.
    ///
    /// Uses PlaygroundWebApplicationFactory (deterministic auth) to isolate exception handling.
    ///
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Fact]
    public async Task PreservationD_SampleCommandWithIOException_Returns_Handled_Response()
    {
        var response = await _fixedAuthClient.PostAsJsonAsync(
            "/Exceptions/SampleCommandWithIOException",
            new { Description = "test" });

        // The MediatR exception handler catches InvalidOperationException
        // and returns SampleCommandComplete { Id = Guid.Empty } with HTTP 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        var id = json.GetProperty("id").GetGuid();
        Assert.Equal(Guid.Empty, id);
    }

    /// <summary>
    /// Preservation D: POST /Exceptions/SampleCommandWithException with deterministic auth
    /// returns an HTTP 200 response. The MediatR ExceptionHandler catches the generic Exception
    /// and returns SampleCommandComplete with Id = Guid.Empty.
    ///
    /// Uses PlaygroundWebApplicationFactory (deterministic auth) to isolate exception handling.
    ///
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Fact]
    public async Task PreservationD_SampleCommandWithException_Returns_Handled_Response()
    {
        var response = await _fixedAuthClient.PostAsJsonAsync(
            "/Exceptions/SampleCommandWithException",
            new { Description = "test" });

        // The MediatR exception handler catches Exception
        // and returns SampleCommandComplete { Id = Guid.Empty } with HTTP 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        var id = json.GetProperty("id").GetGuid();
        Assert.Equal(Guid.Empty, id);
    }

    #endregion

    #region Preservation E — Stream Endpoints

    /// <summary>
    /// Preservation E: GET /StreamRequests/SampleStreamEntity returns HTTP 200.
    ///
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public async Task PreservationE_SampleStreamEntity_Returns_OK()
    {
        var response = await _unfixedClient.GetAsync("/StreamRequests/SampleStreamEntity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Preservation E: GET /StreamRequests/SampleStreamEntityWithPipeFilter returns HTTP 200.
    ///
    /// **Validates: Requirements 3.5**
    /// </summary>
    [Fact]
    public async Task PreservationE_SampleStreamEntityWithPipeFilter_Returns_OK()
    {
        var response = await _unfixedClient.GetAsync(
            "/StreamRequests/SampleStreamEntityWithPipeFilter");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Preservation F — SampleRequest Processing

    /// <summary>
    /// Preservation F: POST /Requests/SampleRequest with a valid body processes
    /// successfully through pipelines (not affected by auth since SampleRequest
    /// is IRequest, not ICommand) and returns HTTP 200 with a valid response.
    ///
    /// **Validates: Requirements 3.6**
    /// </summary>
    [Fact]
    public async Task PreservationF_SampleRequest_Processes_Successfully()
    {
        var response = await _unfixedClient.PostAsJsonAsync(
            "/Requests/SampleRequest",
            new { Description = "preservation test" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // SampleRequest returns SampleRequestComplete with the Id from the request
        var id = json.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
    }

    /// <summary>
    /// Preservation F (Property-Based): For various description strings,
    /// SampleRequest always processes successfully and returns a non-empty Id.
    /// This confirms the request pipeline is unaffected by auth changes.
    ///
    /// **Validates: Requirements 3.6**
    /// </summary>
    [Theory]
    [InlineData("test")]
    [InlineData("hello world")]
    [InlineData("sample data")]
    [InlineData("preservation check")]
    [InlineData("a")]
    public async Task PreservationF_SampleRequest_Always_Succeeds_With_Various_Descriptions(
        string description)
    {
        var client = _unfixedFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/Requests/SampleRequest",
            new { Description = description });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        var id = json.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
    }

    #endregion

    #region Preservation G — FluentValidation

    /// <summary>
    /// Preservation G: POST /Requests/SampleCommand with empty description
    /// triggers validation errors. With PlaygroundWebApplicationFactory (deterministic auth),
    /// the ValidationBehavior throws a ValidationException. The MediatR exception handler
    /// catches it and returns SampleCommandComplete with Id = Guid.Empty (HTTP 200),
    /// OR the exception propagates and ASP.NET returns 500.
    ///
    /// Either way, the command does NOT succeed with a valid Id — validation is enforced.
    ///
    /// Uses PlaygroundWebApplicationFactory to isolate validation behavior from random auth.
    ///
    /// **Validates: Requirements 3.7**
    /// </summary>
    [Fact]
    public async Task PreservationG_EmptyDescription_Triggers_Validation_Error()
    {
        var response = await _fixedAuthClient.PostAsJsonAsync(
            "/Requests/SampleCommand",
            new { Description = "" });

        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            // If HTTP 200, the MediatR exception handler caught the ValidationException
            // and returned SampleCommandComplete { Id = Guid.Empty }
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            var id = json.GetProperty("id").GetGuid();
            Assert.Equal(
                Guid.Empty,
                id);
        }
        else
        {
            // If not 200, it's an error response (400 or 500) — validation was enforced
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest
                || response.StatusCode == HttpStatusCode.InternalServerError,
                $"Expected 200 (handled), 400, or 500 for validation error, " +
                $"got {response.StatusCode}. Body: {content}");
        }
    }

    #endregion
}
