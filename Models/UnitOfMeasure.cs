using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models
{
    public class UnitOfMeasure
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 3)]
        public byte DecimalPlaces { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<CatalogProduct> CatalogProducts { get; set; }
            = new List<CatalogProduct>();

        public ICollection<Product> ProductListings { get; set; }
            = new List<Product>();
    }
}
