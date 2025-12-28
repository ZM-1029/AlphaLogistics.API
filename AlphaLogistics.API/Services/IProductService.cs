using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IProductService
    {
        // Product CRUD
        Task<ProductDto> CreateProductAsync(int vendorId, CreateProductDto createDto);
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<List<ProductDto>> GetAllProductsAsync(bool? isActive = null);
        Task<List<ProductDto>> GetProductsByVendorAsync(int vendorId, bool? isActive = null);
        Task<List<ProductDto>> GetProductsBySubCategoryAsync(int subCategoryId, bool? isActive = null);
        Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(int productId); // Soft delete
        Task<bool> RestoreProductAsync(int productId);
        Task<bool> DeleteProductPermanentlyAsync(int productId); // Hard delete (admin only)

        // Category & SubCategory CRUD
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);
        Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createDto);
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<List<SubCategoryDto>> GetAllSubCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int categoryId);
        Task<SubCategoryDto> GetSubCategoryByIdAsync(int subCategoryId);
        Task<bool> DeleteCategoryAsync(int categoryId);
        Task<bool> DeleteSubCategoryAsync(int subCategoryId);

        // Vendor-specific operations
        Task<ProductDto> UpdateVendorProductAsync(int vendorId, int productId, UpdateProductDto updateDto);
        Task<bool> DeleteVendorProductAsync(int vendorId, int productId);

        // Search and Filter
        Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<List<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    }
}
