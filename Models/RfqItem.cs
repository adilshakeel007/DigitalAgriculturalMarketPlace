using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DigitalAgriculturalMarketplace.Models;
public class RfqItem
{
    public int Id { get; set; }
    public int RfqId { get; set; }
    public int CatalogProductId { get; set; }
    public int UnitOfMeasureId { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal RequestedQuantity { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal AcceptedQuantity { get; set; }
    [Column(TypeName="decimal(18,2)")] public decimal? MaximumBudgetPerUnit { get; set; }
    [StringLength(1000)] public string? Specifications { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; }=[];
    public Rfq? Rfq { get; set; }
    public CatalogProduct? CatalogProduct { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public ICollection<QuotationItem> QuotationItems { get; set; }=[];
    public ICollection<QuotationAllocation> Allocations { get; set; }=[];
}
