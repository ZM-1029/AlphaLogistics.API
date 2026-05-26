using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Serilog;
using System.Data;
using System.Transactions;
using WALMS.API.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
                    order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    order.PradeshId,
                    order.DeliveryAddress,
                    order.Branch,
                    order.CourierPartner,
                    order.DeliveryType,
                    order.DeliveryInstuctions,
                    order.Remark,
                    order.DeliveryCharge,
                    OrderItems = order?.OrderItems?.Select(item => new
                    {
                        Id = item.Id,

                        Product = new
                        {
                            item.ProductMaster?.Id,
                            item.ProductMaster?.SKU,
                            item.ProductMaster?.ProductName,
                            item.ProductMaster?.ProductImages?.FirstOrDefault()?.ImageUrl,
                            item.ProductMaster?.SubCategoryId,
                            item.ProductColour,
                            item.ProductSize,
                            SubcategoryName = subCategory.FirstOrDefault(x => x.Id == item.ProductMaster?.SubCategoryId)?.Name,
                            CategoryId = subCategory.FirstOrDefault(x => x.Id == item.ProductMaster?.SubCategoryId)?.CategoryId,
                            CategoryName = category.FirstOrDefault(x => x.Id == subCategory.FirstOrDefault(s => s.Id == item.ProductMaster?.SubCategoryId)?.CategoryId)?.Name,
                        },

                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,

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

        public async Task<int> OrderCount(OrderListDTO data)
        {
            var query = _context.OrderMasters.AsQueryable();

            if (data.VendorId.HasValue && data.VendorId > 0)
            {
                var vendorProductIds = _context.ProductMasters
                    .Where(p => p.VendorId == data.VendorId.Value)
                    .Select(p => p.Id);

                query = query.Where(o => o.OrderItems!
                    .Any(oi => vendorProductIds.Contains(oi.ProductId)));
            }

            if (data.userId != null && data.userId > 0)
                query = query.Where(x => x.UserId == data.userId);

            if (data.from.HasValue && data.to.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(data.from.Value.Date, DateTimeKind.Utc);
                var toUtc   = DateTime.SpecifyKind(data.to.Value.Date,   DateTimeKind.Utc);
                query = query.Where(x => x.OrderDate >= fromUtc && x.OrderDate < toUtc.AddDays(1));
            }

            if (data.statusId.HasValue && data.statusId > 0)
                query = query.Where(x => x.Status == data.statusId);

            return await query.CountAsync();
        }
        public async Task<List<dynamic>?> GetOrderList(OrderListDTO data)
        {
            try 
            {
                var query = _context.OrderMasters
                         .Include(o => o.OrderItems)
                             .ThenInclude(oi => oi.ProductMaster)
                         .AsQueryable();

                var products= await _context.ProductMasters.Where(x=>x.VendorId==14).ToListAsync();
                if (data.VendorId.HasValue && data.VendorId > 0)
                {
                    query = query.Where(o => o.OrderItems!
                        .Any(oi => oi.ProductMaster != null
                                && oi.ProductMaster.VendorId == data.VendorId.Value));
                }

                var orders = await query.ToListAsync();

                if (data.userId != null && data.userId>0)
                { 
                    orders = orders.Where(x => x.UserId == data.userId).ToList();
                }

                if (!orders.Any()) return null;

                if (data.from.HasValue && data.to.HasValue)
                {
                    orders = orders.Where(x => x.OrderDate.Date >= data.from.Value.Date && x.OrderDate.Date <= data.to.Value.Date).ToList();
                }

                if (data.statusId.HasValue && data.statusId>0)
                {
                    orders = orders.Where(x => x.Status == data.statusId).ToList();
                }

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
                x.DeliveryCharge,
                x.DeliveryType,
                x.DeliveryInstuctions,
                x.DeliveryAddress,
                x.Branch,
                x.CourierPartner,
                x.Remark,
                x.DeliveryDate
                }).OrderByDescending(x=>x.OrderDate).ToList();

                response = response
                            .Skip((data.page - 1) * data.pageSize)
                            .Take(data.pageSize)
                            .ToList();

                return response;
            }
            catch (Exception ex)
            {
                Log.Error($"OrderService/GetOrderList: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AssignPradesh(int orderId, int pradeshId)
        {
            var order= _context.OrderMasters.FirstOrDefault(x => x.Id == orderId);
            if (order == null) { return false; }

            order.PradeshId= pradeshId;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<(int OrderId, string OrderNumber)> PlaceOrder(OrderDTO order)
        {
            try
            {
                var orderNumber = $"AL-{DateTime.Now.Ticks}";
                var orderTotal = order.OrderItems.Sum(item => (item.UnitPrice ?? 0) * item.Quantity);
                var userId = _userContext.UserId > 0 ? _userContext.UserId : (order.UserId ?? 0);

                if (userId <= 0)
                {
                    Log.Error("Order Service/PlaceOrder: UserId is zero");
                    return (0, string.Empty);
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
                    DeliveryAddress = order.DeliveryAddress,
                    Branch = order.Branch,
                    CourierPartner = order.CourierPartner,
                    DeliveryType = order.DeliveryType,
                    DeliveryInstuctions = order.DeliveryInstuctions,
                    Remark = order.Remark,
                    PradeshId = order.PradeshId,
                    PaymentTypeId = order.PaymentTypeId,
                    PaymentUrl = order.PaymentUrl,
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
                            UnitPrice = item.UnitPrice ?? 0,
                            ProductColour=item.ProductColour,
                            ProductSize = item.ProductSize,
                        };

                        _context.OrderItems.Add(orderItem);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (orderData.Id, orderData.OrderNumber);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Order Service/PlaceOrder: {ex.Message}");
                return (0, string.Empty);
            }
        }
        public async Task<bool> UpdateOrder(int orderId, OrderDTO order)
        {
            try
            {
                var userId = _userContext.UserId;

                if (userId <= 0)
                {
                    Log.Error("Order Service/UpdateOrder: UserId is zero");
                    return false;
                }

                var existingOrder = await _context.OrderMasters
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (existingOrder == null)
                {
                    Log.Error($"Order Service/UpdateOrder: Order not found for Id {orderId}");
                    return false;
                }

                var orderTotal = order.OrderItems
                    .Sum(item => (item.UnitPrice ?? 0) * item.Quantity);

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // ✅ Update OrderMaster fields
                    existingOrder.DeliveryCharge = order.DeliveryCharge ?? 0;
                    existingOrder.TotalAmount = orderTotal + (order.DeliveryCharge ?? 0);
                    existingOrder.DeliveryAddress = order.DeliveryAddress;
                    existingOrder.Branch = order.Branch;
                    existingOrder.CourierPartner = order.CourierPartner;
                    existingOrder.DeliveryType = order.DeliveryType;
                    existingOrder.DeliveryInstuctions = order.DeliveryInstuctions;
                    existingOrder.Remark = order.Remark;
                    existingOrder.PradeshId = order.PradeshId;
                    existingOrder.IsPlacedByAdmin = order.IsPlacedByAdmin;
                    existingOrder.Status = AppConstants.OrderStatus.Pending; // optional
                    //existingOrder.UpdatedDate = DateTime.UtcNow; // if column exists

                    await _context.SaveChangesAsync();

                    // ✅ Remove old items
                    _context.OrderItems.RemoveRange(existingOrder.OrderItems);
                    await _context.SaveChangesAsync();

                    // ✅ Add updated items
                    foreach (var item in order.OrderItems)
                    {
                        var orderItem = new OrderItems
                        {
                            OrderId = existingOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice ?? 0,
                            ProductSize=item.ProductSize,
                            ProductColour=item.ProductColour,
                        };

                        _context.OrderItems.Add(orderItem);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Order Service/UpdateOrder: {ex.Message}");
                return false;
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

        public async Task<byte[]> ExportOrdersToExcelAsync(int? userId, DateTime? from, DateTime? to, int? statusId)
        {
            try
            {
                // Build query with filtering
                var query = _context.OrderMasters
                    .Include(o => o.UserMaster)
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                if (userId != null)
                {
                    query = query.Where(x => x.UserId == userId);
                }

                if (from.HasValue && to.HasValue)
                {
                    var fromUtc = DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc);
                    var toUtc   = DateTime.SpecifyKind(to.Value.Date,   DateTimeKind.Utc);
                    query = query.Where(x => x.OrderDate >= fromUtc && x.OrderDate < toUtc.AddDays(1));
                }

                if (statusId.HasValue)
                {
                    query = query.Where(x => x.Status == statusId);
                }

                // Get order status mapping
                var orderStatusSection = _configuration
                    .GetSection("OrderStatus")
                    .Get<Dictionary<string, string>>();

                // Get all orders first, then process
                var ordersList = await query.Include(o=>o.UserMaster)
                    .OrderByDescending(x => x.OrderDate)
                    .ToListAsync();

                if (!ordersList.Any())
                {
                    throw new Exception("No orders found for export");
                }

                // Create a list to hold processed orders
                var orders = ordersList.Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.OrderDate,
                    x.Status,
                    CustomerName = x.UserMaster != null ? x.UserMaster.UserName : "N/A",
                    x.TotalAmount,
                    x.DeliveryCharge,
                    GrandTotal = x.TotalAmount + (x.DeliveryCharge ?? 0),
                    x.DeliveryAddress,
                    x.DeliveryType,
                    x.Branch,
                    x.CourierPartner,
                    ItemCount = x.OrderItems != null ? x.OrderItems.Count : 0,
                    x.IsPlacedByAdmin,
                    x.Remark,
                    // Use reflection to get the private DeliveryDate field if needed
                    DeliveryDate = GetDeliveryDate(x), // We'll create a helper method
                    StatusName = orderStatusSection != null && orderStatusSection.ContainsValue(x.Status.ToString())
                        ? orderStatusSection.FirstOrDefault(s => s.Value == x.Status.ToString()).Key
                        : "Unknown"
                }).ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Create Excel package
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Orders Report");

                // Set column headers
                var headers = new string[]
                {
            "Order ID", "Order Number", "Order Date", "Delivery Date", "Status",
            "Customer Name", "Total Amount", "Delivery Charge", "Grand Total",
            "Delivery Address", "Delivery Type", "Branch", "Courier Partner",
            "Item Count", "Placed By Admin", "Remark"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Add data rows
                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cells[row, 1].Value = order.Id;
                    worksheet.Cells[row, 2].Value = order.OrderNumber;

                    // Order Date
                    worksheet.Cells[row, 3].Value = order.OrderDate;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";

                    // Delivery Date (handle null)
                    if (order.DeliveryDate.HasValue)
                    {
                        worksheet.Cells[row, 4].Value = order.DeliveryDate.Value;
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
                    }
                    else
                    {
                        worksheet.Cells[row, 4].Value = "Not Delivered";
                    }

                    worksheet.Cells[row, 5].Value = order.StatusName;
                    worksheet.Cells[row, 6].Value = order.CustomerName;

                    // Amount columns - CORRECTED column indices
                    worksheet.Cells[row, 7].Value = order.TotalAmount;           // Column 7: Total Amount
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";

                    worksheet.Cells[row, 8].Value = order.DeliveryCharge ?? 0;   // Column 8: Delivery Charge
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";

                    worksheet.Cells[row, 9].Value = order.GrandTotal;            // Column 9: Grand Total
                    worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0.00";

                    worksheet.Cells[row, 10].Value = order.DeliveryAddress;
                    worksheet.Cells[row, 11].Value = order.DeliveryType;
                    worksheet.Cells[row, 12].Value = order.Branch;
                    worksheet.Cells[row, 13].Value = order.CourierPartner;
                    worksheet.Cells[row, 14].Value = order.ItemCount;
                    worksheet.Cells[row, 15].Value = order.IsPlacedByAdmin ? "Yes" : "No";
                    worksheet.Cells[row, 16].Value = order.Remark;

                    // Add border to each cell in the row
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    row++;
                }

                // Auto-fit columns for better readability
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Add summary information
                var lastRow = row;
                worksheet.Cells[lastRow + 2, 6].Value = "Total Orders:";
                worksheet.Cells[lastRow + 2, 7].Value = orders.Count;
                worksheet.Cells[lastRow + 2, 7].Style.Font.Bold = true;

                worksheet.Cells[lastRow + 3, 6].Value = "Total Amount:";
                worksheet.Cells[lastRow + 3, 7].Value = orders.Sum(o => o.GrandTotal);
                worksheet.Cells[lastRow + 3, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[lastRow + 3, 7].Style.Font.Bold = true;

                // Add filter to headers
                worksheet.Cells[1, 1, 1, headers.Length].AutoFilter = true;

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                Log.Error($"OrderExportService/ExportOrdersToExcelAsync: {ex.Message}");
                throw;
            }
        }

        // Helper method to get DeliveryDate (if it's private)
        private DateTime? GetDeliveryDate(OrderMaster order)
        {
            // If DeliveryDate is private, you might need to:
            // 1. Make it public temporarily: Change "private DateTime? DeliveryDate" to "public DateTime? DeliveryDate"
            // 2. Or add a public getter: public DateTime? GetDeliveryDate() => DeliveryDate;
            // 3. Or use reflection (not recommended for production):

            // Option 3: Using reflection (remove if you make it public)
            try
            {
                var property = typeof(OrderMaster).GetProperty("DeliveryDate",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (property != null)
                {
                    return property.GetValue(order) as DateTime?;
                }
            }
            catch
            {
                // Log or handle error
            }

            return null;
        }

        public async Task<DeliveryLabelDto?> GetDeliveryLabelData(int orderId)
        {
            var order = await _context.OrderMasters
                .Include(o => o.UserMaster)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return null;

            string pradeshName = "";
            if (order.PradeshId.HasValue)
            {
                var pradesh = await _context.PradeshMasters
                    .FirstOrDefaultAsync(p => p.Id == order.PradeshId.Value);
                pradeshName = pradesh?.Name ?? "";
            }

            return new DeliveryLabelDto
            {
                OrderNumber = order.OrderNumber ?? "",
                CustomerName = order.UserMaster?.UserName ?? "",
                Phone = order.UserMaster?.Phone ?? "",
                Address = order.DeliveryAddress ?? order.UserMaster?.Address ?? "",
                Pradesh = pradeshName,
                DeliveryInstruction = order.DeliveryInstuctions ?? ""
            };
        }

        public async Task<bool> UploadPaymentProof(int orderId, IFormFile file)
        {
            try
            {
                var order = await _context.OrderMasters.FirstOrDefaultAsync(x => x.Id == orderId);
                if (order == null) return false;

                var currDirectory = Directory.GetCurrentDirectory();
                var uploadsFolder = Path.Combine(currDirectory, "uploads", "payments");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Delete old file if exists
                if (!string.IsNullOrEmpty(order.PaymentUrl))
                {
                    var oldPath = Path.Combine(currDirectory, order.PaymentUrl.TrimStart('/'));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

                order.PaymentUrl = $"/uploads/payments/{uniqueFileName}";
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"OrderService/UploadPaymentProof: {ex.Message}");
                return false;
            }
        }
    }
}
