using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class CartItem
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

        // Calculated property
        public decimal TotalPrice => Price * Quantity;

        // Navigation property (optional, for displaying category info)
        public string? CategoryName { get; set; }
    }
}