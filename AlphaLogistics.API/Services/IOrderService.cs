using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IOrderService
    {
        public  Task<bool> UpdateOrder(int orderId, OrderDTO order);
        public  Task<int> OrderCount(OrderListDTO data);
        public  Task<bool> AssignPradesh(int orderId, int pradeshId);
        public Task<int> PlaceOrder(OrderDTO orders);
        public  Task<List<dynamic>?> GetOrderList(OrderListDTO data);
        public  Task<dynamic?> GetOrderById(int orderId);
        //public Task<bool> EditOrder(OrderDTO orders);
        public Task<bool> ChangeStatus(int orderId, int statusId);
        public  Task<bool> IsExistingSKU(string sku);
        public Task<bool> CancelOrder(int orderId);
        public  Task<dynamic> OrderTrackingData(int orderId);
        public  Task<byte[]> ExportOrdersToExcelAsync(int? userId, DateTime? from, DateTime? to, int? statusId);
        public Task<DeliveryLabelDto?> GetDeliveryLabelData(int orderId);
    }
}
