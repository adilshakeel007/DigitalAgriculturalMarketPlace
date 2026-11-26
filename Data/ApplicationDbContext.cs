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
        public DbSet<SellerProfile> SellerProfiles { get; set; }
        public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
        public DbSet<CatalogProduct> CatalogProducts { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<InventoryReservation> InventoryReservations { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
        public DbSet<Rfq> Rfqs { get; set; }
        public DbSet<RfqItem> RfqItems { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<QuotationAllocation> QuotationAllocations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<SellerProfile>()
                .HasOne(s => s.User)
                .WithOne(u => u.SellerProfile)
                .HasForeignKey<SellerProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SellerProfile>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<SellerProfile>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_SellerProfiles_VerificationStatus",
                    "[VerificationStatus] IN ('Unverified', 'Verified', 'Rejected')"));

            modelBuilder.Entity<UnitOfMeasure>()
                .HasIndex(u => u.Code)
                .IsUnique();

            modelBuilder.Entity<UnitOfMeasure>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_UnitsOfMeasure_DecimalPlaces",
                    "[DecimalPlaces] BETWEEN 0 AND 3"));

            modelBuilder.Entity<UnitOfMeasure>().HasData(
                new UnitOfMeasure { Id = 1, Code = "kg", Name = "Kilogram", DecimalPlaces = 3, IsActive = true },
                new UnitOfMeasure { Id = 2, Code = "tonne", Name = "Tonne", DecimalPlaces = 3, IsActive = true },
                new UnitOfMeasure { Id = 3, Code = "bag", Name = "Bag", DecimalPlaces = 0, IsActive = true },
                new UnitOfMeasure { Id = 4, Code = "crate", Name = "Crate", DecimalPlaces = 0, IsActive = true },
                new UnitOfMeasure { Id = 5, Code = "piece", Name = "Piece", DecimalPlaces = 0, IsActive = true });

            modelBuilder.Entity<CatalogProduct>()
                .HasOne(c => c.DefaultUnit)
                .WithMany(u => u.CatalogProducts)
                .HasForeignKey(c => c.DefaultUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CatalogProduct>()
                .HasIndex(c => new { c.Category, c.Name });

            modelBuilder.Entity<CatalogProduct>()
                .HasIndex(c => new
                {
                    c.SubmittedByUserId,
                    c.Name,
                    c.Category,
                    c.DefaultUnitId,
                    c.Status
                })
                .IsUnique();

            modelBuilder.Entity<CatalogProduct>()
                .HasOne(c => c.SubmittedByUser)
                .WithMany(u => u.SubmittedCatalogProducts)
                .HasForeignKey(c => c.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CatalogProduct>()
                .HasOne(c => c.ReviewedByUser)
                .WithMany(u => u.ReviewedCatalogProducts)
                .HasForeignKey(c => c.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CatalogProduct>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_CatalogProducts_Status",
                    "[Status] IN ('Pending', 'Approved', 'Rejected')"));

            modelBuilder.Entity<CatalogProduct>()
                .Property(c => c.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<CatalogProduct>()
                .Property(c => c.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Product>()
    .HasOne(p => p.Farmer)
    .WithMany(u => u.Products)
    .HasForeignKey(p => p.FarmerId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.CatalogProduct)
                .WithMany(c => c.ProductListings)
                .HasForeignKey(p => p.CatalogProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.UnitOfMeasure)
                .WithMany(u => u.ProductListings)
                .HasForeignKey(p => p.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.FarmerId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.FarmerId, p.IsActive });

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.CatalogProductId, p.IsActive });

            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Product>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Products_AvailableQuantity_NonNegative",
                        "[AvailableQuantity] IS NULL OR [AvailableQuantity] >= 0");
                    t.HasCheckConstraint(
                        "CK_Products_ReservedQuantity_NonNegative",
                        "[ReservedQuantity] >= 0");
                    t.HasCheckConstraint(
                        "CK_Products_MinimumOrderQuantity_Positive",
                        "[MinimumOrderQuantity] IS NULL OR [MinimumOrderQuantity] > 0");
                });

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

            modelBuilder.Entity<Order>().Property(o => o.RowVersion).IsRowVersion();
            modelBuilder.Entity<Order>().ToTable(t => t.HasCheckConstraint("CK_Orders_Quantity_Positive", "[Quantity] > 0"));
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique().HasFilter("[OrderNumber] IS NOT NULL");
            modelBuilder.Entity<Order>().HasIndex(o => new { o.SellerId, o.Status });
            modelBuilder.Entity<Order>().HasOne(o => o.Seller).WithMany().HasForeignKey(o => o.SellerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<OrderItem>().HasIndex(x => x.OrderId);
            modelBuilder.Entity<OrderItem>().HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>().HasOne(x => x.Product).WithMany(p => p.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>().HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderItem>().ToTable(t => t.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0"));

            modelBuilder.Entity<InventoryReservation>().Property(x => x.RowVersion).IsRowVersion();
            modelBuilder.Entity<InventoryReservation>().HasIndex(x => x.OperationKey).IsUnique();
            modelBuilder.Entity<InventoryReservation>().HasIndex(x => new { x.ProductId, x.Status });
            modelBuilder.Entity<InventoryReservation>().HasOne(x => x.Product).WithMany(p => p.InventoryReservations).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryReservation>().HasOne(x => x.OrderItem).WithOne(i => i.Reservation).HasForeignKey<InventoryReservation>(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryReservation>().ToTable(t => { t.HasCheckConstraint("CK_InventoryReservations_Quantity_Positive", "[Quantity] > 0"); t.HasCheckConstraint("CK_InventoryReservations_Status", "[Status] IN ('Active','Released','Consumed')"); t.HasCheckConstraint("CK_InventoryReservations_TerminalState", "NOT ([ReleasedAt] IS NOT NULL AND [ConsumedAt] IS NOT NULL)"); });

            modelBuilder.Entity<InventoryMovement>().HasIndex(x => x.OperationKey).IsUnique();
            modelBuilder.Entity<InventoryMovement>().HasIndex(x => new { x.ProductId, x.CreatedAt });
            modelBuilder.Entity<InventoryMovement>().HasOne(x => x.Product).WithMany(p => p.InventoryMovements).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryMovement>().HasOne(x => x.PerformedByUser).WithMany().HasForeignKey(x => x.PerformedByUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<InventoryMovement>().ToTable(t => { t.HasCheckConstraint("CK_InventoryMovements_AvailableAfter", "[AvailableAfter] >= 0"); t.HasCheckConstraint("CK_InventoryMovements_ReservedAfter", "[ReservedAfter] >= 0"); });

            modelBuilder.Entity<OrderStatusHistory>().HasIndex(x => x.OperationKey).IsUnique();
            modelBuilder.Entity<OrderStatusHistory>().HasIndex(x => new { x.OrderId, x.ChangedAt });
            modelBuilder.Entity<OrderStatusHistory>().HasOne(x => x.Order).WithMany(o => o.StatusHistory).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderStatusHistory>().HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rfq>().Property(x=>x.RowVersion).IsRowVersion();
            modelBuilder.Entity<Rfq>().HasIndex(x=>x.RfqNumber).IsUnique();
            modelBuilder.Entity<Rfq>().HasIndex(x=>new{x.BuyerId,x.Status});
            modelBuilder.Entity<Rfq>().HasOne(x=>x.Buyer).WithMany().HasForeignKey(x=>x.BuyerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rfq>().ToTable(t=>t.HasCheckConstraint("CK_Rfqs_Status","[Status] IN ('Draft','Open','PartiallyFulfilled','Fulfilled','Closed','Cancelled')"));

            modelBuilder.Entity<RfqItem>().Property(x=>x.RowVersion).IsRowVersion();
            modelBuilder.Entity<RfqItem>().HasIndex(x=>new{x.RfqId,x.CatalogProductId,x.UnitOfMeasureId}).IsUnique();
            modelBuilder.Entity<RfqItem>().HasOne(x=>x.Rfq).WithMany(x=>x.Items).HasForeignKey(x=>x.RfqId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RfqItem>().HasOne(x=>x.CatalogProduct).WithMany().HasForeignKey(x=>x.CatalogProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RfqItem>().HasOne(x=>x.UnitOfMeasure).WithMany().HasForeignKey(x=>x.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RfqItem>().ToTable(t=>{t.HasCheckConstraint("CK_RfqItems_RequestedQuantity","[RequestedQuantity] > 0");t.HasCheckConstraint("CK_RfqItems_AcceptedQuantity","[AcceptedQuantity] >= 0 AND [AcceptedQuantity] <= [RequestedQuantity]");});

            modelBuilder.Entity<Quotation>().Property(x=>x.RowVersion).IsRowVersion();
            modelBuilder.Entity<Quotation>().HasIndex(x=>x.QuotationNumber).IsUnique();
            modelBuilder.Entity<Quotation>().HasIndex(x=>new{x.RfqId,x.SellerId}).IsUnique();
            modelBuilder.Entity<Quotation>().HasOne(x=>x.Rfq).WithMany(x=>x.Quotations).HasForeignKey(x=>x.RfqId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Quotation>().HasOne(x=>x.Seller).WithMany().HasForeignKey(x=>x.SellerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Quotation>().ToTable(t=>t.HasCheckConstraint("CK_Quotations_Status","[Status] IN ('Draft','Submitted','PartiallyAccepted','Accepted','Rejected','Withdrawn','Expired')"));

            modelBuilder.Entity<QuotationItem>().Property(x=>x.RowVersion).IsRowVersion();
            modelBuilder.Entity<QuotationItem>().HasIndex(x=>new{x.QuotationId,x.RfqItemId}).IsUnique();
            modelBuilder.Entity<QuotationItem>().HasOne(x=>x.Quotation).WithMany(x=>x.Items).HasForeignKey(x=>x.QuotationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationItem>().HasOne(x=>x.RfqItem).WithMany(x=>x.QuotationItems).HasForeignKey(x=>x.RfqItemId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationItem>().HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationItem>().ToTable(t=>{t.HasCheckConstraint("CK_QuotationItems_OfferedQuantity","[OfferedQuantity] > 0");t.HasCheckConstraint("CK_QuotationItems_AcceptedQuantity","[AcceptedQuantity] >= 0 AND [AcceptedQuantity] <= [OfferedQuantity]");t.HasCheckConstraint("CK_QuotationItems_UnitPrice","[UnitPrice] > 0");});

            modelBuilder.Entity<QuotationAllocation>().HasIndex(x=>x.OperationKey).IsUnique();
            modelBuilder.Entity<QuotationAllocation>().HasIndex(x=>new{x.RfqItemId,x.CreatedAt});
            modelBuilder.Entity<QuotationAllocation>().HasOne(x=>x.QuotationItem).WithMany(x=>x.Allocations).HasForeignKey(x=>x.QuotationItemId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationAllocation>().HasOne(x=>x.RfqItem).WithMany(x=>x.Allocations).HasForeignKey(x=>x.RfqItemId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationAllocation>().HasOne(x=>x.Buyer).WithMany().HasForeignKey(x=>x.BuyerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationAllocation>().HasOne(x=>x.Seller).WithMany().HasForeignKey(x=>x.SellerId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationAllocation>().HasOne(x=>x.Order).WithMany().HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<QuotationAllocation>().ToTable(t=>{t.HasCheckConstraint("CK_QuotationAllocations_Quantity","[AcceptedQuantity] > 0");t.HasCheckConstraint("CK_QuotationAllocations_Price","[AcceptedUnitPrice] > 0");});

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

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.OperationKey)
                .IsUnique()
                .HasFilter("[OperationKey] IS NOT NULL");

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
