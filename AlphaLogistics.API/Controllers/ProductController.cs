using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(IProductService productService, ILogger<ProductController> logger, IConfiguration configuration)
        {
            _productService = productService;
            _logger = logger;
            _configuration = configuration;
        }

        #region Product APIs

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BulkProductApproval(BulkProductApprovalDto data)
        {
            if (data == null || !data.ProductIds.Any()) return ErrorResponse<string>("Invalid input");

            try
            {
               var success= await _productService.BulkApproveProducts(data.ProductIds);
                if (success)
                {
                    return SuccessResponse<string>("Products approved successfully");
                }
                else
                    return ErrorResponse<string>("Failed to approve products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk product approval");
                return ErrorResponse<string>(ex.Message);
            }
        }

        public record BulkProductApprovalDto
        {
            public List<int> ProductIds { get; set; }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct(int VendorId,[FromForm] CreateProductDto createDto)
        {
            try
            {
                var result = await _productService.CreateProductAsync(VendorId, createDto, HttpContext);
                return CreatedResponse(result, "Product created successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized product creation attempt");
                return ErrorResponse<string>("Error while creating the product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ConflictResponse<string>(ex.Message);
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts( ProductQueryDto dto)
        {
            try
            {
                var result = await _productService.GetAllProductsAsync(dto);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveProducts()
        {
            try
            {
                var result = await _productService.GetActiveProduct();
                if (result == null) return NoContentResponse<string>("No active or approved product found");
                return SuccessResponse(result,"Product retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/vendor/{vendorId}
        [HttpGet("{vendorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByVendor(int vendorId)
        {
            try
            {
                var products = await _productService.GetProductsByVendorAsync(vendorId);
                return SuccessResponse(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting products for vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        // GET: api/product/category/{subCategoryId}
        [HttpGet("{subCategoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsBySubCategory(int subCategoryId)
        {
            try
            {
                var products = await _productService.GetProductsBySubCategoryAsync(subCategoryId);
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
        [HttpPut("{vendorId}/{productId}")]
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
        [HttpDelete("{vendorId}/{productId}")]
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
        [HttpPost("{id}")]
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
        [HttpDelete("{id}")]
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


        // GET: api/product/price-range?min={minPrice}&max={maxPrice}
        [HttpGet]
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

        /// <summary>
        /// Gets product sizes from appsettings (e.g. Small, Medium, Large, XLarge, XXLarge).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProductSize()
        {
            var sizes = _configuration.GetSection("ProductSize").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            return SuccessResponse(sizes);
        }

        /// <summary>
        /// Gets product colours from appsettings (e.g. Red, Blue, Green, etc.).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProductColour()
        {
            var colours = _configuration.GetSection("ProductColour").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            return SuccessResponse(colours);
        }

        #endregion

        #region Category/Subcategory APIs
        // Category Management Endpoints
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto createDto)
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
        /*[HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateCategory(CreateCategoryDto createDto)
        {
            try
            {
                var category = await _productService.(createDto);
                return CreatedResponse(category, "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ErrorResponse<string>(ex.Message);
            }
        }*/

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateSubCategory( CreateSubCategoryDto createDto)
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

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateCategory(CreateCategoryDto createDto)
        {
            if (createDto.Id <= 0) return ErrorResponse<string>("Invalid Id provided");

            var isUpdated = await _productService.UpdateCategoryAsync(createDto);
            if (isUpdated != null) return SuccessResponse(createDto,"Category updated successfully");

            return ErrorResponse<string>("Duplicate name or Internal error");
        }
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateSubCategory(CreateSubCategoryDto createDto)
        {
            if (createDto.Id <= 0) return ErrorResponse<string>("Invalid Id provided");
            var isUpdated = await _productService.UpdateSubCategoryAsync(createDto);
            if (isUpdated != null) return SuccessResponse(createDto, "SubCategory updated successfully");
            return ErrorResponse<string>("Duplicate name or Internal error");

        }

        [HttpGet]
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            if (id <= 0) return ErrorResponse<string>("Invalid Id");

            var category = await _productService.GetCategoryByIdAsync(id);
            if (category == null) return NoContentResponse<string>("No category found");
            return SuccessResponse(category,"Category retrieved successfully");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetSubCategoryById(int id)
        {
            var subcategory = await _productService.GetSubCategoryByIdAsync(id);
            if (subcategory == null) return NoContentResponse<string>("No category found");
            return SuccessResponse(subcategory,"Subcategory retrieved successfully");
        }

        [HttpGet]
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSubCategoriesByCategoryId(int categoryId)
        {
            try
            {
                var subCategories = (await _productService.GetAllSubCategoriesAsync()).Where(x=>x.CategoryId==categoryId);
                return SuccessResponse(subCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subcategories");
                return ErrorResponse<string>(ex.Message);
            }
        }

        #endregion



    }
}
