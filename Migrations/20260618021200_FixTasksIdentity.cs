using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class FixTasksIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            // Make Tasks.Id auto-increment using a sequence
            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS ""Tasks_Id_seq"" OWNED BY ""Tasks"".""Id"";
                SELECT setval('""Tasks_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Tasks""), 0) + 1, false);
                ALTER TABLE ""Tasks"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Tasks_Id_seq""');
            ");

            // Also fix Comments.Id and Notifications.Id and OrganizationMembers.Id / OrganizationInvites.Id
            // in case they have the same issue
            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS ""Comments_Id_seq"" OWNED BY ""Comments"".""Id"";
                SELECT setval('""Comments_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Comments""), 0) + 1, false);
                ALTER TABLE ""Comments"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Comments_Id_seq""');
            ");

            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS ""Notifications_Id_seq"" OWNED BY ""Notifications"".""Id"";
                SELECT setval('""Notifications_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Notifications""), 0) + 1, false);
                ALTER TABLE ""Notifications"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Notifications_Id_seq""');
            ");

            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS ""Organizations_Id_seq"" OWNED BY ""Organizations"".""Id"";
                SELECT setval('""Organizations_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""Organizations""), 0) + 1, false);
                ALTER TABLE ""Organizations"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Organizations_Id_seq""');
            ");

            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS ""OrganizationMembers_Id_seq"" OWNED BY ""OrganizationMembers"".""Id"";
                SELECT setval('""OrganizationMembers_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""OrganizationMembers""), 0) + 1, false);
                ALTER TABLE ""OrganizationMembers"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""OrganizationMembers_Id_seq""');
            ");

            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS ""OrganizationInvites_Id_seq"" OWNED BY ""OrganizationInvites"".""Id"";
                SELECT setval('""OrganizationInvites_Id_seq""', COALESCE((SELECT MAX(""Id"") FROM ""OrganizationInvites""), 0) + 1, false);
                ALTER TABLE ""OrganizationInvites"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""OrganizationInvites_Id_seq""');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            migrationBuilder.Sql(@"ALTER TABLE ""Tasks"" ALTER COLUMN ""Id"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Comments"" ALTER COLUMN ""Id"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Notifications"" ALTER COLUMN ""Id"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Organizations"" ALTER COLUMN ""Id"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""OrganizationMembers"" ALTER COLUMN ""Id"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""OrganizationInvites"" ALTER COLUMN ""Id"" DROP DEFAULT;");
        }
    }
}