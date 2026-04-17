# Implementation Plan: MediatR 14 Upgrade

## Overview

Upgrade the MediatR Playground from MediatR 12.5.0 to 14.1.0 in four phases: package upgrades, configuration and registration changes, new de-duplication demo code, and documentation updates. Each task builds incrementally so the solution compiles after every step.

## Tasks

- [x] 1. Upgrade NuGet package references
  - [x] 1.1 Bump MediatR to 14.1.0 in Domain project
    - In `src/MediatR.Playground.Domain/MediatR.Playground.Domain.csproj`, change `<PackageReference Include="MediatR" Version="12.5.0" />` to `Version="14.1.0"`
    - _Requirements: 1.1, 1.2_

  - [x] 1.2 Bump MediatR to 14.1.0 in Pipelines project
    - In `src/MediatR.Playground.Pipelines/MediatR.Playground.Pipelines.csproj`, change `<PackageReference Include="MediatR" Version="12.5.0" />` to `Version="14.1.0"`
    - _Requirements: 2.1, 2.2_

  - [x] 1.3 Replace MediatR with MediatR.Contracts 2.0.1 in Model project
    - In `src/MediatR.Playground.Model/MediatR.Playground.Model.csproj`, replace `<PackageReference Include="MediatR" Version="12.5.0" />` with `<PackageReference Include="MediatR.Contracts" Version="2.0.1" />`
    - Verify the Model project has no remaining reference to the full MediatR package
    - _Requirements: 3.1, 3.2, 3.3_

- [x] 2. Checkpoint — Restore and build
  - Run `dotnet restore src/MediatR.Playground.sln` and `dotnet build src/MediatR.Playground.sln`
  - Ensure all three projects compile without errors against the new package versions
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Configure MediatR license key
  - [x] 3.1 Add MediatR license key section to appsettings.json
    - In `src/MediatR.Playground.API/appsettings.json`, add a `"MediatR": { "LicenseKey": "" }` section with an empty placeholder value
    - _Requirements: 4.1_

  - [x] 3.2 Add MediatR license key to appsettings.Development.json
    - In `src/MediatR.Playground.API/appsettings.Development.json`, add a `"MediatR": { "LicenseKey": "<your-license-key>" }` section with a placeholder value
    - _Requirements: 4.2_

- [x] 4. Modernize ServiceExtension registration and wire license key
  - [x] 4.1 Update AddMediatorSample to accept IConfiguration and set license key
    - In `src/MediatR.Playground.Domain/ServiceExtension.cs`, change the `AddMediatorSample` signature to accept `IConfiguration configuration` as a second parameter
    - Add `using Microsoft.Extensions.Configuration;` import
    - Inside the `AddMediatR` lambda, set `cfg.LicenseKey = configuration["MediatR:LicenseKey"];`
    - _Requirements: 4.3, 4.4_

  - [x] 4.2 Replace AddTransient behavior registrations with AddOpenBehavior / AddOpenStreamBehavior
    - Move all six `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(...))` calls into the `AddMediatR` lambda as `cfg.AddOpenBehavior(typeof(...))` calls, preserving the existing registration order:
      1. `cfg.AddOpenBehavior(typeof(CachingBehavior<,>))`
      2. `cfg.AddOpenBehavior(typeof(LoggingBehavior<,>))`
      3. `cfg.AddOpenBehavior(typeof(ValidationBehavior<,>))`
      4. `cfg.AddOpenBehavior(typeof(GlobalExceptionHandlingBehavior<,>))`
      5. `cfg.AddOpenBehavior(typeof(CommandAuthorizationBehavior<,>))`
      6. `cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>))`
    - Move both `services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(...))` calls into the lambda as `cfg.AddOpenStreamBehavior(typeof(...))` calls:
      1. `cfg.AddOpenStreamBehavior(typeof(GenericStreamLoggingBehavior<,>))`
      2. `cfg.AddOpenStreamBehavior(typeof(SampleFilterStreamBehavior<,>))`
    - Remove all old `services.AddTransient` calls for pipeline and stream pipeline behaviors
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [x] 4.3 Update Program.cs to pass builder.Configuration
    - In `src/MediatR.Playground.API/Program.cs`, change `builder.Services.AddMediatorSample()` to `builder.Services.AddMediatorSample(builder.Configuration)`
    - _Requirements: 4.5_

