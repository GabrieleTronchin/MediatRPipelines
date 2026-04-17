# Testing

[← Back to README](../README.md)

This project includes a unit test suite for verifying pipeline behaviors, notification publishers, and validators, as well as tools for manual API testing.

## Running the Tests

Run all tests from the repository root:

```bash
dotnet test src/MediatR.Playground.sln
```

The test project is located at `test/MediatR.Playground.Tests/` and uses **xUnit** as the test framework, **NSubstitute** for mocking, and **FsCheck.Xunit** for property-based tests.

### Test Suite Overview

| Test Class | Component Under Test | Description |
|------------|---------------------|-------------|
| `BaselineSmokeTests` | Full API | Integration smoke tests that verify endpoints respond correctly with the full DI container |
| `ValidationBehaviorTests` | `ValidationBehavior` | Verifies FluentValidation integration — throws on invalid input, passes through on valid input |
| `CommandAuthorizationBehaviorTests` | `CommandAuthorizationBehavior` | Verifies authorization checks — throws when denied, passes through when allowed |
| `LoggingBehaviorTests` | `LoggingBehavior` | Verifies logging before/after handler execution and correct delegation |
| `CachingBehaviorTests` | `CachingBehavior` | Verifies cache miss delegation and cache key usage with FusionCache |
| `UnitOfWorkBehaviorTests` | `UnitOfWorkBehavior` | Verifies transaction commit on success and rollback on failure |
| `MultipleNotificationPublisherTests` | `MultipleNotificationPublisher` | Verifies routing to the correct publisher (sequential, parallel, priority) based on notification type |
| `PriorityNotificationPublisherTests` | `PriorityNotificationPublisher` | Verifies handlers execute in ascending priority order, including a property-based test |
| `SampleCommandValidatorTests` | `SampleCommandValidator` | Verifies FluentValidation rules for `SampleCommand` fields |

## HTTP File

The file `src/MediatR.Playground.API/MediatR.Playground.API.http` contains pre-configured HTTP requests for every API endpoint. You can use it with any editor that supports `.http` files (Visual Studio, VS Code with the REST Client extension, JetBrains Rider).

To use it:

1. Start the API:
   ```bash
   dotnet run --project src/MediatR.Playground.API
   ```
2. Open `src/MediatR.Playground.API/MediatR.Playground.API.http` in your editor.
3. Click the "Send Request" link above any request to execute it.

The file covers all endpoint groups: requests, transactions, notifications, exceptions, and stream requests.

## Stream Client Script

Standard HTTP clients wait for the full response before displaying anything. The PowerShell script `scripts/stream-client.ps1` connects to the streaming endpoints and prints each JSON element as it arrives from the server, so you can observe the real-time streaming behavior.

Make sure the API is running, then use the script:

```powershell
# Stream with the default logging pipeline
.\scripts\stream-client.ps1

# Stream with the authorization filter pipeline
.\scripts\stream-client.ps1 -Endpoint streamfilter
```

## See Also

- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
- [Unit of Work](./unit-of-work.md) — transaction management pipeline
- [Exception Handling](./exception-handling.md) — per-request exception handlers
- [Notifications](./notifications.md) — notification publishing strategies
