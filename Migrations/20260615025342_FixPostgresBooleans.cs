using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class FixPostgresBooleans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            // Fix AspNetUsers boolean columns
            migrationBuilder.Sql(@"
                ALTER TABLE ""AspNetUsers""
                    ALTER COLUMN ""EmailConfirmed"" TYPE boolean USING ""EmailConfirmed""::boolean,
                    ALTER COLUMN ""PhoneNumberConfirmed"" TYPE boolean USING ""PhoneNumberConfirmed""::boolean,
                    ALTER COLUMN ""TwoFactorEnabled"" TYPE boolean USING ""TwoFactorEnabled""::boolean,
                    ALTER COLUMN ""LockoutEnabled"" TYPE boolean USING ""LockoutEnabled""::boolean;
            ");

            // Fix Tasks boolean columns
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tasks""
                    ALTER COLUMN ""IsCompleted"" TYPE boolean USING ""IsCompleted""::boolean,
                    ALTER COLUMN ""HasAlarm"" TYPE boolean USING ""HasAlarm""::boolean,
                    ALTER COLUMN ""IsRecurrent"" TYPE boolean USING ""IsRecurrent""::boolean;
            ");

            // Fix Organizations boolean column
            migrationBuilder.Sql(@"
                ALTER TABLE ""Organizations""
                    ALTER COLUMN ""AllowJoinRequests"" TYPE boolean USING ""AllowJoinRequests""::boolean;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            migrationBuilder.Sql(@"
                ALTER TABLE ""AspNetUsers""
                    ALTER COLUMN ""EmailConfirmed"" TYPE integer USING ""EmailConfirmed""::integer,
                    ALTER COLUMN ""PhoneNumberConfirmed"" TYPE integer USING ""PhoneNumberConfirmed""::integer,
                    ALTER COLUMN ""TwoFactorEnabled"" TYPE integer USING ""TwoFactorEnabled""::integer,
                    ALTER COLUMN ""LockoutEnabled"" TYPE integer USING ""LockoutEnabled""::integer;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Tasks""
                    ALTER COLUMN ""IsCompleted"" TYPE integer USING ""IsCompleted""::integer,
                    ALTER COLUMN ""HasAlarm"" TYPE integer USING ""HasAlarm""::integer,
                    ALTER COLUMN ""IsRecurrent"" TYPE integer USING ""IsRecurrent""::integer;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Organizations""
                    ALTER COLUMN ""AllowJoinRequests"" TYPE integer USING ""AllowJoinRequests""::integer;
            ");
        }
    }
}