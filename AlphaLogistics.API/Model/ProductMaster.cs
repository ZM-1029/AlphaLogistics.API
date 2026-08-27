using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class ProductMaster
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("VendorMaster")]
        public int VendorId { get; set; }
        [ForeignKey("SubCategoryMaster")]
        public int SubCategoryId { get; set; }
        public string ProductName { get; set; }
        public bool IsComboType { get; set; }
        /// <summary>When true, the product is marked as a Flash Sale item.</summary>
        public bool IsFlashSale { get; set; }
        public string? SKU { get; set; }
        public string Description { get; set; }
        /// <summary>Technical product specifications, shown separately from the description.</summary>
        public string? Specification { get; set; }
        /// <summary>List of items included in the box (what comes in the box).</summary>
        public string? WhatsInTheBox { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int StockQuantity { get; set; }
        /// <summary>Comma-separated list of colours (e.g. Red,Blue,Green).</summary>
        public string? Colours { get; set; }
        /// <summary>Product size (e.g. S, M, L, XL).</summary>
        public string? Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public string? BrandName { get; set; }
        public string? Warranty { get; set; }
        public string? VideoUrl { get; set; }
        public ICollection<CartMaster>? CartMasters { get; set; }
        public VendorMaster? VendorMaster { get; set; }
        public ICollection<ProductImages>? ProductImages { get; set; }
        public SubCategoryMaster? SubCategoryMaster { get; set; }

    }
}
