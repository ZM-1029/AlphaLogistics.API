using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class CreateProductDto
    {
        [Required]
        public string ProductName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        public List<IFormFile>? ProductImages { get; set; }
    }
}
