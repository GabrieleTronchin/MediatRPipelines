# Requirements Document

## Introduction

This specification covers four areas of improvement for the MediatR Playground project:

1. **Test Suite Improvement** — Adding isolated unit tests with mocking (NSubstitute) for all pipeline behaviors, notification publishers, and the FluentValidation validator, complementing the 11 existing baseline smoke tests.
2. **Deprecation Warning Fixes** — Eliminating all compilation warnings (ASPDEPR002, CS8618, CS8425, CS8714) to achieve a clean, warning-free build.
3. **README Restructuring** — Rewriting README.md with a new title, reordered sections, a separate testing section in docs/, and simplified PS1 script documentation.
4. **Documentation Updates** — Adding AI-generated disclaimers to existing docs/ files and creating a dedicated docs/testing.md file.

## Glossary

- **Test_Suite**: The set of xUnit tests in the `test/MediatR.Playground.Tests/` project, including existing smoke tests and new isolated unit tests
- **Pipeline_Behavior**: A MediatR component that intercepts requests in the processing pipeline (e.g. ValidationBehavior, LoggingBehavior, CommandAuthorizationBehavior, CachingBehavior, UnitOfWorkBehavior)
- **ValidationBehavior**: The command pipeline behavior that performs FluentValidation and throws `ValidationException` when errors are found
- **CommandAuthorizationBehavior**: The command pipeline behavior that checks authorization via `IAuthService` and throws an exception if the operation is not allowed
- **LoggingBehavior**: The command pipeline behavior that logs the start and end of request processing and delegates to the next handler
- **CachingBehavior**: The query pipeline behavior that uses FusionCache to return results from cache (cache hit) or delegate to the next handler (cache miss)
- **UnitOfWorkBehavior**: The transactional command pipeline behavior that manages begin/commit/rollback of a transaction via `IUnitOfWork`
- **MultipleNotificationPublisher**: The custom publisher that routes notifications to the correct publisher (sequential, parallel, priority) based on the notification's marker interfaces
- **PriorityNotificationPublisher**: The publisher that orders notification handlers by priority (ascending numeric value) and executes them in order
- **SampleCommandValidator**: The FluentValidation validator for `SampleCommand` that verifies `Id` is not empty/Guid.Empty and `Description` is not empty
- **NSubstitute**: The .NET mocking library used to create isolated dependency mocks in tests
- **Endpoint_File**: A C# file in the `src/MediatR.Playground.API/Endpoints/` folder that defines minimal API routes
- **WithOpenApi**: The extension method deprecated in .NET 10 (warning ASPDEPR002) used in endpoints to generate OpenAPI metadata
- **EnumeratorCancellation_Attribute**: The `[EnumeratorCancellation]` attribute required by the C# compiler on `CancellationToken` parameters in async-iterator methods (warning CS8425)
- **Build_Output**: The output of the `dotnet build src/MediatR.Playground.sln` command reporting warnings and compilation errors
- **README**: The `README.md` file at the repository root describing the project, structure, endpoints, and getting started instructions
- **Docs_Folder**: The `docs/` folder containing markdown documentation files for each project topic
- **Testing_Doc**: The `docs/testing.md` file dedicated to test suite documentation and testing strategies
- **Stream_Client_Script**: The PowerShell script `scripts/stream-client.ps1` used to test streaming endpoints

## Requirements

### Requirement 1: Unit Tests for ValidationBehavior

**User Story:** As a developer, I want isolated unit tests for `ValidationBehavior`, so that I can verify FluentValidation works correctly in the MediatR pipeline.

#### Acceptance Criteria

1. WHEN a request with invalid data is processed by ValidationBehavior and at least one validator returns errors, THE Test_Suite SHALL verify that ValidationBehavior throws a `FluentValidation.ValidationException`
2. WHEN a request with valid data is processed by ValidationBehavior and no validator returns errors, THE Test_Suite SHALL verify that ValidationBehavior invokes the `next()` delegate and returns the response
3. WHEN no validators are registered for the request type, THE Test_Suite SHALL verify that ValidationBehavior invokes the `next()` delegate without throwing exceptions

### Requirement 2: Unit Tests for CommandAuthorizationBehavior

