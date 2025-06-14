using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } // Lưu tên sản phẩm tại thời điểm đặt hàng

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; } // Giá tại thời điểm đặt hàng

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; } // UnitPrice * Quantity

        public string? ProductImageUrl { get; set; } // Lưu ảnh sản phẩm tại thời điểm đặt hàng
    }
}
