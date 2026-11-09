-- ==============================================================================
-- SMARTPROCURE ENTERPRISE ERP - POWER BI ANALYTICAL VIEWS
-- Engine: Microsoft SQL Server 2019 / 2022 / Azure SQL / Power BI Desktop
-- Target Organization: Apex Manufacturing Industries Pvt. Ltd.
-- ==============================================================================

USE SmartProcureDb;
GO

-- 1. View: Procurement Spending Summary
IF OBJECT_ID('dbo.vw_ProcurementSpending', 'V') IS NOT NULL DROP VIEW dbo.vw_ProcurementSpending;
GO
CREATE VIEW dbo.vw_ProcurementSpending AS
SELECT 
    po.POId,
    po.PONumber,
    po.PODate,
    YEAR(po.PODate) AS POYear,
    MONTH(po.PODate) AS POMonth,
    DATENAME(month, po.PODate) AS MonthName,
    FORMAT(po.PODate, 'yyyy-MM') AS YearMonth,
    po.VendorId,
    v.VendorCode,
    v.CompanyName AS VendorName,
    v.City AS VendorCity,
    v.State AS VendorState,
    v.VendorCategory,
    v.VendorRating,
    po.WarehouseId,
    w.WarehouseName,
    po.Subtotal,
    po.TaxAmount,
    po.DiscountAmount,
    po.GrandTotal,
    CASE po.Status
        WHEN 1 THEN 'Draft'
        WHEN 2 THEN 'PendingApproval'
        WHEN 3 THEN 'Approved'
        WHEN 4 THEN 'SentToVendor'
        WHEN 5 THEN 'PartiallyReceived'
        WHEN 6 THEN 'FullyReceived'
        WHEN 7 THEN 'Closed'
        WHEN 8 THEN 'Cancelled'
        ELSE 'Unknown'
    END AS POStatus
FROM dbo.PurchaseOrders po
LEFT JOIN dbo.Vendors v ON po.VendorId = v.VendorId
LEFT JOIN dbo.Warehouses w ON po.WarehouseId = w.WarehouseId;
GO

-- 2. View: Inventory Valuation & Stock Health
IF OBJECT_ID('dbo.vw_InventoryValuation', 'V') IS NOT NULL DROP VIEW dbo.vw_InventoryValuation;
GO
CREATE VIEW dbo.vw_InventoryValuation AS
SELECT 
    b.BalanceId,
    b.WarehouseId,
    w.WarehouseCode,
    w.WarehouseName,
    w.Location AS WarehouseLocation,
    b.MaterialId,
    m.MaterialCode,
    m.MaterialName,
    c.CategoryName,
    u.UOMCode,
    m.UnitPrice AS StandardCost,
    b.AvailableStock,
    b.ReservedStock,
    (b.AvailableStock + b.ReservedStock) AS TotalStock,
    ((b.AvailableStock + b.ReservedStock) * m.UnitPrice) AS TotalValuationINR,
    m.ReorderLevel,
    m.MaximumStockLevel,
    CASE 
        WHEN b.AvailableStock <= m.ReorderLevel THEN 'Low Stock (Reorder Needed)'
        WHEN b.AvailableStock > m.MaximumStockLevel THEN 'Overstocked'
        ELSE 'Optimal Stock'
    END AS StockStatus,
    b.LastUpdated
FROM dbo.InventoryBalances b
INNER JOIN dbo.Materials m ON b.MaterialId = m.MaterialId
INNER JOIN dbo.Warehouses w ON b.WarehouseId = w.WarehouseId
LEFT JOIN dbo.MaterialCategories c ON m.CategoryId = c.CategoryId
LEFT JOIN dbo.UnitsOfMeasure u ON m.BaseUOMId = u.UOMId;
GO

