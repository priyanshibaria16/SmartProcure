using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class PaymentsController : BaseApiController
{
    private readonly IFinanceService _financeService;

    public PaymentsController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPayments([FromQuery] int? invoiceId, [FromQuery] int? vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _financeService.GetPaymentsAsync(invoiceId, vendorId, page, pageSize);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,FinanceManager")]
    public async Task<IActionResult> RecordPayment([FromBody] PaymentCreateDto dto)
    {
        var response = await _financeService.RecordPaymentAsync(CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
