using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var count = _orderService.OrderCount(data);
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
    }
}
