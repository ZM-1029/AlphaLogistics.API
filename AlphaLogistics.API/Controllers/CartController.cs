using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: api/cart/my-cart
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            try
            {
                var cart = await _cartService.GetCartForCurrentUserAsync();
                return SuccessResponse(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for current user");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/cart/user/{userId}
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetCartByUserId(int userId)
        {
            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return SuccessResponse(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cart for user {userId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // POST: api/cart/add
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                var cartItem = await _cartService.AddToCartForCurrentUserAsync(addToCartDto);
                return SuccessResponse(cartItem, "Item added to cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // PUT: api/cart/update/{cartItemId}
        //[HttpPut("{cartItemId}")]
        //public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto updateDto)
        //{
        //    try
        //    {
        //        var cartItem = await _cartService.UpdateCartItemAsync(cartItemId, updateDto);
        //        return SuccessResponse(cartItem, "Cart item updated successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error updating cart item {cartItemId}");
        //        return ErrorResponse<string>(ex.Message);
        //    }
        //}

     
        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                await _cartService.RemoveFromCartAsync(cartItemId);
                return SuccessResponse<string>("", "Item removed from cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cart item {cartItemId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveProductFromCart(int productId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _cartService.RemoveProductFromCartAsync(userId, productId);
                return SuccessResponse<string>("", "Product removed from cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing product {productId} from cart");
                return ErrorResponse<string>(ex.Message);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _cartService.ClearCartAsync(userId);
                return SuccessResponse<string>("", "Cart cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartTotal()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var total = await _cartService.GetCartTotalAsync(userId);
                return SuccessResponse(new { Total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var count = await _cartService.GetCartItemCountAsync(userId);
                return SuccessResponse(new { ItemCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count");
                return ErrorResponse<string>(ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetInactiveCartItems()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var inactiveItems = await _cartService.GetInactiveCartItemsAsync(userId);
                return SuccessResponse(inactiveItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inactive cart items");
                return ErrorResponse<string>(ex.Message);
            }
        }


        [HttpPost("{targetUserId}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> MergeCarts(int targetUserId)
        {
            try
            {
                var sourceUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await _cartService.MergeCartsAsync(sourceUserId, targetUserId);
                return SuccessResponse<string>("", "Carts merged successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error merging carts to user {targetUserId}");
                return ErrorResponse<string>(ex.Message);
            }
        }
    }
}
