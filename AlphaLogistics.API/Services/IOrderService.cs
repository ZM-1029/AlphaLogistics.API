using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IOrderService
    {
        public Task<bool> PlaceOrder(OrderDTO orders);
        public Task<bool> CancelOrder(int orderId);
        public Task<bool> EditOrder(OrderDTO orders);
        public Task<bool> ChangeStatus(int orderId, int statusId);
        public Task<bool> GetOrderList();
        public Task<OrderDTO> GetOrderById(int orderId);

    }
}
