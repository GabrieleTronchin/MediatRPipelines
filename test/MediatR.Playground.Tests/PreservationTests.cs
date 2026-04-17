using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FakeAuth.Service;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediatR.Playground.Tests;

/// <summary>
/// Preservation Tests (Property 2).
///
/// These tests verify existing (unfixed) behavior is preserved.
/// They must PASS on the current unfixed code — they capture baseline behavior
/// that should remain unchanged after bug fixes are applied.
///
/// Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7
/// </summary>
public class PreservationTests : IClassFixture<UnfixedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PreservationTests(UnfixedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Preservation A — Random Auth When Toggle Absent

    /// <summary>
    /// **Validates: Requirements 3.1**
    ///
    /// Preservation A: When no FakeAuth:AlwaysAuthorize config exists,
    /// AuthService.OperationAlowed() produces both true and false results
    /// over many calls (statistical property confirming randomness).
    ///
    /// This is a unit-level test — directly instantiates AuthService with
    /// an empty configuration (no FakeAuth:AlwaysAuthorize key).
    /// </summary>
    [Fact]
    public void PreservationA_AuthService_Produces_Random_Results_When_No_Config()
    {
        var emptyConfig = new ConfigurationBuilder().Build();
        var authService = new AuthService(emptyConfig);
        const int callCount = 20;

        var results = Enumerable.Range(0, callCount)
            .Select(_ => authService.OperationAlowed().IsSuccess)
            .ToList();

        var allTrue = results.All(r => r);
        var allFalse = results.All(r => !r);

        // With random Bool(), the probability of all-true or all-false over 20 calls
        // is 2 * (0.5^20) ≈ 0.000002 — effectively impossible.
        Assert.False(
            allTrue,
            "All 20 auth calls returned true — expected random mix of true/false.");
        Assert.False(
            allFalse,
            "All 20 auth calls returned false — expected random mix of true/false.");
    }

    #endregion

    #region Preservation B — Empty Entity List

    /// <summary>
    /// **Validates: Requirements 3.2**
    ///
    /// Preservation B: GET /TransactionRequests/SampleEntity with no entities
    /// returns HTTP 200 with an empty JSON array [].
    /// </summary>
    [Fact]
    public async Task PreservationB_GetAllEntities_Returns_Empty_List()
    {
        var response = await _client.GetAsync("/TransactionRequests/SampleEntity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.Equal(0, json.GetArrayLength());
    }

    #endregion

    #region Preservation C — Default Result for Missing Entity

    /// <summary>
    /// **Validates: Requirements 3.3**
    ///
    /// Preservation C: GET /TransactionRequests/SampleEntity/{random-guid}
    /// returns a response with default/empty entity values.
    /// </summary>
    [Fact]
    public async Task PreservationC_GetEntityById_Returns_Default_For_Missing()
    {
        var randomId = Guid.NewGuid();
        var response = await _client.GetAsync($"/TransactionRequests/SampleEntity/{randomId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // Missing entity returns default values
        var id = json.GetProperty("id").GetGuid();
        var description = json.GetProperty("description").GetString();

        // The id will be Guid.Empty or the default — entity was not found
        // Description will be empty string (default)
        Assert.True(
            string.IsNullOrEmpty(description),
            $"Expected empty description for missing entity, got: '{description}'");
    }

    #endregion

    #region Preservation D — Exception Handling

    /// <summary>
    /// **Validates: Requirements 3.4**
    ///
    /// Preservation D: Exception endpoints return a response (not a crash).
    /// On unfixed code, SampleCommand goes through auth pipeline which may randomly deny,
    /// but MediatR's IRequestExceptionHandler catches all exceptions and returns
    /// SampleCommandComplete with Id = Guid.Empty. So the response is always HTTP 200.
    /// </summary>
    [Fact]
    public async Task PreservationD_IOException_Endpoint_Returns_Response()
    {
        var response = await _client.PostAsJsonAsync(
            "/Exceptions/SampleCommandWithIOException",
            new { Description = "test" });

        // The endpoint always returns HTTP 200 because MediatR exception handlers
        // catch the exception and return a handled response.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content), "Expected non-empty response body.");

        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.True(json.TryGetProperty("id", out _), "Response should contain 'id' property.");
    }

    [Fact]
    public async Task PreservationD_Exception_Endpoint_Returns_Response()
    {
        var response = await _client.PostAsJsonAsync(
            "/Exceptions/SampleCommandWithException",
            new { Description = "test" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content), "Expected non-empty response body.");

        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.True(json.TryGetProperty("id", out _), "Response should contain 'id' property.");
    }

    #endregion

    #region Preservation E — Stream Endpoints

    /// <summary>
    /// **Validates: Requirements 3.5**
    ///
    /// Preservation E: Stream endpoints return HTTP 200.
    /// </summary>
    [Fact]
    public async Task PreservationE_StreamEntity_Returns_OK()
    {
        var response = await _client.GetAsync("/StreamRequests/SampleStreamEntity");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PreservationE_StreamEntityWithPipeFilter_Returns_OK()
    {
        var response = await _client.GetAsync("/StreamRequests/SampleStreamEntityWithPipeFilter");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Preservation F — SampleRequest Processing

    /// <summary>
    /// **Validates: Requirements 3.6**
    ///
    /// Preservation F: POST /Requests/SampleRequest processes successfully.
    /// SampleRequest implements IRequest (not ICommand), so it bypasses the
    /// CommandAuthorizationBehavior entirely — no auth randomness.
    /// </summary>
    [Fact]
    public async Task PreservationF_SampleRequest_Processes_Successfully()
    {
        var response = await _client.PostAsJsonAsync(
            "/Requests/SampleRequest",
            new { Description = "preservation test" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // SampleRequest handler returns SampleRequestComplete with the request Id
        var id = json.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
    }

    #endregion

    #region Preservation G — FluentValidation

    /// <summary>
    /// **Validates: Requirements 3.7**
    ///
    /// Preservation G: POST /Requests/SampleCommand with empty description
    /// triggers a validation error. ValidationBehavior runs BEFORE
    /// CommandAuthorizationBehavior in the pipeline, so validation errors
    /// occur before auth randomness can interfere.
    ///
    /// The ValidationException is caught by MediatR's IRequestExceptionHandler
    /// (ExceptionHandler for SampleCommand), which returns SampleCommandComplete
    /// with Id = Guid.Empty and HTTP 200.
    /// </summary>
    [Fact]
    public async Task PreservationG_EmptyDescription_Returns_Error()
    {
        var response = await _client.PostAsJsonAsync(
            "/Requests/SampleCommand",
            new { Description = "" });

        // MediatR's ExceptionHandler catches the ValidationException and returns
        // a handled response with Id = Guid.Empty, so HTTP status is 200.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // The exception handler returns SampleCommandComplete with Id = Guid.Empty
        var id = json.GetProperty("id").GetGuid();
        Assert.Equal(Guid.Empty, id);
    }

    #endregion
}
