using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediatR.Playground.Persistence;

internal class SampleEntityConfiguration : IEntityTypeConfiguration<SampleEntity>
{
    public void Configure(EntityTypeBuilder<SampleEntity> builder)
    {
        builder.HasKey(t => t.Id);
    }
}
