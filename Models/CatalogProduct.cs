using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models
{
    public class CatalogProduct
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? DefaultUnitId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public int? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public int? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? ReviewNotes { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public UnitOfMeasure? DefaultUnit { get; set; }
        public User? SubmittedByUser { get; set; }
        public User? ReviewedByUser { get; set; }

        public ICollection<Product> ProductListings { get; set; }
            = new List<Product>();
    }
}
