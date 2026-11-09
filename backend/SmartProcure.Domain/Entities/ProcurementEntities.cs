using SmartProcure.Domain.Enums;

namespace SmartProcure.Domain.Entities;

public class PurchaseRequisition : BaseEntity
{
    public int RequisitionId { get; set; }
    public string RequisitionNumber { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public User? RequestedByUser { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime RequiredDeliveryDate { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public decimal TotalEstimatedAmount { get; set; }
    public string BusinessJustification { get; set; } = string.Empty;
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
    public string? ApprovalRemarks { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }

    public ICollection<PurchaseRequisitionItem> Items { get; set; } = new List<PurchaseRequisitionItem>();
    public ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();
    public ICollection<VendorQuotation> Quotations { get; set; } = new List<VendorQuotation>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class PurchaseRequisitionItem
{
    public int ItemId { get; set; }
    public int RequisitionId { get; set; }
    public PurchaseRequisition? Requisition { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Remarks { get; set; }
}

public class ApprovalHistory
{
    public int HistoryId { get; set; }
    public int RequisitionId { get; set; }
    public PurchaseRequisition? Requisition { get; set; }
    public int ApproverUserId { get; set; }
    public User? ApproverUser { get; set; }
    public string Action { get; set; } = string.Empty; // Submitted, Approved, Rejected, ChangesRequested
    public string Remarks { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}

public class VendorQuotation : BaseEntity
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public int RequisitionId { get; set; }
    public PurchaseRequisition? Requisition { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Submitted;
    public string? SelectionJustification { get; set; }
    public decimal TotalQuotedAmount { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public int DeliveryDays { get; set; } = 7;

    public ICollection<VendorQuotationItem> Items { get; set; } = new List<VendorQuotationItem>();
}

public class VendorQuotationItem
{
    public int QuotationItemId { get; set; }
    public int QuotationId { get; set; }
    public VendorQuotation? Quotation { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public decimal QuotedUnitPrice { get; set; }
    public decimal TaxPercentage { get; set; } = 18.0m;
    public decimal LineTotal { get; set; }
    public int DeliveryLeadDays { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseOrder : BaseEntity
{
    public int POId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public int? RequisitionId { get; set; }
    public PurchaseRequisition? Requisition { get; set; }
    public DateTime PODate { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public int CreatedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<SupplierInvoice> SupplierInvoices { get; set; } = new List<SupplierInvoice>();
}

public class PurchaseOrderItem
{
    public int POItemId { get; set; }
    public int POId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } = 18.0m;
    public decimal LineSubtotal { get; set; }
    public decimal LineTax { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReceivedQuantity { get; set; } = 0;
}
