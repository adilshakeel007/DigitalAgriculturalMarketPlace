using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalAgriculturalMarketplace.Models;

public class OrderItem
{
    public long Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    [Required, StringLength(150)] public string ProductNameSnapshot { get; set; } = string.Empty;
    [StringLength(100)] public string? UnitNameSnapshot { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal Quantity { get; set; }
    [Column(TypeName="decimal(18,2)")] public decimal UnitPrice { get; set; }
    [Column(TypeName="decimal(18,2)")] public decimal LineTotal { get; set; }
    public int SellerId { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Order? Order { get; set; }
    public Product? Product { get; set; }
    public User? Seller { get; set; }
    public InventoryReservation? Reservation { get; set; }
}
