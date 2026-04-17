using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MediatR.Playground.Tests;

/// <summary>
/// WebApplicationFactory that does NOT replace the random FakeAuth service.
/// This factory preserves the original (buggy) behavior so exploration tests
/// can surface counterexamples that prove each bug exists.
/// It explicitly disables the AlwaysAuthorize toggle so the random auth
/// behavior from the original code is preserved.
/// </summary>
public class UnfixedWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Override the AlwaysAuthorize setting to false so the random
        // AuthService behavior is preserved for exploration/preservation tests.
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FakeAuth:AlwaysAuthorize"] = "false"
            });
        });
    }
}
