using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        public OrderController(IConfiguration configuration, IOrderService orderService)
        {
            _configuration = configuration;
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult GetOrderStatuses()
        {
            var orderStatusSection = _configuration
                .GetSection("OrderStatus")
                .Get<Dictionary<string, string>>();

            var result = orderStatusSection?
                .Select(x => new
                {
                    Id = int.Parse(x.Value),
                    Name = x.Key,
                    // Label = FormatLabel(x.Key)
                })
                .OrderBy(x => x.Id)
                .ToList();

            return SuccessResponse(result, "Data retrieved successfully");
        }

        [HttpGet]
        public IActionResult PaymentOptions()
        {
            var orderStatusSection = _configuration
                .GetSection("PaymentOptions")
                .Get<Dictionary<string, string>>();

            var result = orderStatusSection?
                .Select(x => new
                {
                    Id = int.Parse(x.Value),
                    Name = x.Key,
                    // Label = FormatLabel(x.Key)
                })
                .OrderBy(x => x.Id)
                .ToList();

            return SuccessResponse(result, "Data retrieved successfully");
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(OrderDTO order)
        {
            var (orderId, orderNumber) = await _orderService.PlaceOrder(order);

            if (orderId > 0)
                return SuccessResponse(new { OrderId = orderId, OrderNumber = orderNumber }, "Order placed successfully");

            return ErrorResponse<string>("Error while placing order");
        }

        [HttpPost]
        public async Task<IActionResult> UploadPaymentProof(int orderId, IFormFile file)
        {
            if (orderId <= 0) return ErrorResponse<string>("Invalid order Id");
            if (file == null || file.Length == 0) return ErrorResponse<string>("File is required");

            var result = await _orderService.UploadPaymentProof(orderId, file);
            if (result) return SuccessResponse(result, "Payment proof uploaded successfully");

            return ErrorResponse<string>("Order not found or upload failed");
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateOrder(int orderId, OrderDTO order)
        {
            if (orderId <= 0) { return ErrorResponse<string>("Invalid order Id"); }

            var isupdated = await _orderService.UpdateOrder(orderId, order);
            if (isupdated) return SuccessResponse(isupdated, "Order updated successfully");
            return ErrorResponse<string>("Error while updating the order!");

        }

        [HttpPost]
        public async Task<IActionResult> AssignPradesh(int pradeshId, int orderId)
        {
            var isAssigned =await  _orderService.AssignPradesh(pradeshId,orderId);
            if (isAssigned) return SuccessResponse("Pradesh assigned successfully!");

            return ErrorResponse<string>("Internal issue. Please contact to the support team!");
        }

        [HttpPost]
        public async Task<IActionResult> OrderList(OrderListDTO data)
        {
            var orderList = await _orderService.GetOrderList(data);
            var count = await _orderService.OrderCount(data);
            if (orderList != null && orderList.Any())
            {
                var response = new { TotalCount= count, orderList };
                return SuccessResponse(response, "Data retrieved successfully");
            }
            else
                return SuccessResponse<string>(null,"No orders found");

            //return ErrorResponse<string>("No orders found");
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order != null)
                return SuccessResponse(order, "Data retrieved successfully");
            else
                return ErrorResponse<string>("No orders found");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeOrderStatus(int orderId, int statusId)
        {
            var ischanged = await _orderService.ChangeStatus(orderId, statusId);
            if (ischanged)
            {
                // send mail to user about order status change
                return SuccessResponse("Data retrieved successfully");
            }
            else
            {
                // send mail to user about order status change
                return ErrorResponse<string>("No orders found");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var cancelled = await _orderService.CancelOrder(orderId);
            if (cancelled) return Ok(new { Status = true, Message = "Order cancelled successfully" });
            else return Ok(new { Status = false, Message = "Order can not be cancelled at this stage" });
        }

        [HttpGet]
        public async Task<IActionResult> OrderTracking(int orderId)
        {
            var data = await _orderService.OrderTrackingData(orderId);
            return SuccessResponse(data, "Data retrieved successfully");
        }

        [HttpGet]
        public async Task<IActionResult> IsExistingSKU(string sku)
        {
            var isExisting = await _orderService.IsExistingSKU(sku);
            if (isExisting) return Ok(new { Status = true, Message = "SKU exist in database" });
            else return Ok(new { Status = false, Message = "No sku found with provided name" });
        }

        /// <summary>
        /// Returns a printable delivery label (HTML page) for an order. Open in browser and print.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PrintDeliveryLabel(int orderId)
        {
            var label = await _orderService.GetDeliveryLabelData(orderId);
            if (label == null)
                return ErrorResponse<string>("Order not found");

            var html = BuildDeliveryLabelHtml(label);
            return Content(html, "text/html");
        }

        private static string BuildDeliveryLabelHtml(DeliveryLabelDto dto)
        {
            string Esc(string? s) => string.IsNullOrEmpty(s) ? "" : System.Net.WebUtility.HtmlEncode(s);
            string date = DateTime.Now.ToString("dd MMM yyyy");
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Delivery Label - {Esc(dto.OrderNumber)}</title>
  <style>
    * {{ box-sizing: border-box; margin: 0; padding: 0; }}
    body {{
      font-family: Arial, Helvetica, sans-serif;
      background: #e8e8e8;
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 30px 16px;
      min-height: 100vh;
    }}

    /* ── Label wrapper: 4×6 inch at 96 dpi = 384×576px ── */
    .label {{
      width: 384px;
      background: #fff;
      border: 2px solid #000;
      font-size: 12px;
      color: #000;
    }}

    /* ── Top bar ── */
    .top-bar {{
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: #111;
      color: #fff;
      padding: 8px 12px;
    }}
    .top-bar .company {{ font-size: 15px; font-weight: 700; letter-spacing: 1px; }}
    .top-bar .label-type {{ font-size: 10px; letter-spacing: 2px; text-transform: uppercase; opacity: 0.75; }}

    /* ── Order number band ── */
    .order-band {{
      background: #f0f0f0;
      border-top: 1px solid #000;
      border-bottom: 1px solid #000;
      padding: 6px 12px;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }}
    .order-band .order-label {{ font-size: 9px; text-transform: uppercase; letter-spacing: 1px; color: #555; }}
    .order-band .order-number {{ font-size: 16px; font-weight: 700; letter-spacing: 0.5px; }}
    .order-band .order-date {{ font-size: 10px; color: #444; text-align: right; }}

    /* ── Section dividers ── */
    .section {{
      padding: 10px 12px;
      border-bottom: 1px solid #ddd;
    }}
    .section:last-child {{ border-bottom: none; }}
    .section-title {{
      font-size: 8px;
      text-transform: uppercase;
      letter-spacing: 1.5px;
      color: #777;
      margin-bottom: 5px;
      font-weight: 700;
    }}

    /* ── Ship-to block ── */
    .ship-to-name {{
      font-size: 20px;
      font-weight: 700;
      line-height: 1.2;
      margin-bottom: 4px;
    }}
    .ship-to-phone {{
      font-size: 14px;
      font-weight: 600;
      margin-bottom: 4px;
      letter-spacing: 0.3px;
    }}
    .ship-to-address {{
      font-size: 12px;
      line-height: 1.5;
      color: #222;
    }}

    /* ── Instructions ── */
    .instructions {{
      font-size: 11px;
      line-height: 1.5;
      color: #333;
      font-style: italic;
    }}
    .no-instruction {{ color: #aaa; font-style: italic; font-size: 10px; }}

    /* ── Barcode-style stripe ── */
    .barcode-stripe {{
      display: flex;
      height: 36px;
      gap: 2px;
      padding: 4px 12px;
      align-items: flex-end;
      background: #fff;
      border-bottom: 1px solid #ddd;
      overflow: hidden;
    }}
    .barcode-stripe span {{
      display: inline-block;
      background: #000;
      width: 2px;
      border-radius: 1px;
    }}

    /* ── Footer ── */
    .footer {{
      background: #111;
      color: #aaa;
      font-size: 8px;
      text-align: center;
      padding: 5px 12px;
      letter-spacing: 0.5px;
    }}

    /* ── Print button (hidden on print) ── */
    .print-btn {{
      margin-top: 20px;
      padding: 10px 32px;
      background: #111;
      color: #fff;
      border: none;
      border-radius: 5px;
      font-size: 13px;
      cursor: pointer;
      letter-spacing: 0.5px;
    }}
    .print-btn:hover {{ background: #333; }}

    @media print {{
      body {{ background: #fff; padding: 0; }}
      .label {{ border: 1.5px solid #000; box-shadow: none; }}
      .print-btn {{ display: none; }}
    }}
  </style>
</head>
<body>
  <div class=""label"">

    <!-- Header -->
    <div class=""top-bar"">
      <span class=""company"">&#9650; AlphaLogistics</span>
      <span class=""label-type"">Delivery Label</span>
    </div>

    <!-- Order number -->
    <div class=""order-band"">
      <div>
        <div class=""order-label"">Order No.</div>
        <div class=""order-number"">{Esc(dto.OrderNumber)}</div>
      </div>
      <div class=""order-date"">
        <div class=""order-label"">Date</div>
        <div>{date}</div>
      </div>
    </div>

    <!-- Barcode-style decoration -->
    <div class=""barcode-stripe"">
      {GenerateBarcodeStripes(dto.OrderNumber)}
    </div>

    <!-- Ship To -->
    <div class=""section"">
      <div class=""section-title"">&#9654; Ship To</div>
      <div class=""ship-to-name"">{Esc(dto.CustomerName)}</div>
      <div class=""ship-to-phone"">&#128222; {Esc(dto.Phone)}</div>
      <div class=""ship-to-address"">
        {Esc(dto.Address)}{(string.IsNullOrWhiteSpace(dto.Pradesh) ? "" : $", {Esc(dto.Pradesh)}")}
      </div>
    </div>

    <!-- Delivery Instructions -->
    <div class=""section"">
      <div class=""section-title"">&#9993; Delivery Instructions</div>
      {(string.IsNullOrWhiteSpace(dto.DeliveryInstruction)
          ? @"<div class=""no-instruction"">No special instructions</div>"
          : $@"<div class=""instructions"">{Esc(dto.DeliveryInstruction)}</div>")}
    </div>

    <!-- Footer -->
    <div class=""footer"">AlphaLogistics &mdash; Handle with care &mdash; {date}</div>
  </div>

  <button type=""button"" class=""print-btn"" onclick=""window.print()"">&#128438; Print Label</button>
</body>
</html>";
        }

        private static string GenerateBarcodeStripes(string? input)
        {
            var rng = new Random(string.IsNullOrEmpty(input) ? 42 : input.GetHashCode());
            var sb = new System.Text.StringBuilder();
            int totalWidth = 0;
            while (totalWidth < 340)
            {
                int w = rng.Next(1, 5);
                int h = rng.Next(16, 32);
                sb.Append($"<span style=\"width:{w}px;height:{h}px\"></span>");
                totalWidth += w + 2;
            }
            return sb.ToString();
        }

        [HttpGet]
        public async Task<IActionResult> ExportOrdersToExcel(int? userId, DateTime? from, DateTime? to, int? statusId)
        {
            var fileContent = await _orderService.ExportOrdersToExcelAsync(userId, from, to, statusId);
            if (fileContent != null && fileContent.Length > 0)
            {
                var fileName = $"Orders_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                return ErrorResponse<string>("No orders found to export");
            }
        }

        #region Payment APIs

        [HttpGet]
        public async Task<IActionResult> CreateBankTransferProcess(bool isBanTransfer)
        {

            if (isBanTransfer)
            {    
                var BankDetails = new BankAccountDetails
                {
                    BankName = _configuration["BankTransfer:BankName"],
                    AccountHolderName = _configuration["BankTransfer:AccountHolderName"],
                    AccountNumber = _configuration["BankTransfer:AccountNumber"],
                    Branch = _configuration["BankTransfer:Branch"],
                };

                return SuccessResponse(BankDetails,"Please transfer the amount and send payment confirmation");

            }
            else
            {

                // QR transfer insruction
                return SuccessResponse(new
                {
                    QRCodeUrl = "/uploads/payment/0efb7098-1072-496a-8048-615471dbb0ee_Jack"
                    
                },"Please scan the QR and send payment confirmation");

            }
        }

        #endregion
    }
}
