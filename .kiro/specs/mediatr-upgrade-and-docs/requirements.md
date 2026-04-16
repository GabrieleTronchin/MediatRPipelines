# Requirements Document — MediatR Upgrade and Documentation

## Introduction

This document defines the requirements for upgrading the MediatR Playground repository to .NET 10 (LTS), upgrading MediatR to the latest free (Apache-2.0) version, updating all NuGet packages to their latest stable versions, and improving the project documentation. Documentation will be organized in a `docs/` folder with dedicated files for each topic, linked from the main README. The README will maintain references to the author's Medium articles for in-depth reading, without reproducing their content.

## Glossary

- **Build_System**: The .NET build system that manages the MediatR.Playground.sln solution and all contained projects
- **Package_Manager**: The NuGet system responsible for resolving and installing package dependencies
- **README**: The README.md file at the repository root containing the main project documentation
- **Docs_Folder**: The `docs/` folder at the repository root containing detailed documentation files
- **MediatR_Pipeline**: The MediatR behaviors (IPipelineBehavior, IStreamPipelineBehavior) that intercept the request/response flow
- **Notification_Publisher**: The custom publishers that manage MediatR notification distribution (sequential, parallel, priority-based)
- **Exception_Handler**: The MediatR exception handlers (IRequestExceptionHandler) and the global exception handling behavior
- **API_Project**: The MediatR.Playground.API project that exposes REST endpoints
- **Domain_Project**: The MediatR.Playground.Domain project containing command, query, and notification handlers
- **Model_Project**: The MediatR.Playground.Model project containing models, commands, queries, and notifications
- **Pipelines_Project**: The MediatR.Playground.Pipelines project containing pipeline behavior implementations
- **Persistence_Project**: The MediatR.Playground.Persistence project containing the persistence layer with Entity Framework Core
- **FakeAuth_Project**: The FakeAuth.Service project that simulates an authentication service
- **Medium_Articles**: The articles published by the author on Medium covering the patterns implemented in the project

## Requirements

### Requirement 1: Upgrade to .NET 10

**User Story:** As a developer, I want to upgrade the project from .NET 9 to .NET 10 (LTS), so that I benefit from long-term support, performance improvements, and the latest framework features.

#### Acceptance Criteria

1. THE Build_System SHALL update the TargetFramework from net9.0 to net10.0 in all project files (API_Project, Domain_Project, Model_Project, Pipelines_Project, Persistence_Project, FakeAuth_Project)
2. WHEN the target framework is updated to net10.0, THE Build_System SHALL compile the solution without errors
3. IF the .NET 10 upgrade introduces breaking changes in any project, THEN THE Build_System SHALL resolve incompatibilities by adapting the source code
4. THE Package_Manager SHALL upgrade all Microsoft.* packages (Microsoft.AspNetCore.OpenApi, Microsoft.EntityFrameworkCore.InMemory, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.DependencyInjection.Abstractions) to their latest stable 10.x versions compatible with .NET 10

### Requirement 2: Upgrade MediatR to the latest free version

**User Story:** As a developer, I want to upgrade MediatR to the latest Apache-2.0 licensed (free) version, so that I have the latest features and fixes without incurring commercial license costs.

#### Acceptance Criteria

1. THE Package_Manager SHALL upgrade the MediatR package from version 12.4.1 to version 12.5.0 in all projects that reference it (Domain_Project, Model_Project, Pipelines_Project)
2. WHEN the MediatR package is upgraded to version 12.5.0, THE Build_System SHALL compile the solution without errors
3. WHEN the MediatR package is upgraded, THE Pipelines_Project SHALL maintain the functionality of all existing pipeline behaviors (LoggingBehavior, ValidationBehavior, CommandAuthorizationBehavior, CachingBehavior, UnitOfWorkBehavior, GenericStreamLoggingBehavior, SampleFilterStreamBehavior)
4. WHEN the MediatR package is upgraded, THE Domain_Project SHALL maintain the functionality of all existing exception handlers (ExceptionHandler, InvalidOperationExceptionHandler, GlobalExceptionHandlingBehavior)
5. WHEN the MediatR package is upgraded, THE Domain_Project SHALL maintain the functionality of all custom notification publishers (MultipleNotificationPublisher, PriorityNotificationPublisher, CustomNotificationPublisher)
6. THE Package_Manager SHALL keep the MediatR version at 12.5.0 and not upgrade to subsequent versions (13.x+) that use the commercial RPL-1.5 license

### Requirement 3: Upgrade NuGet packages to latest stable versions

**User Story:** As a developer, I want to upgrade all NuGet packages to their latest stable versions, so that I benefit from security fixes, performance improvements, and new features.

