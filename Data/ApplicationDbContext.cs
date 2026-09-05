using DigitalAgriculturalMarketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketPlace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
           : base(options)
        { 
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
    .HasOne(p => p.Farmer)
    .WithMany(u => u.Products)
    .HasForeignKey(p => p.FarmerId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
    .HasOne(o => o.Consumer)
    .WithMany(u => u.Orders)
    .HasForeignKey(o => o.ConsumerId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
        .HasOne(r => r.Consumer)
        .WithMany(u => u.Reviews)
        .HasForeignKey(r => r.ConsumerId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
    .HasOne(m => m.Sender)
    .WithMany(u => u.SentMessages)
    .HasForeignKey(m => m.SenderId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Product)
                .WithMany(p => p.Messages)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Notification>()
    .HasOne(n => n.User)
    .WithMany(u => u.Notifications)
    .HasForeignKey(n => n.UserId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
    .HasIndex(r => new
    {
        r.ConsumerId,
        r.ProductId
    })
    .IsUnique();

        }

        
        


    
    }
}
