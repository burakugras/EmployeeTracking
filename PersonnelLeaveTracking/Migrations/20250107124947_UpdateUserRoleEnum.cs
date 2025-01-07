using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonnelLeaveTracking.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Users");

            
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"Role\" TYPE integer USING CASE " +
                                 "WHEN \"Role\" = 'Admin' THEN 0 " +
                                 "WHEN \"Role\" = 'User' THEN 1 " +
                                 "ELSE NULL END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Role sütununu tekrar string türüne dönüştürmek için ters işlem
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"Role\" TYPE text USING CASE " +
                                 "WHEN \"Role\" = 0 THEN 'Admin' " +
                                 "WHEN \"Role\" = 1 THEN 'User' " +
                                 "ELSE NULL END");

            // Title sütununu yeniden ekliyoruz
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