#### Acceptance Criteria

1. THE Package_Manager SHALL upgrade the FluentValidation package from version 11.11.0 to the latest stable version in Domain_Project and Pipelines_Project
2. THE Package_Manager SHALL upgrade the FluentValidation.DependencyInjectionExtensions package from version 11.11.0 to the latest stable version in Domain_Project
3. THE Package_Manager SHALL upgrade the ZiggyCreatures.FusionCache package from version 2.1.0 to the latest stable version in Pipelines_Project
4. THE Package_Manager SHALL upgrade the Bogus package from version 35.6.2 to the latest stable version in FakeAuth_Project
5. THE Package_Manager SHALL upgrade the Swashbuckle.AspNetCore package from version 7.2.0 to the latest stable version compatible with .NET 10 in API_Project
6. WHEN all packages are upgraded, THE Build_System SHALL compile the solution without errors
7. IF a package upgrade introduces breaking changes, THEN THE Build_System SHALL resolve incompatibilities by adapting the source code

### Requirement 4: Create documentation folder

**User Story:** As a developer, I want to have a `docs/` folder with dedicated documentation files for each topic, so that documentation is organized and easily navigable within the repository.

#### Acceptance Criteria

1. THE Docs_Folder SHALL be created at the repository root with the path `docs/`
2. THE Docs_Folder SHALL contain separate Markdown files for each main topic: pipelines, unit of work, exception handling, global exception handling, notifications, stream requests, caching
3. THE Docs_Folder SHALL contain a dedicated file for the Priority Notification Publisher, documenting the priority-based notification pattern implemented in the project (this is new content, not covered by an existing Medium article)
4. WHEN a documentation file covers a topic addressed by a Medium_Articles entry, THE file SHALL include a link to the corresponding Medium article for further reading
5. THE Docs_Folder SHALL use a tone and writing style consistent with the existing project README

### Requirement 5: MediatR Pipelines documentation

**User Story:** As a developer, I want to have detailed documentation on MediatR Pipelines in the repository, so that I can understand the implemented patterns with references to articles for deeper reading.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a file describing the MediatR Pipeline concept, how to implement an IPipelineBehavior, and the pre/post processing mechanism
2. THE Docs_Folder SHALL document each pipeline behavior implemented in the project: LoggingBehavior (logging with execution time measurement), ValidationBehavior (validation with FluentValidation), CommandAuthorizationBehavior (authorization with IAuthService)
3. THE Docs_Folder SHALL describe the pipeline filtering technique using custom interfaces (ICommand, IQueryRequest, ITransactionCommand) that inherit from IRequest
4. THE Docs_Folder SHALL document the pipeline behavior registration order and how it determines the execution order
5. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — MediatR Pipelines" for further reading

### Requirement 6: Unit of Work pattern documentation

**User Story:** As a developer, I want to have documentation on the Unit of Work pattern implemented with MediatR pipeline, so that I can understand how transactions are managed in the project.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for the Unit of Work pattern describing how UnitOfWorkBehavior manages transactions via IUnitOfWork
2. THE Docs_Folder SHALL document the transactional flow: begin transaction, execute handler, commit on success, and rollback on error
3. THE Docs_Folder SHALL describe the ITransactionCommand interface and how it is used to distinguish transactional commands from regular commands
4. THE Docs_Folder SHALL document the Repository pattern implemented with IRepository and EntityFrameworkRepository
5. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — Unit Of Work Pattern with MediatR Pipeline" for further reading

### Requirement 7: Exception handling documentation

**User Story:** As a developer, I want to have documentation on exception handling with MediatR, so that I can understand the different levels of error handling available in the project.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for exception handling via IRequestExceptionHandler, describing how to intercept specific exceptions for individual requests
2. THE Docs_Folder SHALL document the difference between ExceptionHandler (handles all exceptions for SampleCommand) and InvalidOperationExceptionHandler (handles only InvalidOperationException for SampleCommand)
3. THE Docs_Folder SHALL describe the RequestExceptionHandlerState mechanism and how SetHandled allows providing an alternative response
4. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — Handle Exceptions with MediatR" for further reading

### Requirement 8: Global exception handling documentation

**User Story:** As a developer, I want to have documentation on global exception handling with MediatR pipeline, so that I can understand how to centralize error logging.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for GlobalExceptionHandlingBehavior, describing how to implement a pipeline behavior that intercepts all exceptions
2. THE Docs_Folder SHALL document the difference between global exception handling (IPipelineBehavior with where TRequest : notnull) and request-specific exception handling (IRequestExceptionHandler)
3. THE Docs_Folder SHALL describe when to use each approach: global for centralized logging, specific for fallback responses
4. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — Global Exception Handler with MediatR" for further reading

