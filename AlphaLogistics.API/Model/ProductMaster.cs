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
        public string? SKU { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int StockQuantity { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public ICollection<CartMaster>? CartMasters { get; set; }
        public VendorMaster? VendorMaster { get; set; }
        public ICollection<ProductImages>? ProductImages { get; set; }
        public SubCategoryMaster? SubCategoryMaster { get; set; }

    }
}
