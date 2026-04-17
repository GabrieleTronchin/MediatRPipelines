using MediatR.Playground.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Playground.Tests;

/// <summary>
/// WebApplicationFactory that uses a unique in-memory database name per instance.
/// This ensures complete data isolation from other test classes that share the
/// default "SampleDb" in-memory database.
/// </summary>
public class IsolatedDbWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"SampleDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FakeAuth:AlwaysAuthorize"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SampleDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Re-register with a unique database name
            services.AddDbContext<SampleDbContext>(options =>
            {
                options
                    .UseInMemoryDatabase(_dbName)
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });
        });
    }
}
