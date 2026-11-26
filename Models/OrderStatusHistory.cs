using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models;

public class OrderStatusHistory
{
    public long Id { get; set; }
    public int OrderId { get; set; }
    [StringLength(30)] public string? FromStatus { get; set; }
    [Required, StringLength(30)] public string ToStatus { get; set; } = string.Empty;
    public int? ChangedByUserId { get; set; }
    [StringLength(500)] public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    [Required, StringLength(150)] public string OperationKey { get; set; } = string.Empty;
    public Order? Order { get; set; }
    public User? ChangedByUser { get; set; }
}
