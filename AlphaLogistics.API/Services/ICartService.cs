using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);
        Task<CartItemResponseDto> AddToCartAsync(int userId, AddToCartDto addToCartDto);
        Task<CartItemResponseDto> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto updateDto);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<bool> RemoveProductFromCartAsync(int userId, int productId);
        Task<decimal> GetCartTotalAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
        Task<List<CartItemResponseDto>> GetInactiveCartItemsAsync(int userId);
        Task<bool> MergeCartsAsync(int sourceUserId, int targetUserId);
        Task<CartResponseDto> GetCartForCurrentUserAsync();
        Task<CartItemResponseDto> AddToCartForCurrentUserAsync(AddToCartDto addToCartDto);
    }
}
