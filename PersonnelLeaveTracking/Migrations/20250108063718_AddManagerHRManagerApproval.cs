using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonnelLeaveTracking.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerHRManagerApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "LeaveRequests");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByHRManager",
                table: "LeaveRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByManager",
                table: "LeaveRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedByHRManager",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByManager",
                table: "LeaveRequests");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "LeaveRequests",
                type: "text",
                nullable: true);
        }
    }
}