**User Story:** As a developer, I want isolated unit tests for `CommandAuthorizationBehavior`, so that I can verify the authorization check works correctly in the pipeline.

#### Acceptance Criteria

1. WHEN the authorization service returns `IsSuccess = false` with an exception, THE Test_Suite SHALL verify that CommandAuthorizationBehavior throws the exception returned by the authorization service
2. WHEN the authorization service returns `IsSuccess = false` without a specific exception, THE Test_Suite SHALL verify that CommandAuthorizationBehavior throws a generic `Exception`
3. WHEN the authorization service returns `IsSuccess = true`, THE Test_Suite SHALL verify that CommandAuthorizationBehavior invokes the `next()` delegate and returns the response

### Requirement 3: Unit Tests for LoggingBehavior

**User Story:** As a developer, I want isolated unit tests for `LoggingBehavior`, so that I can verify the behavior correctly delegates to the next handler in the pipeline.

#### Acceptance Criteria

1. WHEN a request is processed by LoggingBehavior, THE Test_Suite SHALL verify that LoggingBehavior invokes the `next()` delegate and returns the response produced by the delegate
2. WHEN a request is processed by LoggingBehavior, THE Test_Suite SHALL verify that LoggingBehavior logs a message before and after invoking the `next()` delegate

### Requirement 4: Unit Tests for CachingBehavior

**User Story:** As a developer, I want isolated unit tests for `CachingBehavior`, so that I can verify cache hit and cache miss behavior in the query pipeline.

#### Acceptance Criteria

1. WHEN a query is processed by CachingBehavior and the result is not in cache (cache miss), THE Test_Suite SHALL verify that CachingBehavior invokes the `next()` delegate and returns the response
2. WHEN a query is processed by CachingBehavior, THE Test_Suite SHALL verify that CachingBehavior uses the request's `CacheKey` to interact with FusionCache

### Requirement 5: Unit Tests for UnitOfWorkBehavior

**User Story:** As a developer, I want isolated unit tests for `UnitOfWorkBehavior`, so that I can verify transaction management (commit and rollback) in the pipeline.

#### Acceptance Criteria

1. WHEN a transactional request is successfully processed by the `next()` delegate, THE Test_Suite SHALL verify that UnitOfWorkBehavior invokes `BeginTransaction()`, then `Commit()`, and finally `Dispose()` on the transaction
2. WHEN the `next()` delegate throws an exception during transactional request processing, THE Test_Suite SHALL verify that UnitOfWorkBehavior invokes `RollbackAsync()` on the transaction and `Dispose()`
3. WHEN the `next()` delegate throws an exception, THE Test_Suite SHALL verify that UnitOfWorkBehavior returns the default value for the response type instead of propagating the exception

### Requirement 6: Unit Tests for MultipleNotificationPublisher

**User Story:** As a developer, I want isolated unit tests for `MultipleNotificationPublisher`, so that I can verify notifications are routed to the correct publisher based on notification type.

#### Acceptance Criteria

1. WHEN a notification implementing `IPriorityNotification` is published, THE Test_Suite SHALL verify that MultipleNotificationPublisher delegates to PriorityNotificationPublisher
2. WHEN a notification implementing `IParallelNotification` is published, THE Test_Suite SHALL verify that MultipleNotificationPublisher executes all handlers (parallel behavior via TaskWhenAllPublisher)
3. WHEN a notification implementing neither `IPriorityNotification` nor `IParallelNotification` is published, THE Test_Suite SHALL verify that MultipleNotificationPublisher executes all handlers sequentially (sequential behavior via ForeachAwaitPublisher)

### Requirement 7: Unit Tests for PriorityNotificationPublisher

**User Story:** As a developer, I want isolated unit tests for `PriorityNotificationPublisher`, so that I can verify handlers are executed in ascending priority order.

#### Acceptance Criteria

1. WHEN a notification is published via PriorityNotificationPublisher with handlers having different priorities, THE Test_Suite SHALL verify that handlers are executed in ascending priority order (lower numeric value first)
2. WHEN a handler does not implement `IPriorityNotificationHandler` and has no Priority property, THE Test_Suite SHALL verify that PriorityNotificationPublisher assigns the default priority (99) to that handler

