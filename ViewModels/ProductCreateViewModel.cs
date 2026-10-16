using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 1000000)]
        public decimal PricePerUnit { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(typeof(decimal), "0", "1000000")]
        public decimal Quantity { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Select an approved catalog product.")]
        public int? CatalogProductId { get; set; }

        [Required(ErrorMessage = "Select a unit of measure.")]
        public int? UnitOfMeasureId { get; set; }

        public List<DigitalAgriculturalMarketplace.Models.CatalogProduct> CatalogProducts { get; set; } = new();

        public List<DigitalAgriculturalMarketplace.Models.UnitOfMeasure> Units { get; set; } = new();

        public IFormFile? ImageFile { get; set; }
    }
}
