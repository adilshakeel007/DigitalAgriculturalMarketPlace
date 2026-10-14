using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models
{
    public class SellerProfile
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(30)]
        public string VerificationStatus { get; set; } = "Unverified";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public User? User { get; set; }
    }
}
