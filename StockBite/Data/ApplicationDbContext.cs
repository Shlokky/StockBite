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
        //i just added dbser vendorproducts and fixed some namespace missedmatch changes here 
        public DbSet<VendorProduct> VendorProducts => Set<VendorProduct>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Consumer> Consumers => Set<Consumer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
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
            });

            modelBuilder.Entity<Consumer>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            });
        }
    }
}
