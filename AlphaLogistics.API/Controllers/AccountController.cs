using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Mvc;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Returns account statement with commission, tax, and net amount.
        /// vendorId = 0 or omitted → all vendors (admin view).
        /// startDate / endDate are optional; omit for all-time.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AccountStatement(
            int? vendorId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var result = await _accountService.GetAccountStatement(vendorId, startDate, endDate);
            return SuccessResponse(result, "Account statement retrieved successfully");
        }

        /// <summary>
        /// Downloads account statement as a CSV file.
        /// Same filters as AccountStatement.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportAccountStatement(
            int? vendorId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var csvBytes = await _accountService.ExportAccountStatement(vendorId, startDate, endDate);
            var fileName = vendorId.HasValue && vendorId > 0
                ? $"AccountStatement_Vendor{vendorId}_{DateTime.UtcNow:yyyyMMdd}.csv"
                : $"AccountStatement_All_{DateTime.UtcNow:yyyyMMdd}.csv";

            return File(csvBytes, "text/csv", fileName);
        }

        /// <summary>
        /// Updates payment transfer status for an order.
        /// status: 1 = Payment Transfer Pending | 2 = Payment Successfully Transferred
        /// </summary>
        [HttpPatch]
        public async Task<IActionResult> UpdatePaymentTransferStatus(int orderId, int status)
        {
            if (status != 1 && status != 2)
                return BadRequest(new { message = "Invalid status. Use 1 (Pending) or 2 (Transferred)." });

            var result = await _accountService.UpdatePaymentTransferStatus(orderId, status);
            return SuccessResponse(result, result ? "Payment transfer status updated" : "Order not found");
        }
    }
}
