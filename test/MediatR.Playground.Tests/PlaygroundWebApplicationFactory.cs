using FakeAuth.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Playground.Tests;

/// <summary>
/// Custom WebApplicationFactory that replaces the random FakeAuth service
/// with a deterministic one so tests are not flaky.
/// </summary>
public class PlaygroundWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the random auth service with one that always succeeds
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAuthService));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddTransient<IAuthService, AlwaysSuccessAuthService>();
        });
    }
}

/// <summary>
/// A deterministic auth service that always returns success.
/// </summary>
public class AlwaysSuccessAuthService : IAuthService
{
    public AuthResponse OperationAlowed()
    {
        return new AuthResponse { IsSuccess = true };
    }
}
