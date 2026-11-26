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
        public string? UnitCode { get; set; }
        public byte DecimalPlaces { get; set; }
        public decimal MinimumOrderQuantity { get; set; } = 0.001m;
        [Required]
        public string OperationKey { get; set; } = Guid.NewGuid().ToString("N");

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(typeof(decimal), "0.001", "1000000", ErrorMessage = "Quantity must be greater than zero.")]
        public decimal Quantity { get; set; } = 1;
    }
}
