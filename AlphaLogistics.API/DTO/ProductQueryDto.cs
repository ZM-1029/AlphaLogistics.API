using Microsoft.AspNetCore.Mvc;

namespace AlphaLogistics.API.DTO
{
    public class ProductQueryDto
    {
           public int page { get; set; } = 1;

           public int pageSize { get; set; } = 10;

           public bool? isActive { get; set; } = null;

           public int? categoryId { get; set; } = null;

           public int? subCategoryId { get; set; } = null;

           public string? search { get; set; } = null;

           public decimal? minPrice { get; set; } = null;

           public decimal? maxPrice { get; set; } = null;

           public string? sortBy { get; set; } = "createdAt";

           public string? sortOrder { get; set; } = "desc";
           public string? globalSearchQuery { get; set; } = null;
    }
}
