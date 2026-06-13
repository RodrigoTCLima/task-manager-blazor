using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddKanbanStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KanbanStatus",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KanbanViewOthersPolicy",
                table: "Organizations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KanbanViewPolicy",
                table: "Organizations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KanbanStatus",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "KanbanViewOthersPolicy",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "KanbanViewPolicy",
                table: "Organizations");
        }
    }
}
