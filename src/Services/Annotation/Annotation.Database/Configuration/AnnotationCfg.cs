using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreciPoint.Ims.Services.Annotation.Domain;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Database.Configuration;

internal class AnnotationCfg : IEntityTypeConfiguration<AnnotationShape>
{
    public void Configure(EntityTypeBuilder<AnnotationShape> annotationBuilder)
    {
        annotationBuilder
            .ToTable("Annotations", AnnotationDbContext.DefaultSchema)
            .HasComment("Contains rows that define Annotations linked to a slide image.");

        annotationBuilder
            .Property(annotation => annotation.Id)
            .IsRequired()
            .HasComment("Uniquely identifies an Annotation.");

        annotationBuilder
            .HasKey(annotation => annotation.Id);

        annotationBuilder
            .Property(annotation => annotation.SlideImageId)
            .IsRequired()
            .HasComment("The unique identifier of a slide image");

        annotationBuilder
            .Property(annotation => annotation.Type)
            .IsRequired()
            .HasComment("Describe the annotation type. defined by it's coordinates");

        annotationBuilder
            .Property(annotation => annotation.Visibility)
            .IsRequired()
            .HasComment("Describe the annotation visibility.");

        annotationBuilder
            .Property(annotation => annotation.Label)
            .HasMaxLength(DomainConstants.AnnotationLabelLength)
            .HasComment("The brief description of annotation");

        annotationBuilder
            .Property(annotation => annotation.Description)
            .HasMaxLength(DomainConstants.AnnotationDescriptionLength)
            .HasComment("The description of annotation");

        annotationBuilder
            .Property(annotation => annotation.Shape)
            .IsRequired()
            .HasColumnType("Geometry")
            .HasComment("Contains geometry information");

        annotationBuilder
            .Property(annotation => annotation.Color)
            .HasComment("Contains color information information");

        annotationBuilder
            .Property(annotation => annotation.Confidence)
            .HasComment("Describe the confidence value of this annotation. used by AI and ML");

        annotationBuilder
            .Property(annotation => annotation.IsVisible)
            .IsRequired()
            .HasComment("Describe if the annotation is hidden.");

        annotationBuilder
            .Property(annotation => annotation.CreatedBy)
            .IsRequired()
            .HasComment("Contains the user id that created the record");

        annotationBuilder
            .Property(annotation => annotation.CreatedDate)
            .IsRequired()
            .HasComment("Contains the creation date");

        annotationBuilder
            .Property(annotation => annotation.LastModifiedBy)
            .HasComment("Contains the user id that modified the values");

        annotationBuilder
            .Property(annotation => annotation.LastModifiedDate)
            .HasComment("Contains the modification date");

        annotationBuilder
            .HasIndex(annotation => annotation.SlideImageId);

        annotationBuilder
            .HasIndex(annotation => annotation.CreatedBy);

        annotationBuilder
            .HasIndex(annotation => annotation.Shape)
            .HasMethod("GIST");

        annotationBuilder
            .HasOne(annotation => annotation.Import)
            .WithMany(import => import.Annotations)
            .HasForeignKey(annotation => annotation.ImportId)
            .OnDelete(DeleteBehavior.Cascade);

        annotationBuilder
            .HasOne(annotation => annotation.Folder)
            .WithMany(folder => folder.Annotations)
            .HasForeignKey(annotation => annotation.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        annotationBuilder
            .HasOne(annotation => annotation.SlideImage)
            .WithMany(slideImage => slideImage.Annotations)
            .HasForeignKey(annotation => annotation.SlideImageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}