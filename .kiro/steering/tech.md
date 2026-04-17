# Tech Stack

## Runtime & Framework
- **.NET 10** (LTS) — all projects target `net10.0`
- **ASP.NET Core** minimal API (no controllers)
- **C#** with nullable reference types and implicit usings enabled

## Key Libraries

| Package | Version | Purpose |
|---------|---------|---------|
| MediatR | 14.1.0 | Mediator pattern (commercial RPL-1.5 license — requires license key) |
| MediatR.Contracts | 2.0.1 | Lightweight contract interfaces (Model project only) |
| FluentValidation | 12.1.1 | Request validation in pipeline behaviors |
| ZiggyCreatures.FusionCache | 2.6.0 | Caching layer for query pipelines |
| Swashbuckle.AspNetCore | 10.1.7 | Swagger UI / OpenAPI |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.6 | In-memory database for persistence |
| Bogus | 35.6.5 | Fake data generation (FakeAuth.Service) |

### MediatR Package Split
- **Domain** and **Pipelines** projects reference the full `MediatR 14.1.0` package (handlers, DI, pipeline behaviors)
- **Model** project references only `MediatR.Contracts 2.0.1` (lightweight interfaces: `IRequest`, `INotification`, etc.)

### MediatR License Key
MediatR 14.1.0 requires a license key configured via `appsettings.json` / `appsettings.Development.json` and read through `IConfiguration`. See `docs/upgrade-mediatr-14.md` for details.

### Free Version
The last free Apache-2.0 release (MediatR 12.5.0) is available on the [`net10-mediatr12.5.0`](https://github.com/GabrieleTronchin/MediatRPipelines/commits/net10-mediatr12.5.0) branch.

## Build System
- Solution file: `src/MediatR.Playground.sln`
- SDK-style `.csproj` files
- Test project: `src/MediatR.Playground.Tests` (xUnit)

## Common Commands

```bash
# Restore dependencies
dotnet restore src/MediatR.Playground.sln

# Build the solution
dotnet build src/MediatR.Playground.sln

# Run tests
dotnet test src/MediatR.Playground.sln

# Run the API
dotnet run --project src/MediatR.Playground.API
```

Swagger UI is available at the default launch URL when running in Development mode.

## Important Constraints
- MediatR license key must be configured before running the API
- Pipeline behaviors are registered via `cfg.AddOpenBehavior()` / `cfg.AddOpenStreamBehavior()` inside the `AddMediatR` lambda (MediatR 14 pattern)
