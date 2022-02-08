using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.LocalMigrations
{
    public partial class Local_RenameProblemDataToSourceAndAddConditionalInput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Problems",
                newName: "Source");

            migrationBuilder.AddColumn<uint>(
                name: "ProblemInputId",
                table: "Problems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProblemsInputs",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProblemId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemsInputs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InputValues",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ProblemInputId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InputValues_ProblemsInputs_ProblemInputId",
                        column: x => x.ProblemInputId,
                        principalTable: "ProblemsInputs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Problems_ProblemInputId",
                table: "Problems",
                column: "ProblemInputId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InputValues_ProblemInputId",
                table: "InputValues",
                column: "ProblemInputId");

            migrationBuilder.AddForeignKey(
                name: "FK_Problems_ProblemsInputs_ProblemInputId",
                table: "Problems",
                column: "ProblemInputId",
                principalTable: "ProblemsInputs",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Problems_ProblemsInputs_ProblemInputId",
                table: "Problems");

            migrationBuilder.DropTable(
                name: "InputValues");

            migrationBuilder.DropTable(
                name: "ProblemsInputs");

            migrationBuilder.DropIndex(
                name: "IX_Problems_ProblemInputId",
                table: "Problems");

            migrationBuilder.DropColumn(
                name: "ProblemInputId",
                table: "Problems");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Problems",
                newName: "Data");
        }
    }
}
