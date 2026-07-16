using Microsoft.EntityFrameworkCore;
using Payment.Data.Entities;

namespace Payment.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => x.OrderId).IsUnique(); // One payment record per order
            });

            modelBuilder.Entity<IdempotencyRecord>(entity =>
            {
                entity.HasKey(x => x.EventId);
            });
        }
    }
}
