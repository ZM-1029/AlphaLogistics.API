using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Transactions;
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

        public Task<bool> ChangeStatus(int orderId, int statusId)
        {
           var existingOrder = _context.OrderMasters.FirstOrDefault(x => x.Id == orderId);
            if (existingOrder != null)
            {
                existingOrder.Status = statusId;
                _context.SaveChanges();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

       /* public Task<bool> EditOrder(OrderDTO orders)
        {
            throw new NotImplementedException();
        }*/

        public async Task<dynamic?> GetOrderById(int orderId)
        {
            try
            {
                var order = _context.OrderMasters?.Include(o => o.OrderItems)!.ThenInclude(x => x.ProductMaster).ThenInclude(x => x.ProductImages).FirstOrDefault(x => x.Id == orderId);

                var category = await _context.CategoryMasters.ToListAsync();
                var subCategory = await _context.SubCategoryMasters.ToListAsync();

                if (order == null) return null;

                var response = (dynamic)new
                {
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    OrderItems = order?.OrderItems?.Select(item => new
                    {
                        Id = item.Id,

                        Product = new
                        {
                            item.ProductMaster?.Id,
                            item.ProductMaster?.ProductName,
                            item.ProductMaster?.ProductImages?.FirstOrDefault()?.ImageUrl,
                            item.ProductMaster?.SubCategoryId,
                            SubcategoryName = subCategory.FirstOrDefault(x => x.Id == item.ProductMaster?.SubCategoryId)?.Name,
                            CategoryId = subCategory.FirstOrDefault(x => x.Id == item.ProductMaster?.SubCategoryId)?.CategoryId,
                            CategoryName = category.FirstOrDefault(x => x.Id == subCategory.FirstOrDefault(s => s.Id == item.ProductMaster?.SubCategoryId)?.CategoryId)?.Name,
                        },

                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                return response;
            }
            catch(Exception ex)
            {
                Log.Error($"OrderService/GetOrderById: {ex.Message}");
                return null;
            }
        }

        public async Task<List<dynamic>?> GetOrderList(int userId)
        {
            try 
            { 
                var orders = await _context.OrderMasters.Where(x=>x.UserId==userId).ToListAsync();
                if (!orders.Any()) return null;

                var response = orders.Select(x => (dynamic)new { 
                x.Id,
                x.OrderNumber,
                x.OrderDate,
                x.Status,
                x.TotalAmount,
                }).OrderBy(x=>x.OrderDate).ToList();

                return response;
            }
            catch (Exception ex)
            {
                Log.Error($"OrderService/GetOrderList: {ex.Message}");
                return null;
            }
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
                    TotalAmount = orderTotal??0,
                    Status = AppConstants.OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                };
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
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
                                UnitPrice = item.UnitPrice??0
                            };

                            _context.OrderItems.Add(orderItem);
                        }

                        _context.SaveChanges();

                        return Task.FromResult(true);
                    }
                    transactionScope.Complete();
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
