# Requirements Document

## Introduction

Upgrade the MediatR Playground project from MediatR 12.5.0 (last free Apache-2.0 version) to MediatR 14.1.0 (latest commercial version). This upgrade modernizes the NuGet package references across all projects, adopts the new behavior registration API (`AddOpenBehavior` / `AddOpenStreamBehavior`), introduces the MediatR.Contracts package separation for the Model project, adds a notification handler de-duplication demo, configures the MediatR license key via `IConfiguration`, and updates the project documentation to reflect all changes.

## Glossary

- **MediatR**: The mediator pattern library used throughout the project for request/response, notification, and stream handling.
- **MediatR.Contracts**: A lightweight NuGet package containing only the MediatR contract interfaces (`IRequest`, `INotification`, `IStreamRequest`) without the full mediator implementation.
- **ServiceExtension**: The static class in `MediatR.Playground.Domain` that registers MediatR and all pipeline behaviors into the DI container via the `AddMediatorSample` extension method.
- **Pipeline_Behavior**: An implementation of `IPipelineBehavior<TRequest, TResponse>` that intercepts the MediatR request/response pipeline.
- **Stream_Pipeline_Behavior**: An implementation of `IStreamPipelineBehavior<TRequest, TResponse>` that intercepts the MediatR stream request pipeline.
- **AddOpenBehavior**: A MediatR 14.1.0 configuration method that registers an open-generic pipeline behavior type inside the `AddMediatR` configuration block.
- **AddOpenStreamBehavior**: A MediatR 14.1.0 configuration method that registers an open-generic stream pipeline behavior type inside the `AddMediatR` configuration block.
- **License_Key**: A string value required by MediatR 14.1.0 to activate the library, read from application configuration at startup.
- **Notification_Deduplication**: A MediatR 14.1.0 feature that ensures each notification handler type executes only once per publish call, even if the handler is registered multiple times in the DI container.
- **DeduplicationNotification**: A new notification type created to demonstrate the Notification_Deduplication feature.
- **DeduplicationNotificationHandler**: A notification handler for DeduplicationNotification that tracks invocation count per notification ID using a thread-safe static counter.
- **NotificationEndpoint**: The endpoint class in `MediatR.Playground.API` that maps all notification-related HTTP routes.
- **Invocation_Counter**: A thread-safe static dictionary in DeduplicationNotificationHandler that records how many times the handler was invoked for each notification ID.

## Requirements

### Requirement 1: Upgrade MediatR NuGet Package in Domain Project

**User Story:** As a developer, I want the Domain project to reference MediatR 14.1.0, so that the project uses the latest mediator implementation with new features.

#### Acceptance Criteria

1. WHEN the solution is restored, THE Domain project SHALL resolve MediatR version 14.1.0 from its package reference.
2. WHEN the solution is built, THE Domain project SHALL compile without errors against MediatR 14.1.0.

### Requirement 2: Upgrade MediatR NuGet Package in Pipelines Project

**User Story:** As a developer, I want the Pipelines project to reference MediatR 14.1.0, so that pipeline behaviors compile against the latest MediatR API.

#### Acceptance Criteria

1. WHEN the solution is restored, THE Pipelines project SHALL resolve MediatR version 14.1.0 from its package reference.
2. WHEN the solution is built, THE Pipelines project SHALL compile without errors against MediatR 14.1.0.

### Requirement 3: Replace MediatR with MediatR.Contracts in Model Project

**User Story:** As a developer, I want the Model project to reference only MediatR.Contracts instead of the full MediatR package, so that the Model layer depends only on lightweight contract interfaces and demonstrates the contracts separation pattern.

#### Acceptance Criteria

1. WHEN the solution is restored, THE Model project SHALL resolve MediatR.Contracts as its MediatR-related package reference instead of the full MediatR package.
2. WHEN the solution is built, THE Model project SHALL compile without errors using only the interfaces provided by MediatR.Contracts.
3. THE Model project SHALL NOT have a package reference to the full MediatR package.

### Requirement 4: Configure MediatR License Key via IConfiguration

**User Story:** As a developer, I want the MediatR license key to be read from application configuration, so that the key is not hardcoded in source code and can be managed per environment.

#### Acceptance Criteria

1. THE `appsettings.json` file SHALL contain an empty placeholder for the MediatR license key.
2. THE `appsettings.Development.json` file SHALL contain the actual MediatR license key value.
3. WHEN MediatR is registered in the DI container, THE ServiceExtension SHALL read the license key from `IConfiguration` and pass it to the MediatR configuration.
4. WHEN `AddMediatorSample` is called, THE ServiceExtension SHALL accept an `IConfiguration` parameter in addition to `IServiceCollection`.
5. WHEN the API starts, THE Program.cs SHALL pass `builder.Configuration` to the `AddMediatorSample` method.

### Requirement 5: Modernize Pipeline Behavior Registration with AddOpenBehavior

**User Story:** As a developer, I want pipeline behaviors to be registered using `AddOpenBehavior` inside the MediatR configuration block, so that behavior registration is centralized and follows the MediatR 14.1.0 recommended pattern.

#### Acceptance Criteria

