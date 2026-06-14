using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;
using WALMS.API.Common;

namespace AlphaLogistics.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly AlphaLogisticsContext _context;
        private readonly IConfiguration _config;

        public AccountService(AlphaLogisticsContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private decimal GetCommissionRate(string? customerType)
        {
            var cfg = _config.GetSection("VendorType").Get<Dictionary<string, string>>();
            if (cfg == null || string.IsNullOrEmpty(customerType)) return 0;
            return cfg.TryGetValue(customerType, out var rateStr)
                   && decimal.TryParse(rateStr, out var rate) ? rate : 0;
        }

        private Dictionary<int, string> GetOrderStatusMap()
        {
            var cfg = _config.GetSection("OrderStatus").Get<Dictionary<string, string>>();
            return cfg?.ToDictionary(x => int.Parse(x.Value), x => x.Key)
                   ?? new Dictionary<int, string>();
        }

        private static string ResolvePaymentStatus(OrderMaster order, Dictionary<int, string> statusMap)
        {
            if (order.PaymentTransferStatus == 2)
                return "Payment Successfully Transferred";

            // COD (PaymentTypeId == 1): cash collected only on delivery
            bool isCod = order.PaymentTypeId == 1;

            // Online payment (non-COD with PaymentTypeId set): customer already paid
            bool onlinePaymentReceived = order.PaymentTypeId != null && !isCod;

            if (onlinePaymentReceived || order.Status == AppConstants.OrderStatus.Delivered)
                return "Payment Transfer Pending";

            return statusMap.GetValueOrDefault(order.Status, "Unknown");
        }

        private static AccountSummary BuildSummary(List<AccountStatementRow> rows)
        {
            return new AccountSummary
            {
                TotalNetAmount = rows.Sum(r => r.NetAmount),
                TotalPaymentTransferPending = rows
                    .Where(r => r.PaymentStatus == "Payment Transfer Pending")
                    .Sum(r => r.NetAmount),
                TotalPaymentSuccessfullyTransferred = rows
                    .Where(r => r.PaymentStatus == "Payment Successfully Transferred")
                    .Sum(r => r.NetAmount)
            };
        }

        private static List<AccountStatementRow> BuildRows(
            IEnumerable<(OrderItems oi, OrderMaster om, string customerName)> data,
            Dictionary<int, string> skuMap,
            decimal commissionRate,
            Dictionary<int, string> statusMap)
        {
            return data
                .GroupBy(x => x.om.Id)
                .Select(g =>
                {
                    var order = g.First().om;
                    var customerName = g.First().customerName;
                    var itemTotal = g.Sum(x => x.oi.UnitPrice * x.oi.Quantity);
                    var deliveryCharge = order.DeliveryCharge ?? 0;
                    var commission = Math.Round(itemTotal * commissionRate / 100, 2);
                    var tax = Math.Round(commission * 0.01m, 2); // 1% of commission
                    var netAmount = itemTotal - deliveryCharge - commission - tax;
                    var skus = g.Select(x => skuMap.GetValueOrDefault(x.oi.ProductId, "N/A"))
                                .Distinct();

                    return new AccountStatementRow
                    {
                        OrderId = order.Id,
                        OrderNumber = order.OrderNumber,
                        OrderDate = order.OrderDate,
                        SKU = string.Join(", ", skus),
                        CustomerName = customerName,
                        TotalAmount = itemTotal,
                        DeliveryCharges = deliveryCharge,
                        CommissionRate = commissionRate,
                        Commission = commission,
                        Tax = tax,
                        NetAmount = netAmount,
                        PaymentStatus = ResolvePaymentStatus(order, statusMap),
                        PaymentTransferStatus = order.PaymentTransferStatus
                    };
                })
                .OrderByDescending(r => r.OrderDate)
                .ToList();
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        // ─── Statement building ──────────────────────────────────────────────────

        private async Task<(List<AccountStatementRow> rows, Dictionary<int, string> skuMap)>
            FetchVendorRows(int vendorId, DateTime? startDate, DateTime? endDate,
                            Dictionary<int, string> statusMap, decimal commissionRate)
        {
            var vendorProducts = await _context.ProductMasters
                .Where(p => p.VendorId == vendorId)
                .Select(p => new { p.Id, p.SKU })
                .ToListAsync();

            var productIds = vendorProducts.Select(p => p.Id).ToList();
            var skuMap = vendorProducts.ToDictionary(p => p.Id, p => p.SKU ?? "N/A");

            var query = from oi in _context.OrderItems
                        join om in _context.OrderMasters on oi.OrderId equals om.Id
                        join u in _context.UserMasters on om.UserId equals u.Id
                        where productIds.Contains(oi.ProductId)
                              && om.Status != AppConstants.OrderStatus.Cancelled
                              && om.Status != AppConstants.OrderStatus.Refunded
                        select new { oi, om, CustomerName = u.UserName };

            if (startDate.HasValue)
                query = query.Where(x => x.om.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(x => x.om.OrderDate < endDate.Value.AddDays(1));

            var raw = await query.ToListAsync();
            var rows = BuildRows(
                raw.Select(x => (x.oi, x.om, x.CustomerName)),
                skuMap, commissionRate, statusMap);

            return (rows, skuMap);
        }

        // ─── Public API ──────────────────────────────────────────────────────────

        public async Task<object?> GetAccountStatement(int? vendorId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var statusMap = GetOrderStatusMap();

                if (vendorId.HasValue && vendorId > 0)
                {
                    var vendor = await _context.VendorMasters
                        .FirstOrDefaultAsync(v => v.Id == vendorId);
                    if (vendor == null) return null;

                    var rate = GetCommissionRate(vendor.CustomerType);
                    var (rows, _) = await FetchVendorRows(vendorId.Value, startDate, endDate, statusMap, rate);

                    return new VendorStatementResult
                    {
                        VendorId = vendor.Id,
                        VendorName = vendor.VendorName,
                        VendorType = vendor.CustomerType,
                        CommissionRate = rate,
                        Statement = rows,
                        Summary = BuildSummary(rows)
                    };
                }
                else
                {
                    // Admin — all vendors
                    var query = from oi in _context.OrderItems
                                join om in _context.OrderMasters on oi.OrderId equals om.Id
                                join u in _context.UserMasters on om.UserId equals u.Id
                                join pm in _context.ProductMasters on oi.ProductId equals pm.Id
                                join vm in _context.VendorMasters on pm.VendorId equals vm.Id
                                where om.Status != AppConstants.OrderStatus.Cancelled
                                      && om.Status != AppConstants.OrderStatus.Refunded
                                select new { oi, om, CustomerName = u.UserName, pm, vm };

                    if (startDate.HasValue)
                        query = query.Where(x => x.om.OrderDate >= startDate.Value);
                    if (endDate.HasValue)
                        query = query.Where(x => x.om.OrderDate < endDate.Value.AddDays(1));

                    var raw = await query.ToListAsync();

                    var vendorStatements = raw
                        .GroupBy(x => new { x.vm.Id, x.vm.VendorName, x.vm.CustomerType })
                        .Select(vg =>
                        {
                            var rate = GetCommissionRate(vg.Key.CustomerType);
                            var skuMap = vg.Select(x => x.pm)
                                          .DistinctBy(p => p.Id)
                                          .ToDictionary(p => p.Id, p => p.SKU ?? "N/A");
                            var rows = BuildRows(
                                vg.Select(x => (x.oi, x.om, x.CustomerName)),
                                skuMap, rate, statusMap);

                            return new VendorStatementResult
                            {
                                VendorId = vg.Key.Id,
                                VendorName = vg.Key.VendorName,
                                VendorType = vg.Key.CustomerType,
                                CommissionRate = rate,
                                Statement = rows,
                                Summary = BuildSummary(rows)
                            };
                        })
                        .ToList();

                    return new
                    {
                        VendorStatements = vendorStatements,
                        OverallSummary = new
                        {
                            TotalNetAmount = vendorStatements.Sum(v => v.Summary.TotalNetAmount),
                            TotalPaymentTransferPending = vendorStatements.Sum(v => v.Summary.TotalPaymentTransferPending),
                            TotalPaymentSuccessfullyTransferred = vendorStatements.Sum(v => v.Summary.TotalPaymentSuccessfullyTransferred)
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error($"AccountService/GetAccountStatement: {ex.Message}");
                return null;
            }
        }

        public async Task<byte[]> ExportAccountStatement(int? vendorId, DateTime? startDate, DateTime? endDate)
        {
            var statusMap = GetOrderStatusMap();
            var sb = new StringBuilder();

            const string header = "Vendor Name,Vendor Type,Order ID,Order Number,Order Date,SKU,Customer Name," +
                                   "Total Amount,Delivery Charges,Commission Rate (%),Commission,Tax (1%),Net Amount,Payment Status";
            sb.AppendLine(header);

            if (vendorId.HasValue && vendorId > 0)
            {
                var vendor = await _context.VendorMasters.FirstOrDefaultAsync(v => v.Id == vendorId);
                if (vendor != null)
                {
                    var rate = GetCommissionRate(vendor.CustomerType);
                    var (rows, _) = await FetchVendorRows(vendorId.Value, startDate, endDate, statusMap, rate);
                    AppendRowsToCsv(sb, rows, vendor.VendorName, vendor.CustomerType);
                }
            }
            else
            {
                var query = from oi in _context.OrderItems
                            join om in _context.OrderMasters on oi.OrderId equals om.Id
                            join u in _context.UserMasters on om.UserId equals u.Id
                            join pm in _context.ProductMasters on oi.ProductId equals pm.Id
                            join vm in _context.VendorMasters on pm.VendorId equals vm.Id
                            where om.Status != AppConstants.OrderStatus.Cancelled
                                  && om.Status != AppConstants.OrderStatus.Refunded
                            select new { oi, om, CustomerName = u.UserName, pm, vm };

                if (startDate.HasValue)
                    query = query.Where(x => x.om.OrderDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(x => x.om.OrderDate < endDate.Value.AddDays(1));

                var raw = await query.ToListAsync();

                foreach (var vg in raw.GroupBy(x => new { x.vm.Id, x.vm.VendorName, x.vm.CustomerType }))
                {
                    var rate = GetCommissionRate(vg.Key.CustomerType);
                    var skuMap = vg.Select(x => x.pm)
                                   .DistinctBy(p => p.Id)
                                   .ToDictionary(p => p.Id, p => p.SKU ?? "N/A");
                    var rows = BuildRows(
                        vg.Select(x => (x.oi, x.om, x.CustomerName)),
                        skuMap, rate, statusMap);
                    AppendRowsToCsv(sb, rows, vg.Key.VendorName, vg.Key.CustomerType);
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private void AppendRowsToCsv(StringBuilder sb, List<AccountStatementRow> rows,
                                      string vendorName, string vendorType)
        {
            foreach (var r in rows)
            {
                sb.AppendLine(
                    $"{EscapeCsv(vendorName)},{EscapeCsv(vendorType)}," +
                    $"{r.OrderId},{EscapeCsv(r.OrderNumber)},{r.OrderDate:yyyy-MM-dd}," +
                    $"{EscapeCsv(r.SKU)},{EscapeCsv(r.CustomerName)}," +
                    $"{r.TotalAmount},{r.DeliveryCharges},{r.CommissionRate}," +
                    $"{r.Commission},{r.Tax},{r.NetAmount},{EscapeCsv(r.PaymentStatus)}");
            }
        }

        public async Task<bool> UpdatePaymentTransferStatus(int orderId, int status)
        {
            try
            {
                var order = await _context.OrderMasters.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order == null) return false;

                order.PaymentTransferStatus = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"AccountService/UpdatePaymentTransferStatus: {ex.Message}");
                return false;
            }
        }
    }
}
