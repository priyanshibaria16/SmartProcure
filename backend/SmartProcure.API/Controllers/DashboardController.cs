using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpi")]
    [Authorize]
    public async Task<IActionResult> GetKpis()
    {
        var response = await _dashboardService.GetKpisAsync();
        return Ok(response);
    }

    [HttpGet("charts")]
    [Authorize]
    public async Task<IActionResult> GetCharts()
    {
        var monthlySpend = await _dashboardService.GetMonthlySpendAsync();
        var poStatus = await _dashboardService.GetPoStatusDistributionAsync();
        var deptSpend = await _dashboardService.GetDepartmentSpendAsync();
        var topVendors = await _dashboardService.GetTopVendorsAsync();
        var lowStock = await _dashboardService.GetLowStockItemsAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                monthlySpend = monthlySpend.Data,
                poStatus = poStatus.Data,
                deptSpend = deptSpend.Data,
                topVendors = topVendors.Data,
                lowStock = lowStock.Data
            }
        });
    }

    [HttpGet("activities")]
    [Authorize]
    public async Task<IActionResult> GetRecentActivities()
    {
        var response = await _dashboardService.GetRecentActivitiesAsync();
        return Ok(response);
    }
}
