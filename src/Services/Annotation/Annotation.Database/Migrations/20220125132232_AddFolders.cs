using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PreciPoint.Ims.Services.Annotation.Database.Migrations
{
    public partial class AddFolders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FolderId",
                schema: "ims",
                table: "Annotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Folders",
                schema: "ims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Uniquely identifies a Folder"),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Contains the folder name"),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: "Contains the folder description"),
                    BriefDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Contains the folder brief description"),
                    DisplayOder = table.Column<int>(type: "integer", nullable: false),
                    ParentFolderId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalSchema: "ims",
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Contains rows that define folders structure to group annotations");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_FolderId",
                schema: "ims",
                table: "Annotations",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderId",
                schema: "ims",
                table: "Folders",
                column: "ParentFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_Folders_FolderId",
                schema: "ims",
                table: "Annotations",
                column: "FolderId",
                principalSchema: "ims",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_Folders_FolderId",
                schema: "ims",
                table: "Annotations");

            migrationBuilder.DropTable(
                name: "Folders",
                schema: "ims");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_FolderId",
                schema: "ims",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "FolderId",
                schema: "ims",
                table: "Annotations");
        }
    }
}
