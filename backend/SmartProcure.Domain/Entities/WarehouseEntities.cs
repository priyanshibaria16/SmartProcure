using SmartProcure.Domain.Enums;

namespace SmartProcure.Domain.Entities;

public class GoodsReceipt : BaseEntity
{
    public int GRNId { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public int POId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    public string DeliveryChallanNumber { get; set; } = string.Empty;
    public int ReceivedByUserId { get; set; }
    public User? ReceivedByUser { get; set; }
    public InspectionStatus InspectionStatus { get; set; } = InspectionStatus.Pending;
    public string? Remarks { get; set; }

    public ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
    public ICollection<SupplierInvoice> SupplierInvoices { get; set; } = new List<SupplierInvoice>();
}

public class GoodsReceiptItem
{
    public int GRNItemId { get; set; }
    public int GRNId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }
    public int POItemId { get; set; }
    public PurchaseOrderItem? PurchaseOrderItem { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal PreviouslyReceivedQuantity { get; set; }
    public decimal CurrentReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? RejectionReason { get; set; }
}

public class InventoryBalance
{
    public int BalanceId { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal AvailableStock { get; set; }
    public decimal ReservedStock { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class InventoryTransaction
{
    public int TransactionId { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public string ReferenceDocumentType { get; set; } = string.Empty; // GoodsReceipt, GoodsIssue, Adjustment
    public string ReferenceDocumentNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public string? Remarks { get; set; }
}