- [x] 5. Checkpoint — Build and verify registration changes
  - Run `dotnet build src/MediatR.Playground.sln` and confirm the solution compiles
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Create DeduplicationNotification model
  - [x] 6.1 Create DeduplicationNotification record
    - Create file `src/MediatR.Playground.Model/Notifications/DeduplicationNotification.cs`
    - Define `DeduplicationNotification` as a `record` implementing `INotification` in the `MediatR.Playground.Model.Notifications` namespace
    - Add `Guid Id { get; set; }` and `DateTime NotificationTime { get; set; }` properties
    - Follow the same pattern as `SampleNotification`
    - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [x] 7. Create DeduplicationNotificationHandler with invocation tracking
  - [x] 7.1 Create DeduplicationNotificationHandler class
    - Create directory and file `src/MediatR.Playground.Domain/NotificationHandler/Deduplication/DeduplicationNotificationHandler.cs`
    - Define `internal class DeduplicationNotificationHandler` implementing `INotificationHandler<DeduplicationNotification>` in the `MediatR.Playground.Domain.NotificationHandler.Deduplication` namespace
    - Use primary constructor with `ILogger<DeduplicationNotificationHandler> logger`
    - Add a `private static readonly ConcurrentDictionary<Guid, int> InvocationCounter = new()` field
    - In the `Handle` method, use `InvocationCounter.AddOrUpdate(notification.Id, 1, (_, c) => c + 1)` to increment the count
    - Log the handler name, notification Id, NotificationTime, and current invocation count
    - _Requirements: 7.1, 7.2, 7.3, 7.4_

  - [x] 7.2 Register DeduplicationNotificationHandler twice in ServiceExtension
    - In `src/MediatR.Playground.Domain/ServiceExtension.cs`, after the `AddMediatR` block, add two `services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>()` calls
    - Add the necessary `using` statements for the Deduplication namespace and `DeduplicationNotification`
    - _Requirements: 8.1, 8.2_

- [x] 8. Add DeduplicationNotification endpoint
  - [x] 8.1 Add POST /Notifications/DeduplicationNotification route
    - In `src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`, add a new `MapPost` for `/DeduplicationNotification` to the existing notification group
    - The endpoint creates a `DeduplicationNotification` with `Id = Guid.NewGuid()` and `NotificationTime = DateTime.Now`
    - Publishes via `mediator.Publish(notification, cancellationToken)`
    - Returns `new { notification.Id, notification.NotificationTime, Type = "Deduplication" }`
    - Add `.WithName("DeduplicationNotification")`, `.WithSummary(...)`, `.WithDescription(...)`, and `.Produces(StatusCodes.Status200OK, typeof(object))`
    - Add `using MediatR.Playground.Model.Notifications;` if not already present
    - _Requirements: 9.1, 9.2, 9.3, 9.4_

- [x] 9. Checkpoint — Build and verify new components
  - Run `dotnet build src/MediatR.Playground.sln` and confirm the solution compiles with all new code
  - Ensure all tests pass, ask the user if questions arise.

- [x] 10. Create migration guide documentation
  - [x] 10.1 Create docs/upgrade-mediatr-14.md
    - Create `docs/upgrade-mediatr-14.md` with sections covering:
      - Package version changes for Domain (12.5.0 → 14.1.0), Pipelines (12.5.0 → 14.1.0), and Model (MediatR 12.5.0 → MediatR.Contracts 2.0.1)
      - License key configuration approach (appsettings.json placeholder, appsettings.Development.json actual key, IConfiguration injection)
      - Behavior registration migration from `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` to `cfg.AddOpenBehavior(typeof(...))` and `cfg.AddOpenStreamBehavior(typeof(...))`
      - MediatR.Contracts separation pattern and its benefits (lightweight Model project dependency)
      - Notification handler de-duplication feature and how it prevents duplicate handler execution
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_

- [x] 11. Update existing documentation
  - [x] 11.1 Update docs/notifications.md with de-duplication section
    - Add a "Notification Handler De-duplication" section explaining MediatR 14's built-in de-duplication feature
    - Add `DeduplicationNotification` to the Notification Models table with `INotification` implements and `Sequential` strategy
    - Add `POST /Notifications/DeduplicationNotification` to the API Endpoints table with `DeduplicationNotification` type and `Sequential` strategy
    - _Requirements: 11.1, 11.2, 11.3_

  - [x] 11.2 Update docs/pipelines.md with AddOpenBehavior pattern
    - Update the Registration Order section to show the `cfg.AddOpenBehavior(typeof(...))` and `cfg.AddOpenStreamBehavior(typeof(...))` pattern as the current approach
    - Note the old `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` pattern as the previous approach (pre-MediatR 14)
    - _Requirements: 12.1, 12.2_

- [x] 12. Final checkpoint — Full build verification
  - Run `dotnet build src/MediatR.Playground.sln` and confirm the entire solution compiles cleanly
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks are ordered: package upgrades → configuration → new code → documentation
- Each task references specific acceptance criteria from the requirements document
- Checkpoints ensure incremental validation after each major phase
- The design explicitly states property-based testing does not apply to this feature (package upgrade and DI wiring migration)
- Pipeline behavior registration order is preserved exactly to maintain existing execution order (Requirement 5.5)
- The DeduplicationNotificationHandler is intentionally registered twice to demonstrate MediatR 14's de-duplication feature
