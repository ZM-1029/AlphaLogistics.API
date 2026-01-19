using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.Model
{
    public class OrderMaster
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("UserMaster")]
        public int UserId { get; set; }
        public string OrderNumber { get; set; }            
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public bool IsPlacedByAdmin { get; set; } = false;
        
        public UserMaster? UserMaster { get; set; }
        public ICollection<OrderItems>? OrderItems { get; set; }

    }
}
