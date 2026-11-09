using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.DTOs;

public class RequisitionItemDto
{
    public int ItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string UOMCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Remarks { get; set; }
}

public class RequisitionItemCreateDto
{
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public string? Remarks { get; set; }
}

public class RequisitionDto
{
    public int RequisitionId { get; set; }
    public string RequisitionNumber { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }
    public PriorityLevel Priority { get; set; }
    public string PriorityName => Priority.ToString();
    public decimal TotalEstimatedAmount { get; set; }
    public string BusinessJustification { get; set; } = string.Empty;
    public RequisitionStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? ApprovalRemarks { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public List<RequisitionItemDto> Items { get; set; } = new();
    public List<ApprovalHistoryDto> ApprovalHistories { get; set; } = new();
}

public class RequisitionCreateDto
{
    public int DepartmentId { get; set; }
    public DateTime RequiredDeliveryDate { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public string BusinessJustification { get; set; } = string.Empty;
    public bool SubmitImmediately { get; set; } = false;
    public List<RequisitionItemCreateDto> Items { get; set; } = new();
}

public class ApprovalActionDto
{
    public string Action { get; set; } = string.Empty; // Approve, Reject, RequestChanges
    public string Remarks { get; set; } = string.Empty;
}

public class ApprovalHistoryDto
{
    public int HistoryId { get; set; }
    public int ApproverUserId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
}

public class QuotationItemDto
{
    public int QuotationItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal QuotedUnitPrice { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal LineTotal { get; set; }
    public int DeliveryLeadDays { get; set; }
    public string? Notes { get; set; }
}

public class QuotationDto
{
    public int QuotationId { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public int RequisitionId { get; set; }
    public string RequisitionNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal VendorRating { get; set; }
    public DateTime QuotationDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public QuotationStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? SelectionJustification { get; set; }
    public decimal TotalQuotedAmount { get; set; }
    public string PaymentTerms { get; set; } = string.Empty;
    public int DeliveryDays { get; set; }
    public List<QuotationItemDto> Items { get; set; } = new();
}

public class QuotationCreateDto
{
    public int RequisitionId { get; set; }
    public int VendorId { get; set; }
    public DateTime ValidUntil { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public int DeliveryDays { get; set; } = 7;
    public List<QuotationItemCreateDto> Items { get; set; } = new();
}

public class QuotationItemCreateDto
{
    public int MaterialId { get; set; }
    public decimal QuotedUnitPrice { get; set; }
    public decimal TaxPercentage { get; set; } = 18m;
    public int DeliveryLeadDays { get; set; } = 7;
    public string? Notes { get; set; }
}

public class QuotationSelectDto
{
    public string SelectionJustification { get; set; } = string.Empty;
}

public class PurchaseOrderItemDto
{
    public int POItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string UOMCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineSubtotal { get; set; }
    public decimal LineTax { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal PendingQuantity => Math.Max(0, Quantity - ReceivedQuantity);
}

public class PurchaseOrderDto
{
    public int POId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string VendorGSTIN { get; set; } = string.Empty;
    public string VendorPAN { get; set; } = string.Empty;
    public string VendorAddress { get; set; } = string.Empty;
    public int? RequisitionId { get; set; }
    public string? RequisitionNumber { get; set; }
    public DateTime PODate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string PaymentTerms { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? CancellationReason { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderCreateDto
{
    public int? RequisitionId { get; set; }
    public int VendorId { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string PaymentTerms { get; set; } = "Net 30";
    public string ShippingAddress { get; set; } = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501";
    public string BillingAddress { get; set; } = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501";
    public int WarehouseId { get; set; } = 1;
    public decimal DiscountAmount { get; set; } = 0;
    public List<PurchaseOrderItemCreateDto> Items { get; set; } = new();
}

public class PurchaseOrderItemCreateDto
{
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } = 18m;
}
