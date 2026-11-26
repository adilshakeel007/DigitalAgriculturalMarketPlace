using System.ComponentModel.DataAnnotations;
namespace DigitalAgriculturalMarketplace.ViewModels;
public class InventoryAdjustmentViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentAvailable { get; set; }
    public decimal Reserved { get; set; }
    [Range(typeof(decimal),"0","999999999")] public decimal NewAvailable { get; set; }
    [Required, StringLength(500,MinimumLength=3)] public string Reason { get; set; } = string.Empty;
    public string OperationKey { get; set; } = Guid.NewGuid().ToString("N");
}
