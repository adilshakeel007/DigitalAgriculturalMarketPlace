using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DigitalAgriculturalMarketplace.Models;
public class QuotationItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public int RfqItemId { get; set; }
    public int ProductId { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal OfferedQuantity { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal AcceptedQuantity { get; set; }
    [Column(TypeName="decimal(18,2)")] public decimal UnitPrice { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; }=[];
    public Quotation? Quotation { get; set; }
    public RfqItem? RfqItem { get; set; }
    public Product? Product { get; set; }
    public ICollection<QuotationAllocation> Allocations { get; set; }=[];
}
