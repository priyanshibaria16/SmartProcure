-- ==============================================================================
-- SMARTPROCURE ENTERPRISE ERP - DATABASE CREATION & SCHEMA SCRIPT
-- Engine: Microsoft SQL Server 2019 / 2022 / Azure SQL
-- Target Organization: Apex Manufacturing Industries Pvt. Ltd.
-- Author: Senior ERP Solution Architect & SAP MM Functional Consultant
-- ==============================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SmartProcureDb')
BEGIN
    CREATE DATABASE SmartProcureDb;
END
GO

USE SmartProcureDb;
GO

-- 1. Roles
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
CREATE TABLE dbo.Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- 2. Departments
IF OBJECT_ID('dbo.Departments', 'U') IS NULL
CREATE TABLE dbo.Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode NVARCHAR(20) NOT NULL UNIQUE,
    DepartmentName NVARCHAR(100) NOT NULL,
    ManagerName NVARCHAR(100) NULL,
    Budget DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- 3. Warehouses
IF OBJECT_ID('dbo.Warehouses', 'U') IS NULL
CREATE TABLE dbo.Warehouses (
    WarehouseId INT IDENTITY(1,1) PRIMARY KEY,
    WarehouseCode NVARCHAR(20) NOT NULL UNIQUE,
    WarehouseName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200) NULL,
    CapacitySqFt DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- 4. Users
IF OBJECT_ID('dbo.Users', 'U') IS NULL
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(25) NULL,
    DepartmentId INT NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Users_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

-- 5. MaterialCategories
IF OBJECT_ID('dbo.MaterialCategories', 'U') IS NULL
CREATE TABLE dbo.MaterialCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryCode NVARCHAR(20) NOT NULL UNIQUE,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL
);
GO

-- 6. UnitsOfMeasure
IF OBJECT_ID('dbo.UnitsOfMeasure', 'U') IS NULL
CREATE TABLE dbo.UnitsOfMeasure (
    UOMId INT IDENTITY(1,1) PRIMARY KEY,
    UOMCode NVARCHAR(10) NOT NULL UNIQUE,
    UOMName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NULL
);
GO

-- 7. Vendors
IF OBJECT_ID('dbo.Vendors', 'U') IS NULL
CREATE TABLE dbo.Vendors (
    VendorId INT IDENTITY(1,1) PRIMARY KEY,
    VendorCode NVARCHAR(20) NOT NULL UNIQUE,
    CompanyName NVARCHAR(150) NOT NULL,
    ContactPerson NVARCHAR(100) NULL,
    Email NVARCHAR(150) NULL,
    Phone NVARCHAR(25) NULL,
    Address NVARCHAR(255) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    Country NVARCHAR(100) NOT NULL DEFAULT N'India',
    PostalCode NVARCHAR(20) NULL,
    GSTIN NVARCHAR(20) NULL,
    PAN NVARCHAR(15) NULL,
    PaymentTerms NVARCHAR(50) NOT NULL DEFAULT N'Net 30',
    BankAccountRef NVARCHAR(100) NULL,
    BankName NVARCHAR(100) NULL,
    IFSCCode NVARCHAR(20) NULL,
    VendorCategory NVARCHAR(100) NULL,
    VendorRating DECIMAL(3,2) NOT NULL DEFAULT 4.00,
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- 8. Materials
IF OBJECT_ID('dbo.Materials', 'U') IS NULL
CREATE TABLE dbo.Materials (
    MaterialId INT IDENTITY(1,1) PRIMARY KEY,
    MaterialCode NVARCHAR(30) NOT NULL UNIQUE,
    MaterialName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(255) NULL,
    CategoryId INT NOT NULL,
    MaterialType NVARCHAR(50) NOT NULL DEFAULT N'Raw Material',
    BaseUOMId INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    ReorderLevel DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    MaximumStockLevel DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    CurrentStock DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    PreferredVendorId INT NULL,
    StorageLocation NVARCHAR(100) NULL DEFAULT N'Store A',
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Materials_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.MaterialCategories(CategoryId),
    CONSTRAINT FK_Materials_UOM FOREIGN KEY (BaseUOMId) REFERENCES dbo.UnitsOfMeasure(UOMId),
    CONSTRAINT FK_Materials_Vendors FOREIGN KEY (PreferredVendorId) REFERENCES dbo.Vendors(VendorId)
);
GO

-- 9. PurchaseRequisitions
IF OBJECT_ID('dbo.PurchaseRequisitions', 'U') IS NULL
CREATE TABLE dbo.PurchaseRequisitions (
    RequisitionId INT IDENTITY(1,1) PRIMARY KEY,
    RequisitionNumber NVARCHAR(30) NOT NULL UNIQUE,
    RequestedByUserId INT NOT NULL,
    DepartmentId INT NOT NULL,
    RequestDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    RequiredDeliveryDate DATETIME2 NOT NULL,
    Priority INT NOT NULL DEFAULT 2, -- Medium
    TotalEstimatedAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    BusinessJustification NVARCHAR(500) NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- Draft
    ApprovalRemarks NVARCHAR(500) NULL,
    ApprovedByUserId INT NULL,
    ApprovedDate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_PR_Users FOREIGN KEY (RequestedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_PR_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId)
);
GO

-- 10. PurchaseRequisitionItems
IF OBJECT_ID('dbo.PurchaseRequisitionItems', 'U') IS NULL
CREATE TABLE dbo.PurchaseRequisitionItems (
    ItemId INT IDENTITY(1,1) PRIMARY KEY,
    RequisitionId INT NOT NULL,
    MaterialId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    EstimatedUnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    Remarks NVARCHAR(255) NULL,
    CONSTRAINT FK_PRItems_PR FOREIGN KEY (RequisitionId) REFERENCES dbo.PurchaseRequisitions(RequisitionId) ON DELETE CASCADE,
    CONSTRAINT FK_PRItems_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId)
);
GO

