using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.MediatRPipelines.Persistence;

public static class ServicesExtensions
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services)
    {

        services.AddDbContext<SampleDbContext>(options =>
        {
            options.UseInMemoryDatabase("SampleDb")
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });

        return services;

    }
}
