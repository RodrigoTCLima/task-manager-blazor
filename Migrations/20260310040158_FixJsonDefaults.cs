using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class FixJsonDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "'[]'",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedByUserId",
                table: "Tasks",
                type: "TEXT",
                nullable: true,
                defaultValueSql: "'[]'",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReviewByUserId",
                table: "Tasks",
                type: "TEXT",
                nullable: true,
                defaultValueSql: "'[]'",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DependencyOnTaskIds",
                table: "Tasks",
                type: "TEXT",
                nullable: true,
                defaultValueSql: "'[]'",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValueSql: "'[]'");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedByUserId",
                table: "Tasks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValueSql: "'[]'");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewByUserId",
                table: "Tasks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValueSql: "'[]'");

            migrationBuilder.AlterColumn<string>(
                name: "DependencyOnTaskIds",
                table: "Tasks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValueSql: "'[]'");
        }
    }
}
