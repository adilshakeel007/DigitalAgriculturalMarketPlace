using System.ComponentModel.DataAnnotations;
namespace DigitalAgriculturalMarketplace.Models;
public class Rfq
{
    public int Id { get; set; }
    [Required,StringLength(40)] public string RfqNumber { get; set; }=string.Empty;
    public int BuyerId { get; set; }
    [Required,StringLength(200)] public string Title { get; set; }=string.Empty;
    [StringLength(2000)] public string? Description { get; set; }
    [Required,StringLength(300)] public string DeliveryLocation { get; set; }=string.Empty;
    public DateTime RequiredByDate { get; set; }
    [Required,StringLength(30)] public string Status { get; set; }="Draft";
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; }=[];
    public User? Buyer { get; set; }
    public ICollection<RfqItem> Items { get; set; }=[];
    public ICollection<Quotation> Quotations { get; set; }=[];
}
