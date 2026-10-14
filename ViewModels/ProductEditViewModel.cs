using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }
            = string.Empty;

        [Required]
        public string Category { get; set; }
            = string.Empty;

        [Range(0.01, 1000000)]
        public decimal PricePerUnit { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public string? CurrentImageUrl { get; set; }

        public IFormFile? NewImageFile { get; set; }
    }
}
