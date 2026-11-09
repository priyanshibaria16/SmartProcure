using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

public class MasterDataController : BaseApiController
{
    private readonly IMasterDataService _masterService;

    public MasterDataController(IMasterDataService masterService)
    {
        _masterService = masterService;
    }

    // --- Vendors ---

    [HttpGet("/api/vendors")]
    [Authorize]
    public async Task<IActionResult> GetVendors([FromQuery] string? search, [FromQuery] bool? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _masterService.GetVendorsAsync(search, status, page, pageSize);
        return Ok(response);
    }

    [HttpGet("/api/vendors/{id}")]
    [Authorize]
    public async Task<IActionResult> GetVendorById(int id)
    {
        var response = await _masterService.GetVendorByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost("/api/vendors")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> CreateVendor([FromBody] VendorCreateDto dto)
    {
        var response = await _masterService.CreateVendorAsync(dto);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetVendorById), new { id = response.Data!.VendorId }, response);
    }

    [HttpPut("/api/vendors/{id}")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> UpdateVendor(int id, [FromBody] VendorUpdateDto dto)
    {
        var response = await _masterService.UpdateVendorAsync(id, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPatch("/api/vendors/{id}/status")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> ToggleVendorStatus(int id)
    {
        var response = await _masterService.ToggleVendorStatusAsync(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    // --- Materials ---

    [HttpGet("/api/materials")]
    [Authorize]
    public async Task<IActionResult> GetMaterials([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] bool? lowStockOnly, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _masterService.GetMaterialsAsync(search, categoryId, lowStockOnly, page, pageSize);
        return Ok(response);
    }

    [HttpGet("/api/materials/{id}")]
    [Authorize]
    public async Task<IActionResult> GetMaterialById(int id)
    {
        var response = await _masterService.GetMaterialByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost("/api/materials")]
    [Authorize(Roles = "Administrator,PurchaseManager,WarehouseManager")]
    public async Task<IActionResult> CreateMaterial([FromBody] MaterialCreateDto dto)
    {
        var response = await _masterService.CreateMaterialAsync(dto);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetMaterialById), new { id = response.Data!.MaterialId }, response);
    }

    [HttpPut("/api/materials/{id}")]
    [Authorize(Roles = "Administrator,PurchaseManager,WarehouseManager")]
    public async Task<IActionResult> UpdateMaterial(int id, [FromBody] MaterialUpdateDto dto)
    {
        var response = await _masterService.UpdateMaterialAsync(id, dto);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    // --- Supporting Masters ---

    [HttpGet("/api/departments")]
    [Authorize]
    public async Task<IActionResult> GetDepartments()
    {
        var response = await _masterService.GetDepartmentsAsync();
        return Ok(response);
    }

    [HttpGet("/api/warehouses")]
    [Authorize]
    public async Task<IActionResult> GetWarehouses()
    {
        var response = await _masterService.GetWarehousesAsync();
        return Ok(response);
    }

    [HttpGet("/api/categories")]
    [Authorize]
    public async Task<IActionResult> GetCategories()
    {
        var response = await _masterService.GetCategoriesAsync();
        return Ok(response);
    }

    [HttpGet("/api/uom")]
    [Authorize]
    public async Task<IActionResult> GetUnitsOfMeasure()
    {
        var response = await _masterService.GetUnitsOfMeasureAsync();
        return Ok(response);
    }
}
