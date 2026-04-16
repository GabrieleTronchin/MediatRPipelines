# Implementation Plan: MediatR Upgrade and Documentation

## Overview

This plan covers upgrading the MediatR Playground solution to .NET 10 (LTS) with updated NuGet packages, and creating a structured `docs/` folder with topic-specific documentation files plus an updated README. Tasks are ordered so that framework upgrades come first (since documentation references updated package versions), followed by documentation creation, and finally the README rewrite that ties everything together.

## Tasks

- [-] 0. Create test project for the solution
  - [ ] 0.1 Create xUnit test project `MediatR.Playground.Tests`
    - Create `test/MediatR.Playground.Tests/MediatR.Playground.Tests.csproj` targeting the current framework (net9.0, will be upgraded with the rest in Task 1)
    - Add xUnit, Microsoft.NET.Test.Sdk, and xunit.runner.visualstudio packages
    - Add project references to API, Domain, Model, Pipelines, Persistence, and FakeAuth.Service projects
    - Add the test project to the solution file `src/MediatR.Playground.sln`
  - [ ] 0.2 Write baseline smoke tests
    - Write a test that verifies the DI container builds successfully (WebApplicationFactory or manual ServiceCollection)
    - Write tests that verify MediatR can resolve and execute a sample request, command, and notification
    - Write a test that verifies each pipeline behavior is registered and executes in the expected order
    - Write a test that verifies the custom notification publishers (MultipleNotificationPublisher, PriorityNotificationPublisher) work correctly
  - [ ] 0.3 Run tests and confirm they pass on the current codebase (pre-upgrade baseline)
    - Run `dotnet test` and confirm all tests pass before any upgrades begin
    - This establishes a green baseline to detect regressions introduced by the upgrades

- [ ] 1. Upgrade TargetFramework to .NET 10 in all projects
  - Update `TargetFramework` from `net9.0` to `net10.0` in all 6 `.csproj` files:
    - `src/MediatR.Playground.API/MediatR.Playground.API.csproj`
    - `src/MediatR.Playground.Domain/MediatR.Playground.Domain.csproj`
    - `src/MediatR.Playground.Model/MediatR.Playground.Model.csproj`
    - `src/MediatR.Playground.Pipelines/MediatR.Playground.Pipelines.csproj`
    - `src/MediatR.Playground.Persistence/MediatR.Playground.Persistence.csproj`
    - `src/FakeAuth.Service/FakeAuth.Service.csproj`
  - _Requirements: 1.1_

- [ ] 2. Upgrade Microsoft.* packages to 10.x
  - Use `dotnet add package` to upgrade each Microsoft package to its latest stable 10.x version:
    - `Microsoft.AspNetCore.OpenApi` in API project
    - `Microsoft.EntityFrameworkCore.InMemory` in Persistence project
    - `Microsoft.Extensions.Logging.Abstractions` in Domain project
    - `Microsoft.Extensions.DependencyInjection.Abstractions` in Persistence project
  - _Requirements: 1.4_

- [ ] 3. Upgrade MediatR to 12.5.0
  - Upgrade MediatR from 12.4.1 to exactly 12.5.0 in all projects that reference it:
    - `src/MediatR.Playground.Model/MediatR.Playground.Model.csproj`
    - `src/MediatR.Playground.Domain/MediatR.Playground.Domain.csproj`
    - `src/MediatR.Playground.Pipelines/MediatR.Playground.Pipelines.csproj`
  - Use `dotnet add package MediatR --version 12.5.0` to pin the exact version
  - Do NOT upgrade to 13.x+ (commercial RPL-1.5 license)
  - _Requirements: 2.1, 2.6_

- [ ] 4. Upgrade third-party NuGet packages
  - [ ] 4.1 Upgrade FluentValidation packages
    - Upgrade `FluentValidation` to latest stable in Domain and Pipelines projects
    - Upgrade `FluentValidation.DependencyInjectionExtensions` to latest stable in Domain project
    - _Requirements: 3.1, 3.2_
  - [ ] 4.2 Upgrade FusionCache, Bogus, and Swashbuckle
    - Upgrade `ZiggyCreatures.FusionCache` to latest stable in Pipelines project
    - Upgrade `Bogus` to latest stable in FakeAuth.Service project
    - Upgrade `Swashbuckle.AspNetCore` to latest stable compatible with .NET 10 in API project
    - _Requirements: 3.3, 3.4, 3.5_

- [ ] 5. Build and test verification checkpoint
  - Run `dotnet build` on the solution and confirm it compiles without errors
  - If any breaking changes are introduced by the upgrades, adapt the source code to resolve them
  - Run `dotnet test` and confirm all baseline tests from Task 0 still pass after the upgrades
  - If tests fail, fix the issues before proceeding — ask the user if questions arise
  - _Requirements: 1.2, 1.3, 2.2, 2.3, 2.4, 2.5, 3.6, 3.7_

