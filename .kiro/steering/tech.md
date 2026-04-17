# Tech Stack

## Runtime & Framework
- **.NET 10** (LTS) — all projects target `net10.0`
- **ASP.NET Core** minimal API (no controllers)
- **C#** with nullable reference types and implicit usings enabled

## Key Libraries

| Package | Version | Purpose |
|---------|---------|---------|
| MediatR | 12.5.0 | Mediator pattern (last free Apache-2.0 version — do NOT upgrade to 13+) |
| FluentValidation | 12.1.1 | Request validation in pipeline behaviors |
| ZiggyCreatures.FusionCache | 2.6.0 | Caching layer for query pipelines |
| Swashbuckle.AspNetCore | 10.1.7 | Swagger UI / OpenAPI |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.6 | In-memory database for persistence |
| Bogus | 35.6.5 | Fake data generation (FakeAuth.Service) |

## Build System
- Solution file: `src/MediatR.Playground.sln`
- SDK-style `.csproj` files
- No test projects currently exist in the solution

## Common Commands

```bash
# Restore dependencies
dotnet restore src/MediatR.Playground.sln

# Build the solution
dotnet build src/MediatR.Playground.sln

# Run the API
dotnet run --project src/MediatR.Playground.API
```

Swagger UI is available at the default launch URL when running in Development mode.

## Important Constraints
- MediatR must stay at **12.5.0** — versions 13+ use a commercial license (RPL-1.5)
- No test framework is set up yet; if adding tests, choose xUnit (standard for .NET)
