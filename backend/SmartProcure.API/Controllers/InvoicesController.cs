using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Enums;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class InvoicesController : BaseApiController
{
    private readonly IFinanceService _financeService;

    public InvoicesController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceStatus? status, [FromQuery] int? vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _financeService.GetInvoicesAsync(status, vendorId, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var response = await _financeService.GetInvoiceByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,FinanceManager")]
    public async Task<IActionResult> CreateInvoice([FromBody] SupplierInvoiceCreateDto dto)
    {
        var response = await _financeService.CreateInvoiceAsync(dto);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetInvoiceById), new { id = response.Data!.InvoiceId }, response);
    }

    [HttpPost("{id}/verify")]
    [Authorize(Roles = "Administrator,FinanceManager")]
    public async Task<IActionResult> VerifyInvoice(int id)
    {
        var response = await _financeService.VerifyInvoiceAsync(id, CurrentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Administrator,FinanceManager")]
    public async Task<IActionResult> ApproveInvoice(int id, [FromBody] string remarks)
    {
        var response = await _financeService.ApproveInvoiceAsync(id, CurrentUserId, remarks);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Administrator,FinanceManager")]
    public async Task<IActionResult> RejectInvoice(int id, [FromBody] string remarks)
    {
        var response = await _financeService.RejectInvoiceAsync(id, CurrentUserId, remarks);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
