using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AlphaLogistics.API.Services
{
    public class CartService : ICartService
    {
        private readonly AlphaLogisticsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(AlphaLogisticsContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to get current user ID from claims
        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new Exception("User not authenticated");
            return userId;
        }

        private CartItemResponseDto ConvertToCartItemDto(CartMaster cartItem)
        {
            return new CartItemResponseDto
            {
                CartItemId = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.ProductMaster?.ProductName ?? "Product Not Found",
                ProductDescription = cartItem.ProductMaster?.Description ?? string.Empty,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                ProductImage = cartItem.ProductMaster?.ProductImages?.FirstOrDefault()?.ImageUrl,
                VendorId = cartItem.ProductMaster?.VendorId ?? 0,
                VendorName = cartItem.ProductMaster?.VendorMaster?.VendorName ?? "Unknown Vendor",
                CreatedAt = cartItem.CreatedAt,
                IsProductActive = cartItem.ProductMaster?.IsActive ?? false,
                AvailableStock = cartItem.ProductMaster?.StockQuantity ?? 0
            };
        }


        public async Task<CartResponseDto> GetCartByUserIdAsync(int userId)
        {
            var user = await _context.UserMasters
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            var cartItems = await _context.CartMasters
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.VendorMaster)
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.ProductImages)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var cartItemDtos = cartItems.Select(ConvertToCartItemDto).ToList();

            var lastUpdated = cartItems.Any() ? cartItems.Max(c => c.CreatedAt) : (DateTime?)null;

            return new CartResponseDto
            {
                UserId = userId,
                UserName = user.UserName,
                UserEmail = user.Email ?? string.Empty,
                CartItems = cartItemDtos,
                LastUpdated = lastUpdated
            };
        }

        public async Task<CartItemResponseDto> AddToCartAsync(int userId, AddToCartDto addToCartDto)
        {
            var user = await _context.UserMasters
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            var product = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == addToCartDto.ProductId);

            if (product == null)
                throw new Exception("Product not found");

            if (!product.IsActive)
                throw new Exception("Product is not active");

            if (product.StockQuantity < addToCartDto.Quantity)
                throw new Exception($"Insufficient stock. Available: {product.StockQuantity}");

            var existingCartItem = await _context.CartMasters
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.VendorMaster)
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == addToCartDto.ProductId);

            if (existingCartItem != null)
            {
                var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;

                if (product.StockQuantity < newQuantity)
                    throw new Exception($"Insufficient stock. Available: {product.StockQuantity}");

                existingCartItem.Quantity = newQuantity;
                existingCartItem.UnitPrice = product.Price;
            }
            else
            {
                //for new item add
                var cartItem = new CartMaster
                {
                    UserId = userId,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    UnitPrice = product.Price,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CartMasters.Add(cartItem);
                existingCartItem = cartItem;
            }

            await _context.SaveChangesAsync();

            if (existingCartItem.ProductMaster == null)
            {
                existingCartItem = await _context.CartMasters
                    .Include(c => c.ProductMaster)
                        .ThenInclude(p => p.VendorMaster)
                    .Include(c => c.ProductMaster)
                        .ThenInclude(p => p.ProductImages)
                    .FirstOrDefaultAsync(c => c.Id == existingCartItem.Id);
            }

            return ConvertToCartItemDto(existingCartItem);
        }

       /* public async Task<CartItemResponseDto> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto updateDto)
        {
            var cartItem = await _context.CartMasters
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.VendorMaster)
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            // Verify product is still available
            if (cartItem.ProductMaster == null)
                throw new Exception("Product not found");

            if (!cartItem.ProductMaster.IsActive)
                throw new Exception("Product is no longer active");

            // Check stock availability
            if (cartItem.ProductMaster.StockQuantity < updateDto.Quantity)
                throw new Exception($"Insufficient stock. Available: {cartItem.ProductMaster.StockQuantity}");

            // Update quantity and price
            cartItem.Quantity = updateDto.Quantity;
            cartItem.UnitPrice = cartItem.ProductMaster.Price;

            await _context.SaveChangesAsync();

            return ConvertToCartItemDto(cartItem);
        }*/

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.CartMasters
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            _context.CartMasters.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cartItems = await _context.CartMasters
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                throw new Exception("Cart is already empty");

            _context.CartMasters.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveProductFromCartAsync(int userId, int productId)
        {
            var cartItem = await _context.CartMasters
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
                throw new Exception("Product not found in cart");

            _context.CartMasters.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetCartTotalAsync(int userId)
        {
            var cartItems = await _context.CartMasters
                .Include(c => c.ProductMaster)
                .Where(c => c.UserId == userId &&
                           c.ProductMaster != null &&
                           c.ProductMaster.IsActive)
                .ToListAsync();

            var total = cartItems
                .Where(c => c.ProductMaster.StockQuantity >= c.Quantity)
                .Sum(c => c.Quantity * c.UnitPrice);

            return total;
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            var itemCount = await _context.CartMasters
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            return itemCount;
        }

        // Get inactive cart items (products that are no longer active or out of stock)
        public async Task<List<CartItemResponseDto>> GetInactiveCartItemsAsync(int userId)
        {
            var cartItems = await _context.CartMasters
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.VendorMaster)
                .Include(c => c.ProductMaster)
                    .ThenInclude(p => p.ProductImages)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var inactiveItems = cartItems
                .Where(c => c.ProductMaster == null ||
                           !c.ProductMaster.IsActive ||
                           c.ProductMaster.StockQuantity < c.Quantity)
                .Select(ConvertToCartItemDto)
                .ToList();

            return inactiveItems;
        }

        // Merge carts from one user to another (useful for guest to logged-in user conversion)
        public async Task<bool> MergeCartsAsync(int sourceUserId, int targetUserId)
        {
            var sourceCartItems = await _context.CartMasters
                .Where(c => c.UserId == sourceUserId)
                .ToListAsync();

            if (!sourceCartItems.Any())
                return true; 

            foreach (var sourceItem in sourceCartItems)
            {
                // Check if item already exists in target cart
                var existingItem = await _context.CartMasters
                    .FirstOrDefaultAsync(c => c.UserId == targetUserId &&
                                             c.ProductId == sourceItem.ProductId);

                if (existingItem != null)
                {
                    
                    existingItem.Quantity += sourceItem.Quantity;
                    // Update price to latest
                    var product = await _context.ProductMasters
                        .FirstOrDefaultAsync(p => p.Id == sourceItem.ProductId);
                    if (product != null)
                        existingItem.UnitPrice = product.Price;
                }
                else
                {
                    // Add new item to target cart
                    var newCartItem = new CartMaster
                    {
                        UserId = targetUserId,
                        ProductId = sourceItem.ProductId,
                        Quantity = sourceItem.Quantity,
                        UnitPrice = sourceItem.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CartMasters.Add(newCartItem);
                }
            }

            _context.CartMasters.RemoveRange(sourceCartItems);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CartResponseDto> GetCartForCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartItemResponseDto> AddToCartForCurrentUserAsync(AddToCartDto addToCartDto)
        {
            var userId = GetCurrentUserId();
            return await AddToCartAsync(userId, addToCartDto);
        }
    }
}
