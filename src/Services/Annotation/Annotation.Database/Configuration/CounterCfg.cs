using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Database.Configuration;

internal class CounterCfg : IEntityTypeConfiguration<Counter>
{
    public void Configure(EntityTypeBuilder<Counter> counterBuilder)
    {
        counterBuilder
            .ToTable("Counters", AnnotationDbContext.DefaultSchema)
            .HasComment("Contains rows that define counters that belongs to a specific groups.");

        counterBuilder
            .Property(counter => counter.Id)
            .IsRequired()
            .HasComment("Uniquely identifies a Counter");

        counterBuilder
            .HasKey(counter => counter.Id);

        counterBuilder
            .Property(counter => counter.Shape)
            .IsRequired()
            .HasColumnType("geometry (point)")
            .HasComment("Contains geometry information");

        counterBuilder
            .HasOne(counter => counter.CounterGroup)
            .WithMany(counterGroup => counterGroup.Counters)
            .HasForeignKey(counterGroup => counterGroup.GroupCounterId)
            .OnDelete(DeleteBehavior.Cascade);

        counterBuilder
            .HasIndex(counter => counter.Shape)
            .HasMethod("GIST");
    }
}