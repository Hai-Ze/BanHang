using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xác nhận")]
        Pending = 0,

        [Display(Name = "Đã xác nhận")]
        Confirmed = 1,

        [Display(Name = "Đang chuẩn bị")]
        Processing = 2,

        [Display(Name = "Đang giao hàng")]
        Shipping = 3,

        [Display(Name = "Đã giao hàng")]
        Delivered = 4,

        [Display(Name = "Đã hủy")]
        Cancelled = 5,

        [Display(Name = "Hoàn trả")]
        Returned = 6
    }
}
