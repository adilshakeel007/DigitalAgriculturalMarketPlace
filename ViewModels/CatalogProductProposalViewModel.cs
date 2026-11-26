using System.ComponentModel.DataAnnotations;
using DigitalAgriculturalMarketplace.Models;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class CatalogProductProposalViewModel
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int? DefaultUnitId { get; set; }

        public List<UnitOfMeasure> Units { get; set; } = new();
    }
}
