using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // POST: api/product/vendor/{vendorId}
        [HttpPost("vendor/{vendorId}")]
        public async Task<IActionResult> CreateProduct(int vendorId, [FromForm] CreateProductDto createDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(vendorId, createDto);
                return CreatedResponse(product, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating product for vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return SuccessResponse(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product {id}");
                return NoContentResponse<string>(ex.Message);
            }
        }

        // GET: api/product
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts([FromQuery] bool? isActive = null)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync(isActive);
                return SuccessResponse(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/vendor/{vendorId}
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetProductsByVendor(int vendorId, [FromQuery] bool? isActive = null)
        {
            try
            {
                var products = await _productService.GetProductsByVendorAsync(vendorId, isActive);
                return SuccessResponse(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting products for vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/category/{subCategoryId}
        [HttpGet("category/{subCategoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsBySubCategory(int subCategoryId, [FromQuery] bool? isActive = null)
        {
            try
            {
                var products = await _productService.GetProductsBySubCategoryAsync(subCategoryId, isActive);
                return SuccessResponse(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting products for subcategory {subCategoryId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // PUT: api/product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto updateDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateDto);
                return SuccessResponse(product, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // PUT: api/product/vendor/{vendorId}/{productId}
        [HttpPut("vendor/{vendorId}/{productId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> UpdateVendorProduct(int vendorId, int productId, [FromForm] UpdateProductDto updateDto)
        {
            try
            {
                var product = await _productService.UpdateVendorProductAsync(vendorId, productId, updateDto);
                return SuccessResponse(product, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {productId} for vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // DELETE: api/product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return SuccessResponse<string>("", "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // DELETE: api/product/vendor/{vendorId}/{productId}
        [HttpDelete("vendor/{vendorId}/{productId}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> DeleteVendorProduct(int vendorId, int productId)
        {
            try
            {
                await _productService.DeleteVendorProductAsync(vendorId, productId);
                return SuccessResponse<string>("", "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {productId} for vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // POST: api/product/{id}/restore
        [HttpPost("{id}/restore")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            try
            {
                await _productService.RestoreProductAsync(id);
                return SuccessResponse<string>("", "Product restored successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring product {id}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // DELETE: api/product/{id}/permanent
        [HttpDelete("{id}/permanent")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteProductPermanently(int id)
        {
            try
            {
                await _productService.DeleteProductPermanentlyAsync(id);
                return SuccessResponse<string>("", "Product permanently deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error permanently deleting product {id}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/search?term={searchTerm}
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProducts([FromQuery] string term)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(term);
                return SuccessResponse(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching products with term {term}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/price-range?min={minPrice}&max={maxPrice}
        [HttpGet("price-range")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByPriceRange([FromQuery] decimal min, [FromQuery] decimal max)
        {
            try
            {
                var products = await _productService.GetProductsByPriceRangeAsync(min, max);
                return SuccessResponse(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting products in price range {min}-{max}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // Category Management Endpoints
        [HttpPost("category")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                var category = await _productService.CreateCategoryAsync(createDto);
                return CreatedResponse(category, "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost("subcategory")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryDto createDto)
        {
            try
            {
                var subCategory = await _productService.CreateSubCategoryAsync(createDto);
                return CreatedResponse(subCategory, "SubCategory created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subcategory");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                return SuccessResponse(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpGet("subcategories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSubCategories()
        {
            try
            {
                var subCategories = await _productService.GetAllSubCategoriesAsync();
                return SuccessResponse(subCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subcategories");
                return ErrorResponse<string>(ex.Message);
            }
        }
    }
}
