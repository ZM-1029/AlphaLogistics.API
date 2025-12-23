using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class CartMaster
    {
        public int Id { get; set; }
        [ForeignKey("ProductMaster")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProductMaster? ProductMaster { get; set; }
    }
}
