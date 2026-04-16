# Implementation Plan: Test Suite and Deprecation Fixes

## Overview

This plan implements four independent areas: adding an isolated unit test suite with NSubstitute and FsCheck.Xunit, fixing all compiler deprecation warnings, restructuring the README, and updating documentation with disclaimers and a dedicated testing doc. Tasks are ordered so that test infrastructure comes first, then test files, then deprecation fixes, then documentation changes, with a final build verification checkpoint.

## Tasks

- [x] 1. Add NSubstitute and FsCheck.Xunit packages to the test project
  - Add `NSubstitute` NuGet package to `test/MediatR.Playground.Tests/MediatR.Playground.Tests.csproj`
  - Add `FsCheck.Xunit` NuGet package to `test/MediatR.Playground.Tests/MediatR.Playground.Tests.csproj`
  - Verify the project restores and compiles with the new packages
  - _Requirements: 9.1, 9.2_

- [x] 2. Create unit tests for pipeline behaviors
  - [x] 2.1 Create `ValidationBehaviorTests.cs`
    - Create `test/MediatR.Playground.Tests/ValidationBehaviorTests.cs`
    - Test that `ValidationBehavior` throws `FluentValidation.ValidationException` when validators return errors
    - Test that `ValidationBehavior` calls `next()` and returns the response when validation passes
    - Test that `ValidationBehavior` calls `next()` without throwing when no validators are registered
    - Mock `IValidator<SampleCommand>` and `RequestHandlerDelegate<SampleCommandComplete>` with NSubstitute
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 2.2 Create `CommandAuthorizationBehaviorTests.cs`
    - Create `test/MediatR.Playground.Tests/CommandAuthorizationBehaviorTests.cs`
    - Test that the behavior throws the specific exception from `AuthResponse.Exception` when `IsSuccess = false`
    - Test that the behavior throws a generic `Exception` when `IsSuccess = false` and `Exception` is null
    - Test that the behavior calls `next()` and returns the response when `IsSuccess = true`
    - Mock `IAuthService` and `RequestHandlerDelegate<SampleCommandComplete>` with NSubstitute
    - _Requirements: 2.1, 2.2, 2.3_

  - [x] 2.3 Create `LoggingBehaviorTests.cs`
    - Create `test/MediatR.Playground.Tests/LoggingBehaviorTests.cs`
    - Test that `LoggingBehavior` calls `next()` and returns the delegate's response
    - Test that `LoggingBehavior` logs messages before and after calling `next()` (verify via `ILogger` mock)
    - Mock `ILogger<LoggingBehavior>` and `RequestHandlerDelegate<SampleCommandComplete>` with NSubstitute
    - _Requirements: 3.1, 3.2_

  - [x] 2.4 Create `CachingBehaviorTests.cs`
    - Create `test/MediatR.Playground.Tests/CachingBehaviorTests.cs`
    - Test that `CachingBehavior` invokes `next()` and returns the response on cache miss
    - Test that `CachingBehavior` uses the request's `CacheKey` when interacting with `IFusionCache`
    - Mock `ILogger<CachingBehavior>`, `IFusionCache`, and `RequestHandlerDelegate` with NSubstitute
    - Use existing `GetAllSampleEntitiesQuery` / `GetAllSampleEntitiesQueryResult` types that implement `IQueryRequest`
    - _Requirements: 4.1, 4.2_

  - [x] 2.5 Create `UnitOfWorkBehaviorTests.cs`
    - Create `test/MediatR.Playground.Tests/UnitOfWorkBehaviorTests.cs`
    - Test that on success, `UnitOfWorkBehavior` calls `BeginTransaction()`, then `Commit()`, then `Dispose()` on the transaction
    - Test that on `next()` throwing, `UnitOfWorkBehavior` calls `RollbackAsync()` on the transaction and `Dispose()`
    - Test that on `next()` throwing, `UnitOfWorkBehavior` returns `default(TResponse)` instead of propagating the exception
    - Mock `IUnitOfWork`, `IDbContextTransaction`, `ILogger<UnitOfWorkBehavior>`, and `RequestHandlerDelegate` with NSubstitute
    - Use existing `AddSampleEntityCommand` / `AddSampleEntityCommandComplete` types that implement `ITransactionCommand`
    - _Requirements: 5.1, 5.2, 5.3_

