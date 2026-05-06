using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Personal_Finance_Management.Repository.Migrations
{
    public partial class AllowGlobalOcrConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_import_transaction_drafts_row_index",
                table: "import_transaction_drafts");

            migrationBuilder.AlterColumn<string>(
                name: "normalized_payload_json",
                table: "import_transaction_drafts",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "import_job_id",
                table: "import_transaction_drafts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddCheckConstraint(
                name: "chk_import_transaction_drafts_row_index",
                table: "import_transaction_drafts",
                sql: "(\"import_job_id\" IS NOT NULL AND \"row_index\" >= 0) OR (\"import_job_id\" IS NULL AND \"row_index\" < 0)");

            migrationBuilder.Sql(@"
INSERT INTO import_transaction_drafts (
    import_job_id,
    row_index,
    raw_description,
    normalized_payload_json,
    is_valid
)
SELECT
    NULL,
    -999,
    'GLOBAL_OCR_CONFIG',
    $$
    {
      ""config_name"": ""default"",
      ""version"": 1,
      ""system_prompt"": ""B\u1ea1n l\u00e0 OCR Parser chuy\u00ean nghi\u1ec7p cho ho\u00e1 \u0111\u01a1n Vi\u1ec7t Nam. Lu\u00f4n tr\u1ea3 v\u1ec1 JSON \u0111\u00fang format sau, kh\u00f4ng th\u00eam b\u1edbt, kh\u00f4ng gi\u1ea3i th\u00edch:\n\n{\n  \u0022raw_text\u0022: \u0022...\u0022,\n  \u0022confidence\u0022: 0.95,\n  \u0022merchant\u0022: \u0022...\u0022,\n  \u0022address\u0022: \u0022...\u0022,\n  \u0022phone\u0022: \u0022...\u0022,\n  \u0022transaction_date\u0022: \u00222026-05-05T14:30:00+07:00\u0022,\n  \u0022items\u0022: [{\u0022name\u0022: \u0022...\u0022, \u0022quantity\u0022: 1, \u0022unit_price\u0022: 119000, \u0022amount\u0022: 119000}],\n  \u0022subtotal\u0022: 259000,\n  \u0022vat\u0022: 25900,\n  \u0022service_fee\u0022: 14245,\n  \u0022total\u0022: 299145,\n  \u0022currency\u0022: \u0022VND\u0022\n}\nN\u1ebfu kh\u00f4ng parse \u0111\u01b0\u1ee3c field n\u00e0o th\u00ec \u0111\u1ec3 null."",
      ""json_schema"": {
        ""raw_text"": ""string"",
        ""confidence"": ""number"",
        ""merchant"": ""string|null"",
        ""transaction_date"": ""string|null"",
        ""items"": ""array"",
        ""total"": ""number""
      },
      ""temperature"": 0.0,
      ""max_tokens"": 2000
    }
    $$::jsonb,
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM import_transaction_drafts
    WHERE import_job_id IS NULL AND row_index = -999
);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM import_transaction_drafts
WHERE import_job_id IS NULL AND row_index = -999;
");

            migrationBuilder.DropCheckConstraint(
                name: "chk_import_transaction_drafts_row_index",
                table: "import_transaction_drafts");

            migrationBuilder.AlterColumn<Guid>(
                name: "import_job_id",
                table: "import_transaction_drafts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "normalized_payload_json",
                table: "import_transaction_drafts",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "chk_import_transaction_drafts_row_index",
                table: "import_transaction_drafts",
                sql: "\"row_index\" >= 0");
        }
    }
}
