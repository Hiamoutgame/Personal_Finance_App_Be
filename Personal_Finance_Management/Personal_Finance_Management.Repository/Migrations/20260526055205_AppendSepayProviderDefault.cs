using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Personal_Finance_Management.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AppendSepayProviderDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE bank_connection_sessions ALTER COLUMN provider_code SET DEFAULT 'sepay';");
            migrationBuilder.Sql("UPDATE bank_connection_sessions SET provider_code = 'sepay' WHERE provider_code = 'casso';");
            migrationBuilder.Sql("UPDATE financial_accounts SET provider_code = 'sepay', provider_name = 'SePay' WHERE provider_code = 'casso';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE bank_connection_sessions ALTER COLUMN provider_code SET DEFAULT 'casso';");
            migrationBuilder.Sql("UPDATE bank_connection_sessions SET provider_code = 'casso' WHERE provider_code = 'sepay';");
            migrationBuilder.Sql("UPDATE financial_accounts SET provider_code = 'casso', provider_name = 'Casso' WHERE provider_code = 'sepay';");
        }
    }
}
