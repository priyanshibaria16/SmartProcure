namespace SmartProcure.Application.DTOs;

public class DashboardKpiDto
{
    public int TotalPurchaseRequisitions { get; set; }
    public int PendingApprovals { get; set; }
    public int TotalPurchaseOrders { get; set; }
    public decimal TotalProcurementValue { get; set; }
    public int TotalGoodsReceived { get; set; }
    public int PendingInvoices { get; set; }
    public decimal OutstandingPayments { get; set; }
    public int TotalActiveVendors { get; set; }
    public int TotalActiveMaterials { get; set; }
    public int LowStockItemsCount { get; set; }
}

public class MonthlySpendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int OrderCount { get; set; }
}

public class PoStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public class DeptSpendDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Budget { get; set; }
}

public class TopVendorDto
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
    public decimal Rating { get; set; }
}

public class LowStockItemDto
{
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal Deficit => Math.Max(0, ReorderLevel - CurrentStock);
}

public class RecentActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
