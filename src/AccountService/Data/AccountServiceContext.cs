using AccountService.Domain.Outbox;
using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data; 

public class AccountServiceContext(DbContextOptions<AccountServiceContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .Property(a => a.Version)
            .IsRowVersion()
            .HasConversion<uint>()
            .HasColumnName("xmin");

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Version)
            .IsRowVersion()
            .HasConversion<uint>()
            .HasColumnName("xmin");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Type).HasConversion<string>();
            entity.Property(a => a.Currency).HasMaxLength(3);
            entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
            entity.Property(a => a.InterestRate).HasColumnType("decimal(5,4)");
            entity.HasIndex(a => a.OwnerId)
                .HasMethod("HASH");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Type).HasConversion<string>();
            entity.Property(t => t.Currency).HasMaxLength(3);
            entity.Property(t => t.Sum).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Description).HasMaxLength(255);
            entity.ToTable(t => t.HasCheckConstraint("CK_Transfer_Amount", @"""Type"" <> 'Transfer' OR (""Sum"" <> 0 AND ""CounterpartyAccountId"" IS NOT NULL)"));
            entity.HasIndex(t => new { t.AccountId, t.DateTime })
                .HasDatabaseName("IX_Transactions_AccountId_DateTime");
            entity.HasIndex(t => t.DateTime)
                .HasMethod("GIST")
                .HasDatabaseName("IX_Transactions_DateTime_Gist");
        });
    }
}