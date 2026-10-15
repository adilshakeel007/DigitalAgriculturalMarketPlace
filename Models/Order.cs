using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalAgriculturalMarketplace.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Consumer who placed the order
        public int ConsumerId { get; set; }

        // Product being ordered
        public int ProductId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.001", "1000000")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public int? SellerId { get; set; }
        [StringLength(40)] public string? OrderNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? Subtotal { get; set; }
        [StringLength(500)] public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation Properties
        public User? Consumer { get; set; }

        public Product? Product { get; set; }
        public User? Seller { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