1. WHEN MediatR is configured, THE ServiceExtension SHALL register all `IPipelineBehavior<,>` types using `cfg.AddOpenBehavior(typeof(...))` inside the `AddMediatR` configuration lambda.
2. WHEN MediatR is configured, THE ServiceExtension SHALL register all `IStreamPipelineBehavior<,>` types using `cfg.AddOpenStreamBehavior(typeof(...))` inside the `AddMediatR` configuration lambda.
3. THE ServiceExtension SHALL NOT contain any `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` calls for pipeline behavior registration.
4. THE ServiceExtension SHALL NOT contain any `services.AddTransient(typeof(IStreamPipelineBehavior<,>), ...)` calls for stream pipeline behavior registration.
5. WHEN the solution is built and the API is started, THE pipeline behaviors SHALL execute in the same order as before the migration.

### Requirement 6: Create DeduplicationNotification Model

**User Story:** As a developer, I want a new DeduplicationNotification type in the Model project, so that the notification handler de-duplication feature of MediatR 14.1.0 can be demonstrated.

#### Acceptance Criteria

1. THE DeduplicationNotification SHALL be defined as a record in the `MediatR.Playground.Model.Notifications` namespace.
2. THE DeduplicationNotification SHALL implement `INotification`.
3. THE DeduplicationNotification SHALL have an `Id` property of type `Guid`.
4. THE DeduplicationNotification SHALL have a `NotificationTime` property of type `DateTime`.

### Requirement 7: Create DeduplicationNotificationHandler with Invocation Tracking

**User Story:** As a developer, I want a handler for DeduplicationNotification that tracks how many times it is invoked per notification ID, so that the de-duplication behavior can be observed and verified.

#### Acceptance Criteria

1. THE DeduplicationNotificationHandler SHALL be located in the `MediatR.Playground.Domain.NotificationHandler.Deduplication` namespace.
2. THE DeduplicationNotificationHandler SHALL implement `INotificationHandler<DeduplicationNotification>`.
3. WHEN the handler processes a DeduplicationNotification, THE DeduplicationNotificationHandler SHALL increment a thread-safe static Invocation_Counter keyed by the notification Id.
4. WHEN the handler processes a DeduplicationNotification, THE DeduplicationNotificationHandler SHALL log the handler name, notification Id, NotificationTime, and current invocation count for that Id.

### Requirement 8: Register DeduplicationNotificationHandler Twice in DI

**User Story:** As a developer, I want the DeduplicationNotificationHandler to be registered twice in the DI container, so that the demo simulates a common duplicate registration mistake and shows MediatR 14.1.0 de-duplication preventing double execution.

#### Acceptance Criteria

1. WHEN MediatR services are registered, THE ServiceExtension SHALL register `DeduplicationNotificationHandler` as a notification handler for `DeduplicationNotification` twice.
2. WHEN a DeduplicationNotification is published, THE DeduplicationNotificationHandler SHALL execute only once per publish call due to MediatR 14.1.0 Notification_Deduplication.

### Requirement 9: Add Deduplication Notification Endpoint

**User Story:** As a developer, I want a new HTTP endpoint to publish a DeduplicationNotification, so that the de-duplication feature can be triggered and observed via Swagger.

#### Acceptance Criteria

1. THE NotificationEndpoint SHALL expose a `POST /Notifications/DeduplicationNotification` route.
2. WHEN the endpoint is called, THE NotificationEndpoint SHALL create a DeduplicationNotification with a new `Guid` for Id and `DateTime.Now` for NotificationTime.
3. WHEN the endpoint is called, THE NotificationEndpoint SHALL publish the DeduplicationNotification via `IMediator.Publish`.
4. WHEN the endpoint completes, THE NotificationEndpoint SHALL return a JSON object containing the notification Id, NotificationTime, and Type set to "Deduplication".

### Requirement 10: Create MediatR 14 Migration Guide Documentation

**User Story:** As a developer, I want a migration guide document at `docs/upgrade-mediatr-14.md`, so that the upgrade steps and rationale are documented for reference.

#### Acceptance Criteria

1. THE migration guide SHALL be located at `docs/upgrade-mediatr-14.md`.
2. THE migration guide SHALL describe the package version changes for each project (Domain, Pipelines, Model).
3. THE migration guide SHALL describe the license key configuration approach.
4. THE migration guide SHALL describe the behavior registration migration from `AddTransient` to `AddOpenBehavior` and `AddOpenStreamBehavior`.
5. THE migration guide SHALL describe the MediatR.Contracts separation pattern and its benefits.
6. THE migration guide SHALL describe the notification handler de-duplication feature.

### Requirement 11: Update Notifications Documentation

**User Story:** As a developer, I want the notifications documentation to include the de-duplication feature, so that the documentation stays current with the codebase.

#### Acceptance Criteria

1. WHEN the notifications documentation is updated, THE `docs/notifications.md` file SHALL include a section describing the Notification_Deduplication feature.
2. THE notifications documentation SHALL list the DeduplicationNotification in the notification models table.
3. THE notifications documentation SHALL list the `POST /Notifications/DeduplicationNotification` endpoint in the API endpoints table.

### Requirement 12: Update Pipelines Documentation

**User Story:** As a developer, I want the pipelines documentation to reflect the new `AddOpenBehavior` registration pattern, so that the documentation matches the updated code.

#### Acceptance Criteria

1. WHEN the pipelines documentation is updated, THE `docs/pipelines.md` file SHALL show the `AddOpenBehavior` and `AddOpenStreamBehavior` registration pattern in the Registration Order section.
2. THE pipelines documentation SHALL NOT show the old `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` registration pattern as the current approach.
