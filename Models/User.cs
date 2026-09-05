using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string UserType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Product> Products { get; set; }
    = new List<Product>();
        public ICollection<Order> Orders { get; set; }
    = new List<Order>();
        public ICollection<Review> Reviews { get; set; }
    = new List<Review>();
        public ICollection<Message> SentMessages { get; set; }
    = new List<Message>();

        public ICollection<Message> ReceivedMessages { get; set; }
            = new List<Message>();
        public ICollection<Notification> Notifications { get; set; }
    = new List<Notification>();
    }
}
