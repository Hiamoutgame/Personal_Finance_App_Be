using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Personal_Finance_Management.Repository.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260516123700_AddFinancialAccountDefaultConstraint")]
    public partial class AddFinancialAccountDefaultConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_financial_accounts_user_default",
                table: "financial_accounts");

            migrationBuilder.Sql("""
                UPDATE financial_accounts fa
                SET is_default = FALSE,
                    updated_at = NOW()
                WHERE fa.is_default = TRUE
                  AND fa.id NOT IN (
                      SELECT DISTINCT ON (user_id) id
                      FROM financial_accounts
                      WHERE is_default = TRUE
                      ORDER BY user_id, is_active DESC, updated_at DESC, created_at DESC, id
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "ux_financial_accounts_one_default_per_user",
                table: "financial_accounts",
                column: "user_id",
                unique: true,
                filter: "\"is_default\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_financial_accounts_one_default_per_user",
                table: "financial_accounts");

            migrationBuilder.CreateIndex(
                name: "ix_financial_accounts_user_default",
                table: "financial_accounts",
                columns: new[] { "user_id", "is_default" });
        }
    }
}
