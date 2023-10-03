using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreciPoint.Ims.Services.Annotation.Domain;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Database.Configuration;

internal class CounterGroupCfg : IEntityTypeConfiguration<CounterGroup>
{
    public void Configure(EntityTypeBuilder<CounterGroup> counterGroupBuilder)
    {
        counterGroupBuilder
            .ToTable("CounterGroups", AnnotationDbContext.DefaultSchema)
            .HasComment("Contains rows that define counter groups linked to annotation.");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.Id)
            .IsRequired()
            .HasComment("Uniquely identifies a Counter Group.");

        counterGroupBuilder
            .HasKey(counterGroup => counterGroup.Id);

        counterGroupBuilder
            .Property(counterGroup => counterGroup.Label)
            .HasMaxLength(DomainConstants.GroupCounterLabelLength)
            .HasComment("The brief description of annotation");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.Description)
            .HasMaxLength(DomainConstants.GroupCounterDescriptionLength)
            .HasComment("The description of annotation");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.CreatedBy)
            .IsRequired()
            .HasComment("Contains the user id that created the record");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.CreatedDate)
            .IsRequired()
            .HasComment("Contains the creation date");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.LastModifiedBy)
            .HasComment("Contains the user id that modified the values");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.LastModifiedDate)
            .HasComment("Contains the modification date");

        counterGroupBuilder
            .HasOne(counterGroup => counterGroup.Annotation)
            .WithMany(counterGroup => counterGroup.CounterGroups)
            .HasForeignKey(counterGroup => counterGroup.AnnotationId)
            .OnDelete(DeleteBehavior.Cascade);

        counterGroupBuilder
            .HasIndex(counterGroup => counterGroup.CreatedBy);
    }
}