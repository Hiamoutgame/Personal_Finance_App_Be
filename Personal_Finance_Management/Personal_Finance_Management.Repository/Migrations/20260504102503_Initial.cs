using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Personal_Finance_Management.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    last_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    status_reason = table.Column<string>(type: "text", nullable: true),
                    preferred_currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    is_onboarding_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.id);
                    table.CheckConstraint("chk_accounts_status", "\"status\" IN ('Active','Banned')");
                    table.ForeignKey(
                        name: "fk_accounts_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ai_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    system_prompt = table.Column<string>(type: "text", nullable: false),
                    temperature = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0.7m),
                    max_tokens = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    api_key_encrypted = table.Column<string>(type: "text", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_by_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_settings", x => x.id);
                    table.CheckConstraint("chk_ai_settings_max_tokens", "\"max_tokens\" > 0");
                    table.CheckConstraint("chk_ai_settings_temperature", "\"temperature\" >= 0 AND \"temperature\" <= 2");
                    table.ForeignKey(
                        name: "fk_ai_settings_accounts_updated_by_admin_id",
                        column: x => x.updated_by_admin_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    metadata_json = table.Column<string>(type: "json", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    actor_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_logs_accounts_actor_account_id",
                        column: x => x.actor_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "broadcasts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    target_audience = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "All"),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Queued"),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    target_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    delivered_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_by_admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_broadcasts", x => x.id);
                    table.CheckConstraint("chk_broadcasts_counts", "\"target_count\" >= 0 AND \"delivered_count\" >= 0 AND \"delivered_count\" <= \"target_count\"");
                    table.CheckConstraint("chk_broadcasts_status", "\"status\" IN ('Queued','Sent','Failed','Cancelled')");
                    table.ForeignKey(
                        name: "fk_broadcasts_accounts_created_by_admin_id",
                        column: x => x.created_by_admin_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_accounts_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financial_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    connection_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    provider_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provider_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    external_account_ref = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    masked_account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    account_holder_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    sync_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NeverSynced"),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_sync_error = table.Column<string>(type: "text", nullable: true),
                    access_token_ref = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    consent_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_sync_cursor = table.Column<string>(type: "text", nullable: true),
                    webhook_subscription_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_financial_accounts", x => x.id);
                    table.CheckConstraint("chk_financial_accounts_account_type", "\"account_type\" IN ('Cash','Bank','EWallet','Other')");
                    table.CheckConstraint("chk_financial_accounts_connection_mode", "\"connection_mode\" IN ('Manual','LinkedApi')");
                    table.CheckConstraint("chk_financial_accounts_sync_status", "\"sync_status\" IN ('NeverSynced','Synced','Syncing','Error','Disconnected')");
                    table.ForeignKey(
                        name: "fk_financial_accounts_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "jar_setups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    method_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jar_setups", x => x.id);
                    table.CheckConstraint("chk_jar_setups_method_type", "\"method_type\" IN ('SixJars','Rule503020','Custom','Undecided')");
                    table.ForeignKey(
                        name: "fk_jar_setups_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "onboarding_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_income = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    occupation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    financial_goal_types = table.Column<string>(type: "text", nullable: true),
                    budget_method_preference = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Undecided"),
                    age_range = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    spending_challenges = table.Column<string>(type: "text", nullable: true),
                    recommended_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_onboarding_profiles", x => x.id);
                    table.CheckConstraint("chk_onboarding_profiles_budget_method_preference", "\"budget_method_preference\" IN ('SixJars','Rule503020','Custom','Undecided')");
                    table.CheckConstraint("chk_onboarding_profiles_monthly_income", "\"monthly_income\" IS NULL OR \"monthly_income\" >= 0");
                    table.CheckConstraint("chk_onboarding_profiles_recommended_method", "\"recommended_method\" IS NULL OR \"recommended_method\" IN ('SixJars','Rule503020','Custom','Undecided')");
                    table.ForeignKey(
                        name: "fk_onboarding_profiles_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "json", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    broadcast_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.CheckConstraint("chk_notifications_type", "\"type\" IN ('SpendingAlert','GoalUpdate','Reminder','System','Broadcast')");
                    table.ForeignKey(
                        name: "fk_notifications_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_broadcasts_broadcast_id",
                        column: x => x.broadcast_id,
                        principalTable: "broadcasts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    day_of_month = table.Column<short>(type: "smallint", nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    note = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    notify_days_before = table.Column<short>(type: "smallint", nullable: true, defaultValue: (short)1),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reminders", x => x.id);
                    table.CheckConstraint("chk_reminders_amount", "\"amount\" IS NULL OR \"amount\" >= 0");
                    table.CheckConstraint("chk_reminders_day_of_month", "\"day_of_month\" IS NULL OR \"day_of_month\" BETWEEN 1 AND 31");
                    table.CheckConstraint("chk_reminders_frequency", "\"frequency\" IS NULL OR \"frequency\" IN ('Daily','Weekly','Monthly','Quarterly','Yearly')");
                    table.CheckConstraint("chk_reminders_notify_days_before", "\"notify_days_before\" IS NULL OR \"notify_days_before\" >= 0");
                    table.CheckConstraint("chk_reminders_status", "\"status\" IN ('Active','Paused','Completed','Cancelled')");
                    table.ForeignKey(
                        name: "fk_reminders_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reminders_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    original_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    stored_file_path = table.Column<string>(type: "text", nullable: false),
                    bank_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    estimated_rows = table.Column<int>(type: "integer", nullable: true),
                    parsed_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    failed_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    financial_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_jobs", x => x.id);
                    table.CheckConstraint("chk_import_jobs_counts", "\"parsed_count\" >= 0 AND \"failed_count\" >= 0 AND (\"estimated_rows\" IS NULL OR \"estimated_rows\" >= 0)");
                    table.CheckConstraint("chk_import_jobs_progress", "\"progress\" BETWEEN 0 AND 100");
                    table.CheckConstraint("chk_import_jobs_status", "\"status\" IN ('Pending','Processing','AwaitingReview','Completed','Failed')");
                    table.ForeignKey(
                        name: "fk_import_jobs_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_import_jobs_financial_accounts_financial_account_id",
                        column: x => x.financial_account_id,
                        principalTable: "financial_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "jars",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    jar_setup_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jars", x => x.id);
                    table.CheckConstraint("chk_jars_status", "\"status\" IN ('Active','Paused','Archived')");
                    table.ForeignKey(
                        name: "fk_jars_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_jars_jar_setups_jar_setup_id",
                        column: x => x.jar_setup_id,
                        principalTable: "jar_setups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    target_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saved_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    due_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    note = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linked_jar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goals", x => x.id);
                    table.CheckConstraint("chk_goals_amounts", "\"target_amount\" > 0 AND \"saved_amount\" >= 0");
                    table.CheckConstraint("chk_goals_status", "\"status\" IN ('Active','Completed','Cancelled')");
                    table.ForeignKey(
                        name: "fk_goals_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_goals_jars_linked_jar_id",
                        column: x => x.linked_jar_id,
                        principalTable: "jars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_transaction_drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    raw_description = table.Column<string>(type: "text", nullable: true),
                    edited_note = table.Column<string>(type: "text", nullable: true),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    validation_error = table.Column<string>(type: "text", nullable: true),
                    normalized_payload_json = table.Column<string>(type: "json", nullable: true),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    edited_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    edited_jar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_transaction_drafts", x => x.id);
                    table.CheckConstraint("chk_import_transaction_drafts_row_index", "\"row_index\" >= 0");
                    table.CheckConstraint("chk_import_transaction_drafts_type", "\"type\" IS NULL OR \"type\" IN ('Income','Expense')");
                    table.ForeignKey(
                        name: "fk_import_transaction_drafts_categories_edited_category_id",
                        column: x => x.edited_category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_import_transaction_drafts_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_import_transaction_drafts_jars_edited_jar_id",
                        column: x => x.edited_jar_id,
                        principalTable: "jars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "spending_limits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    alert_at_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    jar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spending_limits", x => x.id);
                    table.CheckConstraint("chk_spending_limits_alert_percentage", "\"alert_at_percentage\" > 0 AND \"alert_at_percentage\" <= 100");
                    table.CheckConstraint("chk_spending_limits_amount", "\"limit_amount\" > 0");
                    table.CheckConstraint("chk_spending_limits_period", "\"period\" IN ('Daily','Monthly')");
                    table.CheckConstraint("chk_spending_limits_target", "\"jar_id\" IS NOT NULL OR \"category_id\" IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_spending_limits_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spending_limits_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_spending_limits_jars_jar_id",
                        column: x => x.jar_id,
                        principalTable: "jars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transactions_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    raw_description = table.Column<string>(type: "text", nullable: true),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    posted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    source_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    external_transaction_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    jar_balance_after_allocation = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    from_jar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_jar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_payload_json = table.Column<string>(type: "json", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    financial_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    import_job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.CheckConstraint("chk_transactions_amount_by_type", "(\"type\" = 'Income' AND \"transactions_amount\" > 0) OR (\"type\" = 'Expense' AND \"transactions_amount\" < 0)");
                    table.CheckConstraint("chk_transactions_jar_direction", "\"from_jar_id\" IS NULL OR \"to_jar_id\" IS NULL OR \"from_jar_id\" <> \"to_jar_id\"");
                    table.CheckConstraint("chk_transactions_source_type", "\"source_type\" IN ('Manual','Imported','OCR','Jar','System')");
                    table.CheckConstraint("chk_transactions_type", "\"type\" IN ('Income','Expense')");
                    table.ForeignKey(
                        name: "fk_transactions_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_financial_accounts_financial_account_id",
                        column: x => x.financial_account_id,
                        principalTable: "financial_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_import_jobs_import_job_id",
                        column: x => x.import_job_id,
                        principalTable: "import_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_jars_from_jar_id",
                        column: x => x.from_jar_id,
                        principalTable: "jars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_jars_to_jar_id",
                        column: x => x.to_jar_id,
                        principalTable: "jars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "goal_contributions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_jar_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_goal_contributions", x => x.id);
                    table.CheckConstraint("chk_goal_contributions_amount", "\"amount\" > 0");
                    table.ForeignKey(
                        name: "fk_goal_contributions_accounts_user_id",
                        column: x => x.user_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_goal_contributions_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_goal_contributions_jars_source_jar_id",
                        column: x => x.source_jar_id,
                        principalTable: "jars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_email",
                table: "accounts",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_last_login_at",
                table: "accounts",
                column: "last_login_at");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_role_status",
                table: "accounts",
                columns: new[] { "role_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_username",
                table: "accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_settings_updated_by_admin_id",
                table: "ai_settings",
                column: "updated_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_created_at",
                table: "audit_logs",
                columns: new[] { "actor_account_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_broadcasts_created_by_admin_id",
                table: "broadcasts",
                column: "created_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_broadcasts_status_scheduled_at",
                table: "broadcasts",
                columns: new[] { "status", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_default_active",
                table: "categories",
                columns: new[] { "is_default", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_owner_active",
                table: "categories",
                columns: new[] { "owner_user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_financial_accounts_sync_status",
                table: "financial_accounts",
                column: "sync_status");

            migrationBuilder.CreateIndex(
                name: "ix_financial_accounts_user_default",
                table: "financial_accounts",
                columns: new[] { "user_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_financial_accounts_user_id",
                table: "financial_accounts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_goal_contributions_goal_created_at",
                table: "goal_contributions",
                columns: new[] { "goal_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_goal_contributions_source_jar_id",
                table: "goal_contributions",
                column: "source_jar_id");

            migrationBuilder.CreateIndex(
                name: "ix_goal_contributions_user_created_at",
                table: "goal_contributions",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_goals_linked_jar_id",
                table: "goals",
                column: "linked_jar_id");

            migrationBuilder.CreateIndex(
                name: "ix_goals_user_status",
                table: "goals",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_import_jobs_account_uploaded_at",
                table: "import_jobs",
                columns: new[] { "financial_account_id", "uploaded_at" });

            migrationBuilder.CreateIndex(
                name: "ix_import_jobs_user_uploaded_at",
                table: "import_jobs",
                columns: new[] { "user_id", "uploaded_at" });

            migrationBuilder.CreateIndex(
                name: "ix_import_transaction_drafts_edited_category_id",
                table: "import_transaction_drafts",
                column: "edited_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_transaction_drafts_edited_jar_id",
                table: "import_transaction_drafts",
                column: "edited_jar_id");

            migrationBuilder.CreateIndex(
                name: "ix_import_transaction_drafts_import_job_id",
                table: "import_transaction_drafts",
                column: "import_job_id");

            migrationBuilder.CreateIndex(
                name: "uq_import_transaction_drafts_job_row",
                table: "import_transaction_drafts",
                columns: new[] { "import_job_id", "row_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_jar_setups_user_id",
                table: "jar_setups",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_jars_jar_setup_id",
                table: "jars",
                column: "jar_setup_id");

            migrationBuilder.CreateIndex(
                name: "ix_jars_user_id",
                table: "jars",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_jars_user_status",
                table: "jars",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_broadcast_id",
                table: "notifications",
                column: "broadcast_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_created_at",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_unread",
                table: "notifications",
                columns: new[] { "user_id", "is_read" },
                filter: "\"is_read\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_onboarding_profiles_user_id",
                table: "onboarding_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reminders_category_id",
                table: "reminders",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_reminders_user_status",
                table: "reminders",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spending_limits_category_id",
                table: "spending_limits",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_spending_limits_jar_id",
                table: "spending_limits",
                column: "jar_id");

            migrationBuilder.CreateIndex(
                name: "ix_spending_limits_user_active",
                table: "spending_limits",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_transactions_account_date",
                table: "transactions",
                columns: new[] { "financial_account_id", "transaction_date" },
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_category_id",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_from_jar_id",
                table: "transactions",
                column: "from_jar_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_import_job_id",
                table: "transactions",
                column: "import_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_to_jar_id",
                table: "transactions",
                column: "to_jar_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_user_category_date",
                table: "transactions",
                columns: new[] { "user_id", "category_id", "transaction_date" },
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_user_date",
                table: "transactions",
                columns: new[] { "user_id", "transaction_date" },
                filter: "\"is_deleted\" = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_settings");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "goal_contributions");

            migrationBuilder.DropTable(
                name: "import_transaction_drafts");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "onboarding_profiles");

            migrationBuilder.DropTable(
                name: "reminders");

            migrationBuilder.DropTable(
                name: "spending_limits");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "broadcasts");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "import_jobs");

            migrationBuilder.DropTable(
                name: "jars");

            migrationBuilder.DropTable(
                name: "financial_accounts");

            migrationBuilder.DropTable(
                name: "jar_setups");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
