using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalAgriculturalMarketplace.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Consumer who placed the order
        public int ConsumerId { get; set; }

        // Product being ordered
        public int ProductId { get; set; }

        [Required]
        [Range(1, 1000000)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public User? Consumer { get; set; }

        public Product? Product { get; set; }
    }
}