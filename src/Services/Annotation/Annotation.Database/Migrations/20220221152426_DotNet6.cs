using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreciPoint.Ims.Services.Annotation.Database.Migrations
{
    public partial class DotNet6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "ims",
                table: "Imports",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Contains the modification date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldComment: "Contains the modification date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "ims",
                table: "Imports",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Contains the creation date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldComment: "Contains the creation date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "ims",
                table: "CounterGroups",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Contains the modification date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldComment: "Contains the modification date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "ims",
                table: "CounterGroups",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Contains the creation date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldComment: "Contains the creation date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "ims",
                table: "Annotations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Contains the modification date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true,
                oldComment: "Contains the modification date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "ims",
                table: "Annotations",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Contains the creation date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldComment: "Contains the creation date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "ims",
                table: "Imports",
                type: "timestamp without time zone",
                nullable: true,
                comment: "Contains the modification date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Contains the modification date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "ims",
                table: "Imports",
                type: "timestamp without time zone",
                nullable: false,
                comment: "Contains the creation date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Contains the creation date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "ims",
                table: "CounterGroups",
                type: "timestamp without time zone",
                nullable: true,
                comment: "Contains the modification date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Contains the modification date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "ims",
                table: "CounterGroups",
                type: "timestamp without time zone",
                nullable: false,
                comment: "Contains the creation date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Contains the creation date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "ims",
                table: "Annotations",
                type: "timestamp without time zone",
                nullable: true,
                comment: "Contains the modification date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Contains the modification date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                schema: "ims",
                table: "Annotations",
                type: "timestamp without time zone",
                nullable: false,
                comment: "Contains the creation date",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Contains the creation date");
        }
    }
}