-- 11. ApprovalHistories
IF OBJECT_ID('dbo.ApprovalHistories', 'U') IS NULL
CREATE TABLE dbo.ApprovalHistories (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    RequisitionId INT NOT NULL,
    ApproverUserId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    Remarks NVARCHAR(500) NULL,
    ActionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AppHist_PR FOREIGN KEY (RequisitionId) REFERENCES dbo.PurchaseRequisitions(RequisitionId) ON DELETE CASCADE,
    CONSTRAINT FK_AppHist_Users FOREIGN KEY (ApproverUserId) REFERENCES dbo.Users(UserId)
);
GO

-- 12. VendorQuotations
IF OBJECT_ID('dbo.VendorQuotations', 'U') IS NULL
CREATE TABLE dbo.VendorQuotations (
    QuotationId INT IDENTITY(1,1) PRIMARY KEY,
    QuotationNumber NVARCHAR(30) NOT NULL UNIQUE,
    RequisitionId INT NOT NULL,
    VendorId INT NOT NULL,
    QuotationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ValidUntil DATETIME2 NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- Submitted
    SelectionJustification NVARCHAR(500) NULL,
    TotalQuotedAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    PaymentTerms NVARCHAR(50) NOT NULL DEFAULT N'Net 30',
    DeliveryDays INT NOT NULL DEFAULT 7,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Quotations_PR FOREIGN KEY (RequisitionId) REFERENCES dbo.PurchaseRequisitions(RequisitionId),
    CONSTRAINT FK_Quotations_Vendors FOREIGN KEY (VendorId) REFERENCES dbo.Vendors(VendorId)
);
GO

-- 13. VendorQuotationItems
IF OBJECT_ID('dbo.VendorQuotationItems', 'U') IS NULL
CREATE TABLE dbo.VendorQuotationItems (
    QuotationItemId INT IDENTITY(1,1) PRIMARY KEY,
    QuotationId INT NOT NULL,
    MaterialId INT NOT NULL,
    QuotedUnitPrice DECIMAL(18,2) NOT NULL,
    TaxPercentage DECIMAL(5,2) NOT NULL DEFAULT 18.00,
    LineTotal DECIMAL(18,2) NOT NULL,
    DeliveryLeadDays INT NOT NULL DEFAULT 7,
    Notes NVARCHAR(255) NULL,
    CONSTRAINT FK_QItems_Quotation FOREIGN KEY (QuotationId) REFERENCES dbo.VendorQuotations(QuotationId) ON DELETE CASCADE,
    CONSTRAINT FK_QItems_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId)
);
GO

