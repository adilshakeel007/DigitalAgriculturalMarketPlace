using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalAgriculturalMarketplace.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerUnit { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, 1000000, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime ListedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public int FarmerId { get; set; }

        public int? CatalogProductId { get; set; }

        public int? UnitOfMeasureId { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? AvailableQuantity { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal ReservedQuantity { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? MinimumOrderQuantity { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public User? Farmer { get; set; }
        public CatalogProduct? CatalogProduct { get; set; }
        public UnitOfMeasure? UnitOfMeasure { get; set; }
        public ICollection<Order> Orders { get; set; }
    = new List<Order>();
        public ICollection<Review> Reviews { get; set; }
    = new List<Review>();
        public ICollection<Message> Messages { get; set; }
    = new List<Message>();
    }
}
