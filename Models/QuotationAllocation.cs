using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DigitalAgriculturalMarketplace.Models;
public class QuotationAllocation
{
    public long Id { get; set; }
    public int QuotationItemId { get; set; }
    public int RfqItemId { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public int OrderId { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal AcceptedQuantity { get; set; }
    [Column(TypeName="decimal(18,2)")] public decimal AcceptedUnitPrice { get; set; }
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
    [Required,StringLength(150)] public string OperationKey { get; set; }=string.Empty;
    public QuotationItem? QuotationItem { get; set; }
    public RfqItem? RfqItem { get; set; }
    public User? Buyer { get; set; }
    public User? Seller { get; set; }
    public Order? Order { get; set; }
}
