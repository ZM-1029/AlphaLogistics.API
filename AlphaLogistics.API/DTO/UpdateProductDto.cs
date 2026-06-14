using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class UpdateProductDto
    {
        public string? ProductName { get; set; }
        public bool IsComboType { get; set; } = false;
        public List<int>? ComboProductIds { get; set; }
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int? StockQuantity { get; set; }
        public decimal? CostPrice { get; set; }

        public int? SubCategoryId { get; set; }
        public bool? IsActive { get; set; }
        /// <summary>Colours as comma-separated string (e.g. Red,Blue,Green).</summary>
        public string? Colours { get; set; }
        /// <summary>Product size (e.g. S, M, L, XL).</summary>
        public string? Size { get; set; }
        public List<IFormFile>? ProductImages { get; set; }
        public List<int>? ImagesToDelete { get; set; }
        public string? BrandName { get; set; }
        public string? Warranty { get; set; }
        public IFormFile? ProductVideo { get; set; }
        public bool DeleteVideo { get; set; } = false;
    }
}
