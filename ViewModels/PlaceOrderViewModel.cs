using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class PlaceOrderViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal PricePerUnit { get; set; }

        public int AvailableQuantity { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 1000000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;
    }
}
