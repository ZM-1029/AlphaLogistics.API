using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using WALMS.API.Common;
namespace AlphaLogistics.API.Services
{
    public class DashBoardService:IDashBoardService
    {
        private readonly AlphaLogisticsContext _context;
        public DashBoardService(AlphaLogisticsContext context)
        {
            _context = context;
        }

        public async Task<object> GraphData(int vendorId)
        {
            try
            {
                if (vendorId > 0)
                {
                    // VENDOR-SPECIFIC LOGIC - Optimized
                    var vendorProductIds = await _context.ProductMasters
                        .Where(x => x.VendorId == vendorId)
                        .Select(x => x.Id)
                        .ToListAsync();

                    // Get all order items for vendor's products in one query
                    var orderItemsQuery = _context.OrderItems
                        .Where(x => vendorProductIds.Contains(x.ProductId));

                    var orderItems = await orderItemsQuery.ToListAsync();

                    var orderIds = orderItems.Select(x => x.OrderId).Distinct().ToList();

                    // Get valid (non-cancelled/refunded) orders
                    var validOrders = await _context.OrderMasters
                        .Where(x => orderIds.Contains(x.Id) &&
                                   x.Status != AppConstants.OrderStatus.Cancelled &&
                                   x.Status != AppConstants.OrderStatus.Refunded)
                        .ToListAsync();

                    var validOrderIds = validOrders.Select(x => x.Id).ToList();
                    var validOrderItems = orderItems
                        .Where(x => validOrderIds.Contains(x.OrderId))
                        .ToList();

                    // Calculate metrics
                    var totalRevenue = validOrderItems.Sum(x => x.UnitPrice * x.Quantity);
                    var totalOrders = orderIds.Count;
                    var deliveredOrders = validOrders.Count;
                    var cancelOrRefundedOrders = totalOrders - deliveredOrders;

                    // Optimized order response query - single database query
                    var orderResponse = await (
                        from oi in _context.OrderItems
                        join p in _context.ProductMasters on oi.ProductId equals p.Id
                        join pi in _context.ProductImages on p.Id equals pi.ProductId
                        join sc in _context.SubCategoryMasters on p.SubCategoryId equals sc.Id
                        join c in _context.CategoryMasters on sc.CategoryId equals c.Id
                        join om in _context.OrderMasters on oi.OrderId equals om.Id
                        where validOrderIds.Contains(om.Id) &&
                              vendorProductIds.Contains(p.Id)
                        select new
                        {
                            om.Id,
                            om.OrderNumber,
                            om.TotalAmount,
                            om.Status,
                            om.OrderDate,
                            ProductImage = pi.ImageUrl,
                            ProductName = p.ProductName,
                            Category = c.Name
                        }
                    ).Distinct()
                     .OrderBy(x => x.OrderDate)
                     .ToListAsync();

                    return new
                    {
                        TotalRevenue = totalRevenue,
                        TotalOrders = totalOrders,
                        DeliveredOrders = deliveredOrders,
                        CancelOrRefundedOrders = cancelOrRefundedOrders,
                        VendorCount = 1, // Since it's vendor-specific
                        OrderList = orderResponse
                    };
                }
                else
                {
                    // ALL VENDORS LOGIC - Optimized

                    // Get valid orders in one query
                    var validOrders = await _context.OrderMasters
                        .Where(x => x.Status != AppConstants.OrderStatus.Cancelled &&
                                   x.Status != AppConstants.OrderStatus.Refunded)
                        .ToListAsync();

                    // Calculate metrics in parallel
                    var totalRevenueTask = Task.FromResult(validOrders.Sum(x => x.TotalAmount));
                    var deliveredOrdersTask = Task.FromResult(validOrders.Count);
                    var totalOrdersTask = _context.OrderMasters.CountAsync();
                    var vendorCountTask = _context.VendorMasters.CountAsync();

                    await Task.WhenAll(totalRevenueTask, deliveredOrdersTask, totalOrdersTask, vendorCountTask);

                    var totalRevenue = await totalRevenueTask;
                    var deliveredOrders = await deliveredOrdersTask;
                    var totalOrders = await totalOrdersTask;
                    var vendorCount = await vendorCountTask;
                    var cancelOrRefundedOrders = totalOrders - deliveredOrders;

                    // Optimized order response query - single database query
                    var validOrderIds = validOrders.Select(x => x.Id).ToList();

                    var orderResponse = await (
                        from o in _context.OrderMasters
                        join oi in _context.OrderItems on o.Id equals oi.OrderId
                        join p in _context.ProductMasters on oi.ProductId equals p.Id
                        join pi in _context.ProductImages on p.Id equals pi.ProductId
                        join sc in _context.SubCategoryMasters on p.SubCategoryId equals sc.Id
                        join c in _context.CategoryMasters on sc.CategoryId equals c.Id
                        where validOrderIds.Contains(o.Id)
                        select new
                        {
                            o.Id,
                            o.OrderNumber,
                            o.TotalAmount,
                            o.Status,
                            o.OrderDate,
                            ProductImage = pi.ImageUrl,
                            ProductName = p.ProductName,
                            Category = c.Name
                        }
                    ).Distinct()
                     .OrderBy(x => x.OrderDate)
                     .ToListAsync();

                    return new
                    {
                        TotalRevenue = totalRevenue,
                        TotalOrders = totalOrders,
                        DeliveredOrders = deliveredOrders,
                        CancelOrRefundedOrders = cancelOrRefundedOrders,
                        VendorCount = vendorCount,
                        OrderList = orderResponse
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DashBoardService/GraphData :{ex.Message}");
                return null;
            }
        }

        public async Task<object> GetMonthlySalesReport(int vendorId)
        {
            try
            {
                var year=DateTime.UtcNow.Year;
                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, 12, 31).AddDays(1).AddTicks(-1);

                if (vendorId > 0)
                {
                    // Vendor-specific report
                    var monthlyData = await (
                        from oi in _context.OrderItems
                        join om in _context.OrderMasters on oi.OrderId equals om.Id
                        join pm in _context.ProductMasters on oi.ProductId equals pm.Id
                        where pm.VendorId == vendorId &&
                              om.OrderDate >= startDate &&
                              om.OrderDate <= endDate &&
                              om.Status != AppConstants.OrderStatus.Cancelled &&
                              om.Status != AppConstants.OrderStatus.Refunded
                        group new { oi, om } by new { om.OrderDate.Month } into g
                        orderby g.Key.Month
                        select new
                        {
                            Month = g.Key.Month,
                            Revenue = g.Sum(x => x.oi.UnitPrice * x.oi.Quantity),
                            Orders = g.Select(x => x.om.Id).Distinct().Count(),
                            Items = g.Count()
                        }
                    ).ToListAsync();

                    // Add month names
                    var formattedData = monthlyData.Select(x => new
                    {
                        x.Month,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Month),
                        x.Revenue,
                        x.Orders,
                        x.Items
                    }).ToList();

                    return new
                    {
                        Year = year,
                        MonthlyData = formattedData,
                        Summary = new
                        {
                            TotalRevenue = formattedData.Sum(x => x.Revenue),
                            TotalOrders = formattedData.Sum(x => x.Orders),
                            TotalItems = formattedData.Sum(x => x.Items),
                            AverageMonthlyRevenue = formattedData.Average(x => x.Revenue)
                        }
                    };
                }
                else
                {
                    // All vendors report
                    var monthlyData = await _context.OrderMasters
                        .Where(x => x.OrderDate >= startDate &&
                                   x.OrderDate <= endDate &&
                                   x.Status != AppConstants.OrderStatus.Cancelled &&
                                   x.Status != AppConstants.OrderStatus.Refunded)
                        .GroupBy(x => x.OrderDate.Month)
                        .Select(g => new
                        {
                            Month = g.Key,
                            Revenue = g.Sum(x => x.TotalAmount),
                            Orders = g.Count(),
                            Customers = g.Select(x => x.UserId).Distinct().Count(),
                            AvgOrderValue = g.Average(x => x.TotalAmount)
                        })
                        .OrderBy(x => x.Month)
                        .ToListAsync();

                    // Add month names
                    var formattedData = monthlyData.Select(x => new
                    {
                        x.Month,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Month),
                        x.Revenue,
                        x.Orders,
                        x.Customers,
                        x.AvgOrderValue
                    }).ToList();

                    return new
                    {
                        Year = year,
                        MonthlyData = formattedData,
                        Summary = new
                        {
                            TotalRevenue = formattedData.Sum(x => x.Revenue),
                            TotalOrders = formattedData.Sum(x => x.Orders),
                            TotalCustomers = formattedData.Sum(x => x.Customers),
                            AverageMonthlyRevenue = formattedData.Average(x => x.Revenue),
                            OverallAvgOrderValue = formattedData.Average(x => x.AvgOrderValue)
                        },
                        VendorCount = await _context.VendorMasters.CountAsync()
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DashBoardService/GetMonthlySalesReport :{ex.Message}");
                return null;
            }
        }

      /*  private object CalculateSummary(List<dynamic> monthlyData)
        {
            if (monthlyData == null || !monthlyData.Any())
                return new { TotalRevenue = 0M, TotalOrders = 0, AverageMonthlyRevenue = 0M };

            return new
            {
                TotalRevenue = monthlyData.Sum(x => (decimal)x.Revenue),
                TotalOrders = monthlyData.Sum(x => (int)x.Orders),
                AverageMonthlyRevenue = monthlyData.Average(x => (decimal)x.Revenue)
            };
        }*/
       
    }
}
