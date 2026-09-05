using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class SendMessageViewModel
    {
        public int ReceiverId { get; set; }

        public int? ProductId { get; set; }

        public string ReceiverName { get; set; } = string.Empty;

        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(
            2000,
            ErrorMessage = "Message cannot exceed 2000 characters."
        )]
        public string Content { get; set; } = string.Empty;
    }
}
