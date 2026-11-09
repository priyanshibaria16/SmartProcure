using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class QuotationsController : BaseApiController
{
    private readonly IProcurementService _procurementService;

    public QuotationsController(IProcurementService procurementService)
    {
        _procurementService = procurementService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetQuotations([FromQuery] int requisitionId)
    {
        var response = await _procurementService.GetQuotationsByPRAsync(requisitionId);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> CreateQuotation([FromBody] QuotationCreateDto dto)
    {
        var response = await _procurementService.CreateQuotationAsync(dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/select")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> SelectQuotation(int id, [FromBody] QuotationSelectDto dto)
    {
        var response = await _procurementService.SelectQuotationAsync(id, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
