using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets (21 bảng)
    public DbSet<Role> Roles { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<OnboardingProfile> OnboardingProfiles { get; set; }
    public DbSet<JarSetup> JarSetups { get; set; }
    public DbSet<FinancialAccount> FinancialAccounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Jar> Jars { get; set; }
    public DbSet<JarAllocation> JarAllocations { get; set; }
    public DbSet<JarAllocationItem> JarAllocationItems { get; set; }
    public DbSet<JarTransfer> JarTransfers { get; set; }
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
        // ── 1. roles ──────────────────────────────────────────────
        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles");

            builder.Property(r => r.Code)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(r => r.Code)
                .IsUnique();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(r => r.Description)
                .HasColumnType("text");
        });

        // ── 2. accounts ───────────────────────────────────────────
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

            builder.Property(a => a.HashPassword)
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

            builder.Property(a => a.PreferredCurrency)
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasDefaultValue("VND");

            builder.Property(a => a.IsOnboardingCompleted)
                        .HasDefaultValue(false);

            // N-1: Account → Role
            builder.HasOne(a => a.Role)
                        .WithMany(r => r.Accounts)
                        .HasForeignKey(a => a.RoleId)
                        .OnDelete(DeleteBehavior.Restrict);
            // Restrict: không xoá Role khi còn Account tham chiếu
        });

        // ── 3. audit_logs ─────────────────────────────────────────
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


            builder.HasOne(a => a.Account)
                            .WithMany(acc => acc.AuditLogs)
                            .HasForeignKey(a => a.ActorAccountId)
                            .OnDelete(DeleteBehavior.Restrict);
            // Restrict: giữ log lại dù account bị xoá/banned
        });

        // ── 4. onboarding_profiles ────────────────────────────────
        modelBuilder.Entity<OnboardingProfile>(builder =>
        {
            builder.ToTable("onboarding_profiles");

            builder.Property(o => o.BudgetMethodPreference)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Undecided");

            builder.Property(o => o.OccupationType)
                .HasMaxLength(50);

            builder.Property(o => o.AgeRange)
                .HasMaxLength(30);

            // 1-1: OnboardingProfile giữ FK UserId → Account
            builder.HasOne(o => o.User)
                .WithOne(a => a.OnboardingProfile)
                .HasForeignKey<OnboardingProfile>(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cascade: xoá Account → xoá OnboardingProfile theo
            builder.Property(x => x.FinancialGoalTypes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
        });

        // ── 5. jar_setups ─────────────────────────────────────────
        modelBuilder.Entity<JarSetup>(builder =>
        {
            builder.ToTable("jar_setups");

            builder.Property(j => j.MethodType)
                .IsRequired()
                .HasMaxLength(30);


            builder.HasOne(j => j.User)
                .WithOne(a => a.JarSetup)
                .HasForeignKey<JarSetup>(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── 6. financial_accounts ─────────────────────────────────
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
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            builder.Property(f => f.CurrentBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(f => f.AvailableBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(f => f.SyncStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("NeverSynced");

            builder.Property(f => f.IsDefault).HasDefaultValue(false);
            builder.Property(f => f.IsActive).HasDefaultValue(true);

            // N-1: FinancialAccount → Account
            builder.HasOne(f => f.User)
                .WithMany(a => a.FinancialAccounts)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── 7. categories ─────────────────────────────────────────
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

            // N-1 (optional): Category → Account (owner, nullable)
            builder.HasOne(c => c.OwnerUser)
                .WithMany()
                .HasForeignKey(c => c.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            // SetNull: xoá Account → category hệ thống vẫn giữ (OwnerUserId = null)
        });

        // ── 8. jars ───────────────────────────────────────────────
        modelBuilder.Entity<Jar>(builder =>
        {
            builder.ToTable("jars");

            builder.Property(j => j.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(j => j.Percentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(j => j.Balance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(j => j.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            builder.Property(j => j.Color).HasMaxLength(20);
            builder.Property(j => j.Icon).HasMaxLength(50);
            builder.Property(j => j.IsDefault).HasDefaultValue(false);

            builder.Property(j => j.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            // N-1: Jar → Account
            builder.HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // N-1 (optional): Jar → JarSetup
            builder.HasOne(j => j.JarSetup)
                .WithMany(js => js.Jars)
                .HasForeignKey(j => j.JarSetupId)
                .OnDelete(DeleteBehavior.SetNull);
            // SetNull: xoá JarSetup → Jar vẫn tồn tại, JarSetupId = null
        });


        // ── 9. jar_allocations ────────────────────────────────────
        modelBuilder.Entity<JarAllocation>(builder =>
        {
            builder.ToTable("jar_allocations");

            builder.Property(j => j.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.Note)
                .HasColumnType("text");

            // N-1: JarAllocation → Account
            builder.HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1 (optional): JarAllocation → FinancialAccount
            builder.HasOne(j => j.SourceFinancialAccount)
                .WithMany()
                .HasForeignKey(j => j.SourceFinancialAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-N: JarAllocation → N JarAllocationItem
            // (config ở block JarAllocationItem)
        });

        // ── 10. jar_allocation_items ──────────────────────────────
        modelBuilder.Entity<JarAllocationItem>(builder =>
        {
            builder.ToTable("jar_allocation_items");

            builder.Property(j => j.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.BalanceAfterAllocation)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // N-1: JarAllocationItem → JarAllocation
            builder.HasOne(j => j.Allocation)
                .WithMany(a => a.Items)
                .HasForeignKey(j => j.AllocationId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cascade: xoá JarAllocation → xoá tất cả items theo

            // N-1: JarAllocationItem → Jar
            builder.HasOne(j => j.Jar)
                .WithMany()
                .HasForeignKey(j => j.JarId)
                .OnDelete(DeleteBehavior.Restrict);
            // Restrict: không xoá Jar khi còn item tham chiếu
        });

        // ── 11. jar_transfers ─────────────────────────────────────
        modelBuilder.Entity<JarTransfer>(builder =>
        {
            builder.ToTable("jar_transfers");

            builder.Property(j => j.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(j => j.Note)
                .HasColumnType("text");

            // N-1: JarTransfer → Account
            builder.HasOne(j => j.User)
                .WithMany()
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1: JarTransfer → Jar (from)
            // Hai FK đến cùng bảng Jars → phải dùng HasForeignKey chỉ rõ từng FK
            // và tắt Cascade để tránh "multiple cascade paths"
            builder.HasOne(j => j.FromJar)
                .WithMany()
                .HasForeignKey(j => j.FromJarId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1: JarTransfer → Jar (to)
            builder.HasOne(j => j.ToJar)
                .WithMany()
                .HasForeignKey(j => j.ToJarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── 12. import_jobs ───────────────────────────────────────
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

            // N-1: ImportJob → Account
            builder.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1: ImportJob → FinancialAccount
            builder.HasOne(i => i.FinancialAccount)
                .WithMany()
                .HasForeignKey(i => i.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N: ImportJob → N ImportTransactionDraft
            // (config ở block ImportTransactionDraft)
        });

        // ── 13. transactions ──────────────────────────────────────
        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.ToTable("transactions");

            builder.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Note).HasColumnType("text");
            builder.Property(t => t.RawDescription).HasColumnType("text");
            builder.Property(t => t.RawPayloadJson).HasColumnType("json");
            builder.Property(t => t.ExternalTransactionId).HasMaxLength(150);

            builder.Property(t => t.SourceType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Manual");

            builder.Property(t => t.IsDeleted).HasDefaultValue(false);

            // Index thường dùng để query theo user + date
            builder.HasIndex(t => new { t.UserId, t.TransactionDate });

            // N-1: Transaction → Account
            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1: Transaction → FinancialAccount
            builder.HasOne(t => t.FinancialAccount)
                .WithMany()
                .HasForeignKey(t => t.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1 (optional): Transaction → Jar
            builder.HasOne(t => t.Jar)
                .WithMany()
                .HasForeignKey(t => t.JarId)
                .OnDelete(DeleteBehavior.SetNull);

            // N-1 (optional): Transaction → Category
            builder.HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // N-1 (optional): Transaction → ImportJob
            builder.HasOne(t => t.ImportJob)
                .WithMany()
                .HasForeignKey(t => t.ImportJobId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        // ── 14. spending_limits ───────────────────────────────────
        modelBuilder.Entity<SpendingLimit>(builder =>
        {
            builder.ToTable("spending_limits");

            builder.Property(s => s.LimitAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.AlertAtPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(s => s.Period)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Monthly");

            builder.Property(s => s.IsActive).HasDefaultValue(true);

            // N-1: SpendingLimit → Account
            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // N-1 (optional): SpendingLimit → Jar
            builder.HasOne(s => s.Jar)
                .WithMany()
                .HasForeignKey(s => s.JarId)
                .OnDelete(DeleteBehavior.SetNull);

            // N-1 (optional): SpendingLimit → Category
            builder.HasOne(s => s.Category)
                .WithMany()
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ── 15. goals ─────────────────────────────────────────────
        modelBuilder.Entity<Goal>(builder =>
        {
            builder.ToTable("goals");

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(g => g.TargetAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.SavedAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(g => g.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(g => g.Note).HasColumnType("text");

            // N-1: Goal → Account
            builder.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // N-1 (optional): Goal → Jar
            builder.HasOne(g => g.LinkedJar)
                .WithMany()
                .HasForeignKey(g => g.LinkedJarId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1-N: Goal → N GoalContribution
            // (config ở block GoalContribution)
        });

        // ── 16. goal_contributions ────────────────────────────────
        modelBuilder.Entity<GoalContribution>(builder =>
        {
            builder.ToTable("goal_contributions");

            builder.Property(g => g.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.Note).HasColumnType("text");

            // N-1: GoalContribution → Goal
            builder.HasOne(g => g.Goal)
                .WithMany(goal => goal.Contributions)
                .HasForeignKey(g => g.GoalId)
                .OnDelete(DeleteBehavior.Cascade);

            // N-1: GoalContribution → Account
            builder.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-1 (optional): GoalContribution → Jar
            builder.HasOne(g => g.SourceJar)
                .WithMany()
                .HasForeignKey(g => g.SourceJarId)
                .OnDelete(DeleteBehavior.SetNull);

            // N-1 (optional): GoalContribution → FinancialAccount
            builder.HasOne(g => g.SourceFinancialAccount)
                .WithMany()
                .HasForeignKey(g => g.SourceFinancialAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ── 17. reminders ─────────────────────────────────────────
        modelBuilder.Entity<Reminder>(builder =>
        {
            builder.ToTable("reminders");

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.Amount).HasColumnType("decimal(18,2)");

            builder.Property(r => r.Frequency)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Monthly");

            builder.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.Property(r => r.NotifyDaysBefore)
                .HasDefaultValue((short)1);

            builder.Property(r => r.Note).HasColumnType("text");

            // N-1: Reminder → Account
            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // N-1 (optional): Reminder → Category
            builder.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ── 18. broadcasts ────────────────────────────────────────
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

            // N-1: Broadcast → Account (admin)
            builder.HasOne(b => b.CreatByAdmin)
                .WithMany()
                .HasForeignKey(b => b.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-N: Broadcast → N Notification
            // (config ở block Notification)
        });

        // ── 19. notifications ─────────────────────────────────────
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

            // Index để query nhanh thông báo chưa đọc của 1 user
            builder.HasIndex(n => new { n.UserId, n.IsRead });

            // N-1: Notification → Account
            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // N-1 (optional): Notification → Broadcast
            builder.HasOne(n => n.Broadcast)
                .WithMany(b => b.Notifications)
                .HasForeignKey(n => n.BroadcastId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        // ── 20. import_transaction_drafts ─────────────────────────
        modelBuilder.Entity<ImportTransactionDraft>(builder =>
        {
            builder.ToTable("import_transaction_drafts");

            builder.Property(d => d.Amount).HasColumnType("decimal(18,2)");
            builder.Property(d => d.Type).HasMaxLength(20);
            builder.Property(d => d.RawDescription).HasColumnType("text");
            builder.Property(d => d.SuggestedNote).HasColumnType("text");
            builder.Property(d => d.ValidationError).HasColumnType("text");
            builder.Property(d => d.NormalizedPayloadJson).HasColumnType("json");
            builder.Property(d => d.IsValid).HasDefaultValue(true);

            // Unique constraint: (import_job_id, row_index) — theo DBML
            builder.HasIndex(d => new { d.ImportJobId, d.RowIndex })
                .IsUnique();

            // N-1: ImportTransactionDraft → ImportJob
            builder.HasOne(d => d.ImportJob)
                .WithMany(j => j.Drafts)
                .HasForeignKey(d => d.ImportJobId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cascade: xoá ImportJob → xoá tất cả Drafts theo

            // N-1 (optional): ImportTransactionDraft → Category (suggested)
            builder.HasOne(d => d.SuggestedCategory)
                .WithMany()
                .HasForeignKey(d => d.SuggestedCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // N-1 (optional): ImportTransactionDraft → Jar (suggested)
            builder.HasOne(d => d.SuggestedJar)
                .WithMany()
                .HasForeignKey(d => d.SuggestedJarId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        // ── 21. ai_settings ───────────────────────────────────────
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
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0.7m);

            builder.Property(a => a.MaxTokens)
                .IsRequired()
                .HasDefaultValue(1000);

            builder.Property(a => a.ApiKeyEncrypted).HasColumnType("text");
            builder.Property(a => a.IsEnabled).HasDefaultValue(true);

            // N-1 (optional): AiSetting → Account (admin)
            builder.HasOne(a => a.UpdatedByAdmin)
                .WithMany()
                .HasForeignKey(a => a.UpdatedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}