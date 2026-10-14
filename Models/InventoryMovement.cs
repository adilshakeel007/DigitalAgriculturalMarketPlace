using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalAgriculturalMarketplace.Models;

public class InventoryMovement
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    [Required, StringLength(40)] public string MovementType { get; set; } = string.Empty;
    [Column(TypeName="decimal(18,3)")] public decimal AvailableDelta { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal ReservedDelta { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal AvailableAfter { get; set; }
    [Column(TypeName="decimal(18,3)")] public decimal ReservedAfter { get; set; }
    [StringLength(40)] public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int? PerformedByUserId { get; set; }
    [StringLength(500)] public string? Reason { get; set; }
    [Required, StringLength(150)] public string OperationKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Product? Product { get; set; }
    public User? PerformedByUser { get; set; }
}
