using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class CheckoutViewModel
    {
        // Giỏ hàng
        public ShoppingCart Cart { get; set; } = new ShoppingCart();

        // Thông tin khách hàng
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; } = "";

        // Thông tin giao hàng
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Ghi chú không được quá 1000 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Thông tin thanh toán
        [Display(Name = "Phí vận chuyển")]
        public decimal ShippingFee { get; set; } = 0;

        [Display(Name = "Giảm giá")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "Mã giảm giá")]
        public string? PromoCode { get; set; }

        // Phương thức thanh toán
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        // Calculated properties
        public decimal SubTotal => Cart?.GetTotalPrice() ?? 0;
        public decimal TotalAmount => SubTotal + ShippingFee - DiscountAmount;

        // Validation flags
        public bool AcceptTerms { get; set; } = false;
    }

    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng (COD)")]
        CashOnDelivery = 0,

        [Display(Name = "Chuyển khoản ngân hàng")]
        BankTransfer = 1,

        [Display(Name = "Thẻ tín dụng/Ghi nợ")]
        CreditCard = 2,

        [Display(Name = "Ví điện tử")]
        EWallet = 3
    }
}