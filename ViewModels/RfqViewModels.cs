using System.ComponentModel.DataAnnotations;
using DigitalAgriculturalMarketplace.Models;
namespace DigitalAgriculturalMarketplace.ViewModels;
public class RfqCreateViewModel
{
    public int Id { get; set; }
    [Required,StringLength(200)] public string Title { get; set; }=string.Empty;
    [StringLength(2000)] public string? Description { get; set; }
    [Required,StringLength(300)] public string DeliveryLocation { get; set; }=string.Empty;
    [DataType(DataType.Date)] public DateTime RequiredByDate { get; set; }=DateTime.Today.AddDays(7);
    public List<RfqItemInputViewModel> Items { get; set; }=[];
    public IReadOnlyList<CatalogProduct> CatalogProducts { get; set; }=[];
    public IReadOnlyList<UnitOfMeasure> Units { get; set; }=[];
}
public class RfqItemInputViewModel
{
    [Range(1,int.MaxValue)] public int CatalogProductId { get; set; }
    [Range(1,int.MaxValue)] public int UnitOfMeasureId { get; set; }
    [Range(typeof(decimal),"0.001","999999999")] public decimal RequestedQuantity { get; set; }
    [Range(typeof(decimal),"0.01","999999999")] public decimal? MaximumBudgetPerUnit { get; set; }
    [StringLength(1000)] public string? Specifications { get; set; }
}
public class QuotationCreateViewModel
{
    public int Id { get; set; }
    public int RfqId { get; set; }
    [Required,DataType(DataType.Date)] public DateTime ValidUntil { get; set; }=DateTime.Today.AddDays(7);
    [StringLength(2000)] public string? Notes { get; set; }
    public List<QuotationItemInputViewModel> Items { get; set; }=[];
}
public class QuotationItemInputViewModel
{
    public int RfqItemId { get; set; }
    public int ProductId { get; set; }
    [Range(typeof(decimal),"0","999999999")] public decimal OfferedQuantity { get; set; }
    [Range(typeof(decimal),"0.01","999999999")] public decimal UnitPrice { get; set; }
    [DataType(DataType.Date)] public DateTime EstimatedDeliveryDate { get; set; }=DateTime.Today.AddDays(3);
    [StringLength(1000)] public string? Notes { get; set; }
}
public class AllocationViewModel
{
    public int QuotationItemId { get; set; }
    [Range(typeof(decimal),"0.001","999999999")] public decimal Quantity { get; set; }
    [Required] public string OperationKey { get; set; }=Guid.NewGuid().ToString("N");
}
