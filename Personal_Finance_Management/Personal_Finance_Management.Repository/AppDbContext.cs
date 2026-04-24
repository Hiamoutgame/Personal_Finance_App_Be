using Microsoft.EntityFrameworkCore;

namespace Personal_Finance_Management.Repository;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}
    // public DbSet<User>  Users { get; set; }
    //Tạo bảng
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}