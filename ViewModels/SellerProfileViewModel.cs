using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class SellerProfileViewModel
    {
        [Required]
        [StringLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
