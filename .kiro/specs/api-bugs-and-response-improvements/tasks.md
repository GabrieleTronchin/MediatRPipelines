# Implementation Plan

- [x] 1. Write bug condition exploration tests
  - **Property 1: Bug Condition** - API Bugs: Auth Randomness, Missing Persistence, Empty Notification Responses, Minimal Entity Response, and Missing Swagger Metadata
  - **CRITICAL**: These tests MUST FAIL on unfixed code — failure confirms the bugs exist
  - **DO NOT attempt to fix the tests or the code when they fail**
  - **NOTE**: These tests encode the expected behavior — they will validate the fixes when they pass after implementation
  - **GOAL**: Surface counterexamples that demonstrate each of the five bugs exists
  - **Test Project Setup**: Create `tests/MediatR.Playground.Tests/MediatR.Playground.Tests.csproj` as an xUnit project with references to `Microsoft.AspNetCore.Mvc.Testing`, `FsCheck.Xunit` (for property-based tests), and the API project. Add the test project to `src/MediatR.Playground.sln`
  - **Scoped PBT Approach**: For each bug, scope the property to the concrete failing condition from the design's `isBugCondition` pseudocode
  - **Bug 1 — Auth Randomness**: Test that `POST /Requests/SampleCommand` with a valid body succeeds deterministically. On unfixed code with no `FakeAuth:AlwaysAuthorize` config, send 20+ requests and assert ALL succeed — this will FAIL because ~50% throw random exceptions (`InvalidOperationException`, `ArgumentException`, etc.) from `AuthService.OperationAlowed()`. Counterexample: any request where `AuthResponse.IsSuccess == false`
  - **Bug 2 — Entity Not Persisted**: Test that creating an entity via `POST /TransactionRequests/AddSampleEntity` with `{ "description": "test" }` and then retrieving via `GET /TransactionRequests/SampleEntity/{id}` returns matching non-default values. On unfixed code this will FAIL — GET returns `{ "id": "00000000-...", "description": "", "registrationTime": "0001-01-01..." }` because `UnitOfWorkBehavior` never calls `connection.CommitAsync()` on the `IDbContextTransaction`
  - **Bug 3 — Notification Empty Responses**: Test that `POST /Notifications/SequentialNotification`, `POST /Notifications/ParallelNotification`, and `POST /Notifications/SamplePriorityNotification` each return a non-empty JSON body with `id` and `notificationTime` fields. On unfixed code this will FAIL — SequentialNotification returns a serialized `Task`, ParallelNotification and SamplePriorityNotification return empty bodies because `mediator.Publish()` is not awaited and no value is returned
  - **Bug 4 — Minimal Entity Response**: Test that `POST /TransactionRequests/AddSampleEntity` response includes `id`, `description`, and `eventTime` fields. On unfixed code this will FAIL — response only contains `{ "isSuccess": true }`
  - **Bug 5 — Missing Swagger Metadata**: Test that `GET /openapi/v1.json` (or the Swagger endpoint) returns an OpenAPI spec where every operation has a non-empty `summary` and `description`, and at least one `responses` entry with a schema. On unfixed code this will FAIL — endpoints only have `.WithName()` and `.WithTags()`
  - Run all tests on UNFIXED code
  - **EXPECTED OUTCOME**: Tests FAIL (this is correct — it proves the bugs exist)
  - Document counterexamples found to understand root causes
  - Mark task complete when tests are written, run, and failures are documented
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_

