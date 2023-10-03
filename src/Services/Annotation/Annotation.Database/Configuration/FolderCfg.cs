using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PreciPoint.Ims.Services.Annotation.Domain;
using PreciPoint.Ims.Services.Annotation.Domain.Model;

namespace PreciPoint.Ims.Services.Annotation.Database.Configuration;

internal class FolderCfg : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> folderBuilder)
    {
        folderBuilder
            .ToTable("Folders", AnnotationDbContext.DefaultSchema)
            .HasComment("Contains rows that define folders structure to group annotations");

        folderBuilder
            .Property(counter => counter.Id)
            .IsRequired()
            .HasComment("Uniquely identifies a Folder");

        folderBuilder
            .HasKey(counter => counter.Id);

        folderBuilder
            .Property(counter => counter.Name)
            .HasMaxLength(DomainConstants.FolderNameLength)
            .HasComment("Contains the folder name");

        folderBuilder
            .Property(counter => counter.Description)
            .HasMaxLength(DomainConstants.FolderDescriptionLength)
            .HasComment("Contains the folder description");

        folderBuilder
            .Property(counter => counter.BriefDescription)
            .HasMaxLength(DomainConstants.FolderBriefDescriptionLength)
            .HasComment("Contains the folder brief description");

        folderBuilder
            .HasOne(folder => folder.ParentFolder)
            .WithMany(folder => folder.SubFolders)
            .HasForeignKey(folder => folder.ParentFolderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}