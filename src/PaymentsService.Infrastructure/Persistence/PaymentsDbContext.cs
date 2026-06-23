using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(e =>
        {
            e.ToTable("Wallets");
            e.HasKey(w => w.Id);
            e.Property(w => w.DocumentId).HasMaxLength(20).IsRequired();
            e.Property(w => w.Name).HasMaxLength(120).IsRequired();
            e.Property(w => w.Balance).HasColumnType("decimal(19,4)");
        });

        modelBuilder.Entity<Movement>(e =>
        {
            e.ToTable("Movements");
            e.HasKey(m => m.Id);
            e.Property(m => m.Amount).HasColumnType("decimal(19,4)");
            e.Property(m => m.Type).HasConversion<string>().HasMaxLength(10);
        });

        modelBuilder.Entity<IdempotencyKey>(e =>
        {
            e.ToTable("IdempotencyKeys");
            e.HasKey(k => k.Key);
        });
    }
}