- [x] 2. Write preservation property tests (BEFORE implementing fixes)
  - **Property 2: Preservation** - Existing Endpoint Behavior Unchanged for Non-Bug-Condition Inputs
  - **IMPORTANT**: Follow observation-first methodology — run UNFIXED code, observe actual outputs, then write tests asserting those outputs
  - **Preservation A — Random Auth When Toggle Absent**: Observe that `AuthService.OperationAlowed()` produces both `true` and `false` results over many calls when no `FakeAuth:AlwaysAuthorize` config exists. Write a property-based test: for N calls (N >= 50), assert that NOT all results are `true` AND NOT all results are `false` (statistical property confirming randomness). Verify passes on unfixed code
  - **Preservation B — Empty Entity List**: Observe that `GET /TransactionRequests/SampleEntity` with no entities returns HTTP 200 with an empty JSON array `[]`. Write test asserting this. Verify passes on unfixed code
  - **Preservation C — Default Result for Missing Entity**: Observe that `GET /TransactionRequests/SampleEntity/{random-guid}` returns a default/empty entity result. Write test asserting this. Verify passes on unfixed code
  - **Preservation D — Exception Handling**: Observe that `POST /Exceptions/SampleCommandWithIOException` and `POST /Exceptions/SampleCommandWithException` return error responses (500 or problem details). Write tests asserting exception endpoints still produce error responses. Verify passes on unfixed code
  - **Preservation E — Stream Endpoints**: Observe that `GET /StreamRequests/SampleStreamEntity` and `GET /StreamRequests/SampleStreamEntityWithPipeFilter` return HTTP 200 responses. Write tests asserting stream endpoints respond successfully. Verify passes on unfixed code
  - **Preservation F — SampleRequest Processing**: Observe that `POST /Requests/SampleRequest` with a valid body processes successfully through pipelines (not affected by auth). Write test asserting this. Verify passes on unfixed code
  - **Preservation G — FluentValidation**: Observe that `POST /Requests/SampleCommand` with empty description triggers validation errors. Write test asserting validation still rejects invalid input. Verify passes on unfixed code
  - Run all preservation tests on UNFIXED code
  - **EXPECTED OUTCOME**: Tests PASS (this confirms baseline behavior to preserve)
  - Mark task complete when tests are written, run, and passing on unfixed code
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7_

- [-] 3. Fix Bug 1 — Auth Configuration Toggle

  - [x] 3.1 Add `FakeAuth:AlwaysAuthorize` configuration and update `AuthService`
    - Add `"FakeAuth": { "AlwaysAuthorize": true }` section to `src/MediatR.Playground.API/appsettings.json` and `appsettings.Development.json`
    - Inject `IConfiguration` into `AuthService` constructor in `src/FakeAuth.Service/AuthService.cs`
    - In `OperationAlowed()`, check `configuration.GetValue<bool>("FakeAuth:AlwaysAuthorize")` — if `true`, return `new AuthResponse { IsSuccess = true }` immediately; otherwise fall through to existing Bogus random logic
    - Update DI registration in `src/MediatR.Playground.Domain/ServiceExtension.cs` if needed (AuthService now needs IConfiguration)
    - _Bug_Condition: isBugCondition(input) where input.endpoint == "POST /Requests/SampleCommand" AND config["FakeAuth:AlwaysAuthorize"] IS NOT SET_
    - _Expected_Behavior: With AlwaysAuthorize=true, all SampleCommand requests succeed without exception_
    - _Preservation: With AlwaysAuthorize=false or absent, random auth behavior is preserved_
    - _Requirements: 2.1, 3.1_

  - [x] 3.2 Verify bug condition exploration test for auth now passes
    - **Property 1: Expected Behavior** - Auth Toggle Bypasses Random Authorization
    - **IMPORTANT**: Re-run the SAME auth test from task 1 with `FakeAuth:AlwaysAuthorize=true` in test configuration — do NOT write a new test
    - **EXPECTED OUTCOME**: Test PASSES (confirms auth toggle fix works)
    - _Requirements: 2.1_

  - [x] 3.3 Verify preservation tests for auth still pass
    - **Property 2: Preservation** - Random Auth Preserved When Toggle Disabled
    - **IMPORTANT**: Re-run the SAME preservation tests from task 2 — do NOT write new tests
    - **EXPECTED OUTCOME**: Tests PASS (confirms no regressions to random auth behavior)

