using Microsoft.EntityFrameworkCore;

namespace MediatR.Playground.Persistence;

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options) { }

    public DbSet<SampleEntity> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleDbContext).Assembly);
    }
}
