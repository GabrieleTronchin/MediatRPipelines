# Product Overview

MediatR Playground is a reference/demo ASP.NET Core Web API that explores MediatR features, with a focus on pipeline behaviors. It serves as a companion codebase for a series of Medium articles.

This version uses **MediatR 14.1.0** (commercial, RPL-1.5 license). The free Apache-2.0 version (MediatR 12.5.0) is available on the [`net10-mediatr12.5.0`](https://github.com/GabrieleTronchin/MediatRPipelines/commits/net10-mediatr12.5.0) branch.

The API exposes REST endpoints via Swagger that demonstrate:
- Request/response handling (commands and queries)
- Notification publishing (sequential, parallel, priority-ordered, de-duplication)
- Stream requests with pipeline filtering
- Exception handling (per-request and global)
- Caching via pipeline behavior
- Unit of Work pattern with transaction management

This is an educational playground, not a production application. There is no real database (EF Core InMemory) and no real auth (FakeAuth.Service with Bogus-generated data).
