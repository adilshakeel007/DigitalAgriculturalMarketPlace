using System.ComponentModel.DataAnnotations;
namespace DigitalAgriculturalMarketplace.Models;
public class Quotation
{
    public int Id { get; set; }
    public int RfqId { get; set; }
    public int SellerId { get; set; }
    [Required,StringLength(40)] public string QuotationNumber { get; set; }=string.Empty;
    [Required,StringLength(30)] public string Status { get; set; }="Draft";
    [StringLength(2000)] public string? Notes { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; }=[];
    public Rfq? Rfq { get; set; }
    public User? Seller { get; set; }
    public ICollection<QuotationItem> Items { get; set; }=[];
}
