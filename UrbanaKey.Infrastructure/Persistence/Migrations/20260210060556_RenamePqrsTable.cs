using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanaKey.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePqrsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PqrsEntries",
                table: "PqrsEntries");

            migrationBuilder.RenameTable(
                name: "PqrsEntries",
                newName: "PQRS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PQRS",
                table: "PQRS",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PQRS",
                table: "PQRS");

            migrationBuilder.RenameTable(
                name: "PQRS",
                newName: "PqrsEntries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PqrsEntries",
                table: "PqrsEntries",
                column: "Id");
        }
    }
}
