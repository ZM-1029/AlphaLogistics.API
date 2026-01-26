using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DashBoardController : BaseController
    {
        private readonly IDashBoardService _dashBoardService;
        public DashBoardController(IDashBoardService service)
        {
            _dashBoardService = service;
        }

        [HttpGet]
        public async Task<IActionResult> VendorMonthlySalesReport(int vendorId)
        {
            var result = await _dashBoardService.GetMonthlySalesReport(vendorId);
            return SuccessResponse(result, "Data retrieved successfully");
        }

        [HttpGet]
        public async Task<IActionResult> VendorGraphDataWithOrderList(int vendorId)
        {
            var result = await _dashBoardService.GraphData(vendorId);
            return SuccessResponse(result, "Data retrieved successfully");
        }
    }
}
