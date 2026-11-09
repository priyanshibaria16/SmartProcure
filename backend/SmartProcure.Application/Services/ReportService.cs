using System.Text;
using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.Application.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;

    public ReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateProcurementReportCsvAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.PurchaseOrders
            .Include(p => p.Vendor)
            .Include(p => p.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue) query = query.Where(p => p.PODate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.PODate <= toDate.Value);

        var list = await query.OrderByDescending(p => p.PODate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("PO Number,PO Date,Vendor Code,Vendor Name,Warehouse,Subtotal (INR),Tax (INR),Grand Total (INR),Status");

        foreach (var p in list)
        {
            sb.AppendLine($"\"{p.PONumber}\",\"{p.PODate:yyyy-MM-dd}\",\"{p.Vendor?.VendorCode}\",\"{p.Vendor?.CompanyName}\",\"{p.Warehouse?.WarehouseName}\",{p.Subtotal:F2},{p.TaxAmount:F2},{p.GrandTotal:F2},\"{p.Status}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GenerateInventoryReportCsvAsync()
    {
        var list = await _context.InventoryBalances
            .Include(b => b.Material)
                .ThenInclude(m => m!.Category)
            .Include(b => b.Material)
                .ThenInclude(m => m!.BaseUOM)
            .Include(b => b.Warehouse)
            .AsNoTracking()
            .OrderBy(b => b.Warehouse!.WarehouseName)
            .ThenBy(b => b.Material!.MaterialCode)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Warehouse,Material Code,Material Name,Category,UOM,Unit Price (INR),Available Stock,Reserved Stock,Total Stock,Valuation (INR),Reorder Level,Low Stock Alert");

        foreach (var b in list)
        {
            var mat = b.Material;
            decimal totalStock = b.AvailableStock + b.ReservedStock;
            decimal val = totalStock * (mat?.UnitPrice ?? 0);
            bool isLow = b.AvailableStock <= (mat?.ReorderLevel ?? 0);

            sb.AppendLine($"\"{b.Warehouse?.WarehouseName}\",\"{mat?.MaterialCode}\",\"{mat?.MaterialName}\",\"{mat?.Category?.CategoryName}\",\"{mat?.BaseUOM?.UOMCode}\",{mat?.UnitPrice:F2},{b.AvailableStock:F3},{b.ReservedStock:F3},{totalStock:F3},{val:F2},{mat?.ReorderLevel:F3},\"{(isLow ? "YES" : "NO")}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GenerateVendorPerformanceReportCsvAsync()
    {
        var list = await _context.Vendors
            .Include(v => v.PurchaseOrders)
            .AsNoTracking()
            .OrderByDescending(v => v.VendorRating)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Vendor Code,Company Name,GSTIN,City,State,Rating,Total Orders,Total Spend (INR),Status");

        foreach (var v in list)
        {
            decimal spend = v.PurchaseOrders.Sum(p => p.GrandTotal);
            sb.AppendLine($"\"{v.VendorCode}\",\"{v.CompanyName}\",\"{v.GSTIN}\",\"{v.City}\",\"{v.State}\",{v.VendorRating:F1},{v.PurchaseOrders.Count},{spend:F2},\"{(v.Status ? "Active" : "Inactive")}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GenerateOutstandingPaymentsReportCsvAsync()
    {
        var list = await _context.SupplierInvoices
            .Include(i => i.Vendor)
            .Include(i => i.PurchaseOrder)
            .Where(i => i.OutstandingAmount > 0)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Invoice Number,Vendor Name,PO Number,Invoice Date,Due Date,Total Amount (INR),Paid Amount (INR),Outstanding Balance (INR),Status");

        foreach (var i in list)
        {
            sb.AppendLine($"\"{i.InvoiceNumber}\",\"{i.Vendor?.CompanyName}\",\"{i.PurchaseOrder?.PONumber}\",\"{i.InvoiceDate:yyyy-MM-dd}\",\"{i.DueDate:yyyy-MM-dd}\",{i.TotalAmount:F2},{i.PaidAmount:F2},{i.OutstandingAmount:F2},\"{i.Status}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
