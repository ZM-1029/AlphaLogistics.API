using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IConfiguration _configuration;
        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
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
    }
}
