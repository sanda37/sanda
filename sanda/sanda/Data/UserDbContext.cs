using Microsoft.EntityFrameworkCore;
using sanda.Models;

namespace sanda.Data
{
    public class UserDbContext : DbContext
    {
        // Constructor to accept DbContextOptions
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        // DbSet for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<ServiceItem> ServiceItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Order> Orders { get; set; }
        //  public DbSet<FavoriteService> FavoriteServices { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbContextOptions<UserDbContext> Options { get; internal set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Volunteer entity with simpler configuration
            modelBuilder.Entity<Volunteer>(entity =>
            {
                entity.HasKey(v => v.ID);
                entity.Property(v => v.Nursing);
                entity.Property(v => v.PhysicalTherapy);
                entity.Property(v => v.Balance);
                entity.Property(v => v.CreatedAt);
            });

            // Configure table names explicitly
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<ServiceItem>().ToTable("ServiceItems");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Volunteer>().ToTable("Volunteers");
            //  modelBuilder.Entity<FavoriteService>().ToTable("FavoriteServices");
            modelBuilder.Entity<Wallet>().ToTable("Wallets");

            // Define the relationship between Orders and Volunteers
            modelBuilder.Entity<Order>()
                .HasOne<Volunteer>()
                .WithMany(v => v.AcceptedOrders)
                .HasForeignKey(o => o.VolunteerId)
                .IsRequired(false) // VolunteerId can be null if the order hasn't been assigned yet
                .OnDelete(DeleteBehavior.SetNull); // If a volunteer is deleted, set VolunteerId to null

            // Additional relationship configurations (add as needed based on your models)

            // Example: If User has a relationship with Orders
            // modelBuilder.Entity<Order>()
            //     .HasOne<User>()
            //     .WithMany(u => u.Orders)
            //     .HasForeignKey(o => o.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // Example: If User has a relationship with FavoriteServices
            // modelBuilder.Entity<FavoriteService>()
            //     .HasOne<User>()
            //     .WithMany(u => u.FavoriteServices)
            //     .HasForeignKey(fs => fs.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);

            // Example: If User has a relationship with Wallet
            // modelBuilder.Entity<Wallet>()
            //     .HasOne<User>()
            //     .WithOne(u => u.Wallet)
            //     .HasForeignKey<Wallet>(w => w.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}