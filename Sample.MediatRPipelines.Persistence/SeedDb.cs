using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.MediatRPipelines.Persistence;

public static class SeedDb
{
    public static void Initialize(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<SampleDbContext>() ?? throw new NullReferenceException($"Cannot find any service for {nameof(SampleDbContext)}");
        context.Database.EnsureCreated();
    }


}

