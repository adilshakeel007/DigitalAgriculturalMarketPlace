using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class PlaceOrderViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal PricePerUnit { get; set; }

        public decimal AvailableQuantity { get; set; }
        public string? UnitName { get; set; }
        [Required]
        public string OperationKey { get; set; } = Guid.NewGuid().ToString("N");

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 1000000, ErrorMessage = "Quantity must be at least 1.")]
        public decimal Quantity { get; set; } = 1;
    }
}
