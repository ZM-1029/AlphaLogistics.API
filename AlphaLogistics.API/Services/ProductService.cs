using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var currDirectory = Directory.GetCurrentDirectory();
            var uploadsFolder = Path.Combine(currDirectory, "uploads", "products");
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

        // Create Product
        public async Task<ProductDto> CreateProductAsync(int VendorId, CreateProductDto createDto, HttpContext httpContext)
        {
            // Get current user from context
          /*  var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            // Get current user with role
            var currentUser = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser == null)
                throw new UnauthorizedAccessException("User not found");*/

         
            
               var vendor = await _context.VendorMasters
                    .FirstOrDefaultAsync(v => v.Id == VendorId);

                if (vendor == null || !vendor.IsActive || !vendor.IsApproved)
                    throw new Exception("Vendor profile not found, inactive, or not approved");

               
           
           /* if (currentUser.RoleMaster.Name == "Admin" || currentUser.RoleMaster.Name == "SuperAdmin")
            {

                if (createDto.VendorId <= 0)
                    throw new Exception("Vendor ID is required when creating product as admin");
             
            }
            else
            {
                throw new UnauthorizedAccessException("Unauthorized role for product creation");
            }*/

            var subCategory = await _context.SubCategoryMasters
                .Include(sc => sc.CategoryMaster)
                .FirstOrDefaultAsync(sc => sc.Id == createDto.SubCategoryId);

            if (subCategory == null)
                throw new Exception("SubCategory not found");

            var existingProduct = await _context.ProductMasters
                .FirstOrDefaultAsync(p => p.VendorId == VendorId &&
                                          p.ProductName.ToLower() == createDto.ProductName.ToLower());

            if (existingProduct != null)
                throw new Exception($"Product with name '{createDto.ProductName}' already exists for this vendor");

            var imageUrls = await UploadProductImages(createDto.ProductImages);

   
            var product = new ProductMaster
            {
                VendorId = VendorId,
                SubCategoryId = createDto.SubCategoryId,
                ProductName = createDto.ProductName,
                Description = createDto.Description,
                Price = createDto.Price,
                StockQuantity = createDto.StockQuantity,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ProductMasters.Add(product);
            await _context.SaveChangesAsync();

            if (imageUrls.Any())
            {
                var productImages = new List<ProductImages>();

                for (int i = 0; i < imageUrls.Count; i++)
                {
                    productImages.Add(new ProductImages
                    {
                        ProductId = product.Id,
                        ImageUrl = imageUrls[i],
                    });
                }

                _context.ProductImages.AddRange(productImages);
                await _context.SaveChangesAsync();
            }

            return ConvertToProductDto(product);
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _context.ProductMasters
                .Include(p => p.VendorMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                    .Include(p=>p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found");

            return ConvertToProductDto(product);
        }

        // Get All Products
        public async Task<ProductListResponseDto> GetAllProductsAsync(ProductQueryDto dto)
        {
            // Validate pagination parameters
            dto.page = dto.page < 1 ? 1 : dto.page;
            dto.pageSize = dto.pageSize < 1 ? 10 : (dto.pageSize > 100 ? 100 : dto.pageSize);

            // Build query
            var query = _context.ProductMasters
                .Include(p => p.VendorMaster)
                    .ThenInclude(v => v.UserMaster)
                .Include(p => p.SubCategoryMaster)
                    .ThenInclude(sc => sc.CategoryMaster)
                .Include(p => p.ProductImages) // Include product images
                .AsQueryable();

            // Apply filters
            /*if (dto.isActive!=null && dto.isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == dto.isActive.Value);
            }*/

            if (dto.categoryId.HasValue && dto.categoryId>0)
            {
                query = query.Where(p => p.SubCategoryMaster != null &&
                                        p.SubCategoryMaster.CategoryId == dto.categoryId.Value);
            }

            if (dto.subCategoryId.HasValue && dto.subCategoryId>0)
            {
                query = query.Where(p => p.SubCategoryId == dto.subCategoryId.Value);
            }

            if (dto.minPrice.HasValue && dto.minPrice > 0)
            {
                query = query.Where(p => p.Price >= dto.minPrice.Value);
            }

            if (dto.maxPrice.HasValue && dto.maxPrice > 0)
            {
                query = query.Where(p => p.Price <= dto.maxPrice.Value);
            }

            // Global search across multiple fields
            if (!string.IsNullOrWhiteSpace(dto.globalSearchQuery))
            {
                var searchTerm = dto.globalSearchQuery.ToLower().Trim();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(searchTerm) || // Changed from Name to ProductName
                    p.Description.ToLower().Contains(searchTerm) ||
                    (p.VendorMaster != null && p.VendorMaster.VendorName.ToLower().Contains(searchTerm)) ||
                    (p.SubCategoryMaster != null && p.SubCategoryMaster.Name.ToLower().Contains(searchTerm)) ||
                    (p.SubCategoryMaster != null &&
                     p.SubCategoryMaster.CategoryMaster != null &&
                     p.SubCategoryMaster.CategoryMaster.Name.ToLower().Contains(searchTerm)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)dto.pageSize);

            // Apply sorting
            query = ApplySorting(query, dto.sortBy, dto.sortOrder);

            // Apply pagination
            var products = await query
                .Skip((dto.page - 1) * dto.pageSize)
                .Take(dto.pageSize)
                .ToListAsync();

            // Convert to DTOs
            var productDtos = products.Select(p => ConvertToProductDto(p)).ToList();

            // Return paginated response
            return new ProductListResponseDto
            {
                Products = productDtos,
                CurrentPage = dto.page,
                PageSize = dto.pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPrevious = dto.page > 1,
                HasNext = dto.page < totalPages
            };
        }

        // Helper method for sorting
        private IQueryable<ProductMaster> ApplySorting(
            IQueryable<ProductMaster> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "name" => isDescending
                    ? query.OrderByDescending(p => p.ProductName) // Changed from Name to ProductName
                    : query.OrderBy(p => p.ProductName),
                "price" => isDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "createdat" => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "updatedat" => isDescending
                    ? query.OrderByDescending(p => p.LastUpdatedAt)
                    : query.OrderBy(p => p.LastUpdatedAt),
                "stock" => isDescending
                    ? query.OrderByDescending(p => p.StockQuantity)
                    : query.OrderBy(p => p.StockQuantity),
                _ => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt) // Default sort by createdAt
            };
        }

        // Updated ConvertToProductDto method
        private ProductDto ConvertToProductDto(ProductMaster product)
        {
            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName, // Changed from Name to ProductName
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                LastUpdatedAt = product.LastUpdatedAt,

                VendorId = product.VendorId,
                VendorName = product.VendorMaster?.VendorName ?? "Unknown Vendor",

                SubCategoryId = product.SubCategoryId,
                SubCategoryName = product.SubCategoryMaster?.Name ?? "Unknown Subcategory",

                CategoryId = product.SubCategoryMaster?.CategoryId ?? 0,
                CategoryName = product.SubCategoryMaster?.CategoryMaster?.Name ?? "Unknown Category",

                ProductImages = product.ProductImages?.Select(pi => new ProductImageDto
                {
                    Id = pi.Id,
                    ImageUrl = pi.ImageUrl,
                    ProductId = pi.ProductId
                }).ToList() ?? new List<ProductImageDto>()
            };
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
                productDtos.Add(ConvertToProductDto(product));
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
                productDtos.Add(ConvertToProductDto(product));
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
            return  ConvertToProductDto(product);
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
                productDtos.Add(ConvertToProductDto(product));
            }

            return productDtos;
        }

        public async Task<bool> BulkApproveProducts(List<int> producIds)
        {
            var products =await  _context.ProductMasters.Where(p => producIds.Contains(p.Id)).ToListAsync();
            foreach (var product in products)
            {
                product.IsApproved = true;
                product.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
