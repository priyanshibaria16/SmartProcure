using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Enums;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class RequisitionsController : BaseApiController
{
    private readonly IProcurementService _procurementService;

    public RequisitionsController(IProcurementService procurementService)
    {
        _procurementService = procurementService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRequisitions([FromQuery] RequisitionStatus? status, [FromQuery] int? departmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _procurementService.GetRequisitionsAsync(status, departmentId, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetRequisitionById(int id)
    {
        var response = await _procurementService.GetRequisitionByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,RequisitionCreator,PurchaseManager")]
    public async Task<IActionResult> CreateRequisition([FromBody] RequisitionCreateDto dto)
    {
        var response = await _procurementService.CreateRequisitionAsync(CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetRequisitionById), new { id = response.Data!.RequisitionId }, response);
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Administrator,RequisitionCreator,PurchaseManager")]
    public async Task<IActionResult> SubmitRequisition(int id)
    {
        var response = await _procurementService.SubmitRequisitionAsync(id, CurrentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> ApproveRequisition(int id, [FromBody] ApprovalActionDto dto)
    {
        var response = await _procurementService.ApproveRequisitionAsync(id, CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> RejectRequisition(int id, [FromBody] ApprovalActionDto dto)
    {
        var response = await _procurementService.RejectRequisitionAsync(id, CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Administrator,RequisitionCreator,PurchaseManager")]
    public async Task<IActionResult> CancelRequisition(int id)
    {
        var response = await _procurementService.CancelRequisitionAsync(id, CurrentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
