using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.LocalMigrations
{
    public partial class Local_AddTitleToProblem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Problems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Problems");
        }
    }
}
