using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class SyncRecurrentKanbanStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Tasks SET KanbanStatus = 3 WHERE IsRecurrent = 1 AND IsCompleted = 0;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Tasks SET KanbanStatus = 0 WHERE IsRecurrent = 1 AND KanbanStatus = 3;"
            );
        }
    }
}
