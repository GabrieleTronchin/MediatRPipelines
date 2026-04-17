using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace MediatR.Playground.Tests;

/// <summary>
/// Bug Condition Exploration Tests (Property 1).
///
/// These tests encode the EXPECTED (correct) behavior for each of the five bugs.
/// On UNFIXED code they are expected to FAIL — failure confirms the bugs exist.
/// After the fixes are applied, these same tests should PASS.
///
/// DO NOT modify these tests to make them pass on unfixed code.
/// </summary>
public class BugConditionExplorationTests : IClassFixture<UnfixedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BugConditionExplorationTests(UnfixedWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Bug 1 — Auth Randomness

    /// <summary>
    /// Bug 1: POST /Requests/SampleCommand with a valid body should succeed deterministically.
    ///
    /// On unfixed code, AuthService.OperationAlowed() uses Bogus to randomly return
    /// IsSuccess = true/false (~50% failure rate). The CommandAuthorizationBehavior
    /// throws random exceptions when IsSuccess is false. MediatR's exception handlers
    /// catch these and return SampleCommandComplete with Id = Guid.Empty.
    ///
    /// This test sends 30 requests and asserts ALL return a non-empty Id (not Guid.Empty).
    /// On unfixed code, roughly half will return Guid.Empty due to random auth denial.
    ///
    /// EXPECTED: FAIL on unfixed code (counterexample: any request returning Id = Guid.Empty)
    /// </summary>
    [Fact]
    public async Task Bug1_Auth_SampleCommand_Should_Succeed_Deterministically()
    {
        const int requestCount = 30;
        var failedRequests = new List<(int index, Guid id)>();

        for (int i = 0; i < requestCount; i++)
        {
            var response = await _client.PostAsJsonAsync(
                "/Requests/SampleCommand",
                new { Description = $"Auth test request {i}" });

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var id = json.GetProperty("id").GetGuid();

            if (id == Guid.Empty)
            {
                failedRequests.Add((i, id));
            }
        }

        // All requests should return a valid (non-empty) Id.
        // On unfixed code, ~50% will return Guid.Empty because the exception handler
        // sets Id = Guid.Empty when auth randomly fails.
        Assert.True(
            failedRequests.Count == 0,
            $"Auth randomness bug detected: {failedRequests.Count}/{requestCount} requests " +
            $"returned Guid.Empty due to random auth denial. " +
            $"Counterexample indices: [{string.Join(", ", failedRequests.Select(f => f.index))}]");
    }

    #endregion

    #region Bug 2 — Entity Not Persisted

    /// <summary>
    /// Bug 2: Creating an entity via POST /TransactionRequests/AddSampleEntity and then
    /// retrieving it via GET /TransactionRequests/SampleEntity/{id} should return matching
    /// non-default values.
    ///
    /// On unfixed code, UnitOfWorkBehavior calls _uow.Commit() (which calls SaveChangesAsync)
    /// but never calls connection.CommitAsync() on the IDbContextTransaction. The transaction
    /// is implicitly rolled back on disposal, so the entity is never persisted.
    ///
    /// EXPECTED: FAIL on unfixed code (GET returns default values: empty guid, empty description,
    /// DateTime.MinValue)
    /// </summary>
    [Fact]
    public async Task Bug2_Entity_Should_Be_Persisted_And_Retrievable()
    {
        var description = $"Persistence test {Guid.NewGuid():N}";

        // Create entity
        var createResponse = await _client.PostAsJsonAsync(
            "/TransactionRequests/AddSampleEntity",
            new { Description = description });

        createResponse.EnsureSuccessStatusCode();

        var createJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();

        // The response should contain an id field (Bug 4 also relates to this,
        // but here we focus on persistence). We try to extract id if present,
        // otherwise we check the entity list.
        Guid entityId;
        if (createJson.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
        {
            entityId = idProp.GetGuid();
        }
        else
        {
            // If id is not in the response (Bug 4), retrieve all entities and find ours
            var allResponse = await _client.GetAsync("/TransactionRequests/SampleEntity");
            allResponse.EnsureSuccessStatusCode();
            var allJson = await allResponse.Content.ReadFromJsonAsync<JsonElement>();

            // Find entity with matching description
            var matchingEntity = allJson.EnumerateArray()
                .FirstOrDefault(e =>
                    e.TryGetProperty("description", out var desc) &&
                    desc.GetString() == description);

            Assert.True(
                matchingEntity.ValueKind != JsonValueKind.Undefined,
                $"Entity with description '{description}' not found in entity list. " +
                "Persistence bug: entity was not saved to the database.");

            entityId = matchingEntity.GetProperty("id").GetGuid();
        }

        Assert.NotEqual(Guid.Empty, entityId);

        // Retrieve entity by id
        var getResponse = await _client.GetAsync($"/TransactionRequests/SampleEntity/{entityId}");
        getResponse.EnsureSuccessStatusCode();

        var getJson = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var retrievedId = getJson.GetProperty("id").GetGuid();
        var retrievedDescription = getJson.GetProperty("description").GetString();
        var retrievedEventTime = getJson.GetProperty("eventTime").GetDateTime();

        // Verify non-default values
        Assert.NotEqual(Guid.Empty, retrievedId);
        Assert.False(
            string.IsNullOrEmpty(retrievedDescription),
            "Retrieved entity has empty description — persistence bug: entity data was not saved.");
        Assert.NotEqual(
            default(DateTime),
            retrievedEventTime);

        // Verify values match what was created
        Assert.Equal(entityId, retrievedId);
        Assert.Equal(description, retrievedDescription);
    }

    #endregion

    #region Bug 3 — Notification Empty Responses

    /// <summary>
    /// Bug 3a: POST /Notifications/SequentialNotification should return a non-empty JSON body
    /// with id and notificationTime fields.
    ///
    /// On unfixed code, the endpoint returns mediator.Publish() directly, which returns Task.
    /// The serialized Task object or empty body is returned instead of a confirmation.
    ///
    /// EXPECTED: FAIL on unfixed code (empty body or serialized Task)
    /// </summary>
    [Fact]
    public async Task Bug3a_SequentialNotification_Should_Return_Confirmation_Body()
    {
        var response = await _client.PostAsync("/Notifications/SequentialNotification", null);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(content),
            "SequentialNotification returned empty body. " +
            "Bug: mediator.Publish() returns Task which serializes to empty/null.");

        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(
            json.TryGetProperty("id", out _),
            $"SequentialNotification response missing 'id' field. Actual response: {content}");
        Assert.True(
            json.TryGetProperty("notificationTime", out _),
            $"SequentialNotification response missing 'notificationTime' field. Actual response: {content}");
    }

    /// <summary>
    /// Bug 3b: POST /Notifications/ParallelNotification should return a non-empty JSON body.
    ///
    /// On unfixed code, mediator.Publish() is assigned to a local variable but never awaited
    /// and no value is returned — the lambda returns void, producing an empty HTTP response.
    ///
    /// EXPECTED: FAIL on unfixed code (empty body)
    /// </summary>
    [Fact]
    public async Task Bug3b_ParallelNotification_Should_Return_Confirmation_Body()
    {
        var response = await _client.PostAsync("/Notifications/ParallelNotification", null);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(content),
            "ParallelNotification returned empty body. " +
            "Bug: mediator.Publish() is not awaited and no value is returned.");

        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(
            json.TryGetProperty("id", out _),
            $"ParallelNotification response missing 'id' field. Actual response: {content}");
        Assert.True(
            json.TryGetProperty("notificationTime", out _),
            $"ParallelNotification response missing 'notificationTime' field. Actual response: {content}");
    }

    /// <summary>
    /// Bug 3c: POST /Notifications/SamplePriorityNotification should return a non-empty JSON body.
    ///
    /// On unfixed code, same issue as ParallelNotification — mediator.Publish() is not awaited
    /// and no value is returned.
    ///
    /// EXPECTED: FAIL on unfixed code (empty body)
    /// </summary>
    [Fact]
    public async Task Bug3c_PriorityNotification_Should_Return_Confirmation_Body()
    {
        var response = await _client.PostAsync("/Notifications/SamplePriorityNotification", null);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(content),
            "SamplePriorityNotification returned empty body. " +
            "Bug: mediator.Publish() is not awaited and no value is returned.");

        var json = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(
            json.TryGetProperty("id", out _),
            $"SamplePriorityNotification response missing 'id' field. Actual response: {content}");
        Assert.True(
            json.TryGetProperty("notificationTime", out _),
            $"SamplePriorityNotification response missing 'notificationTime' field. Actual response: {content}");
    }

    #endregion

    #region Bug 4 — Minimal Entity Response

    /// <summary>
    /// Bug 4: POST /TransactionRequests/AddSampleEntity response should include id, description,
    /// and eventTime fields in addition to isSuccess.
    ///
    /// On unfixed code, AddSampleEntityCommandComplete only has IsSuccess property, so the
    /// response is just { "isSuccess": true } — missing entity details.
    ///
    /// EXPECTED: FAIL on unfixed code (response only contains isSuccess)
    /// </summary>
    [Fact]
    public async Task Bug4_AddSampleEntity_Response_Should_Include_Full_Entity_Details()
    {
        var description = "Entity response test";

        var response = await _client.PostAsJsonAsync(
            "/TransactionRequests/AddSampleEntity",
            new { Description = description });

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // Verify the response contains entity details beyond just isSuccess
        Assert.True(
            json.TryGetProperty("id", out var idProp),
            $"AddSampleEntity response missing 'id' field. Actual response: {content}");

        Assert.True(
            json.TryGetProperty("description", out var descProp),
            $"AddSampleEntity response missing 'description' field. Actual response: {content}");

        Assert.True(
            json.TryGetProperty("eventTime", out var eventTimeProp),
            $"AddSampleEntity response missing 'eventTime' field. Actual response: {content}");

        // Verify values are meaningful
        var id = idProp.GetGuid();
        Assert.NotEqual(Guid.Empty, id);

        var desc = descProp.GetString();
        Assert.Equal(description, desc);
    }

    #endregion

    #region Bug 5 — Missing Swagger Metadata

    /// <summary>
    /// Bug 5: The OpenAPI spec should have summary and description for every operation,
    /// and at least one response entry with a schema.
    ///
    /// On unfixed code, endpoints only use .WithName() and .WithTags() — no .WithSummary(),
    /// .WithDescription(), or .Produces&lt;T&gt;() calls.
    ///
    /// EXPECTED: FAIL on unfixed code (operations lack summary/description/response schemas)
    /// </summary>
    [Fact]
    public async Task Bug5_OpenApi_Spec_Should_Have_Complete_Metadata()
    {
        // Try the OpenAPI endpoint — ASP.NET Core 10 uses /openapi/v1.json by default
        var response = await _client.GetAsync("/openapi/v1.json");

        if (!response.IsSuccessStatusCode)
        {
            // Fall back to Swashbuckle endpoint
            response = await _client.GetAsync("/swagger/v1/swagger.json");
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var spec = JsonSerializer.Deserialize<JsonElement>(content);

        Assert.True(
            spec.TryGetProperty("paths", out var paths),
            "OpenAPI spec missing 'paths' property.");

        var operationsWithoutSummary = new List<string>();
        var operationsWithoutDescription = new List<string>();
        var operationsWithoutResponseSchema = new List<string>();

        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                var operationId = $"{method.Name.ToUpper()} {path.Name}";
                var operation = method.Value;

                // Check for summary
                if (!operation.TryGetProperty("summary", out var summary) ||
                    string.IsNullOrWhiteSpace(summary.GetString()))
                {
                    operationsWithoutSummary.Add(operationId);
                }

                // Check for description
                if (!operation.TryGetProperty("description", out var desc) ||
                    string.IsNullOrWhiteSpace(desc.GetString()))
                {
                    operationsWithoutDescription.Add(operationId);
                }

                // Check for at least one response with a schema
                if (operation.TryGetProperty("responses", out var responses))
                {
                    bool hasSchema = false;
                    foreach (var resp in responses.EnumerateObject())
                    {
                        if (resp.Value.TryGetProperty("content", out var respContent))
                        {
                            foreach (var mediaType in respContent.EnumerateObject())
                            {
                                if (mediaType.Value.TryGetProperty("schema", out _))
                                {
                                    hasSchema = true;
                                    break;
                                }
                            }
                        }
                        if (hasSchema) break;
                    }

                    if (!hasSchema)
                    {
                        operationsWithoutResponseSchema.Add(operationId);
                    }
                }
                else
                {
                    operationsWithoutResponseSchema.Add(operationId);
                }
            }
        }

        // Assert all operations have complete metadata
        Assert.True(
            operationsWithoutSummary.Count == 0,
            $"Operations missing summary: [{string.Join(", ", operationsWithoutSummary)}]");

        Assert.True(
            operationsWithoutDescription.Count == 0,
            $"Operations missing description: [{string.Join(", ", operationsWithoutDescription)}]");

        Assert.True(
            operationsWithoutResponseSchema.Count == 0,
            $"Operations missing response schema: [{string.Join(", ", operationsWithoutResponseSchema)}]");
    }

    #endregion
}
