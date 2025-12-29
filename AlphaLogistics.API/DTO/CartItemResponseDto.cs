namespace AlphaLogistics.API.DTO
{
    public class CartItemResponseDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public string? ProductImage { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProductActive { get; set; }
        public bool IsInStock => AvailableStock >= Quantity;
        public int AvailableStock { get; set; }
    }
}
