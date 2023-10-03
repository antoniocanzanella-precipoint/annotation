using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using PreciPoint.Ims.Services.Annotation.Enums;

namespace PreciPoint.Ims.Services.Annotation.Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ims");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:annotation_type", "point,marker,line,circle,rectangular,polygon,polyline,grid")
                .Annotation("Npgsql:Enum:annotation_visibility", "private,public")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "Imports",
                schema: "ims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Uniquely identifies an import."),
                    FileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "The file name"),
                    File = table.Column<byte[]>(type: "bytea", nullable: false, comment: "The file content"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "Contains the user id that created the record"),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Contains the creation date"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "Contains the user id that modified the values"),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Contains the modification date")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imports", x => x.Id);
                },
                comment: "Contains rows that track all annotation imported with a specific file.");

            migrationBuilder.CreateTable(
                name: "Annotations",
                schema: "ims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Uniquely identifies an Annotation."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "The brief description of annotation"),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: "The description of annotation"),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, comment: "Describe if the annotation is hidden."),
                    Confidence = table.Column<double>(type: "double precision", nullable: true, comment: "Describe the confidence value of this annotation. used by AI and ML"),
                    Shape = table.Column<Geometry>(type: "Geometry", nullable: false, comment: "Contains geometry information"),
                    Color = table.Column<int[]>(type: "integer[]", nullable: true, comment: "Contains color information information"),
                    SlideImageId = table.Column<Guid>(type: "uuid", nullable: false, comment: "The unique identifier of a slide image"),
                    Type = table.Column<AnnotationType>(type: "annotation_type", nullable: false, comment: "Describe the annotation type. defined by it's coordinates"),
                    Visibility = table.Column<AnnotationVisibility>(type: "annotation_visibility", nullable: false, comment: "Describe the annotation visibility."),
                    ImportId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "Contains the user id that created the record"),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Contains the creation date"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "Contains the user id that modified the values"),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Contains the modification date")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Annotations_Imports_ImportId",
                        column: x => x.ImportId,
                        principalSchema: "ims",
                        principalTable: "Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Contains rows that define Annotations linked to a slide image.");

            migrationBuilder.CreateTable(
                name: "CounterGroups",
                schema: "ims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Uniquely identifies a Counter Group."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "The brief description of annotation"),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: "The description of annotation"),
                    AnnotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "Contains the user id that created the record"),
                    CreatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "Contains the creation date"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "Contains the user id that modified the values"),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "Contains the modification date")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterGroups_Annotations_AnnotationId",
                        column: x => x.AnnotationId,
                        principalSchema: "ims",
                        principalTable: "Annotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Contains rows that define counter groups linked to annotation.");

            migrationBuilder.CreateTable(
                name: "Counters",
                schema: "ims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Uniquely identifies a Counter"),
                    Shape = table.Column<Geometry>(type: "geometry (point)", nullable: false, comment: "Contains geometry information"),
                    GroupCounterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Counters_CounterGroups_GroupCounterId",
                        column: x => x.GroupCounterId,
                        principalSchema: "ims",
                        principalTable: "CounterGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Contains rows that define counters that belongs to a specific groups.");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_CreatedBy",
                schema: "ims",
                table: "Annotations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_ImportId",
                schema: "ims",
                table: "Annotations",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_Shape",
                schema: "ims",
                table: "Annotations",
                column: "Shape")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_SlideImageId",
                schema: "ims",
                table: "Annotations",
                column: "SlideImageId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterGroups_AnnotationId",
                schema: "ims",
                table: "CounterGroups",
                column: "AnnotationId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterGroups_CreatedBy",
                schema: "ims",
                table: "CounterGroups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Counters_GroupCounterId",
                schema: "ims",
                table: "Counters",
                column: "GroupCounterId");

            migrationBuilder.CreateIndex(
                name: "IX_Counters_Shape",
                schema: "ims",
                table: "Counters",
                column: "Shape")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Imports_CreatedBy",
                schema: "ims",
                table: "Imports",
                column: "CreatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counters",
                schema: "ims");

            migrationBuilder.DropTable(
                name: "CounterGroups",
                schema: "ims");

            migrationBuilder.DropTable(
                name: "Annotations",
                schema: "ims");

            migrationBuilder.DropTable(
                name: "Imports",
                schema: "ims");
        }
    }
}
