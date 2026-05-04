using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository.Entity;

namespace Personal_Finance_Management.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets (18 bảng)
    public DbSet<Role> Roles { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<OnboardingProfile> OnboardingProfiles { get; set; }
    public DbSet<JarSetup> JarSetups { get; set; }
    public DbSet<FinancialAccount> FinancialAccounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Jar> Jars { get; set; }
    public DbSet<ImportJob> ImportJobs { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<SpendingLimit> SpendingLimits { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<GoalContribution> GoalContributions { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Broadcast> Broadcasts { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ImportTransactionDraft> ImportTransactionDrafts { get; set; }
    public DbSet<AiSetting> AiSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles");

            builder.Property(r => r.Id)
                .HasColumnType("smallint")
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Code)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(r => r.Code)
                .IsUnique();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Description)
                .HasColumnType("text");

            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("NOW()");
        });

        modelBuilder.Entity<Account>(builder =>
        {
            builder.ToTable("accounts");

            builder.Property(a => a.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(a => a.Username)
                .IsUnique();

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(a => a.Email)
                .IsUnique();

            builder.Property(a => a.PasswordHash)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.Phone)
                .HasMaxLength(20);

            builder.Property(a => a.AvatarUrl)
                .HasColumnType("text");

            builder.Property(a => a.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(a => a.StatusReason)
                .HasColumnType("text");

            builder.Property(a => a.PreferredCurrency)
                .IsRequired()
                .HasColumnType("char(3)")
                .HasDefaultValue("VND");

            builder.Property(a => a.IsOnboardingCompleted)
                .HasDefaultValue(false);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(a => new { a.RoleId, a.Status })
                .HasDatabaseName("ix_accounts_role_status");

            builder.HasIndex(a => a.LastLoginAt)
                .HasDatabaseName("ix_accounts_last_login_at");

            builder.ToTable(t => t.HasCheckConstraint(
                "chk_accounts_status",
                "\"status\" IN ('Active','Banned')"));

            builder.HasOne(a => a.Role)
                .WithMany(r => r.Accounts)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("audit_logs");

            builder.Property(a => a.ActionType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Description)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(a => a.MetadataJson)
                .HasColumnType("json");

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(a => new { a.ActorAccountId, a.CreatedAt })
                .HasDatabaseName("ix_audit_logs_actor_created_at");

            builder.HasOne(a => a.Account)
                .WithMany(acc => acc.AuditLogs)
                .HasForeignKey(a => a.ActorAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OnboardingProfile>(builder =>
        {
            builder.ToTable("onboarding_profiles");

            builder.Property(o => o.MonthlyIncome)
                .HasColumnType("numeric(18,2)");

            builder.Property(o => o.OccupationType)
                .HasMaxLength(50);

            builder.Property(o => o.FinancialGoalTypes)
                .HasColumnType("text");

            builder.Property(o => o.BudgetMethodPreference)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Undecided");

            builder.Property(o => o.AgeRange)
                .HasMaxLength(30);

            builder.Property(o => o.SpendingChallenges)
                .HasColumnType("text");

            builder.Property(o => o.RecommendedMethod)
                .HasMaxLength(30);

            builder.Property(o => o.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(o => o.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(o => o.UserId)
                .IsUnique();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_onboarding_profiles_monthly_income",
                    "\"monthly_income\" IS NULL OR \"monthly_income\" >= 0");
                t.HasCheckConstraint(
                    "chk_onboarding_profiles_budget_method_preference",
                    "\"budget_method_preference\" IN ('SixJars','Rule503020','Custom','Undecided')");
                t.HasCheckConstraint(
                    "chk_onboarding_profiles_recommended_method",
                    "\"recommended_method\" IS NULL OR \"recommended_method\" IN ('SixJars','Rule503020','Custom','Undecided')");
            });

            builder.HasOne(o => o.User)
                .WithOne(a => a.OnboardingProfile)
                .HasForeignKey<OnboardingProfile>(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JarSetup>(builder =>
        {
            builder.ToTable("jar_setups");

            builder.Property(j => j.MethodType)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(j => j.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(j => j.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(j => j.UserId)
                .IsUnique();

            builder.ToTable(t => t.HasCheckConstraint(
                "chk_jar_setups_method_type",
                "\"method_type\" IN ('SixJars','Rule503020','Custom','Undecided')"));

            builder.HasOne(j => j.User)
                .WithOne(a => a.JarSetup)
                .HasForeignKey<JarSetup>(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialAccount>(builder =>
        {
            builder.ToTable("financial_accounts");

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.AccountType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(f => f.ConnectionMode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(f => f.ProviderCode).HasMaxLength(50);
            builder.Property(f => f.ProviderName).HasMaxLength(100);
            builder.Property(f => f.ExternalAccountId).HasMaxLength(150);
            builder.Property(f => f.ExternalAccountRef).HasMaxLength(150);
            builder.Property(f => f.MaskedAccountNumber).HasMaxLength(50);
            builder.Property(f => f.AccountHolderName).HasMaxLength(150);
            builder.Property(f => f.WebhookSubscriptionId).HasMaxLength(150);

            builder.Property(f => f.Currency)
                .IsRequired()
                .HasColumnType("char(3)")
                .HasDefaultValue("VND");

            builder.Property(f => f.CurrentBalance)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m);

            builder.Property(f => f.SyncStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("NeverSynced");

            builder.Property(f => f.IsDefault).HasDefaultValue(false);
            builder.Property(f => f.IsActive).HasDefaultValue(true);

            builder.Property(f => f.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(f => f.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(f => f.UserId)
                .HasDatabaseName("ix_financial_accounts_user_id");

            builder.HasIndex(f => new { f.UserId, f.IsDefault })
                .HasDatabaseName("ix_financial_accounts_user_default");

            builder.HasIndex(f => f.SyncStatus)
                .HasDatabaseName("ix_financial_accounts_sync_status");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_financial_accounts_account_type",
                    "\"account_type\" IN ('Cash','Bank','EWallet','Other')");
                t.HasCheckConstraint(
                    "chk_financial_accounts_connection_mode",
                    "\"connection_mode\" IN ('Manual','LinkedApi')");
                t.HasCheckConstraint(
                    "chk_financial_accounts_sync_status",
                    "\"sync_status\" IN ('NeverSynced','Synced','Syncing','Error','Disconnected')");
            });

            builder.HasOne(f => f.User)
                .WithMany(a => a.FinancialAccounts)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(builder =>
        {
            builder.ToTable("categories");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Icon).HasMaxLength(50);
            builder.Property(c => c.Color).HasMaxLength(20);
            builder.Property(c => c.IsDefault).HasDefaultValue(false);
            builder.Property(c => c.DisplayOrder).HasDefaultValue(0);
            builder.Property(c => c.IsActive).HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(c => new { c.OwnerUserId, c.IsActive })
                .HasDatabaseName("ix_categories_owner_active");

            builder.HasIndex(c => new { c.IsDefault, c.IsActive })
                .HasDatabaseName("ix_categories_default_active");

            builder.HasOne(c => c.OwnerUser)
                .WithMany()
                .HasForeignKey(c => c.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Jar>(builder =>
        {
            builder.ToTable("jars");

            builder.Property(j => j.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(j => j.Balance)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m);

            builder.Property(j => j.Currency)
                .IsRequired()
                .HasColumnType("char(3)")
                .HasDefaultValue("VND");

            builder.Property(j => j.Color).HasMaxLength(20);
            builder.Property(j => j.Icon).HasMaxLength(50);
            builder.Property(j => j.IsDefault).HasDefaultValue(false);

            builder.Property(j => j.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(j => j.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(j => j.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(j => j.UserId)
                .HasDatabaseName("ix_jars_user_id");

            builder.HasIndex(j => new { j.UserId, j.Status })
                .HasDatabaseName("ix_jars_user_status");

            builder.ToTable(t => t.HasCheckConstraint(
                "chk_jars_status",
                "\"status\" IN ('Active','Paused','Archived')"));

            builder.HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(j => j.JarSetup)
                .WithMany(js => js.Jars)
                .HasForeignKey(j => j.JarSetupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportJob>(builder =>
        {
            builder.ToTable("import_jobs");

            builder.Property(i => i.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.OriginalContentType).HasMaxLength(100);
            builder.Property(i => i.StoredFilePath).IsRequired().HasColumnType("text");
            builder.Property(i => i.BankCode).HasMaxLength(50);

            builder.Property(i => i.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            builder.Property(i => i.Progress).HasDefaultValue(0);
            builder.Property(i => i.ParsedCount).HasDefaultValue(0);
            builder.Property(i => i.FailedCount).HasDefaultValue(0);
            builder.Property(i => i.ErrorMessage).HasColumnType("text");

            builder.Property(i => i.UploadedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(i => i.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(i => new { i.UserId, i.UploadedAt })
                .HasDatabaseName("ix_import_jobs_user_uploaded_at");

            builder.HasIndex(i => new { i.FinancialAccountId, i.UploadedAt })
                .HasDatabaseName("ix_import_jobs_account_uploaded_at");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_import_jobs_status",
                    "\"status\" IN ('Pending','Processing','AwaitingReview','Completed','Failed')");
                t.HasCheckConstraint(
                    "chk_import_jobs_progress",
                    "\"progress\" BETWEEN 0 AND 100");
                t.HasCheckConstraint(
                    "chk_import_jobs_counts",
                    "\"parsed_count\" >= 0 AND \"failed_count\" >= 0 AND (\"estimated_rows\" IS NULL OR \"estimated_rows\" >= 0)");
            });

            builder.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.FinancialAccount)
                .WithMany()
                .HasForeignKey(i => i.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.ToTable("transactions");

            builder.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.TransactionsAmount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(t => t.Note).HasColumnType("text");
            builder.Property(t => t.RawDescription).HasColumnType("text");
            builder.Property(t => t.RawPayloadJson).HasColumnType("json");
            builder.Property(t => t.ExternalTransactionId).HasMaxLength(150);
            builder.Property(t => t.JarBalanceAfterAllocation).HasColumnType("numeric(18,2)");

            builder.Property(t => t.SourceType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Manual");

            builder.Property(t => t.IsDeleted).HasDefaultValue(false);

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(t => t.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(t => new { t.UserId, t.TransactionDate })
                .HasDatabaseName("ix_transactions_user_date")
                .HasFilter("\"is_deleted\" = FALSE");

            builder.HasIndex(t => new { t.FinancialAccountId, t.TransactionDate })
                .HasDatabaseName("ix_transactions_account_date")
                .HasFilter("\"is_deleted\" = FALSE");

            builder.HasIndex(t => new { t.UserId, t.CategoryId, t.TransactionDate })
                .HasDatabaseName("ix_transactions_user_category_date")
                .HasFilter("\"is_deleted\" = FALSE");

            builder.HasIndex(t => t.ImportJobId)
                .HasDatabaseName("ix_transactions_import_job_id");

            builder.HasIndex(t => t.FromJarId)
                .HasDatabaseName("ix_transactions_from_jar_id");

            builder.HasIndex(t => t.ToJarId)
                .HasDatabaseName("ix_transactions_to_jar_id");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_transactions_type",
                    "\"type\" IN ('Income','Expense')");
                t.HasCheckConstraint(
                    "chk_transactions_source_type",
                    "\"source_type\" IN ('Manual','Imported','OCR','Jar','System')");
                t.HasCheckConstraint(
                    "chk_transactions_amount_by_type",
                    "(\"type\" = 'Income' AND \"transactions_amount\" > 0) OR (\"type\" = 'Expense' AND \"transactions_amount\" < 0)");
                t.HasCheckConstraint(
                    "chk_transactions_jar_direction",
                    "\"from_jar_id\" IS NULL OR \"to_jar_id\" IS NULL OR \"from_jar_id\" <> \"to_jar_id\"");
            });

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.FinancialAccount)
                .WithMany()
                .HasForeignKey(t => t.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.FromJar)
                .WithMany()
                .HasForeignKey(t => t.FromJarId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ToJar)
                .WithMany()
                .HasForeignKey(t => t.ToJarId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.ImportJob)
                .WithMany()
                .HasForeignKey(t => t.ImportJobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SpendingLimit>(builder =>
        {
            builder.ToTable("spending_limits");

            builder.Property(s => s.LimitAmount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(s => s.AlertAtPercentage)
                .IsRequired()
                .HasColumnType("numeric(5,2)");

            builder.Property(s => s.Period)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.IsActive).HasDefaultValue(true);

            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(s => s.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(s => new { s.UserId, s.IsActive })
                .HasDatabaseName("ix_spending_limits_user_active");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_spending_limits_amount",
                    "\"limit_amount\" > 0");
                t.HasCheckConstraint(
                    "chk_spending_limits_period",
                    "\"period\" IN ('Daily','Monthly')");
                t.HasCheckConstraint(
                    "chk_spending_limits_alert_percentage",
                    "\"alert_at_percentage\" > 0 AND \"alert_at_percentage\" <= 100");
                t.HasCheckConstraint(
                    "chk_spending_limits_target",
                    "\"jar_id\" IS NOT NULL OR \"category_id\" IS NOT NULL");
            });

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Jar)
                .WithMany()
                .HasForeignKey(s => s.JarId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Goal>(builder =>
        {
            builder.ToTable("goals");

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(g => g.TargetAmount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(g => g.SavedAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m);

            builder.Property(g => g.DueDate)
                .HasColumnType("date");

            builder.Property(g => g.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(g => g.Note).HasColumnType("text");

            builder.Property(g => g.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(g => g.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(g => new { g.UserId, g.Status })
                .HasDatabaseName("ix_goals_user_status");

            builder.HasIndex(g => g.LinkedJarId)
                .HasDatabaseName("ix_goals_linked_jar_id");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_goals_amounts",
                    "\"target_amount\" > 0 AND \"saved_amount\" >= 0");
                t.HasCheckConstraint(
                    "chk_goals_status",
                    "\"status\" IN ('Active','Completed','Cancelled')");
            });

            builder.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.LinkedJar)
                .WithMany()
                .HasForeignKey(g => g.LinkedJarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GoalContribution>(builder =>
        {
            builder.ToTable("goal_contributions");

            builder.Property(g => g.Amount)
                .IsRequired()
                .HasColumnType("numeric(18,2)");

            builder.Property(g => g.Note).HasColumnType("text");

            builder.Property(g => g.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(g => new { g.GoalId, g.CreatedAt })
                .HasDatabaseName("ix_goal_contributions_goal_created_at");

            builder.HasIndex(g => new { g.UserId, g.CreatedAt })
                .HasDatabaseName("ix_goal_contributions_user_created_at");

            builder.ToTable(t => t.HasCheckConstraint(
                "chk_goal_contributions_amount",
                "\"amount\" > 0"));

            builder.HasOne(g => g.Goal)
                .WithMany(goal => goal.Contributions)
                .HasForeignKey(g => g.GoalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.SourceJar)
                .WithMany()
                .HasForeignKey(g => g.SourceJarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reminder>(builder =>
        {
            builder.ToTable("reminders");

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.Amount)
                .HasColumnType("numeric(18,2)");

            builder.Property(r => r.Frequency)
                .HasMaxLength(20);

            builder.Property(r => r.DayOfMonth)
                .HasColumnType("smallint");

            builder.Property(r => r.StartDate)
                .HasColumnType("date")
                .HasDefaultValueSql("CURRENT_DATE");

            builder.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(r => r.NotifyDaysBefore)
                .HasColumnType("smallint")
                .HasDefaultValue((short)1);

            builder.Property(r => r.Note).HasColumnType("text");

            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(r => r.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(r => new { r.UserId, r.Status })
                .HasDatabaseName("ix_reminders_user_status");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_reminders_amount",
                    "\"amount\" IS NULL OR \"amount\" >= 0");
                t.HasCheckConstraint(
                    "chk_reminders_frequency",
                    "\"frequency\" IS NULL OR \"frequency\" IN ('Daily','Weekly','Monthly','Quarterly','Yearly')");
                t.HasCheckConstraint(
                    "chk_reminders_day_of_month",
                    "\"day_of_month\" IS NULL OR \"day_of_month\" BETWEEN 1 AND 31");
                t.HasCheckConstraint(
                    "chk_reminders_status",
                    "\"status\" IN ('Active','Paused','Completed','Cancelled')");
                t.HasCheckConstraint(
                    "chk_reminders_notify_days_before",
                    "\"notify_days_before\" IS NULL OR \"notify_days_before\" >= 0");
            });

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Broadcast>(builder =>
        {
            builder.ToTable("broadcasts");

            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Body)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(b => b.TargetAudience)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("All");

            builder.Property(b => b.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Queued");

            builder.Property(b => b.TargetCount).HasDefaultValue(0);
            builder.Property(b => b.DeliveredCount).HasDefaultValue(0);

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(b => b.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(b => new { b.Status, b.ScheduledAt })
                .HasDatabaseName("ix_broadcasts_status_scheduled_at");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_broadcasts_status",
                    "\"status\" IN ('Queued','Sent','Failed','Cancelled')");
                t.HasCheckConstraint(
                    "chk_broadcasts_counts",
                    "\"target_count\" >= 0 AND \"delivered_count\" >= 0 AND \"delivered_count\" <= \"target_count\"");
            });

            builder.HasOne(b => b.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(b => b.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(builder =>
        {
            builder.ToTable("notifications");

            builder.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Body)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(n => n.MetadataJson).HasColumnType("json");
            builder.Property(n => n.IsRead).HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(n => new { n.UserId, n.CreatedAt })
                .HasDatabaseName("ix_notifications_user_created_at");

            builder.HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("ix_notifications_user_unread")
                .HasFilter("\"is_read\" = FALSE");

            builder.ToTable(t => t.HasCheckConstraint(
                "chk_notifications_type",
                "\"type\" IN ('SpendingAlert','GoalUpdate','Reminder','System','Broadcast')"));

            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Broadcast)
                .WithMany(b => b.Notifications)
                .HasForeignKey(n => n.BroadcastId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportTransactionDraft>(builder =>
        {
            builder.ToTable("import_transaction_drafts");

            builder.Property(d => d.Amount).HasColumnType("numeric(18,2)");
            builder.Property(d => d.Type).HasMaxLength(20);
            builder.Property(d => d.RawDescription).HasColumnType("text");
            builder.Property(d => d.EditedNote).HasColumnType("text");
            builder.Property(d => d.ValidationError).HasColumnType("text");
            builder.Property(d => d.NormalizedPayloadJson).HasColumnType("json");
            builder.Property(d => d.IsValid).HasDefaultValue(true);

            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Property(d => d.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.HasIndex(d => new { d.ImportJobId, d.RowIndex })
                .IsUnique()
                .HasDatabaseName("uq_import_transaction_drafts_job_row");

            builder.HasIndex(d => d.ImportJobId)
                .HasDatabaseName("ix_import_transaction_drafts_import_job_id");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_import_transaction_drafts_row_index",
                    "\"row_index\" >= 0");
                t.HasCheckConstraint(
                    "chk_import_transaction_drafts_type",
                    "\"type\" IS NULL OR \"type\" IN ('Income','Expense')");
            });

            builder.HasOne(d => d.ImportJob)
                .WithMany(j => j.Drafts)
                .HasForeignKey(d => d.ImportJobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.EditedCategory)
                .WithMany()
                .HasForeignKey(d => d.EditedCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.EditedJar)
                .WithMany()
                .HasForeignKey(d => d.EditedJarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AiSetting>(builder =>
        {
            builder.ToTable("ai_settings");

            builder.Property(a => a.ModelName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.SystemPrompt)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(a => a.Temperature)
                .IsRequired()
                .HasColumnType("numeric(3,2)")
                .HasDefaultValue(0.7m);

            builder.Property(a => a.MaxTokens)
                .IsRequired()
                .HasDefaultValue(1000);

            builder.Property(a => a.ApiKeyEncrypted).HasColumnType("text");
            builder.Property(a => a.IsEnabled).HasDefaultValue(true);

            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("NOW()");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "chk_ai_settings_temperature",
                    "\"temperature\" >= 0 AND \"temperature\" <= 2");
                t.HasCheckConstraint(
                    "chk_ai_settings_max_tokens",
                    "\"max_tokens\" > 0");
            });

            builder.HasOne(a => a.UpdatedByAdmin)
                .WithMany()
                .HasForeignKey(a => a.UpdatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