- [x] 4. Fix Bug 2 — Entity Persistence Round-Trip

  - [x] 4.1 Fix the `UnitOfWorkBehavior` transaction flow
    - In `src/MediatR.Playground.Pipelines/TransactionCommand/UnitOfWorkBehavior.cs`, after `await _uow.Commit()` (which calls `SaveChangesAsync`), add `await connection.CommitAsync()` to commit the `IDbContextTransaction`
    - The current code calls `_uow.Commit()` but never commits the actual `IDbContextTransaction` — without `connection.CommitAsync()`, the transaction is implicitly rolled back on disposal in the `finally` block
    - Verify the `using var connection` pattern and `finally { connection.Dispose() }` do not interfere with the committed transaction
    - _Bug_Condition: isBugCondition(input) where POST AddSampleEntity FOLLOWED BY GET SampleEntity/{id} returns default values_
    - _Expected_Behavior: Entity round-trip returns matching non-default values_
    - _Preservation: Empty list for no entities, default result for missing entities unchanged_
    - _Requirements: 2.2, 3.2, 3.3_

  - [x] 4.2 Verify bug condition exploration test for persistence now passes
    - **Property 1: Expected Behavior** - Entity Persistence Round-Trip
    - **IMPORTANT**: Re-run the SAME persistence test from task 1 — do NOT write a new test
    - **EXPECTED OUTCOME**: Test PASSES (confirms entities are persisted and retrievable)
    - _Requirements: 2.2_

  - [x] 4.3 Verify preservation tests for entity queries still pass
    - **Property 2: Preservation** - Empty/Default Results for Missing Entities
    - **IMPORTANT**: Re-run the SAME preservation tests from task 2 — do NOT write new tests
    - **EXPECTED OUTCOME**: Tests PASS (confirms no regressions to empty list and default entity behavior)

- [x] 5. Fix Bug 3 — Notification Endpoint Responses

  - [x] 5.1 Fix notification endpoints to await and return confirmation objects
    - In `src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`:
    - **SequentialNotification**: Change lambda to `async`, `await mediator.Publish(...)`, and return a confirmation object `new { notification.Id, notification.NotificationTime, Type = "Sequential" }`
    - **ParallelNotification**: Change lambda to `async`, `await mediator.Publish(...)`, and return a confirmation object `new { notification.Id, notification.NotificationTime, Type = "Parallel" }`
    - **SamplePriorityNotification**: Change lambda to `async`, `await mediator.Publish(...)`, and return a confirmation object `new { notification.Id, notification.NotificationTime, Type = "Priority" }`
    - Store the notification in a local variable before publishing so the id and timestamp can be returned
    - _Bug_Condition: isBugCondition(input) where notification endpoints return empty body_
    - _Expected_Behavior: Each notification endpoint returns JSON with id, notificationTime, and type_
    - _Preservation: No other endpoints affected_
    - _Requirements: 2.3, 2.4, 2.5_

  - [x] 5.2 Verify bug condition exploration test for notifications now passes
    - **Property 1: Expected Behavior** - Notification Endpoints Return Confirmation
    - **IMPORTANT**: Re-run the SAME notification tests from task 1 — do NOT write new tests
    - **EXPECTED OUTCOME**: Tests PASS (confirms all three notification endpoints return JSON bodies)
    - _Requirements: 2.3, 2.4, 2.5_

  - [x] 5.3 Verify preservation tests still pass
    - **Property 2: Preservation** - Existing Endpoint Behavior Unchanged
    - **IMPORTANT**: Re-run the SAME preservation tests from task 2 — do NOT write new tests
    - **EXPECTED OUTCOME**: Tests PASS (confirms no regressions)

