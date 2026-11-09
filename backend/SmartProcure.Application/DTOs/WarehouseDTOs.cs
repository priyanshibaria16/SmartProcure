using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.DTOs;

public class GoodsReceiptItemDto
{
    public int GRNItemId { get; set; }
    public int POItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string UOMCode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal PreviouslyReceivedQuantity { get; set; }
    public decimal CurrentReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? RejectionReason { get; set; }
}

public class GoodsReceiptDto
{
    public int GRNId { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public int POId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string DeliveryChallanNumber { get; set; } = string.Empty;
    public int ReceivedByUserId { get; set; }
    public string ReceivedByUserName { get; set; } = string.Empty;
    public InspectionStatus InspectionStatus { get; set; }
    public string InspectionStatusName => InspectionStatus.ToString();
    public string? Remarks { get; set; }
    public List<GoodsReceiptItemDto> Items { get; set; } = new();
}

public class GoodsReceiptCreateDto
{
    public int POId { get; set; }
    public int WarehouseId { get; set; }
    public string DeliveryChallanNumber { get; set; } = string.Empty;
    public InspectionStatus InspectionStatus { get; set; } = InspectionStatus.Passed;
    public string? Remarks { get; set; }
    public List<GoodsReceiptItemCreateDto> Items { get; set; } = new();
}

public class GoodsReceiptItemCreateDto
{
    public int POItemId { get; set; }
    public int MaterialId { get; set; }
    public decimal CurrentReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? RejectionReason { get; set; }
}

public class InventoryBalanceDto
{
    public int BalanceId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UOMCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal AvailableStock { get; set; }
    public decimal ReservedStock { get; set; }
    public decimal TotalStock => AvailableStock + ReservedStock;
    public decimal StockValue => TotalStock * UnitPrice;
    public decimal ReorderLevel { get; set; }
    public bool IsLowStock => AvailableStock <= ReorderLevel;
    public DateTime LastUpdated { get; set; }
}

public class InventoryTransactionDto
{
    public int TransactionId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public InventoryTransactionType TransactionType { get; set; }
    public string TransactionTypeName => TransactionType.ToString();
    public decimal Quantity { get; set; }
    public string ReferenceDocumentType { get; set; } = string.Empty;
    public string ReferenceDocumentNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public int CreatedByUserId { get; set; }
    public string? Remarks { get; set; }
}

public class StockAdjustmentDto
{
    public int MaterialId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; } // Positive to add, negative to deduct
    public string Remarks { get; set; } = string.Empty;
}
