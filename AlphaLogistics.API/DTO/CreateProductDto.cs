using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class CreateProductDto
    {
        [Required]
        public string ProductName { get; set; }

        [Required]
        public string Description { get; set; }

        /// <summary>Technical product specifications, shown separately from the description.</summary>
        public string? Specification { get; set; }

        /// <summary>List of items included in the box (what comes in the box).</summary>
        public string? WhatsInTheBox { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public bool IsComboType { get; set; }=false;
        public List<int>? ComboProductIds { get; set; }

        /// <summary>When true, the product is marked as a Flash Sale item.</summary>
        public bool IsFlashSale { get; set; } = false;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        public int? VendorId { get; set; }

        /// <summary>Colours as comma-separated string (e.g. Red,Blue,Green).</summary>
        public string? Colours { get; set; }
        /// <summary>Product size (e.g. S, M, L, XL).</summary>
        public string? Size { get; set; }

        public List<IFormFile>? ProductImages { get; set; }
        public string? BrandName { get; set; }
        public string? Warranty { get; set; }
        public IFormFile? ProductVideo { get; set; }
    }

   
}
