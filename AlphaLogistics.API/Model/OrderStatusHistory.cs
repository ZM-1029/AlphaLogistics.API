namespace AlphaLogistics.API.Model
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
