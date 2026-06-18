using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class FixDateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            // Convert text date columns to timestamp with time zone
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tasks""
                    ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone,
                    ALTER COLUMN ""DueDate"" TYPE timestamp with time zone USING ""DueDate""::timestamp with time zone,
                    ALTER COLUMN ""AlarmTime"" TYPE timestamp with time zone USING ""AlarmTime""::timestamp with time zone;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Organizations""
                    ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""OrganizationMembers""
                    ALTER COLUMN ""JoinedAt"" TYPE timestamp with time zone USING ""JoinedAt""::timestamp with time zone;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""OrganizationInvites""
                    ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone,
                    ALTER COLUMN ""ResolvedAt"" TYPE timestamp with time zone USING ""ResolvedAt""::timestamp with time zone;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Notifications""
                    ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Comments""
                    ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            migrationBuilder.Sql(@"
                ALTER TABLE ""Tasks""
                    ALTER COLUMN ""CreatedAt"" TYPE text USING ""CreatedAt""::text,
                    ALTER COLUMN ""DueDate"" TYPE text USING ""DueDate""::text,
                    ALTER COLUMN ""AlarmTime"" TYPE text USING ""AlarmTime""::text;
            ");

            migrationBuilder.Sql(@"ALTER TABLE ""Organizations"" ALTER COLUMN ""CreatedAt"" TYPE text USING ""CreatedAt""::text;");
            migrationBuilder.Sql(@"ALTER TABLE ""OrganizationMembers"" ALTER COLUMN ""JoinedAt"" TYPE text USING ""JoinedAt""::text;");
            migrationBuilder.Sql(@"
                ALTER TABLE ""OrganizationInvites""
                    ALTER COLUMN ""CreatedAt"" TYPE text USING ""CreatedAt""::text,
                    ALTER COLUMN ""ResolvedAt"" TYPE text USING ""ResolvedAt""::text;
            ");
            migrationBuilder.Sql(@"ALTER TABLE ""Notifications"" ALTER COLUMN ""CreatedAt"" TYPE text USING ""CreatedAt""::text;");
            migrationBuilder.Sql(@"ALTER TABLE ""Comments"" ALTER COLUMN ""CreatedAt"" TYPE text USING ""CreatedAt""::text;");
        }
    }
}