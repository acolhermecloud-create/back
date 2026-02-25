using API.Payloads;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("dash")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpPost("view/one")]
        [Authorize("Admin")]
        public async Task<IActionResult> GetViewOne([FromBody] DashboardPayload payload)
        {
            try
            {
                var totalRevenue = await _dashboardService.TotalRevenue(payload.StartDate, payload.EndDate.AddHours(23));
                var totalRevenueCustomers = await _dashboardService.TotalRevenueCustomers(payload.StartDate, payload.EndDate.AddHours(23));
                var feeCollection = await _dashboardService.FeeCollection(payload.StartDate, payload.EndDate.AddHours(23));
                var totalDigitalStickers = await _dashboardService.TotalDigitalStickers(payload.StartDate, payload.EndDate.AddHours(23));
                var profit = await _dashboardService.NetProfit(payload.StartDate, payload.EndDate.AddHours(23));
                var totalUsers = await _dashboardService.TotalOfCustomers();
                var totalCampaigns = await _dashboardService.TotalOfCampaigns();
                var totalActiveCampaigns = await _dashboardService.TotalActiveCampaigns();
                var conversionRate = await _dashboardService.ConversionRate();

                var dash = new
                {
                    totalRevenue,
                    totalRevenueCustomers,
                    feeCollection,
                    totalDigitalStickers,
                    profit,
                    totalUsers,
                    totalCampaigns,
                    totalActiveCampaigns,
                    conversionRate
                };

                return Ok(new { dash });
            }
            catch (Exception ex)
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = ex.Message }));
            }
        }
    }
}