### Requirement 8: Unit Tests for SampleCommandValidator

**User Story:** As a developer, I want isolated unit tests for `SampleCommandValidator`, so that I can verify FluentValidation rules are correctly configured.

#### Acceptance Criteria

1. WHEN a `SampleCommand` with `Id` equal to `Guid.Empty` is validated, THE Test_Suite SHALL verify that SampleCommandValidator produces a validation error for the `Id` property
2. WHEN a `SampleCommand` with an empty or null `Description` is validated, THE Test_Suite SHALL verify that SampleCommandValidator produces a validation error for the `Description` property
3. WHEN a `SampleCommand` with a valid `Id` (not empty, not `Guid.Empty`) and a non-empty `Description` is validated, THE Test_Suite SHALL verify that SampleCommandValidator produces no validation errors

### Requirement 9: Add NSubstitute to the Test Project

**User Story:** As a developer, I want NSubstitute available as a mocking library in the test project, so that I can create isolated dependency mocks.

#### Acceptance Criteria

1. THE Test_Suite SHALL include the `NSubstitute` NuGet package as a dependency in the `MediatR.Playground.Tests.csproj` project file
2. THE Test_Suite SHALL compile successfully after adding NSubstitute and the new tests

### Requirement 10: Fix ASPDEPR002 Warnings — Deprecated WithOpenApi

**User Story:** As a developer, I want to eliminate ASPDEPR002 warnings from the build, so that the compilation output is clean and compatible with .NET 10.

#### Acceptance Criteria

1. THE Endpoint_File `StreamRequestEndpoint.cs` SHALL use the updated pattern for OpenAPI metadata instead of the 2 deprecated `WithOpenApi()` calls
2. THE Endpoint_File `RequestsAndCommandEndpoints.cs` SHALL use the updated pattern for OpenAPI metadata instead of the 2 deprecated `WithOpenApi()` calls
3. THE Endpoint_File `NotificationEndpoint.cs` SHALL use the updated pattern for OpenAPI metadata instead of the 3 deprecated `WithOpenApi()` calls
4. THE Endpoint_File `TransactionEndpoints.cs` SHALL use the updated pattern for OpenAPI metadata instead of the 3 deprecated `WithOpenApi()` calls
5. THE Endpoint_File `ExceptionsEndpoints.cs` SHALL use the updated pattern for OpenAPI metadata instead of the 3 deprecated `WithOpenApi()` calls
6. WHEN the solution is compiled with `dotnet build`, THE Build_Output SHALL contain no ASPDEPR002 warnings

### Requirement 11: Fix CS8618 Warnings — Non-Nullable Properties Not Initialized

**User Story:** As a developer, I want to eliminate CS8618 warnings from the build, so that non-nullable properties are correctly initialized or declared.

#### Acceptance Criteria

1. THE `SampleEntity.cs` SHALL declare the `Description` property in a way that does not generate CS8618 warning (e.g. with `= string.Empty` or `required`)
2. THE `AddSampleEntityCommand.cs` SHALL declare the `Description` property in a way that does not generate CS8618 warning (e.g. with `= string.Empty` or `required`)
3. WHEN the solution is compiled with `dotnet build`, THE Build_Output SHALL contain no CS8618 warnings

### Requirement 12: Fix CS8425 Warnings — Missing EnumeratorCancellation Attribute

**User Story:** As a developer, I want to eliminate CS8425 warnings from the build, so that async-iterator methods have the `[EnumeratorCancellation]` attribute on the `CancellationToken` parameter.

#### Acceptance Criteria

1. THE `EntityFrameworkRepository.cs` SHALL add the `[EnumeratorCancellation]` attribute to the `CancellationToken` parameter of the `GetStream` method
2. THE `SampleStreamQueryHandler.cs` (in the `StreamEntity` folder) SHALL add the `[EnumeratorCancellation]` attribute to the `CancellationToken` parameter of the `Handle` method
3. THE `SampleStreamQueryHandler.cs` (in the `StreamEntityWithFilter` folder) SHALL add the `[EnumeratorCancellation]` attribute to the `CancellationToken` parameter of the `Handle` method
4. WHEN the solution is compiled with `dotnet build`, THE Build_Output SHALL contain no CS8425 warnings

