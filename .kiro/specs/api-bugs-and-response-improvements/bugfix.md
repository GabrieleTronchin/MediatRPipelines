# Bugfix Requirements Document

## Introduction

This document describes a series of bugs and improvements found in the MediatR Playground API. The issues involve: (1) intermittent failure of `SampleCommand` caused by the fake authorization service generating random results, (2) GET endpoints returning empty/default entities after creation, (3) notification endpoints returning no response body, (4) the overly minimal response from the `AddSampleEntity` endpoint, and (5) insufficient Swagger/OpenAPI documentation across endpoints.

## Bug Analysis

### Current Behavior (Defect)

1.1 WHEN `POST /Requests/SampleCommand` is invoked THEN the system intermittently fails with random exceptions (`InvalidOperationException`, `InvalidDataException`, `ArgumentOutOfRangeException`, `ArgumentException`) because the `CommandAuthorizationBehavior` uses `AuthService.OperationAlowed()` which generates random boolean results via Bogus, and there is no configuration parameter to disable this behavior

1.2 WHEN an entity is created via `POST /TransactionRequests/AddSampleEntity` and subsequently retrieved via `GET /TransactionRequests/SampleEntity/{id}` THEN the system returns an entity with default values (`id: 00000000-0000-0000-0000-000000000000`, `eventTime: 0001-01-01T00:00:00`, `description: ""`) because the `Repository.Add()` method adds the entity to the `DbSet` but the `UnitOfWorkBehavior` does not effectively persist the data — either `SaveChangesAsync()` is not called after the transaction commit, or the transaction commit does not actually persist data in the in-memory database

1.3 WHEN `POST /Notifications/SequentialNotification` is invoked THEN the system returns no body in the HTTP response because the endpoint executes `mediator.Publish()` without returning a result, and the method returns `Task` (void) instead of a response object

1.4 WHEN `POST /Notifications/ParallelNotification` is invoked THEN the system returns no body in the HTTP response because the endpoint assigns the result of `mediator.Publish()` to a local variable without `await` or returning a value

1.5 WHEN `POST /Notifications/SamplePriorityNotification` is invoked THEN the system returns no body in the HTTP response because the endpoint assigns the result of `mediator.Publish()` to a local variable without `await` or returning a value

1.6 WHEN `POST /TransactionRequests/AddSampleEntity` is invoked with a valid body THEN the system returns only `{ "isSuccess": true }` without including the created entity details (id, description, eventTime)

1.7 WHEN the user consults the Swagger/OpenAPI documentation THEN the endpoints do not present descriptions, summaries, response type information, or HTTP status codes, making the documentation unhelpful for understanding the API behavior

### Expected Behavior (Correct)

2.1 WHEN `POST /Requests/SampleCommand` is invoked THEN the system SHALL allow controlling the authorization behavior via a configuration parameter in `appsettings.json` (e.g., `FakeAuth:AlwaysAuthorize`) which, when enabled, bypasses the random logic of `AuthService` and always authorizes the request, eliminating intermittent failures

2.2 WHEN an entity is created via `POST /TransactionRequests/AddSampleEntity` and subsequently retrieved via `GET /TransactionRequests/SampleEntity/{id}` or `GET /TransactionRequests/SampleEntity` THEN the system SHALL return the entity with the correct values (id, description, eventTime) matching those sent in the creation request

2.3 WHEN `POST /Notifications/SequentialNotification` is invoked THEN the system SHALL correctly `await` `mediator.Publish()` and return an HTTP response with a body confirming the notification was published (e.g., an object with id and timestamp of the notification)

2.4 WHEN `POST /Notifications/ParallelNotification` is invoked THEN the system SHALL correctly `await` `mediator.Publish()` and return an HTTP response with a body confirming the notification was published

2.5 WHEN `POST /Notifications/SamplePriorityNotification` is invoked THEN the system SHALL correctly `await` `mediator.Publish()` and return an HTTP response with a body confirming the notification was published

2.6 WHEN `POST /TransactionRequests/AddSampleEntity` is invoked with a valid body THEN the system SHALL return the complete details of the created entity, including id, description, and eventTime, in addition to the success status

2.7 WHEN the user consults the Swagger/OpenAPI documentation THEN the system SHALL present for each endpoint: a descriptive summary, a detailed description of the behavior, documented response types (with `Produces`), and appropriate HTTP status codes (200, 400, 404, 500)

### Unchanged Behavior (Regression Prevention)

3.1 WHEN the configuration parameter `FakeAuth:AlwaysAuthorize` is disabled (or absent) THEN the system SHALL CONTINUE TO use the random logic of `AuthService.OperationAlowed()` for command authorization, maintaining the original playground behavior

3.2 WHEN `GET /TransactionRequests/SampleEntity` is invoked without having created any entities THEN the system SHALL CONTINUE TO return an empty list

3.3 WHEN `GET /TransactionRequests/SampleEntity/{id}` is invoked with a non-existent id THEN the system SHALL CONTINUE TO return an empty/default result

3.4 WHEN the exception endpoints are invoked (`POST /Exceptions/SampleCommandWithIOException`, `POST /Exceptions/SampleCommandWithException`, `GET /Exceptions/NotFoundExceptionGlobalHandler`) THEN the system SHALL CONTINUE TO handle exceptions as expected by the respective handlers

3.5 WHEN the stream endpoints are invoked (`GET /StreamRequests/SampleStreamEntity`, `GET /StreamRequests/SampleStreamEntityWithPipeFilter`) THEN the system SHALL CONTINUE TO work correctly with the existing stream pipelines

3.6 WHEN `POST /Requests/SampleRequest` is invoked THEN the system SHALL CONTINUE TO process the request through the pipelines without being affected by the authorization changes

3.7 WHEN the validation pipeline receives a `SampleCommand` with invalid data (empty Id or empty Description) THEN the system SHALL CONTINUE TO return validation errors via FluentValidation
