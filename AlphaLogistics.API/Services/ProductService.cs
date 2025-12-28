using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;

namespace AlphaLogistics.API.Services
{
    public class ProductService : IProductService
    {
        private readonly AlphaLogisticsContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            AlphaLogisticsContext context,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }
        private async Task<List<string>> UploadProductImages(List<IFormFile>? images)
        {
            var imageUrls = new List<string>();

            if (images == null || !images.Any())
                return imageUrls;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    imageUrls.Add($"/uploads/products/{uniqueFileName}");
                }
            }

            return imageUrls;
        }

        // Helper method to delete image file
        private void DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", "products", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        // Helper method to convert ProductMaster to ProductDto
        private async Task<ProductDto> ConvertToProductDto(ProductMaster product)
        {
            var imageUrls = await _context.ProductImages
                .Where(pi => pi.ProductId == product.Id)
                .Select(pi => pi.ImageUrl)
                .ToListAsync();

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                LastUpdatedAt = product.LastUpdatedAt,
                VendorId = product.VendorId,
                VendorName = product.VendorMaster?.Name ?? string.Empty,
                SubCategoryId = product.SubCategoryId,
                SubCategoryName = product.SubCategoryMaster?.Name ?? string.Empty,
                CategoryName = product.SubCategoryMaster?.CategoryMaster?.Name ?? string.Empty,
                ImageUrls = imageUrls
            };
        }

        // Create Product
        public async Task<ProductDto> CreateProductAsync(int vendorId, CreateProductDto createDto)
        {
            // Verify vendor exists and is active
            var vendor = await _context.VendorMasters
                .FirstOrDefaultAsync(v => v.Id == vendorId && v.IsActive);

            if (vendor == null)
                throw new Exception("Vendor not found or inactive");

            // Verify subcategory exists
            var subCategory = await _context.SubCategoryMasters
                .FirstOrDefaultAsync(sc => sc.Id == createDto.SubCategoryId);

            if (subCategory == null)
                throw new Exception("SubCategory not found");

            // Upload product images
            var imageUrls = await UploadProductImages(createDto.ProductImages);

            // Create product
            var product = new ProductMaster
            {
                VendorId = vendorId,
                SubCategoryId = createDto.SubCategoryId,
                ProductName = createDto.ProductName,
                Description = createDto.Description,
                Price = createDto.Price,
                StockQuantity = createDto.StockQuantity,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ProductMasters.Add(product);
            await _context.SaveChangesAsync();

            // Save product images
            if (imageUrls.Any())
            {
                var productImages = imageUrls.Select(url => new ProductImages
                {
                    ProductId = product.Id,
                    ImageUrl = url
                }).ToList();

                _context.ProductImages.AddRange(productImages);
                await _context.SaveChangesAsync();
            }

            return await ConvertToProductDto(product);
        }

        // Get Product by ID
        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            return await ConvertToProductDto(product);
        }

        // Get All Products
        public async Task<List<ProductDto>> GetAllProductsAsync(bool? isActive = null)
        {
            var query = _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                productDtos.Add(await ConvertToProductDto(product));
            }

            return productDtos;
        }

        // Get Products by Vendor
        public async Task<List<ProductDto>> GetProductsByVendorAsync(int vendorId, bool? isActive = null)
        {
            var query = _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .Where(p => p.VendorId == vendorId)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                productDtos.Add(await ConvertToProductDto(product));
            }

            return productDtos;
        }

        // Get Products by SubCategory
        public async Task<List<ProductDto>> GetProductsBySubCategoryAsync(int subCategoryId, bool? isActive = null)
        {
            var query = _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .Where(p => p.SubCategoryId == subCategoryId)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                productDtos.Add(await ConvertToProductDto(product));
            }

            return productDtos;
        }

        // Update Product
        public async Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto updateDto)
        {
            var product = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            // Update product properties if provided
            if (!string.IsNullOrEmpty(updateDto.ProductName))
                product.ProductName = updateDto.ProductName;

            if (!string.IsNullOrEmpty(updateDto.Description))
                product.Description = updateDto.Description;

            if (updateDto.Price.HasValue)
                product.Price = updateDto.Price.Value;

            if (updateDto.StockQuantity.HasValue)
                product.StockQuantity = updateDto.StockQuantity.Value;

            if (updateDto.SubCategoryId.HasValue)
            {
                var subCategory = await _context.SubCategoryMasters
                    .FirstOrDefaultAsync(sc => sc.Id == updateDto.SubCategoryId.Value);

                if (subCategory == null)
                    throw new Exception("SubCategory not found");

                product.SubCategoryId = updateDto.SubCategoryId.Value;
            }

            if (updateDto.IsActive.HasValue)
                product.IsActive = updateDto.IsActive.Value;

            product.LastUpdatedAt = DateTime.UtcNow;

            // Handle images
            if (updateDto.ProductImages != null && updateDto.ProductImages.Any())
            {
                var newImageUrls = await UploadProductImages(updateDto.ProductImages);
                var newProductImages = newImageUrls.Select(url => new ProductImages
                {
                    ProductId = product.Id,
                    ImageUrl = url
                }).ToList();

                _context.ProductImages.AddRange(newProductImages);
            }

            // Delete specified images
            if (updateDto.ImagesToDelete != null && updateDto.ImagesToDelete.Any())
            {
                var imagesToDelete = await _context.ProductImages
                    .Where(pi => pi.ProductId == productId && updateDto.ImagesToDelete.Contains(pi.ImageUrl))
                    .ToListAsync();

                _context.ProductImages.RemoveRange(imagesToDelete);

                // Delete physical files
                foreach (var imageUrl in updateDto.ImagesToDelete)
                {
                    DeleteImageFile(imageUrl);
                }
            }

            await _context.SaveChangesAsync();
            return await ConvertToProductDto(product);
        }

        // Soft Delete Product
        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.ProductMasters
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            product.IsActive = false;
            product.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Restore Product
        public async Task<bool> RestoreProductAsync(int productId)
        {
            var product = await _context.ProductMasters
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            product.IsActive = true;
            product.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Hard Delete Product (Admin only)
        public async Task<bool> DeleteProductPermanentlyAsync(int productId)
        {
            var product = await _context.ProductMasters
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            // Delete associated images first
            var productImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            // Delete physical image files
            foreach (var image in productImages)
            {
                DeleteImageFile(image.ImageUrl);
            }

            _context.ProductImages.RemoveRange(productImages);
            _context.ProductMasters.Remove(product);

            await _context.SaveChangesAsync();
            return true;
        }

        // Update Vendor's Product (with vendor verification)
        public async Task<ProductDto> UpdateVendorProductAsync(int vendorId, int productId, UpdateProductDto updateDto)
        {
            var product = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .FirstOrDefaultAsync(p => p.Id == productId && p.VendorId == vendorId);

            if (product == null)
                throw new Exception("Product not found or you don't have permission to update it");

            return await UpdateProductAsync(productId, updateDto);
        }

        // Delete Vendor's Product
        public async Task<bool> DeleteVendorProductAsync(int vendorId, int productId)
        {
            var product = await _context.ProductMasters
                .FirstOrDefaultAsync(p => p.Id == productId && p.VendorId == vendorId);

            if (product == null)
                throw new Exception("Product not found or you don't have permission to delete it");

            return await DeleteProductAsync(productId);
        }

        // Category CRUD operations
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
        {
            var category = new CategoryMaster
            {
                Name = createDto.Name,
                Description = createDto.Description
            };

            _context.CategoryMasters.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createDto)
        {
            var category = await _context.CategoryMasters
                .FirstOrDefaultAsync(c => c.Id == createDto.CategoryId);

            if (category == null)
                throw new Exception("Category not found");

            var subCategory = new SubCategoryMaster
            {
                CategoryId = createDto.CategoryId,
                Name = createDto.Name,
                Description = createDto.Description
            };

            _context.SubCategoryMasters.Add(subCategory);
            await _context.SaveChangesAsync();

            return new SubCategoryDto
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                Description = subCategory.Description,
                CategoryId = subCategory.CategoryId,
                CategoryName = category.Name
            };
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.CategoryMasters
                .Include(c => c.SubCategoryMasters)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                SubCategories = c.SubCategoryMasters?.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    CategoryId = sc.CategoryId,
                    CategoryName = c.Name
                }).ToList()
            }).ToList();
        }

        public async Task<List<SubCategoryDto>> GetAllSubCategoriesAsync()
        {
            var subCategories = await _context.SubCategoryMasters
                .Include(sc => sc.CategoryMaster)
                .OrderBy(sc => sc.Name)
                .ToListAsync();

            return subCategories.Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Description = sc.Description,
                CategoryId = sc.CategoryId,
                CategoryName = sc.CategoryMaster?.Name ?? string.Empty
            }).ToList();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _context.CategoryMasters
                .Include(c => c.SubCategoryMasters)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new Exception("Category not found");

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                SubCategories = category.SubCategoryMasters?.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Description = sc.Description,
                    CategoryId = sc.CategoryId,
                    CategoryName = category.Name
                }).ToList()
            };
        }

        public async Task<SubCategoryDto> GetSubCategoryByIdAsync(int subCategoryId)
        {
            var subCategory = await _context.SubCategoryMasters
                .Include(sc => sc.CategoryMaster)
                .FirstOrDefaultAsync(sc => sc.Id == subCategoryId);

            if (subCategory == null)
                throw new Exception("SubCategory not found");

            return new SubCategoryDto
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                Description = subCategory.Description,
                CategoryId = subCategory.CategoryId,
                CategoryName = subCategory.CategoryMaster?.Name ?? string.Empty
            };
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.CategoryMasters
                .Include(c => c.SubCategoryMasters)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new Exception("Category not found");

            // Check if category has subcategories
            if (category.SubCategoryMasters != null && category.SubCategoryMasters.Any())
                throw new Exception("Cannot delete category with existing subcategories");

            _context.CategoryMasters.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSubCategoryAsync(int subCategoryId)
        {
            var subCategory = await _context.SubCategoryMasters
                .FirstOrDefaultAsync(sc => sc.Id == subCategoryId);

            if (subCategory == null)
                throw new Exception("SubCategory not found");

            // Check if subcategory has products
            var hasProducts = await _context.ProductMasters
                .AnyAsync(p => p.SubCategoryId == subCategoryId);

            if (hasProducts)
                throw new Exception("Cannot delete subcategory with existing products");

            _context.SubCategoryMasters.Remove(subCategory);
            await _context.SaveChangesAsync();

            return true;
        }

        // Search Products
        public async Task<List<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync(true);

            var products = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .Where(p => p.IsActive &&
                           (p.ProductName.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.VendorMaster.Name.Contains(searchTerm)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                productDtos.Add(await ConvertToProductDto(product));
            }

            return productDtos;
        }

        // Get Products by Price Range
        public async Task<List<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var products = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();

            var productDtos = new List<ProductDto>();
            foreach (var product in products)
            {
                productDtos.Add(await ConvertToProductDto(product));
            }

            return productDtos;
        }
    }
}
