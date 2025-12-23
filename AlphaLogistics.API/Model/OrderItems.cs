namespace AlphaLogistics.API.Model
{
    public class OrderItems
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public OrderMaster? OrderMaster { get; set; }
    }
}
