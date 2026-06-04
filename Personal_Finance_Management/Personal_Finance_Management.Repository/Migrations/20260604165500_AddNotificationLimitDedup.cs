using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Personal_Finance_Management.Repository.Migrations
{
    public partial class AddNotificationLimitDedup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "limit_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "period_key",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_type",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "threshold_type",
                table: "notifications",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_limit_dedup",
                table: "notifications",
                columns: new[] { "user_id", "limit_id", "target_type", "threshold_type", "period_key" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notifications_limit_dedup",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "limit_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "period_key",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "target_type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "threshold_type",
                table: "notifications");
        }
    }
}
