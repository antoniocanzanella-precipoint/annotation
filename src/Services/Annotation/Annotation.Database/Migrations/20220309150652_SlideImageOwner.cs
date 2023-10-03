using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PreciPoint.Ims.Services.Annotation.Enums;

#nullable disable

namespace PreciPoint.Ims.Services.Annotation.Database.Migrations
{
    public partial class SlideImageOwner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:annotation_permission", "disabled,view,draw")
                .Annotation("Npgsql:Enum:annotation_type", "point,marker,line,circle,rectangular,polygon,polyline,grid")
                .Annotation("Npgsql:Enum:annotation_visibility", "private,public,read_only,editable")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:annotation_type", "point,marker,line,circle,rectangular,polygon,polyline,grid")
                .OldAnnotation("Npgsql:Enum:annotation_visibility", "private,public,read_only,editable")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "SlideImages",
                schema: "ims",
                columns: table => new
                {
                    SlideImageId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Uniquely identifies an import."),
                    Permission = table.Column<AnnotationPermission>(type: "annotation_permission", nullable: false, comment: "Define the global permission granted by the owner of a slide image."),
                    OwnedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "Identify the owner of this slide image.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlideImages", x => x.SlideImageId);
                },
                comment: "Contains rows that track all slide images meta information.");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_SlideImages_SlideImageId",
                schema: "ims",
                table: "Annotations",
                column: "SlideImageId",
                principalSchema: "ims",
                principalTable: "SlideImages",
                principalColumn: "SlideImageId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_SlideImages_SlideImageId",
                schema: "ims",
                table: "Annotations");

            migrationBuilder.DropTable(
                name: "SlideImages",
                schema: "ims");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:annotation_type", "point,marker,line,circle,rectangular,polygon,polyline,grid")
                .Annotation("Npgsql:Enum:annotation_visibility", "private,public,read_only,editable")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:annotation_permission", "disabled,view,draw")
                .OldAnnotation("Npgsql:Enum:annotation_type", "point,marker,line,circle,rectangular,polygon,polyline,grid")
                .OldAnnotation("Npgsql:Enum:annotation_visibility", "private,public,read_only,editable")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
