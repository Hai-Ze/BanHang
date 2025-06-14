using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string OrderNumber { get; set; } // Mã đơn hàng tự động

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public string CustomerId { get; set; } // Foreign key đến ApplicationUser
        public ApplicationUser? Customer { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        [Phone]
        public string CustomerPhone { get; set; }

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ShippingFee { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal FinalAmount { get; set; }

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        [StringLength(1000)]
        public string CancelReason { get; set; }

        // Navigation properties
        public List<OrderDetail>? OrderDetails { get; set; }


    }
}
