using Microsoft.EntityFrameworkCore;
using Shipping.Data.Entities;

namespace Shipping.Data
{
    public class ShippingDbContext : DbContext
    {
        public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

        public DbSet<Shipment> Shipments => Set<Shipment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TrackingNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(x => x.OrderId).IsUnique();
            });
        }
    }
}
