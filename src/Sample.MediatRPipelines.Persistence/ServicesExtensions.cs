using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Sample.MediatRPipelines.Persistence.Repository;
using Sample.MediatRPipelines.Persistence.UoW;

namespace Sample.MediatRPipelines.Persistence;

public static class ServicesExtensions
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services)
    {
        services.AddDbContext<SampleDbContext>(options =>
        {
            options
                .UseInMemoryDatabase("SampleDb")
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });

        services.AddTransient<IRepository<SampleEntity>, EntityFrameworkRepository<SampleEntity>>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
