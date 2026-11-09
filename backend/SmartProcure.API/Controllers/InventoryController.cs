using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetStockBalances([FromQuery] int? warehouseId, [FromQuery] int? categoryId, [FromQuery] bool? lowStockOnly, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _inventoryService.GetStockBalancesAsync(warehouseId, categoryId, lowStockOnly, page, pageSize);
        return Ok(response);
    }

    [HttpGet("low-stock")]
    [Authorize]
    public async Task<IActionResult> GetLowStock([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _inventoryService.GetStockBalancesAsync(null, null, true, page, pageSize);
        return Ok(response);
    }

    [HttpGet("transactions")]
    [Authorize]
    public async Task<IActionResult> GetTransactions([FromQuery] int? materialId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _inventoryService.GetInventoryTransactionsAsync(materialId, page, pageSize);
        return Ok(response);
    }

    [HttpPost("adjustment")]
    [Authorize(Roles = "Administrator,WarehouseManager")]
    public async Task<IActionResult> RecordAdjustment([FromBody] StockAdjustmentDto dto)
    {
        var response = await _inventoryService.RecordStockAdjustmentAsync(CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