- [x] 6. Fix Bug 4 — Enrich AddSampleEntity Response

  - [x] 6.1 Enrich `AddSampleEntityCommandComplete` and update handler
    - In `src/MediatR.Playground.Model/TransactionCommand/AddSampleEntityCommand.cs`, add `Id` (Guid), `Description` (string), and `EventTime` (DateTime) properties to the `AddSampleEntityCommandComplete` record
    - In `src/MediatR.Playground.Domain/TransactionCommandHandler/SampleCommandHandler.cs`, update the `Handle` method to populate `Id = request.Id`, `Description = request.Description`, `EventTime = request.EventTime` in the returned `AddSampleEntityCommandComplete`
    - _Bug_Condition: isBugCondition(input) where POST AddSampleEntity response ONLY CONTAINS { isSuccess }_
    - _Expected_Behavior: Response includes id, description, eventTime in addition to isSuccess_
    - _Preservation: Existing isSuccess field unchanged_
    - _Requirements: 2.6_

  - [x] 6.2 Verify bug condition exploration test for entity response now passes
    - **Property 1: Expected Behavior** - AddSampleEntity Returns Full Entity Details
    - **IMPORTANT**: Re-run the SAME entity response test from task 1 — do NOT write a new test
    - **EXPECTED OUTCOME**: Test PASSES (confirms response includes id, description, eventTime)
    - _Requirements: 2.6_

  - [x] 6.3 Verify preservation tests still pass
    - **Property 2: Preservation** - Existing Endpoint Behavior Unchanged
    - **IMPORTANT**: Re-run the SAME preservation tests from task 2 — do NOT write new tests
    - **EXPECTED OUTCOME**: Tests PASS (confirms no regressions)

- [x] 7. Fix Bug 5 — Swagger/OpenAPI Documentation

  - [x] 7.1 Add OpenAPI metadata to all endpoint registrations
    - In `src/MediatR.Playground.API/Endpoints/RequestsAndCommandEndpoints.cs`: Add `.WithSummary()`, `.WithDescription()`, `.Produces<T>(StatusCodes.Status200OK)`, `.ProducesProblem(StatusCodes.Status400BadRequest)` to SampleCommand and SampleRequest endpoints
    - In `src/MediatR.Playground.API/Endpoints/TransactionEndpoints.cs`: Add `.WithSummary()`, `.WithDescription()`, `.Produces<T>(StatusCodes.Status200OK)`, `.Produces(StatusCodes.Status404NotFound)` to GET endpoints, and `.ProducesProblem(StatusCodes.Status500InternalServerError)` where appropriate
    - In `src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`: Add `.WithSummary()`, `.WithDescription()`, `.Produces<T>(StatusCodes.Status200OK)` to all three notification endpoints
    - In `src/MediatR.Playground.API/Endpoints/ExceptionsEndpoints.cs`: Add `.WithSummary()`, `.WithDescription()`, `.ProducesProblem(StatusCodes.Status500InternalServerError)` to exception endpoints
    - In `src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs`: Add `.WithSummary()`, `.WithDescription()` to stream endpoints (`.Produces<>()` already exists)
    - _Bug_Condition: isBugCondition(input) where Swagger spec LACKS summary OR description OR produces_
    - _Expected_Behavior: Every endpoint has summary, description, and response metadata_
    - _Preservation: Existing endpoint behavior and routing unchanged_
    - _Requirements: 2.7_

  - [x] 7.2 Verify bug condition exploration test for Swagger now passes
    - **Property 1: Expected Behavior** - Swagger Documentation Complete
    - **IMPORTANT**: Re-run the SAME Swagger test from task 1 — do NOT write a new test
    - **EXPECTED OUTCOME**: Test PASSES (confirms all endpoints have summary, description, and response metadata)
    - _Requirements: 2.7_

  - [x] 7.3 Verify preservation tests still pass
    - **Property 2: Preservation** - Existing Endpoint Behavior Unchanged
    - **IMPORTANT**: Re-run the SAME preservation tests from task 2 — do NOT write new tests
    - **EXPECTED OUTCOME**: Tests PASS (confirms no regressions)

- [~] 8. Checkpoint — Ensure all tests pass
  - Run the full test suite: `dotnet test tests/MediatR.Playground.Tests/MediatR.Playground.Tests.csproj`
  - Verify ALL bug condition exploration tests now PASS (bugs are fixed)
  - Verify ALL preservation tests still PASS (no regressions)
  - Build the solution: `dotnet build src/MediatR.Playground.sln`
  - Ensure no compilation errors or warnings
  - Ask the user if questions arise