### Requirement 9: Notifications and notification publisher documentation

**User Story:** As a developer, I want to have documentation on MediatR notifications and custom notification publishers, so that I can understand the different notification distribution strategies implemented in the project.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for notifications describing the MediatR publish-subscribe pattern with INotification and INotificationHandler
2. THE Docs_Folder SHALL document the two standard publishing strategies: ForeachAwaitPublisher (sequential) and TaskWhenAllPublisher (parallel), describing error handling behavior for each
3. THE Docs_Folder SHALL describe the MultipleNotificationPublisher that selects the publishing strategy based on notification type (IPriorityNotification, IParallelNotification, default sequential)
4. THE Docs_Folder SHALL document the marker interfaces (IParallelNotification, IPriorityNotification, IDataUpdateNotification) and their role in strategy selection
5. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — MediatR Notifications and Notification Publisher" for further reading

### Requirement 10: Priority Notification Publisher documentation

**User Story:** As a developer, I want to have documentation on the Priority Notification Publisher, so that I can understand how to implement notifications with priority-based execution order.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for PriorityNotificationPublisher describing the handler ordering mechanism by priority
2. THE Docs_Folder SHALL document the IPriorityNotificationHandler interface and the Priority property used to determine execution order
3. THE Docs_Folder SHALL describe the reflection mechanism used to read priority from handlers and the default value (99) for handlers without priority
4. THE Docs_Folder SHALL document how to create a notification handler with priority, showing the IPriorityNotificationHandler interface implementation
5. WHILE no dedicated Medium article exists for the Priority Notification Publisher, THE Docs_Folder SHALL contain complete and self-contained documentation on this topic

### Requirement 11: Stream Request and Stream Pipeline documentation

**User Story:** As a developer, I want to have documentation on MediatR stream requests and stream pipelines, so that I can understand how streaming data flows are handled in the project.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for stream requests describing the use of IStreamRequest and IAsyncEnumerable for streaming data retrieval
2. THE Docs_Folder SHALL document the two stream pipeline behaviors implemented: GenericStreamLoggingBehavior (generic logging for all streams) and SampleFilterStreamBehavior (authorization-based filter for a specific stream)
3. THE Docs_Folder SHALL describe how IStreamPipelineBehavior differs from IPipelineBehavior and how stream behaviors use yield return to process elements one at a time
4. THE Docs_Folder SHALL document the two filtering techniques: generic pipeline (without where constraints) and specific pipeline (with where clause on concrete type)
5. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — Stream Request and Pipeline With MediatR" for further reading

### Requirement 12: Caching Pipeline documentation

**User Story:** As a developer, I want to have documentation on the caching pipeline implemented with FusionCache, so that I can understand how query caching is managed in the project.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a dedicated file for the caching pipeline describing how CachingBehavior intercepts queries (IQueryRequest) and uses FusionCache to store results
2. THE Docs_Folder SHALL document the cache key mechanism via the IQueryRequest interface and the CacheKey property
3. THE Docs_Folder SHALL describe the cache configuration (SetDuration, SetFailSafe, SetFactoryTimeouts) used in CachingBehavior
4. THE Docs_Folder SHALL describe the advantage of implementing caching at the pipeline level compared to doing it in each individual handler
5. THE Docs_Folder SHALL include a link to the Medium article "C# .NET — Caching Requests With MediatR Pipeline" for further reading

### Requirement 13: Update main README

**User Story:** As a developer, I want the README to be updated with a navigable table of contents, project structure, and links to docs/ folder documentation, so that I have a clear entry point for exploring the repository.

#### Acceptance Criteria

1. THE README SHALL maintain the tone and writing style of the current README, with clear and concise explanations in English
2. THE README SHALL contain a table of contents with internal links to all document sections and links to files in the Docs_Folder
3. THE README SHALL include a section on the project structure describing the role of each project in the solution (API_Project, Domain_Project, Model_Project, Pipelines_Project, Persistence_Project, FakeAuth_Project)
4. THE README SHALL maintain links to Medium_Articles as references for further reading, clearly indicating that additional documentation is available in the Docs_Folder
5. THE README SHALL include a section on the package versions used, specifying that MediatR 12.5.0 is the last version with a free Apache-2.0 license, and that the project targets .NET 10 (LTS)
6. THE README SHALL include a brief introduction for each topic documented in the Docs_Folder, with a link to the corresponding documentation file
7. THE README SHALL update the list of available Swagger endpoints, including all endpoints present in the project (Commands, Requests, Transactions, Notifications, Exceptions, Stream)
