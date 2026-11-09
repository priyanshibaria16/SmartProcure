using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class GoodsReceiptsController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public GoodsReceiptsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetGoodsReceipts([FromQuery] int? poId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _inventoryService.GetGoodsReceiptsAsync(poId, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetGoodsReceiptById(int id)
    {
        var response = await _inventoryService.GetGoodsReceiptByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpGet("pending-pos")]
    [Authorize(Roles = "Administrator,WarehouseManager,PurchaseManager")]
    public async Task<IActionResult> GetPendingPOs()
    {
        var response = await _inventoryService.GetPendingPOsForReceivingAsync();
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,WarehouseManager")]
    public async Task<IActionResult> CreateGoodsReceipt([FromBody] GoodsReceiptCreateDto dto)
    {
        var response = await _inventoryService.CreateGoodsReceiptAsync(CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetGoodsReceiptById), new { id = response.Data!.GRNId }, response);
    }
}
