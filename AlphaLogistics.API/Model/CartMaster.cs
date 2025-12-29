using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class CartMaster
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UserMaster")] 
        public int UserId { get; set; }  // Added this

        [ForeignKey("ProductMaster")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserMaster? UserMaster { get; set; }  // Added This
        public ProductMaster? ProductMaster { get; set; }
    }
}
