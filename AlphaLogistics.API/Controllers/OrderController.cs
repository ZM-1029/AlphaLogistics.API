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
            var orderId = await _orderService.PlaceOrder(order);

            if (orderId > 0) { return SuccessResponse(orderId, "Order placed successfully"); }

            else return ErrorResponse<string>("Error while placing order");
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
                return ErrorResponse<string>("No orders found");
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
            string Esc(string? s) => string.IsNullOrEmpty(s) ? "—" : System.Net.WebUtility.HtmlEncode(s);
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Delivery Label - {Esc(dto.OrderNumber)}</title>
  <style>
    * {{ box-sizing: border-box; margin: 0; padding: 0; }}
    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 16px; background: #f5f5f5; }}
    @media print {{ body {{ background: #fff; padding: 0; }} .no-print {{ display: none; }} }}
    .label {{ max-width: 420px; margin: 0 auto; background: #fff; border: 2px solid #1a1a1a; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
    .label-header {{ background: #1a1a1a; color: #fff; padding: 12px 16px; font-size: 14px; font-weight: 600; letter-spacing: 0.5px; }} 
    .label-body {{ padding: 20px 16px; }}
    .label-row {{ margin-bottom: 14px; }}
    .label-row:last-child {{ margin-bottom: 0; }}
    .label-key {{ font-size: 10px; text-transform: uppercase; letter-spacing: 0.8px; color: #666; margin-bottom: 2px; }}
    .label-val {{ font-size: 15px; font-weight: 500; color: #111; word-break: break-word; }}
    .instruction {{ border-top: 1px dashed #ccc; padding-top: 14px; margin-top: 14px; }}
    .print-btn {{ display: block; max-width: 420px; margin: 16px auto 0; padding: 10px 20px; background: #1a1a1a; color: #fff; border: none; border-radius: 6px; cursor: pointer; font-size: 14px; }}
    .print-btn:hover {{ background: #333; }}
  </style>
</head>
<body>
  <div class=""label"">
    <div class=""label-header"">DELIVERY LABEL</div>
    <div class=""label-body"">
      <div class=""label-row"">
        <div class=""label-key"">Order Number</div>
        <div class=""label-val"">{Esc(dto.OrderNumber)}</div>
      </div>
      <div class=""label-row"">
        <div class=""label-key"">Name</div>
        <div class=""label-val"">{Esc(dto.CustomerName)}</div>
      </div>
      <div class=""label-row"">
        <div class=""label-key"">Phone</div>
        <div class=""label-val"">{Esc(dto.Phone)}</div>
      </div>
      <div class=""label-row"">
        <div class=""label-key"">Address</div>
        <div class=""label-val"">{Esc(dto.Address)}</div>
      </div>
      <div class=""label-row"">
        <div class=""label-key"">Pradesh</div>
        <div class=""label-val"">{Esc(dto.Pradesh)}</div>
      </div>
      <div class=""label-row instruction"">
        <div class=""label-key"">Delivery Instruction</div>
        <div class=""label-val"">{Esc(dto.DeliveryInstruction)}</div>
      </div>
    </div>
  </div>
  <button type=""button"" class=""print-btn no-print"" onclick=""window.print()"">Print Label</button>
  <script>window.onload = function() {{ /* optional: auto-print prompt */ }};</script>
</body>
</html>";
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

                return Ok(new
                {
                    bankInstructions = BankDetails,
                    message = "Please transfer the amount and send payment confirmation"
                });

            }
            else
            {
                // QR transfer insruction
                return Ok(new
                {
                    QRCodeUrl = "/uploads/payment/0efb7098-1072-496a-8048-615471dbb0ee_Jack",
                    message = "Please scan the QR and send payment confirmation"
                });

            }
        }

        #endregion
    }
}
