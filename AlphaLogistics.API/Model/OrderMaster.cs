namespace AlphaLogistics.API.Model
{
    public class OrderMaster
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OrderNumber { get; set; }            
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

    }
}
