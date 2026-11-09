using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("procurement")]
    [Authorize]
    public async Task<IActionResult> ExportProcurement([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var bytes = await _reportService.GenerateProcurementReportCsvAsync(fromDate, toDate);
        return File(bytes, "text/csv", $"Procurement_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("inventory")]
    [Authorize]
    public async Task<IActionResult> ExportInventory()
    {
        var bytes = await _reportService.GenerateInventoryReportCsvAsync();
        return File(bytes, "text/csv", $"Inventory_Stock_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("vendor-performance")]
    [Authorize]
    public async Task<IActionResult> ExportVendorPerformance()
    {
        var bytes = await _reportService.GenerateVendorPerformanceReportCsvAsync();
        return File(bytes, "text/csv", $"Vendor_Performance_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [HttpGet("outstanding-payments")]
    [Authorize]
    public async Task<IActionResult> ExportOutstandingPayments()
    {
        var bytes = await _reportService.GenerateOutstandingPaymentsReportCsvAsync();
        return File(bytes, "text/csv", $"Outstanding_Payments_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
