using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Database.Configuration;

internal class SlideImageCfg : IEntityTypeConfiguration<SlideImage>
{
    public void Configure(EntityTypeBuilder<SlideImage> builder)
    {
        builder
            .ToTable("SlideImages", AnnotationDbContext.DefaultSchema)
            .HasComment("Contains rows that track all slide images meta information.");

        builder
            .Property(slideImage => slideImage.SlideImageId)
            .IsRequired()
            .HasComment("Uniquely identifies an import.");

        builder
            .HasKey(slideImage => slideImage.SlideImageId);

        builder
            .Property(slideImage => slideImage.Permission)
            .IsRequired()
            .HasComment("Define the global permission granted by the owner of a slide image.");

        builder
            .Property(slideImage => slideImage.OwnedBy)
            .IsRequired()
            .HasComment("Identify the owner of this slide image.");
    }
}