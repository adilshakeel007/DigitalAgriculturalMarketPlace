using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation properties
        public User? Sender { get; set; }

        public User? Receiver { get; set; }

        public Product? Product { get; set; }
    }
}