- [ ] 6. Create docs/ folder and documentation files
  - [ ] 6.1 Create `docs/pipelines.md`
    - Document the MediatR Pipeline concept and IPipelineBehavior pre/post processing mechanism
    - Document each pipeline behavior: LoggingBehavior, ValidationBehavior, CommandAuthorizationBehavior
    - Describe pipeline filtering via custom interfaces (ICommand, IQueryRequest, ITransactionCommand)
    - Document pipeline registration order and its effect on execution order
    - Include link to Medium article "C# .NET — MediatR Pipelines"
    - Reference source files in `src/MediatR.Playground.Pipelines/`
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_
  - [ ] 6.2 Create `docs/unit-of-work.md`
    - Document UnitOfWorkBehavior and how it manages transactions via IUnitOfWork
    - Describe the transactional flow: begin, execute, commit on success, rollback on error
    - Document ITransactionCommand interface for distinguishing transactional commands
    - Document the Repository pattern with IRepository and EntityFrameworkRepository
    - Include link to Medium article "C# .NET — Unit Of Work Pattern with MediatR Pipeline"
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  - [ ] 6.3 Create `docs/exception-handling.md`
    - Document exception handling via IRequestExceptionHandler
    - Describe ExceptionHandler vs InvalidOperationExceptionHandler for SampleCommand
    - Document RequestExceptionHandlerState and SetHandled mechanism
    - Include link to Medium article "C# .NET — Handle Exceptions with MediatR"
    - _Requirements: 7.1, 7.2, 7.3, 7.4_
  - [ ] 6.4 Create `docs/global-exception-handling.md`
    - Document GlobalExceptionHandlingBehavior as a pipeline behavior
    - Describe the difference between global (IPipelineBehavior) and request-specific (IRequestExceptionHandler) exception handling
    - Document when to use each approach: global for centralized logging, specific for fallback responses
    - Include link to Medium article "C# .NET — Global Exception Handler with MediatR"
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
  - [ ] 6.5 Create `docs/notifications.md`
    - Document the MediatR publish-subscribe pattern with INotification and INotificationHandler
    - Document ForeachAwaitPublisher (sequential) and TaskWhenAllPublisher (parallel) strategies
    - Describe MultipleNotificationPublisher and its strategy selection based on notification type
    - Document marker interfaces: IParallelNotification, IPriorityNotification, IDataUpdateNotification
    - Include link to Medium article "C# .NET — MediatR Notifications and Notification Publisher"
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_
  - [ ] 6.6 Create `docs/priority-notification-publisher.md`
    - Document PriorityNotificationPublisher and handler ordering by priority
    - Document IPriorityNotificationHandler interface and Priority property
    - Describe the reflection mechanism for reading priority and the default value (99)
    - Show how to create a notification handler with priority
    - This is new, self-contained content — no Medium article exists for this topic
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_
  - [ ] 6.7 Create `docs/stream-requests.md`
    - Document IStreamRequest and IAsyncEnumerable for streaming data retrieval
    - Document GenericStreamLoggingBehavior and SampleFilterStreamBehavior
    - Describe how IStreamPipelineBehavior differs from IPipelineBehavior and the yield return pattern
    - Document generic pipeline (no where constraints) vs specific pipeline (where clause on concrete type)
    - Include link to Medium article "C# .NET — Stream Request and Pipeline With MediatR"
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_
  - [ ] 6.8 Create `docs/caching.md`
    - Document CachingBehavior intercepting IQueryRequest queries with FusionCache
    - Document the cache key mechanism via IQueryRequest.CacheKey
    - Describe cache configuration: SetDuration, SetFailSafe, SetFactoryTimeouts
    - Explain the advantage of pipeline-level caching vs per-handler caching
    - Include link to Medium article "C# .NET — Caching Requests With MediatR Pipeline"
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5_

- [ ] 7. Checkpoint — Review documentation files
  - Verify all 8 documentation files exist in `docs/` with consistent tone and style
  - Verify all Medium article links are correct and all relative source code references are valid
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5_

- [ ] 8. Update README.md
  - Rewrite README.md with the following sections while maintaining the current tone and style:
    - Table of contents with internal anchors and links to `docs/` files
    - Package versions section noting .NET 10, MediatR 12.5.0 (Apache-2.0), and key dependencies
    - Project structure section describing the role of each project (API, Domain, Model, Pipelines, Persistence, FakeAuth)
    - Brief topic summaries for each documented topic with links to corresponding `docs/` files
    - Consolidated Medium articles section with all article links
    - Updated Swagger endpoints list covering all endpoint groups (Commands, Requests, Transactions, Notifications, Exceptions, Stream)
    - Getting started / Testing the application section with Swagger screenshot reference
  - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.6, 13.7_

- [ ] 9. Final checkpoint — Verify everything compiles, tests pass, and links resolve
  - Run `dotnet build` to confirm the solution still compiles after all changes
  - Run `dotnet test` to confirm all tests still pass
  - Verify all internal links between README and docs/ files use correct relative paths

## Notes

- A test project (Task 0) is created first to establish a green baseline before any upgrades, ensuring regressions are caught immediately.
- Build compilation (`dotnet build`) and test execution (`dotnet test`) are the primary verification gates for the upgrade tasks.
- MediatR is pinned to 12.5.0 specifically — do NOT upgrade to 13.x+ which uses the commercial RPL-1.5 license.
- Each documentation file references specific source code files and links to the corresponding Medium article (where one exists).
- The Priority Notification Publisher documentation (task 6.6) is entirely new content since no Medium article covers this topic.
