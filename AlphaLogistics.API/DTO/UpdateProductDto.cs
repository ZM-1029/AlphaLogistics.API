using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class UpdateProductDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int? StockQuantity { get; set; }

        public int? SubCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public List<IFormFile>? ProductImages { get; set; }
        public List<string>? ImagesToDelete { get; set; } // URLs of images to delete
    }
}