-- 14. PurchaseOrders
IF OBJECT_ID('dbo.PurchaseOrders', 'U') IS NULL
CREATE TABLE dbo.PurchaseOrders (
    POId INT IDENTITY(1,1) PRIMARY KEY,
    PONumber NVARCHAR(30) NOT NULL UNIQUE,
    VendorId INT NOT NULL,
    RequisitionId INT NULL,
    PODate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpectedDeliveryDate DATETIME2 NOT NULL,
    PaymentTerms NVARCHAR(50) NOT NULL DEFAULT N'Net 30',
    ShippingAddress NVARCHAR(255) NOT NULL,
    BillingAddress NVARCHAR(255) NOT NULL,
    WarehouseId INT NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    GrandTotal DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Status INT NOT NULL DEFAULT 1, -- Draft
    CreatedByUserId INT NOT NULL,
    ApprovedByUserId INT NULL,
    ApprovedDate DATETIME2 NULL,
    CancellationReason NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_PO_Vendors FOREIGN KEY (VendorId) REFERENCES dbo.Vendors(VendorId),
    CONSTRAINT FK_PO_PR FOREIGN KEY (RequisitionId) REFERENCES dbo.PurchaseRequisitions(RequisitionId),
    CONSTRAINT FK_PO_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId)
);
GO

-- 15. PurchaseOrderItems
IF OBJECT_ID('dbo.PurchaseOrderItems', 'U') IS NULL
CREATE TABLE dbo.PurchaseOrderItems (
    POItemId INT IDENTITY(1,1) PRIMARY KEY,
    POId INT NOT NULL,
    MaterialId INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TaxRate DECIMAL(5,2) NOT NULL DEFAULT 18.00,
    LineSubtotal DECIMAL(18,2) NOT NULL,
    LineTax DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    ReceivedQuantity DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    CONSTRAINT FK_POItems_PO FOREIGN KEY (POId) REFERENCES dbo.PurchaseOrders(POId) ON DELETE CASCADE,
    CONSTRAINT FK_POItems_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId)
);
GO

-- 16. GoodsReceipts
IF OBJECT_ID('dbo.GoodsReceipts', 'U') IS NULL
CREATE TABLE dbo.GoodsReceipts (
    GRNId INT IDENTITY(1,1) PRIMARY KEY,
    GRNNumber NVARCHAR(30) NOT NULL UNIQUE,
    POId INT NOT NULL,
    VendorId INT NOT NULL,
    WarehouseId INT NOT NULL,
    ReceiptDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DeliveryChallanNumber NVARCHAR(50) NULL,
    ReceivedByUserId INT NOT NULL,
    InspectionStatus INT NOT NULL DEFAULT 1, -- Pending
    Remarks NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_GRN_PO FOREIGN KEY (POId) REFERENCES dbo.PurchaseOrders(POId),
    CONSTRAINT FK_GRN_Vendors FOREIGN KEY (VendorId) REFERENCES dbo.Vendors(VendorId),
    CONSTRAINT FK_GRN_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId),
    CONSTRAINT FK_GRN_Users FOREIGN KEY (ReceivedByUserId) REFERENCES dbo.Users(UserId)
);
GO

-- 17. GoodsReceiptItems
IF OBJECT_ID('dbo.GoodsReceiptItems', 'U') IS NULL
CREATE TABLE dbo.GoodsReceiptItems (
    GRNItemId INT IDENTITY(1,1) PRIMARY KEY,
    GRNId INT NOT NULL,
    POItemId INT NOT NULL,
    MaterialId INT NOT NULL,
    OrderedQuantity DECIMAL(18,3) NOT NULL,
    PreviouslyReceivedQuantity DECIMAL(18,3) NOT NULL,
    CurrentReceivedQuantity DECIMAL(18,3) NOT NULL,
    AcceptedQuantity DECIMAL(18,3) NOT NULL,
    RejectedQuantity DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    RejectionReason NVARCHAR(255) NULL,
    CONSTRAINT FK_GRNItems_GRN FOREIGN KEY (GRNId) REFERENCES dbo.GoodsReceipts(GRNId) ON DELETE CASCADE,
    CONSTRAINT FK_GRNItems_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId)
);
GO

-- 18. InventoryBalances
IF OBJECT_ID('dbo.InventoryBalances', 'U') IS NULL
CREATE TABLE dbo.InventoryBalances (
    BalanceId INT IDENTITY(1,1) PRIMARY KEY,
    MaterialId INT NOT NULL,
    WarehouseId INT NOT NULL,
    AvailableStock DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    ReservedStock DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Material_Warehouse UNIQUE (MaterialId, WarehouseId),
    CONSTRAINT FK_InvBal_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId),
    CONSTRAINT FK_InvBal_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId)
);
GO

