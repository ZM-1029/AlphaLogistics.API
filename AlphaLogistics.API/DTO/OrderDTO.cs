using AlphaLogistics.API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaLogistics.API.DTO
{
    public class OrderDTO
    {
       // public int Id { get; set; }
        public bool IsPlacedByAdmin { get; set; }=false;
        public int? UserId { get; set; }
        public int PradeshId { get; set; }
        public string? OrderNumber { get; set; }
        public decimal? DeliveryCharge { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Branch { get; set; }
        public string? CourierPartner { get; set; }
        public string? DeliveryType { get; set; }
        public string? DeliveryInstuctions { get; set; }
        public string? Remark { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? PaymentTypeId { get; set; }
        public string? PaymentUrl { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }

    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductSize { get; set; }
        public string? ProductColour { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        
    }
}