- [x] 3. Checkpoint — Verify pipeline behavior tests compile and pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 4. Create unit tests for notification publishers and validator
  - [ ] 4.1 Create `MultipleNotificationPublisherTests.cs`
    - Create `test/MediatR.Playground.Tests/MultipleNotificationPublisherTests.cs`
    - Test that `MultipleNotificationPublisher` delegates to `PriorityNotificationPublisher` for `IPriorityNotification` notifications
    - Test that `MultipleNotificationPublisher` executes all handlers for `IParallelNotification` notifications (parallel via `TaskWhenAllPublisher`)
    - Test that `MultipleNotificationPublisher` executes all handlers sequentially for plain `INotification` notifications (via `ForeachAwaitPublisher`)
    - Create inline test notification types implementing the appropriate marker interfaces
    - Use `NotificationHandlerExecutor` instances with controlled `HandlerCallback` delegates
    - _Requirements: 6.1, 6.2, 6.3_

  - [ ] 4.2 Create `PriorityNotificationPublisherTests.cs`
    - Create `test/MediatR.Playground.Tests/PriorityNotificationPublisherTests.cs`
    - Test that handlers are executed in ascending priority order (lower numeric value first) using a fixed example with known priorities
    - Test that handlers without `IPriorityNotificationHandler` / `Priority` property receive default priority 99
    - Use `NotificationHandlerExecutor` instances with handler objects that have a `Priority` property
    - _Requirements: 7.1, 7.2_

  - [ ]* 4.3 Write property test for PriorityNotificationPublisher priority ordering
    - **Property 1: Priority-ordered execution**
    - **Validates: Requirements 7.1, 7.2**
    - Use FsCheck.Xunit `[Property]` attribute with `MaxTest = 100`
    - Generate arbitrary lists of handler priorities (including handlers without `Priority` property)
    - Assert that `PriorityNotificationPublisher.Publish` always executes handlers in ascending priority order
    - Tag: `Feature: test-suite-and-deprecation-fixes, Property 1: Priority-ordered execution`

  - [ ] 4.4 Create `SampleCommandValidatorTests.cs`
    - Create `test/MediatR.Playground.Tests/SampleCommandValidatorTests.cs`
    - Test that `SampleCommandValidator` produces a validation error when `Id` is `Guid.Empty`
    - Test that `SampleCommandValidator` produces a validation error when `Description` is empty or null
    - Test that `SampleCommandValidator` produces no errors when `Id` is valid and `Description` is non-empty
    - Directly instantiate `SampleCommandValidator` and call `Validate()` (no mocks needed)
    - _Requirements: 8.1, 8.2, 8.3_

- [ ] 5. Checkpoint — Verify all unit tests compile and pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 6. Fix ASPDEPR002 warnings — Remove deprecated `.WithOpenApi()` calls
  - [ ] 6.1 Remove `.WithOpenApi()` from `StreamRequestEndpoint.cs`
    - Remove the 2 `.WithOpenApi()` calls from `src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs`
    - _Requirements: 10.1_

  - [ ] 6.2 Remove `.WithOpenApi()` from `RequestsAndCommandEndpoints.cs`
    - Remove the 2 `.WithOpenApi()` calls from `src/MediatR.Playground.API/Endpoints/RequestsAndCommandEndpoints.cs`
    - _Requirements: 10.2_

  - [ ] 6.3 Remove `.WithOpenApi()` from `NotificationEndpoint.cs`
    - Remove the 3 `.WithOpenApi()` calls from `src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`
    - _Requirements: 10.3_

  - [ ] 6.4 Remove `.WithOpenApi()` from `TransactionEndpoints.cs`
    - Remove the 3 `.WithOpenApi()` calls from `src/MediatR.Playground.API/Endpoints/TransactionEndpoints.cs`
    - _Requirements: 10.4_

  - [ ] 6.5 Remove `.WithOpenApi()` from `ExceptionsEndpoints.cs`
    - Remove the 3 `.WithOpenApi()` calls from `src/MediatR.Playground.API/Endpoints/ExceptionsEndpoints.cs`
    - _Requirements: 10.5_

- [ ] 7. Fix CS8618 warnings — Initialize non-nullable string properties
  - [ ] 7.1 Fix `SampleEntity.cs`
    - Add `= string.Empty` to the `Description` property in `SampleEntity.cs`
    - _Requirements: 11.1_

  - [ ] 7.2 Fix `AddSampleEntityCommand.cs`
    - Add `= string.Empty` to the `Description` property in `AddSampleEntityCommand.cs`
    - _Requirements: 11.2_

