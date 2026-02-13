using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Globalization;
using WALMS.API.Common;
namespace AlphaLogistics.API.Services
{
    public class DashBoardService:IDashBoardService
    {
        private readonly AlphaLogisticsContext _context;
        private readonly IConfiguration _config;
        public DashBoardService(AlphaLogisticsContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<object> GraphData(int vendorId)
        {
            try
            {
                var orderStatusSection = _config
                .GetSection("OrderStatus")
                .Get<Dictionary<string, string>>();

                var orderStatusList = orderStatusSection?
                    .Select(x => new
                    {
                        Id = int.Parse(x.Value),
                        Name = x.Key,
                        // Label = FormatLabel(x.Key)
                    })
                    .OrderBy(x => x.Id)
                    .ToList();

                if (vendorId > 0)
                {
                    var vendorProductIdsQuery = _context.ProductMasters
                        .Where(x => x.VendorId == vendorId)
                        .Select(x => x.Id);

                    var orderItemsQuery = _context.OrderItems
                        .Where(x => vendorProductIdsQuery.Contains(x.ProductId));

                    var orderIdsQuery = orderItemsQuery
                        .Select(x => x.OrderId)
                        .Distinct();

                    var validOrdersQuery = _context.OrderMasters
                        .Where(x => orderIdsQuery.Contains(x.Id) &&
                                    x.Status != AppConstants.OrderStatus.Cancelled &&
                                    x.Status != AppConstants.OrderStatus.Refunded);

                    var validOrderItemsQuery = orderItemsQuery
                        .Where(x => validOrdersQuery.Select(v => v.Id).Contains(x.OrderId));

                    var totalRevenue = await validOrderItemsQuery
                        .SumAsync(x => x.UnitPrice * x.Quantity);

                    var totalOrders = await orderIdsQuery.CountAsync();
                    var deliveredOrders = await validOrdersQuery.CountAsync();
                    var cancelOrRefundedOrders = totalOrders - deliveredOrders;

                    var orderResponse = await (
                        from oi in _context.OrderItems
                        join p in _context.ProductMasters on oi.ProductId equals p.Id
                        join sc in _context.SubCategoryMasters on p.SubCategoryId equals sc.Id
                        join c in _context.CategoryMasters on sc.CategoryId equals c.Id
                        join om in _context.OrderMasters on oi.OrderId equals om.Id
                        join pi in _context.ProductImages on p.Id equals pi.ProductId into imageGroup
                        where p.VendorId == vendorId &&
                              om.Status != AppConstants.OrderStatus.Cancelled &&
                              om.Status != AppConstants.OrderStatus.Refunded

                             // let status= (orderStatusList != null && orderStatusList.FirstOrDefault(x => x.Id == om.Status) != null) ? orderStatusList.FirstOrDefault(x => x.Id == om.Status).Name : "N/A"
                        select new
                        {
                            om.Id,
                            om.OrderNumber,
                            om.TotalAmount,
                            om.Status,
                            om.OrderDate,
                            ProductName = p.ProductName,
                            Category = c.Name,
                            ProductImages = imageGroup.Select(img => img.ImageUrl).ToList()
                        }
                    )
                    .OrderBy(x => x.OrderDate)
                    .ToListAsync();

                    var Response = orderResponse.Select(x => new
                    {
                        x.Id,
                        x.OrderNumber,
                        x.TotalAmount,
                        Status = orderStatusList?.FirstOrDefault(s => s.Id == x.Id)?.Name ?? "N/A",
                        x.OrderDate,
                        x.ProductName,
                        x.Category,
                        x.ProductImages
                    }).ToList();

                    return new
                    {
                       /* TotalRevenue = totalRevenue,
                        TotalOrders = totalOrders,
                        DeliveredOrders = deliveredOrders,
                        CancelOrRefundedOrders = cancelOrRefundedOrders,
                        VendorCount = 1,*/
                        OrderList = Response
                    };
                }
                else
                {
                    var validOrdersQuery = _context.OrderMasters
                        .Where(x => x.Status != AppConstants.OrderStatus.Cancelled &&
                                    x.Status != AppConstants.OrderStatus.Refunded);

                    // ✅ Run sequentially instead of parallel
                    var totalRevenue = await validOrdersQuery.SumAsync(x => x.TotalAmount);
                    var deliveredOrders = await validOrdersQuery.CountAsync();
                    var totalOrders = await _context.OrderMasters.CountAsync();
                    var vendorCount = await _context.VendorMasters.CountAsync();
                    var cancelOrRefundedOrders = totalOrders - deliveredOrders;

                    var orderResponse = await (
                        from o in _context.OrderMasters
                        where o.Status != AppConstants.OrderStatus.Cancelled &&
                              o.Status != AppConstants.OrderStatus.Refunded
                        join oi in _context.OrderItems on o.Id equals oi.OrderId
                        join p in _context.ProductMasters on oi.ProductId equals p.Id
                        join sc in _context.SubCategoryMasters on p.SubCategoryId equals sc.Id
                        join c in _context.CategoryMasters on sc.CategoryId equals c.Id
                        join pi in _context.ProductImages on p.Id equals pi.ProductId into imageGroup
                       // let status = (orderStatusList != null && orderStatusList.FirstOrDefault(x => x.Id == o.Status) != null) ? orderStatusList.FirstOrDefault(x => x.Id == o.Status).Name : "N/A"

                        select new
                        {
                            o.Id,
                            o.OrderNumber,
                            o.TotalAmount,
                           // Status = status,
                            o.OrderDate,
                            ProductName = p.ProductName,
                            Category = c.Name,
                            ProductImages = imageGroup.Select(img => img.ImageUrl).ToList()
                        }
                    )
                    .OrderBy(x => x.OrderDate)
                    .ToListAsync();

                    var Response = orderResponse.Select(x => new
                    {
                        x.Id,
                        x.OrderNumber,
                        x.TotalAmount,
                        Status = orderStatusList?.FirstOrDefault(s => s.Id == x.Id)?.Name ?? "N/A",
                        x.OrderDate,
                        x.ProductName,
                        x.Category,
                        x.ProductImages
                    }).ToList();


                    return new
                    {
                        /*TotalRevenue = totalRevenue,
                        TotalOrders = totalOrders,
                        DeliveredOrders = deliveredOrders,
                        CancelOrRefundedOrders = cancelOrRefundedOrders,
                        VendorCount = vendorCount,*/
                        OrderList = Response
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
                var year = DateTime.UtcNow.Year;

                DateTime startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime endDate = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);


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
