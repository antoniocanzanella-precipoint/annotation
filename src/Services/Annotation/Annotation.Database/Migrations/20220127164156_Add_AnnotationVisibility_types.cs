using Microsoft.EntityFrameworkCore.Migrations;

namespace PreciPoint.Ims.Services.Annotation.Database.Migrations
{
    public partial class Add_AnnotationVisibility_types : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TYPE annotation_visibility ADD VALUE if not exists 'read_only' AFTER 'public';");
            migrationBuilder.Sql("ALTER TYPE annotation_visibility ADD VALUE if not exists 'editable' AFTER 'public';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