-- 3. View: Three-Way Matching & Accounts Payable Aging
IF OBJECT_ID('dbo.vw_InvoiceAgingAndLiabilities', 'V') IS NOT NULL DROP VIEW dbo.vw_InvoiceAgingAndLiabilities;
GO
CREATE VIEW dbo.vw_InvoiceAgingAndLiabilities AS
SELECT 
    i.InvoiceId,
    i.InvoiceNumber,
    i.VendorId,
    v.CompanyName AS VendorName,
    v.GSTIN AS VendorGSTIN,
    i.POId,
    po.PONumber,
    i.GRNId,
    grn.GRNNumber,
    i.InvoiceDate,
    i.DueDate,
    DATEDIFF(day, i.DueDate, GETUTCDATE()) AS DaysOverdue,
    CASE 
        WHEN DATEDIFF(day, i.DueDate, GETUTCDATE()) <= 0 THEN 'Current (Not Due)'
        WHEN DATEDIFF(day, i.DueDate, GETUTCDATE()) BETWEEN 1 AND 30 THEN '1-30 Days Overdue'
        WHEN DATEDIFF(day, i.DueDate, GETUTCDATE()) BETWEEN 31 AND 60 THEN '31-60 Days Overdue'
        WHEN DATEDIFF(day, i.DueDate, GETUTCDATE()) BETWEEN 61 AND 90 THEN '61-90 Days Overdue'
        ELSE '>90 Days Critical'
    END AS AgingBucket,
    i.Subtotal,
    i.TaxAmount,
    i.TotalAmount,
    i.PaidAmount,
    i.OutstandingAmount,
    CASE i.Status
        WHEN 1 THEN 'PendingVerification'
        WHEN 2 THEN 'Matched'
        WHEN 3 THEN 'Mismatched'
        WHEN 4 THEN 'Approved'
        WHEN 5 THEN 'Rejected'
        WHEN 6 THEN 'PartiallyPaid'
        WHEN 7 THEN 'Paid'
        WHEN 8 THEN 'Cancelled'
        ELSE 'Unknown'
    END AS InvoiceStatus
FROM dbo.SupplierInvoices i
INNER JOIN dbo.Vendors v ON i.VendorId = v.VendorId
LEFT JOIN dbo.PurchaseOrders po ON i.POId = po.POId
LEFT JOIN dbo.GoodsReceipts grn ON i.GRNId = grn.GRNId;
GO

-- 4. View: Department Procurement Expenditure vs Budget
IF OBJECT_ID('dbo.vw_DepartmentExpenditure', 'V') IS NOT NULL DROP VIEW dbo.vw_DepartmentExpenditure;
GO
CREATE VIEW dbo.vw_DepartmentExpenditure AS
SELECT 
    d.DepartmentId,
    d.DepartmentCode,
    d.DepartmentName,
    d.ManagerName,
    d.Budget AS AnnualBudgetINR,
    COUNT(DISTINCT pr.RequisitionId) AS TotalRequisitionsRaised,
    ISNULL(SUM(pr.TotalEstimatedAmount), 0) AS TotalEstimatedPRValueINR,
    ISNULL(SUM(po.GrandTotal), 0) AS TotalCommittedPOSpendINR,
    (d.Budget - ISNULL(SUM(po.GrandTotal), 0)) AS RemainingBudgetINR,
    CASE 
        WHEN d.Budget > 0 THEN ROUND((ISNULL(SUM(po.GrandTotal), 0) / d.Budget) * 100, 2)
        ELSE 0
    END AS BudgetUtilizationPct
FROM dbo.Departments d
LEFT JOIN dbo.PurchaseRequisitions pr ON d.DepartmentId = pr.DepartmentId
LEFT JOIN dbo.PurchaseOrders po ON pr.RequisitionId = po.RequisitionId AND po.Status != 8 -- Exclude cancelled
GROUP BY d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.ManagerName, d.Budget;
GO

PRINT 'SmartProcureDb Power BI Analytical Views deployed successfully.';
GO
