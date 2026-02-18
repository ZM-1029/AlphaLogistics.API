
namespace AlphaLogistics.API.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } 
        public string? SKU { get; set; } 
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }


        public int VendorId { get; set; }
        public string VendorName { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public bool IsComboType { get; set; }
        public List<int>? ComboProductIds { get; set; }
        public List<ProductImageDto> ProductImages { get; set; } = new();
    }
}
