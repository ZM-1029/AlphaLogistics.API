namespace WALMS.API.Common
{
    public class OrderStatusChange
    {
        public List<int> orderId { get; set; }
        public int statusId { get; set; }
        public int RoleId { get; set; }
    }
}