- [ ] 8. Fix CS8425 warnings — Add `[EnumeratorCancellation]` attributes
  - [ ] 8.1 Fix `EntityFrameworkRepository.cs`
    - Add `[EnumeratorCancellation]` attribute to the `CancellationToken` parameter of the `GetStream` method
    - Add `using System.Runtime.CompilerServices;` if not already present
    - _Requirements: 12.1_

  - [ ] 8.2 Fix `StreamEntity/SampleStreamQueryHandler.cs`
    - Add `[EnumeratorCancellation]` attribute to the `CancellationToken` parameter of the `Handle` method
    - Add `using System.Runtime.CompilerServices;` if not already present
    - _Requirements: 12.2_

  - [ ] 8.3 Fix `StreamEntityWithFilter/SampleStreamQueryHandler.cs`
    - Add `[EnumeratorCancellation]` attribute to the `CancellationToken` parameter of the `Handle` method
    - Add `using System.Runtime.CompilerServices;` if not already present
    - _Requirements: 12.3_

- [ ] 9. Fix CS8714 warning — Add `notnull` constraint to `GenericStreamLoggingBehavior`
  - Add `where TRequest : notnull` constraint to the `GenericStreamLoggingBehavior<TRequest, TResponse>` class declaration
  - _Requirements: 13.1_

- [ ] 10. Checkpoint — Verify clean build with 0 warnings
  - Run `dotnet build src/MediatR.Playground.sln` and verify 0 warnings
  - Ensure all tests pass, ask the user if questions arise.
  - _Requirements: 10.6, 11.3, 12.4, 13.2, 14.1, 14.2_

- [ ] 11. Restructure README.md
  - Rewrite `README.md` with new title "MediatR Pipelines Playground"
  - Add version note about .NET 10 and MediatR 12.5.0 (last free Apache-2.0 version) near the top
  - Reorder sections: Project Structure → Swagger Endpoints → Getting Started → MediatR Fundamentals → Topics → Articles → Package Versions
  - Remove the inline "Testing the API" section entirely
  - Add a link to `docs/testing.md` in the Getting Started or Topics section
  - Rename "Medium Articles" section to "Articles" (no "Medium" in the title)
  - Move "Package Versions" table to the bottom (last section)
  - _Requirements: 15.1, 15.2, 15.3, 16.1, 16.2, 16.3, 16.4_

- [ ] 12. Create `docs/testing.md`
  - Create `docs/testing.md` with complete testing documentation
  - Include instructions for running tests (`dotnet test src/MediatR.Playground.sln`)
  - Include description of the `.http` file and how to use it
  - Include simplified PS1 stream client documentation: only the base command `.\scripts\stream-client.ps1` and the `-Endpoint streamfilter` example
  - Do NOT include `-BaseUrl` or other optional parameters
  - _Requirements: 17.1, 17.2, 17.3, 19.1, 19.2_

- [ ] 13. Add AI-generated disclaimers to existing `docs/` files
  - [ ] 13.1 Add disclaimer to `docs/pipelines.md`
    - Insert blockquote disclaimer after the title with link to the pipelines article
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.2 Add disclaimer to `docs/unit-of-work.md`
    - Insert blockquote disclaimer after the title with link to the Unit of Work article
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.3 Add disclaimer to `docs/exception-handling.md`
    - Insert blockquote disclaimer after the title with link to the exception handling article
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.4 Add disclaimer to `docs/global-exception-handling.md`
    - Insert blockquote disclaimer after the title with link to the exception handling article (same article as exception-handling.md)
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.5 Add disclaimer to `docs/notifications.md`
    - Insert blockquote disclaimer after the title with link to the notifications article
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.6 Add disclaimer to `docs/priority-notification-publisher.md`
    - Insert blockquote disclaimer after the title with link to the notifications article (same article as notifications.md)
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.7 Add disclaimer to `docs/stream-requests.md`
    - Insert blockquote disclaimer after the title with link to the stream requests article
    - _Requirements: 18.1, 18.2, 18.3_

  - [ ] 13.8 Add disclaimer to `docs/caching.md`
    - Insert blockquote disclaimer after the title with link to the caching article
    - _Requirements: 18.1, 18.2, 18.3_

- [ ] 14. Final checkpoint — Clean build and all tests pass
  - Run `dotnet build src/MediatR.Playground.sln` and verify 0 warnings
  - Run `dotnet test src/MediatR.Playground.sln` and verify all tests pass (existing baseline smoke tests + all new unit tests)
  - Ensure all tests pass, ask the user if questions arise.
  - _Requirements: 14.1, 14.2_

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation after each major area
- The property-based test (task 4.3) validates the universal priority-ordering property using FsCheck.Xunit with randomized inputs
- Unit tests use NSubstitute for mocking and existing model types (`SampleCommand`, `GetAllSampleEntitiesQuery`, `AddSampleEntityCommand`) to satisfy generic constraints
- Deprecation fixes are purely compile-time changes with no runtime behavior impact