-- 19. InventoryTransactions
IF OBJECT_ID('dbo.InventoryTransactions', 'U') IS NULL
CREATE TABLE dbo.InventoryTransactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    MaterialId INT NOT NULL,
    WarehouseId INT NOT NULL,
    TransactionType INT NOT NULL,
    Quantity DECIMAL(18,3) NOT NULL,
    ReferenceDocumentType NVARCHAR(50) NOT NULL,
    ReferenceDocumentNumber NVARCHAR(50) NOT NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId INT NOT NULL,
    Remarks NVARCHAR(255) NULL,
    CONSTRAINT FK_InvTx_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId),
    CONSTRAINT FK_InvTx_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId)
);
GO

-- 20. SupplierInvoices
IF OBJECT_ID('dbo.SupplierInvoices', 'U') IS NULL
CREATE TABLE dbo.SupplierInvoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    VendorId INT NOT NULL,
    POId INT NOT NULL,
    GRNId INT NULL,
    InvoiceDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    OutstandingAmount DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Status INT NOT NULL DEFAULT 1, -- PendingVerification
    VerificationRemarks NVARCHAR(500) NULL,
    VerifiedByUserId INT NULL,
    VerifiedDate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_Vendor_InvoiceNumber UNIQUE (VendorId, InvoiceNumber),
    CONSTRAINT FK_Invoices_Vendors FOREIGN KEY (VendorId) REFERENCES dbo.Vendors(VendorId),
    CONSTRAINT FK_Invoices_PO FOREIGN KEY (POId) REFERENCES dbo.PurchaseOrders(POId),
    CONSTRAINT FK_Invoices_GRN FOREIGN KEY (GRNId) REFERENCES dbo.GoodsReceipts(GRNId)
);
GO

-- 21. SupplierInvoiceItems
IF OBJECT_ID('dbo.SupplierInvoiceItems', 'U') IS NULL
CREATE TABLE dbo.SupplierInvoiceItems (
    InvoiceItemId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    MaterialId INT NOT NULL,
    BilledQuantity DECIMAL(18,3) NOT NULL,
    BilledUnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_InvItems_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.SupplierInvoices(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_InvItems_Materials FOREIGN KEY (MaterialId) REFERENCES dbo.Materials(MaterialId)
);
GO

-- 22. InvoiceVerifications
IF OBJECT_ID('dbo.InvoiceVerifications', 'U') IS NULL
CREATE TABLE dbo.InvoiceVerifications (
    VerificationId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    POId INT NOT NULL,
    GRNId INT NULL,
    MatchedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PriceVariance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    QuantityVariance DECIMAL(18,3) NOT NULL DEFAULT 0.000,
    VarianceType NVARCHAR(50) NOT NULL DEFAULT N'None',
    IsMatchSuccessful BIT NOT NULL DEFAULT 0,
    Remarks NVARCHAR(500) NOT NULL,
    VerifiedByUserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Verifications_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.SupplierInvoices(InvoiceId) ON DELETE CASCADE
);
GO

-- 23. Payments
IF OBJECT_ID('dbo.Payments', 'U') IS NULL
CREATE TABLE dbo.Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    PaymentNumber NVARCHAR(30) NOT NULL UNIQUE,
    InvoiceId INT NOT NULL,
    VendorId INT NOT NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PaymentAmount DECIMAL(18,2) NOT NULL,
    PaymentMethod INT NOT NULL DEFAULT 1, -- NEFT
    TransactionReference NVARCHAR(100) NOT NULL,
    Status INT NOT NULL DEFAULT 2, -- Completed
    RecordedByUserId INT NOT NULL,
    Remarks NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES dbo.SupplierInvoices(InvoiceId),
    CONSTRAINT FK_Payments_Vendors FOREIGN KEY (VendorId) REFERENCES dbo.Vendors(VendorId)
);
GO

-- 24. AuditLogs
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
CREATE TABLE dbo.AuditLogs (
    AuditId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityName NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(50) NULL,
    PreviousValuesJson NVARCHAR(MAX) NULL,
    NewValuesJson NVARCHAR(MAX) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IPAddress NVARCHAR(50) NULL
);
GO

-- 25. Notifications
IF OBJECT_ID('dbo.Notifications', 'U') IS NULL
CREATE TABLE dbo.Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(150) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(30) NOT NULL DEFAULT N'Info',
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    LinkUrl NVARCHAR(255) NULL
);
GO

-- 26. SystemSettings
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NULL
CREATE TABLE dbo.SystemSettings (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(500) NOT NULL,
    Description NVARCHAR(255) NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

PRINT 'SmartProcureDb Enterprise Schema created successfully.';
GO
