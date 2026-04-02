using Microsoft.EntityFrameworkCore;
using StockBite.Models;

namespace StockBite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<VendorProduct> VendorProducts => Set<VendorProduct>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Consumer> Consumers => Set<Consumer>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<VendorProduct>(entity =>
            {
                entity.HasOne(vp => vp.Vendor)
                    .WithMany(v => v.VendorProducts)
                    .HasForeignKey(vp => vp.VendorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(vp => vp.Product)
                    .WithMany(p => p.VendorProducts)
                    .HasForeignKey(vp => vp.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(vp => vp.Price).HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Vendor)
                    .WithMany(v => v.Orders)
                    .HasForeignKey(o => o.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Product)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(o => o.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Consumer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.ConsumerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(o => o.TotalPrice).HasColumnType("decimal(10,2)");
                entity.Property(o => o.CustomerName).HasMaxLength(200);
                entity.Property(o => o.DeliveryAddress).HasMaxLength(300);
                entity.Property(o => o.PaymentMethod).HasMaxLength(50);
            });

            modelBuilder.Entity<Consumer>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.GuestCode).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.EmailAccessCode).HasMaxLength(20);
                entity.Property(e => e.EmailCodePurpose).HasMaxLength(20);
            });

            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CustomerEmail).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.AdminReply).HasMaxLength(1000);
                entity.Property(e => e.CustomerReply).HasMaxLength(1000);

                entity.HasOne(e => e.Consumer)
                    .WithMany()
                    .HasForeignKey(e => e.ConsumerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
