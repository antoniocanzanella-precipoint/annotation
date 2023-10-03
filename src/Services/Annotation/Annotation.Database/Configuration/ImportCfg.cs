using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreciPoint.Ims.Services.Annotation.Domain;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Database.Configuration;

internal class ImportCfg : IEntityTypeConfiguration<Import>
{
    public void Configure(EntityTypeBuilder<Import> counterGroupBuilder)
    {
        counterGroupBuilder
            .ToTable("Imports", AnnotationDbContext.DefaultSchema)
            .HasComment("Contains rows that track all annotation imported with a specific file.");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.Id)
            .IsRequired()
            .HasComment("Uniquely identifies an import.");

        counterGroupBuilder
            .HasKey(counterGroup => counterGroup.Id);

        counterGroupBuilder
            .Property(counterGroup => counterGroup.FileName)
            .HasMaxLength(DomainConstants.FileNameLenght)
            .IsRequired()
            .HasComment("The file name");

        counterGroupBuilder
            .Property(counterGroup => counterGroup.File)
            .IsRequired()
            .HasComment("The file content");

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
            .HasIndex(counterGroup => counterGroup.CreatedBy);
    }
}