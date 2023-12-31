﻿// <auto-generated />


#nullable disable

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PreciPoint.Ims.Services.Annotation.Database;
using PreciPoint.Ims.Services.Annotation.Enums;
namespace PreciPoint.Ims.Services.Annotation.Database.Migrations
{
    [DbContext(typeof(AnnotationDbContext))]
    [Migration("20220309150652_SlideImageOwner")]
    partial class SlideImageOwner
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "annotation_permission", new[] { "disabled", "view", "draw" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "annotation_type", new[] { "point", "marker", "line", "circle", "rectangular", "polygon", "polyline", "grid" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "annotation_visibility", new[] { "private", "public", "read_only", "editable" });
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.AnnotationShape", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Uniquely identifies an Annotation.");

                    b.Property<int[]>("Color")
                        .HasColumnType("integer[]")
                        .HasComment("Contains color information information");

                    b.Property<double?>("Confidence")
                        .HasColumnType("double precision")
                        .HasComment("Describe the confidence value of this annotation. used by AI and ML");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid")
                        .HasComment("Contains the user id that created the record");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Contains the creation date");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasComment("The description of annotation");

                    b.Property<Guid?>("FolderId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ImportId")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean")
                        .HasComment("Describe if the annotation is hidden.");

                    b.Property<string>("Label")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("The brief description of annotation");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid")
                        .HasComment("Contains the user id that modified the values");

                    b.Property<DateTime?>("LastModifiedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Contains the modification date");

                    b.Property<Geometry>("Shape")
                        .IsRequired()
                        .HasColumnType("Geometry")
                        .HasComment("Contains geometry information");

                    b.Property<Guid>("SlideImageId")
                        .HasColumnType("uuid")
                        .HasComment("The unique identifier of a slide image");

                    b.Property<AnnotationType>("Type")
                        .HasColumnType("annotation_type")
                        .HasComment("Describe the annotation type. defined by it's coordinates");

                    b.Property<AnnotationVisibility>("Visibility")
                        .HasColumnType("annotation_visibility")
                        .HasComment("Describe the annotation visibility.");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("FolderId");

                    b.HasIndex("ImportId");

                    b.HasIndex("Shape");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Shape"), "GIST");

                    b.HasIndex("SlideImageId");

                    b.ToTable("Annotations", "ims");

                    b.HasComment("Contains rows that define Annotations linked to a slide image.");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Counter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Uniquely identifies a Counter");

                    b.Property<Guid>("GroupCounterId")
                        .HasColumnType("uuid");

                    b.Property<Geometry>("Shape")
                        .IsRequired()
                        .HasColumnType("geometry (point)")
                        .HasComment("Contains geometry information");

                    b.HasKey("Id");

                    b.HasIndex("GroupCounterId");

                    b.HasIndex("Shape");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Shape"), "GIST");

                    b.ToTable("Counters", "ims");

                    b.HasComment("Contains rows that define counters that belongs to a specific groups.");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.CounterGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Uniquely identifies a Counter Group.");

                    b.Property<Guid>("AnnotationId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid")
                        .HasComment("Contains the user id that created the record");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Contains the creation date");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasComment("The description of annotation");

                    b.Property<string>("Label")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("The brief description of annotation");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid")
                        .HasComment("Contains the user id that modified the values");

                    b.Property<DateTime?>("LastModifiedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Contains the modification date");

                    b.HasKey("Id");

                    b.HasIndex("AnnotationId");

                    b.HasIndex("CreatedBy");

                    b.ToTable("CounterGroups", "ims");

                    b.HasComment("Contains rows that define counter groups linked to annotation.");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Folder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Uniquely identifies a Folder");

                    b.Property<string>("BriefDescription")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)")
                        .HasComment("Contains the folder brief description");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasComment("Contains the folder description");

                    b.Property<int>("DisplayOder")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasComment("Contains the folder name");

                    b.Property<Guid?>("ParentFolderId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ParentFolderId");

                    b.ToTable("Folders", "ims");

                    b.HasComment("Contains rows that define folders structure to group annotations");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Import", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Uniquely identifies an import.");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid")
                        .HasComment("Contains the user id that created the record");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Contains the creation date");

                    b.Property<byte[]>("File")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasComment("The file content");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("The file name");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid")
                        .HasComment("Contains the user id that modified the values");

                    b.Property<DateTime?>("LastModifiedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Contains the modification date");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.ToTable("Imports", "ims");

                    b.HasComment("Contains rows that track all annotation imported with a specific file.");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.SlideImage", b =>
                {
                    b.Property<Guid>("SlideImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasComment("Uniquely identifies an import.");

                    b.Property<Guid>("OwnedBy")
                        .HasColumnType("uuid")
                        .HasComment("Identify the owner of this slide image.");

                    b.Property<AnnotationPermission>("Permission")
                        .HasColumnType("annotation_permission")
                        .HasComment("Define the global permission granted by the owner of a slide image.");

                    b.HasKey("SlideImageId");

                    b.ToTable("SlideImages", "ims");

                    b.HasComment("Contains rows that track all slide images meta information.");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.AnnotationShape", b =>
                {
                    b.HasOne("PreciPoint.Ims.Services.Annotation.Domain.Model.Folder", "Folder")
                        .WithMany("Annotations")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PreciPoint.Ims.Services.Annotation.Domain.Model.Import", "Import")
                        .WithMany("Annotations")
                        .HasForeignKey("ImportId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PreciPoint.Ims.Services.Annotation.Domain.Model.SlideImage", "SlideImage")
                        .WithMany("Annotations")
                        .HasForeignKey("SlideImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Folder");

                    b.Navigation("Import");

                    b.Navigation("SlideImage");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Counter", b =>
                {
                    b.HasOne("PreciPoint.Ims.Services.Annotation.Domain.Model.CounterGroup", "CounterGroup")
                        .WithMany("Counters")
                        .HasForeignKey("GroupCounterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CounterGroup");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.CounterGroup", b =>
                {
                    b.HasOne("PreciPoint.Ims.Services.Annotation.Domain.Model.AnnotationShape", "Annotation")
                        .WithMany("CounterGroups")
                        .HasForeignKey("AnnotationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Annotation");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Folder", b =>
                {
                    b.HasOne("PreciPoint.Ims.Services.Annotation.Domain.Model.Folder", "ParentFolder")
                        .WithMany("SubFolders")
                        .HasForeignKey("ParentFolderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ParentFolder");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.AnnotationShape", b =>
                {
                    b.Navigation("CounterGroups");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.CounterGroup", b =>
                {
                    b.Navigation("Counters");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Folder", b =>
                {
                    b.Navigation("Annotations");

                    b.Navigation("SubFolders");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.Import", b =>
                {
                    b.Navigation("Annotations");
                });

            modelBuilder.Entity("PreciPoint.Ims.Services.Annotation.Domain.Model.SlideImage", b =>
                {
                    b.Navigation("Annotations");
                });
#pragma warning restore 612, 618
        }
    }
}
