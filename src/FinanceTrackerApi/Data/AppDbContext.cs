using FinanceTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrackerApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Category).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            
            // Guarda o enum como texto ("Income" / "Expense") no SQLite
            entity.Property(t => t.Type).HasConversion<string>();
        });
    }
}