using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalAgriculturalMarketplace.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ConsumerId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Comment cannot exceed 1000 characters."
        )]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; }
            = DateTime.Now;

        // Navigation Properties
        public User? Consumer { get; set; }

        public Product? Product { get; set; }
    }
}