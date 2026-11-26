using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class CatalogModerationViewModel
    {
        public int Id { get; set; }

        [StringLength(1000)]
        public string? ReviewNotes { get; set; }

        [Required]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
