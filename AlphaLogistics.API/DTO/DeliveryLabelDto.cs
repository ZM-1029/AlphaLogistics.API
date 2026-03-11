namespace AlphaLogistics.API.DTO
{
    /// <summary>Data for printing a delivery label on an order.</summary>
    public class DeliveryLabelDto
    {
        public string OrderNumber { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Pradesh { get; set; } = "";
        public string DeliveryInstruction { get; set; } = "";
    }
}
