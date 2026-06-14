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

            var totalOrders = await validOrdersQuery.CountAsync();

            // Per-status count for ALL orders belonging to this vendor (including cancelled/refunded)
            var rawStatusCounts = await _context.OrderMasters
                .Where(x => orderIdsQuery.Contains(x.Id))
                .GroupBy(x => x.Status)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusBreakdown = rawStatusCounts
                .Select(x => new
                {
                    StatusId   = x.StatusId,
                    StatusName = orderStatusList?.FirstOrDefault(s => s.Id == x.StatusId)?.Name ?? "Unknown",
                    Count      = x.Count
                })
                .OrderBy(x => x.StatusId)
                .ToList();

            // Step 1: Get IDs of orders that contain at least one item from this vendor's products
            var vendorOrderIds = await validOrdersQuery.Select(x => x.Id).ToListAsync();

            // Step 2: Load ALL items for those orders — LEFT JOIN on OrderItems too so orders with cascade-deleted items still appear
            var orderResponse = await (
                from om in _context.OrderMasters
                join oi in _context.OrderItems on om.Id equals oi.OrderId into oiGroup
                from oi in oiGroup.DefaultIfEmpty()
                join p in _context.ProductMasters on (oi != null ? oi.ProductId : 0) equals p.Id into pGroup
                from p in pGroup.DefaultIfEmpty()
                join sc in _context.SubCategoryMasters on (p != null ? p.SubCategoryId : 0) equals sc.Id into scGroup
                from sc in scGroup.DefaultIfEmpty()
                join c in _context.CategoryMasters on (sc != null ? sc.CategoryId : 0) equals c.Id into cGroup
                from c in cGroup.DefaultIfEmpty()
                where vendorOrderIds.Contains(om.Id)
                select new
                {
                    om.Id,
                    om.OrderNumber,
                    om.TotalAmount,
                    om.Status,
                    om.OrderDate,
                    HasItem    = oi != null,
                    ProductId  = p  != null ? p.Id          : 0,
                    ProductName = p != null ? p.ProductName  : (oi != null ? "Deleted Product" : null),
                    Category   = c  != null ? c.Name         : "N/A",
                    UnitPrice  = oi != null ? oi.UnitPrice   : 0,
                    Quantity   = oi != null ? oi.Quantity    : 0,
                }
            )
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync();

            // Fetch images separately to avoid row duplication
            var productIds = orderResponse.Where(x => x.ProductId > 0).Select(x => x.ProductId).Distinct().ToList();
            var productImages = await _context.ProductImages
                .Where(x => productIds.Contains(x.ProductId))
                .GroupBy(x => x.ProductId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(img => img.ImageUrl).ToList());

            // One record per order with items nested inside
            var Response = orderResponse
                .GroupBy(x => new { x.Id, x.OrderNumber, x.TotalAmount, x.Status, x.OrderDate })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.OrderNumber,
                    g.Key.TotalAmount,
                    Status = orderStatusList?.FirstOrDefault(s => s.Id == g.Key.Status)?.Name ?? "N/A",
                    g.Key.OrderDate,
                    Items = g.Where(item => item.HasItem).Select(item => new
                    {
                        item.ProductName,
                        item.Category,
                        item.UnitPrice,
                        item.Quantity,
                        SubTotal = item.UnitPrice * item.Quantity,
                        ProductImages = productImages.GetValueOrDefault(item.ProductId) ?? []
                    }).ToList()
                })
                .ToList();

            return new
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                StatusBreakdown = statusBreakdown,
                VendorCount = 1,
                OrderList = Response
            };
        }
        else
        {
            var validOrdersQuery = _context.OrderMasters
                .Where(x => x.Status != AppConstants.OrderStatus.Cancelled &&
                            x.Status != AppConstants.OrderStatus.Refunded);

            var totalRevenue = await validOrdersQuery.SumAsync(x => x.TotalAmount);
            var totalOrders  = await validOrdersQuery.CountAsync();
            var vendorCount  = await _context.VendorMasters.CountAsync();

            // Per-status count across ALL orders (including cancelled/refunded)
            var rawStatusCounts = await _context.OrderMasters
                .GroupBy(x => x.Status)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusBreakdown = rawStatusCounts
                .Select(x => new
                {
                    StatusId   = x.StatusId,
                    StatusName = orderStatusList?.FirstOrDefault(s => s.Id == x.StatusId)?.Name ?? "Unknown",
                    Count      = x.Count
                })
                .OrderBy(x => x.StatusId)
                .ToList();

            // Step 1: get IDs of all valid orders so the LEFT JOIN below is not filtered by status inline
            var allValidOrderIds = await validOrdersQuery.Select(x => x.Id).ToListAsync();

            // Step 2: load ALL items — LEFT JOIN on OrderItems too so orders with cascade-deleted items still appear
            var orderResponse = await (
                from om in _context.OrderMasters
                join oi in _context.OrderItems on om.Id equals oi.OrderId into oiGroup
                from oi in oiGroup.DefaultIfEmpty()
                join p  in _context.ProductMasters     on (oi != null ? oi.ProductId    : 0) equals p.Id   into pGroup
                from p  in pGroup.DefaultIfEmpty()
                join sc in _context.SubCategoryMasters on (p  != null ? p.SubCategoryId : 0) equals sc.Id  into scGroup
                from sc in scGroup.DefaultIfEmpty()
                join c  in _context.CategoryMasters    on (sc != null ? sc.CategoryId   : 0) equals c.Id   into cGroup
                from c  in cGroup.DefaultIfEmpty()
                where allValidOrderIds.Contains(om.Id)
                select new
                {
                    om.Id,
                    om.OrderNumber,
                    om.TotalAmount,
                    om.Status,
                    om.OrderDate,
                    HasItem     = oi != null,
                    ProductId   = p  != null ? p.Id          : 0,
                    ProductName = p  != null ? p.ProductName  : (oi != null ? "Deleted Product" : null),
                    Category    = c  != null ? c.Name         : "N/A",
                    UnitPrice   = oi != null ? oi.UnitPrice   : 0,
                    Quantity    = oi != null ? oi.Quantity    : 0,
                }
            )
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync();

            // Fetch images separately to avoid row duplication
            var productIds = orderResponse.Where(x => x.ProductId > 0).Select(x => x.ProductId).Distinct().ToList();
            var productImages = await _context.ProductImages
                .Where(x => productIds.Contains(x.ProductId))
                .GroupBy(x => x.ProductId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(img => img.ImageUrl).ToList());

            // One record per order with items nested inside
            var Response = orderResponse
                .GroupBy(x => new { x.Id, x.OrderNumber, x.TotalAmount, x.Status, x.OrderDate })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.OrderNumber,
                    g.Key.TotalAmount,
                    Status = orderStatusList?.FirstOrDefault(s => s.Id == g.Key.Status)?.Name ?? "N/A",
                    g.Key.OrderDate,
                    Items = g.Where(item => item.HasItem).Select(item => new
                    {
                        item.ProductName,
                        item.Category,
                        item.UnitPrice,
                        item.Quantity,
                        SubTotal = item.UnitPrice * item.Quantity,
                        ProductImages = productImages.GetValueOrDefault(item.ProductId) ?? []
                    }).ToList()
                })
                .ToList();

            return new
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                StatusBreakdown = statusBreakdown,
                VendorCount = vendorCount,
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
                    // Step 1: Get order IDs that contain this vendor's products (existing products only)
                    var vendorProductIds = _context.ProductMasters
                        .Where(x => x.VendorId == vendorId)
                        .Select(x => x.Id);

                    var vendorOrderIds = await _context.OrderItems
                        .Where(x => vendorProductIds.Contains(x.ProductId))
                        .Select(x => x.OrderId)
                        .Distinct()
                        .ToListAsync();

                    // Step 2: Aggregate ALL items for those orders (LEFT JOIN covers deleted products)
                    var monthlyData = await (
                        from oi in _context.OrderItems
                        join om in _context.OrderMasters on oi.OrderId equals om.Id
                        where vendorOrderIds.Contains(om.Id) &&
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
