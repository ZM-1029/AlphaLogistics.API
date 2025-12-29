namespace AlphaLogistics.API.DTO
{
    public class CartResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<CartItemResponseDto> CartItems { get; set; } = new List<CartItemResponseDto>();
        public int ActiveItemsCount => CartItems.Count(item => item.IsProductActive && item.IsInStock);
        public int InactiveItemsCount => CartItems.Count(item => !item.IsProductActive || !item.IsInStock);
        public decimal CartTotal => CartItems.Where(item => item.IsProductActive && item.IsInStock)
                                             .Sum(item => item.TotalPrice);
        public decimal PotentialSavings { get; set; } // For future discount features
        public DateTime? LastUpdated { get; set; }
    }
}
