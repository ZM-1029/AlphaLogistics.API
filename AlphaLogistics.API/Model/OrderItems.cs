using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class OrderItems
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("OrderMaster")]
        public int OrderId { get; set; }
        [ForeignKey("ProductMaster")]
        public int ProductId { get; set; }
        public string? ProductSize { get; set; }
        public string? ProductColour { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public OrderMaster? OrderMaster { get; set; }
        public ProductMaster? ProductMaster { get; set; }
    }
}
