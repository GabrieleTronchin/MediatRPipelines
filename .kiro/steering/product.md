# Product Overview

MediatR Playground is a reference/demo ASP.NET Core Web API that explores MediatR features, with a focus on pipeline behaviors. It serves as a companion codebase for a series of Medium articles.

The API exposes REST endpoints via Swagger that demonstrate:
- Request/response handling (commands and queries)
- Notification publishing (sequential, parallel, priority-ordered)
- Stream requests with pipeline filtering
- Exception handling (per-request and global)
- Caching via pipeline behavior
- Unit of Work pattern with transaction management

This is an educational playground, not a production application. There is no real database (EF Core InMemory) and no real auth (FakeAuth.Service with Bogus-generated data).