### Requirement 13: Fix CS8714 Warning — Nullability Constraint Mismatch

**User Story:** As a developer, I want to eliminate the CS8714 warning from the build, so that nullability constraints on generic parameters are consistent.

#### Acceptance Criteria

1. THE `GenericStreamLoggingBehavior.cs` SHALL fix the nullability constraint on the `TRequest` generic parameter by adding the `notnull` constraint to align with the `IStreamPipelineBehavior` interface
2. WHEN the solution is compiled with `dotnet build`, THE Build_Output SHALL contain no CS8714 warnings

### Requirement 14: Clean Build With No Warnings

**User Story:** As a developer, I want the entire solution build to be warning-free, so that I have clear and reliable compilation feedback.

#### Acceptance Criteria

1. WHEN the command `dotnet build src/MediatR.Playground.sln` is executed, THE Build_Output SHALL complete with 0 warnings
2. WHEN the command `dotnet test src/MediatR.Playground.sln` is executed, THE Test_Suite SHALL run all tests (existing and new) successfully

### Requirement 15: README Restructuring — Title and Version Note

**User Story:** As a developer, I want the README to have an updated title and a clear note about the versions used, so that the project context is immediately communicated.

#### Acceptance Criteria

1. THE README SHALL have `MediatR Pipelines Playground` as the main title
2. THE README SHALL contain a note indicating the .NET version used and that the project uses the last free version of MediatR available (12.5.0, Apache-2.0)
3. THE README SHALL position the "Package Versions" table at the bottom of the document (last section)

### Requirement 16: README Restructuring — Section Order

**User Story:** As a developer, I want the README sections to be ordered logically, so that navigation and understanding of the project are easier.

#### Acceptance Criteria

1. THE README SHALL present sections in the following order: Project Structure, Swagger Endpoints, Getting Started, MediatR Fundamentals, Topics, Articles, Package Versions
2. THE README SHALL not contain the "Testing the API" section inline — testing documentation must be in a separate `docs/testing.md` file
3. THE README SHALL refer to articles generically as "Articles" without mentioning "Medium" in the section title
4. THE README SHALL contain a link to `docs/testing.md` in the appropriate section (Getting Started or Topics)

### Requirement 17: Separate Testing Documentation

**User Story:** As a developer, I want testing documentation in a dedicated file inside `docs/`, so that the README stays concise.

#### Acceptance Criteria

1. THE Docs_Folder SHALL contain a `testing.md` file with complete documentation about the test suite, including instructions for running tests, description of the `.http` file, and Stream_Client_Script documentation
2. THE Testing_Doc SHALL describe the Stream_Client_Script simply, showing only basic usage without listing advanced parameters (only the base command `.\scripts\stream-client.ps1` and the example with `-Endpoint streamfilter`)
3. THE Testing_Doc SHALL not include documentation for the `-BaseUrl` parameter and other optional script parameters

### Requirement 18: AI-Generated Disclaimer in Existing Docs Files

**User Story:** As a developer, I want existing documentation files in `docs/` to have a disclaimer indicating they were AI-generated based on the original articles, so that readers have the right context.

#### Acceptance Criteria

1. EACH file in Docs_Folder (pipelines.md, unit-of-work.md, exception-handling.md, global-exception-handling.md, notifications.md, priority-notification-publisher.md, stream-requests.md, caching.md) SHALL contain a disclaimer at the top (after the title) indicating the content was AI-generated based on the original article
2. THE disclaimer SHALL include the link to the corresponding original article (the article links currently in the README's "Medium Articles" section)
3. THE disclaimer SHALL be visually distinct from the main content (e.g. with a blockquote or admonition)

### Requirement 19: Simplified Stream Client Script Documentation

**User Story:** As a developer, I want the PS1 script documentation in README/docs to be simple and direct, without unnecessary parameters.

#### Acceptance Criteria

1. THE Stream_Client_Script documentation (in docs/testing.md) SHALL show only two usage examples: the base command and the command with `-Endpoint streamfilter`
2. THE Stream_Client_Script documentation SHALL not include the example with `-BaseUrl` or other optional parameters
