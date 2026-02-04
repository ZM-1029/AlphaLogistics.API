using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Transactions;
using WALMS.API.Common;

namespace AlphaLogistics.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUserContext _userContext;
        private readonly AlphaLogisticsContext _context;
        private readonly IConfiguration _configuration;

        public OrderService(IUserContext userContext,AlphaLogisticsContext context,IConfiguration config)
        {
            _userContext = userContext;
            _context = context;
            _configuration = config;
        }     

        public Task<bool> ChangeStatus(int orderId, int statusId)
        {
           var existingOrder = _context.OrderMasters.FirstOrDefault(x => x.Id == orderId);
            if (existingOrder != null)
            {
                existingOrder.Status = statusId;
               // _context.SaveChanges();

                var existingStatus = _context.OrderStatusHistory
                    .FirstOrDefault(osh => osh.OrderId == orderId && osh.IsActive == true);

                if (existingStatus != null)
                { 
                    existingStatus.IsActive = false;
                    //_context.SaveChanges();
                }

                // make entry in OrderStatus History
                var orderstatus= new OrderStatusHistory
                {
                    OrderId = orderId,
                    StatusId = statusId,
                    CreatedOn = DateTime.UtcNow,
                };

                _context.OrderStatusHistory.Add(orderstatus);
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

        public async Task<List<dynamic>?> GetOrderList(int? userId)
        {
            try 
            { 
                var orders = await _context.OrderMasters.ToListAsync();

                if (userId != null)
                { 
                    orders = orders.Where(x => x.UserId == userId).ToList();
                }

                if (!orders.Any()) return null;

                var orderStatusSection = _configuration
               .GetSection("OrderStatus")
               .Get<Dictionary<string, string>>();

                var statusList = orderStatusSection?
                    .Select(x => new
                    {
                        Id = int.Parse(x.Value),
                        Name = x.Key,
                        // Label = FormatLabel(x.Key)
                    })
                    .OrderBy(x => x.Id)
                    .ToList();

                var response = orders.Select(x => (dynamic)new { 
                x.Id,
                x.OrderNumber,
                x.OrderDate,
                Status= statusList?.FirstOrDefault(s=>s.Id==x.Status)?.Name,
                x.IsPlacedByAdmin,
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
        public async Task<int> PlaceOrder(OrderDTO order)
        {
            try
            {
                var orderNumber = $"AL-{DateTime.Now.Ticks}";
                var orderTotal = order.OrderItems.Sum(item => (item.UnitPrice ?? 0) * item.Quantity);
                var userId = _userContext.UserId;

                if (userId <= 0)
                {
                    Log.Error("Order Service/PlaceOrder: UserId is zero");
                    return 0;
                }

                var orderData = new OrderMaster
                {
                    OrderNumber = orderNumber,
                    UserId = (int)userId,
                    DeliveryCharge = order.DeliveryCharge ?? 0,
                    TotalAmount = orderTotal + (order.DeliveryCharge ?? 0),
                    Status = AppConstants.OrderStatus.Pending,
                    OrderDate = DateTime.UtcNow,
                    IsPlacedByAdmin = order.IsPlacedByAdmin,
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    _context.OrderMasters.Add(orderData);
                    await _context.SaveChangesAsync();

                    foreach (var item in order.OrderItems)
                    {
                        var orderItem = new OrderItems
                        {
                            OrderId = orderData.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice ?? 0
                        };

                        _context.OrderItems.Add(orderItem);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return orderData.Id;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Order Service/PlaceOrder: {ex.Message}");
                return 0;
            }
        }

        public async Task<bool> IsExistingSKU(string sku)
        {
            var isExisting = _context.ProductMasters.FirstOrDefault(x => x.SKU!=null &&  x.SKU.Trim().ToLower() == sku.Trim().ToLower());
             return  isExisting!=null?  true: false;
        }

        public async Task<bool> CancelOrder( int orderId)
        {
            var existingOrder=_context.OrderMasters.FirstOrDefault(x=>x.Id==orderId);
            if (existingOrder != null) 
            {
                if (existingOrder.Status == AppConstants.OrderStatus.Pending ||
                    existingOrder.Status == AppConstants.OrderStatus.Confirmed ||
                    existingOrder.Status == AppConstants.OrderStatus.Packed ||
                    existingOrder.Status == AppConstants.OrderStatus.Processing)
                {
                    existingOrder.Status = AppConstants.OrderStatus.Cancelled;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            } 
            else 
            {
                return false;
            }
        }

        public async Task<dynamic> OrderTrackingData(int orderId)
        { 
            var orderStatusHistory = _context.OrderStatusHistory.Where(x => x.OrderId == orderId).ToList();

            var orderStatusSection = _configuration
              .GetSection("OrderStatus")
              .Get<Dictionary<string, string>>();

            var statusList = orderStatusSection?
                .Select(x => new
                {
                    Id = int.Parse(x.Value),
                    Name = x.Key,
                    // Label = FormatLabel(x.Key)
                })
                .OrderBy(x => x.Id)
                .ToList();

            var trackingData = orderStatusHistory.Select(x => new {
                Status= statusList?.FirstOrDefault(s=>s.Id==x.StatusId)?.Name,
                x.CreatedOn,
                x.IsActive,
            }).OrderBy(x=>x.CreatedOn).ToList();

            return trackingData;
        }
    }
}
