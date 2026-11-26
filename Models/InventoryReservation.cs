using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalAgriculturalMarketplace.Models;

public class InventoryReservation
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public long? OrderItemId { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal Quantity { get; set; }
    [Required, StringLength(20)] public string Status { get; set; } = "Active";
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    [Required, StringLength(150)] public string OperationKey { get; set; } = string.Empty;
    [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Product? Product { get; set; }
    public OrderItem? OrderItem { get; set; }
}
