namespace BanHang.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Thêm sản phẩm vào giỏ hàng
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ, tăng số lượng
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới
                Items.Add(item);
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public void RemoveItem(int productId)
        {
            var itemToRemove = Items.FirstOrDefault(x => x.ProductId == productId);
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
            }
        }

        // Cập nhật số lượng sản phẩm
        public void UpdateQuantity(int productId, int newQuantity)
        {
            var item = Items.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    RemoveItem(productId);
                }
                else
                {
                    item.Quantity = newQuantity;
                }
            }
        }

        // Xóa tất cả sản phẩm trong giỏ
        public void Clear()
        {
            Items.Clear();
        }

        // Tính tổng tiền
        public decimal GetTotalPrice()
        {
            return Items.Sum(item => item.TotalPrice);
        }

        // Tính tổng số lượng sản phẩm
        public int GetTotalQuantity()
        {
            return Items.Sum(item => item.Quantity);
        }

        // Kiểm tra giỏ hàng có trống không
        public bool IsEmpty => !Items.Any();

        // Số lượng loại sản phẩm khác nhau
        public int ItemCount => Items.Count;
    }   
}