using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IOrderService
    {
        public Task<bool> PlaceOrder(OrderDTO orders);
        public  Task<List<dynamic>?> GetOrderList(int userId);
        public  Task<dynamic?> GetOrderById(int orderId);
        //public Task<bool> EditOrder(OrderDTO orders);
        public Task<bool> ChangeStatus(int orderId, int statusId);

    }
}
