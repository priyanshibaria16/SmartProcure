using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DashboardKpiDto>> GetKpisAsync()
    {
        var totalPRs = await _context.PurchaseRequisitions.CountAsync();
        var pendingApprovals = await _context.PurchaseRequisitions
            .CountAsync(r => r.Status == RequisitionStatus.PendingApproval || r.Status == RequisitionStatus.Submitted);

        var totalPOs = await _context.PurchaseOrders.CountAsync();
        var totalProcurementValue = await _context.PurchaseOrders
            .Where(p => p.Status != PurchaseOrderStatus.Cancelled && p.Status != PurchaseOrderStatus.Draft)
            .SumAsync(p => (decimal?)p.GrandTotal) ?? 0m;

        var totalGRNs = await _context.GoodsReceipts.CountAsync();
        var pendingInvoices = await _context.SupplierInvoices
            .CountAsync(i => i.Status == InvoiceStatus.PendingVerification || i.Status == InvoiceStatus.Mismatched);

        var outstandingPayments = await _context.SupplierInvoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Rejected)
            .SumAsync(i => (decimal?)i.OutstandingAmount) ?? 0m;

        var activeVendors = await _context.Vendors.CountAsync(v => v.Status);
        var activeMaterials = await _context.Materials.CountAsync(m => m.Status);
        var lowStockCount = await _context.Materials.CountAsync(m => m.Status && m.CurrentStock <= m.ReorderLevel);

        return ApiResponse<DashboardKpiDto>.Ok(new DashboardKpiDto
        {
            TotalPurchaseRequisitions = totalPRs,
            PendingApprovals = pendingApprovals,
            TotalPurchaseOrders = totalPOs,
            TotalProcurementValue = totalProcurementValue,
            TotalGoodsReceived = totalGRNs,
            PendingInvoices = pendingInvoices,
            OutstandingPayments = outstandingPayments,
            TotalActiveVendors = activeVendors,
            TotalActiveMaterials = activeMaterials,
            LowStockItemsCount = lowStockCount
        });
    }

    public async Task<ApiResponse<List<MonthlySpendDto>>> GetMonthlySpendAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var pos = await _context.PurchaseOrders
            .Where(p => p.PODate >= sixMonthsAgo && p.Status != PurchaseOrderStatus.Cancelled)
            .Select(p => new { p.PODate, p.GrandTotal })
            .ToListAsync();

        var grouped = pos
            .GroupBy(p => new { p.PODate.Year, p.PODate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlySpendDto
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Amount = g.Sum(x => x.GrandTotal),
                OrderCount = g.Count()
            })
            .ToList();

        // If no records in last 6 months, return current month
        if (!grouped.Any())
        {
            grouped.Add(new MonthlySpendDto
            {
                Month = DateTime.UtcNow.ToString("MMM yyyy"),
                Amount = 0,
                OrderCount = 0
            });
        }

        return ApiResponse<List<MonthlySpendDto>>.Ok(grouped);
    }

    public async Task<ApiResponse<List<PoStatusDistributionDto>>> GetPoStatusDistributionAsync()
    {
        var list = await _context.PurchaseOrders
            .GroupBy(p => p.Status)
            .Select(g => new PoStatusDistributionDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                TotalAmount = g.Sum(x => x.GrandTotal)
            })
            .ToListAsync();

        return ApiResponse<List<PoStatusDistributionDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<DeptSpendDto>>> GetDepartmentSpendAsync()
    {
        var depts = await _context.Departments
            .Include(d => d.PurchaseRequisitions)
                .ThenInclude(r => r.PurchaseOrders)
            .AsNoTracking()
            .ToListAsync();

        var list = depts.Select(d => new DeptSpendDto
        {
            DepartmentName = d.DepartmentName,
            Budget = d.Budget,
            Amount = d.PurchaseRequisitions
                .SelectMany(r => r.PurchaseOrders)
                .Where(p => p.Status != PurchaseOrderStatus.Cancelled)
                .Sum(p => p.GrandTotal)
        }).ToList();

        return ApiResponse<List<DeptSpendDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<TopVendorDto>>> GetTopVendorsAsync()
    {
        var list = await _context.PurchaseOrders
            .Include(p => p.Vendor)
            .Where(p => p.Status != PurchaseOrderStatus.Cancelled)
            .GroupBy(p => new { p.VendorId, p.Vendor!.CompanyName, p.Vendor.VendorRating })
            .Select(g => new TopVendorDto
            {
                VendorId = g.Key.VendorId,
                VendorName = g.Key.CompanyName,
                TotalSpent = g.Sum(p => p.GrandTotal),
                OrderCount = g.Count(),
                Rating = g.Key.VendorRating
            })
            .OrderByDescending(v => v.TotalSpent)
            .Take(10)
            .ToListAsync();

        return ApiResponse<List<TopVendorDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<LowStockItemDto>>> GetLowStockItemsAsync()
    {
        var list = await _context.Materials
            .Include(m => m.Category)
            .Include(m => m.BaseUOM)
            .Where(m => m.Status && m.CurrentStock <= m.ReorderLevel)
            .OrderBy(m => m.CurrentStock / (m.ReorderLevel > 0 ? m.ReorderLevel : 1))
            .Select(m => new LowStockItemDto
            {
                MaterialId = m.MaterialId,
                MaterialCode = m.MaterialCode,
                MaterialName = m.MaterialName,
                CategoryName = m.Category != null ? m.Category.CategoryName : string.Empty,
                UOM = m.BaseUOM != null ? m.BaseUOM.UOMCode : string.Empty,
                CurrentStock = m.CurrentStock,
                ReorderLevel = m.ReorderLevel
            })
            .ToListAsync();

        return ApiResponse<List<LowStockItemDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync()
    {
        var activities = new List<RecentActivityDto>();

        var recentPOs = await _context.PurchaseOrders
            .Include(p => p.Vendor)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var p in recentPOs)
        {
            activities.Add(new RecentActivityDto
            {
                Id = p.PONumber,
                Title = $"Purchase Order {p.PONumber}",
                Description = $"Issued to {p.Vendor?.CompanyName ?? "Vendor"} for ₹{p.GrandTotal:N2}",
                Timestamp = p.CreatedAt.ToString("g"),
                Type = "PO",
                Status = p.Status.ToString()
            });
        }

        var recentGRNs = await _context.GoodsReceipts
            .Include(g => g.Vendor)
            .OrderByDescending(g => g.ReceiptDate)
            .Take(5)
            .ToListAsync();

        foreach (var g in recentGRNs)
        {
            activities.Add(new RecentActivityDto
            {
                Id = g.GRNNumber,
                Title = $"Goods Receipt {g.GRNNumber}",
                Description = $"Receipt recorded from {g.Vendor?.CompanyName ?? "Vendor"}",
                Timestamp = g.ReceiptDate.ToString("g"),
                Type = "GRN",
                Status = g.InspectionStatus.ToString()
            });
        }

        return ApiResponse<List<RecentActivityDto>>.Ok(activities.OrderByDescending(a => a.Timestamp).Take(8).ToList());
    }
}
