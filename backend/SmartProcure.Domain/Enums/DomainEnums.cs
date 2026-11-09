namespace SmartProcure.Domain.Enums;

public enum UserRoleType
{
    Administrator = 1,
    RequisitionCreator = 2,
    PurchaseManager = 3,
    WarehouseManager = 4,
    FinanceManager = 5,
    ManagementViewer = 6
}

public enum PriorityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum RequisitionStatus
{
    Draft = 1,
    Submitted = 2,
    PendingApproval = 3,
    Approved = 4,
    Rejected = 5,
    ChangesRequested = 6,
    ConvertedToPO = 7,
    Cancelled = 8
}

public enum QuotationStatus
{
    Submitted = 1,
    UnderReview = 2,
    Selected = 3,
    Rejected = 4
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    SentToVendor = 4,
    PartiallyReceived = 5,
    FullyReceived = 6,
    Closed = 7,
    Cancelled = 8
}

public enum InspectionStatus
{
    Pending = 1,
    Passed = 2,
    PartiallyAccepted = 3,
    Rejected = 4
}

public enum InventoryTransactionType
{
    GoodsReceipt = 1,
    GoodsIssue = 2,
    StockAdjustment = 3,
    Return = 4,
    Transfer = 5
}

public enum InvoiceStatus
{
    PendingVerification = 1,
    Matched = 2,
    Mismatched = 3,
    Approved = 4,
    Rejected = 5,
    PartiallyPaid = 6,
    Paid = 7,
    Cancelled = 8
}

public enum PaymentMethod
{
    NEFT = 1,
    RTGS = 2,
    IMPS = 3,
    Cheque = 4,
    BankTransfer = 5
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}
