using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using WALMS.API.Common;

namespace AlphaLogistics.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUserContext _userContext;
        private readonly AlphaLogisticsContext _context;

        public OrderService(IUserContext userContext,AlphaLogisticsContext context)
        {
            _userContext = userContext;
            _context = context;
        }
        public Task<bool> CancelOrder(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeStatus(int orderId, int statusId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditOrder(OrderDTO orders)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDTO> GetOrderById(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetOrderList()
        {
            throw new NotImplementedException();
        }
        public Task<bool> PlaceOrder(OrderDTO orders)
        {
            try
            {
                var orderNumber = $"AL-{DateTime.Now.Ticks}";
                var orderTotal = orders.OrderItems.Sum(item => item.UnitPrice * item.Quantity);
                var userId = _userContext.UserId;
                var order = new OrderMaster
                {
                    OrderNumber = orderNumber,
                    UserId = (int)userId,
                    TotalAmount = orderTotal,
                    Status = AppConstants.OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                };

                var isAdded = _context.OrderMasters.Add(order);
                if (isAdded != null)
                {
                    foreach (var item in orders.OrderItems)
                    {
                        var orderItem = new OrderItems
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };

                        _context.OrderItems.Add(orderItem);
                    }

                    _context.SaveChanges();

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult(false);
            }
        }
    }
}
