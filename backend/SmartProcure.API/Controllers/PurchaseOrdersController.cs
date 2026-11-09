using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Enums;

namespace SmartProcure.API.Controllers;

[Route("api/[controller]")]
public class PurchaseOrdersController : BaseApiController
{
    private readonly IProcurementService _procurementService;

    public PurchaseOrdersController(IProcurementService procurementService)
    {
        _procurementService = procurementService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPurchaseOrders([FromQuery] PurchaseOrderStatus? status, [FromQuery] int? vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _procurementService.GetPurchaseOrdersAsync(status, vendorId, page, pageSize);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetPurchaseOrderById(int id)
    {
        var response = await _procurementService.GetPurchaseOrderByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderCreateDto dto)
    {
        var response = await _procurementService.CreatePurchaseOrderAsync(CurrentUserId, dto);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetPurchaseOrderById), new { id = response.Data!.POId }, response);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> SendPOToVendor(int id)
    {
        var response = await _procurementService.SendPOToVendorAsync(id, CurrentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Administrator,PurchaseManager")]
    public async Task<IActionResult> CancelPurchaseOrder(int id, [FromBody] string reason)
    {
        var response = await _procurementService.CancelPurchaseOrderAsync(id, CurrentUserId, reason);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("{id}/pdf")]
    [Authorize]
    public async Task<IActionResult> GetPurchaseOrderPdf(int id)
    {
        var response = await _procurementService.GetPurchaseOrderByIdAsync(id);
        if (!response.Success || response.Data == null) return NotFound("Purchase order not found");

        var po = response.Data;
        var html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<title>Purchase Order {po.PONumber}</title>
<style>
  body {{ font-family: 'Helvetica Neue', Arial, sans-serif; margin: 40px; color: #1e293b; }}
  .header {{ border-bottom: 2px solid #2563eb; padding-bottom: 15px; margin-bottom: 25px; display: flex; justify-content: space-between; }}
  .title {{ font-size: 26px; font-weight: bold; color: #1e3a8a; }}
  .meta {{ text-align: right; font-size: 14px; color: #64748b; }}
  .grid {{ display: flex; justify-content: space-between; margin-bottom: 30px; }}
  .box {{ width: 48%; border: 1px solid #e2e8f0; border-radius: 6px; padding: 15px; background: #f8fafc; font-size: 13px; line-height: 1.6; }}
  .box h3 {{ margin-top: 0; font-size: 14px; text-transform: uppercase; color: #475569; border-bottom: 1px solid #cbd5e1; padding-bottom: 5px; }}
  table {{ width: 100%; border-collapse: collapse; margin-bottom: 30px; }}
  th {{ background: #f1f5f9; padding: 10px; text-align: left; font-size: 12px; border-bottom: 2px solid #cbd5e1; color: #334155; }}
  td {{ padding: 10px; border-bottom: 1px solid #e2e8f0; font-size: 13px; }}
  .totals {{ width: 40%; margin-left: auto; border: 1px solid #e2e8f0; padding: 15px; border-radius: 6px; background: #f8fafc; font-size: 14px; line-height: 1.8; }}
  .totals .grand {{ font-size: 16px; font-weight: bold; color: #1e3a8a; border-top: 2px solid #cbd5e1; padding-top: 5px; margin-top: 5px; }}
  .footer {{ margin-top: 40px; border-top: 1px solid #e2e8f0; padding-top: 20px; font-size: 12px; color: #64748b; text-align: center; }}
</style>
</head>
<body>
  <div class='header'>
    <div>
      <div class='title'>APEX MANUFACTURING INDUSTRIES PVT. LTD.</div>
      <div style='font-size: 13px; color: #475569;'>Plot 42, MIDC Bhosari, Pune - 410501, Maharashtra, India</div>
      <div style='font-size: 12px; color: #64748b;'>GSTIN: 27AAACA9999Z1Z5 | PAN: AAACA9999Z</div>
    </div>
    <div class='meta'>
      <div style='font-size: 20px; font-weight: bold; color: #0f172a;'>PURCHASE ORDER</div>
      <div><strong>PO Number:</strong> {po.PONumber}</div>
      <div><strong>Date:</strong> {po.PODate:dd-MMM-yyyy}</div>
      <div><strong>Status:</strong> {po.StatusName}</div>
    </div>
  </div>

  <div class='grid'>
    <div class='box'>
      <h3>Vendor Details</h3>
      <strong>{po.VendorName}</strong><br/>
      {po.VendorAddress}<br/>
      GSTIN: {po.VendorGSTIN}<br/>
      PAN: {po.VendorPAN}<br/>
      Payment Terms: {po.PaymentTerms}
    </div>
    <div class='box'>
      <h3>Shipping & Delivery Address</h3>
      <strong>Warehouse: {po.WarehouseName}</strong><br/>
      Shipping: {po.ShippingAddress}<br/>
      Billing: {po.BillingAddress}<br/>
      Expected Delivery: {po.ExpectedDeliveryDate:dd-MMM-yyyy}
    </div>
  </div>

  <table>
    <thead>
      <tr>
        <th>#</th>
        <th>Material Code</th>
        <th>Material Description</th>
        <th>UOM</th>
        <th style='text-align: right;'>Quantity</th>
        <th style='text-align: right;'>Unit Price (₹)</th>
        <th style='text-align: right;'>Tax (%)</th>
        <th style='text-align: right;'>Line Total (₹)</th>
      </tr>
    </thead>
    <tbody>";

        int idx = 1;
        foreach (var item in po.Items)
        {
            html += $@"
      <tr>
        <td>{idx++}</td>
        <td><strong>{item.MaterialCode}</strong></td>
        <td>{item.MaterialName}</td>
        <td>{item.UOMCode}</td>
        <td style='text-align: right;'>{item.Quantity:N2}</td>
        <td style='text-align: right;'>{item.UnitPrice:N2}</td>
        <td style='text-align: right;'>{item.TaxRate}%</td>
        <td style='text-align: right;'>{item.LineTotal:N2}</td>
      </tr>";
        }

        html += $@"
    </tbody>
  </table>

  <div class='totals'>
    <div style='display: flex; justify-content: space-between;'><span>Subtotal:</span><span>₹{po.Subtotal:N2}</span></div>
    <div style='display: flex; justify-content: space-between;'><span>GST (Tax):</span><span>₹{po.TaxAmount:N2}</span></div>
    <div style='display: flex; justify-content: space-between;'><span>Discounts:</span><span>-₹{po.DiscountAmount:N2}</span></div>
    <div class='grand' style='display: flex; justify-content: space-between;'><span>Grand Total:</span><span>₹{po.GrandTotal:N2}</span></div>
  </div>

  <div class='footer'>
    This is an authorized system-generated Purchase Order issued by SmartProcure ERP for Apex Manufacturing Industries Pvt. Ltd.<br/>
    Authorized Signatory: Vikram Malhotra, Purchase Head
  </div>
</body>
</html>";

        var bytes = Encoding.UTF8.GetBytes(html);
        return File(bytes, "text/html", $"{po.PONumber}.html");
    }
}
