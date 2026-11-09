using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Entities;
using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IApplicationDbContext _context;

    public InventoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<GoodsReceiptDto>>> GetGoodsReceiptsAsync(int? poId, int page, int pageSize)
    {
        var query = _context.GoodsReceipts
            .Include(g => g.PurchaseOrder)
            .Include(g => g.Vendor)
            .Include(g => g.Warehouse)
            .Include(g => g.ReceivedByUser)
            .Include(g => g.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .AsNoTracking()
            .AsQueryable();

        if (poId.HasValue)
        {
            query = query.Where(g => g.POId == poId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(g => g.ReceiptDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => MapToGoodsReceiptDto(g))
            .ToListAsync();

        return ApiResponse<PagedResult<GoodsReceiptDto>>.Ok(new PagedResult<GoodsReceiptDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<GoodsReceiptDto>> GetGoodsReceiptByIdAsync(int id)
    {
        var g = await _context.GoodsReceipts
            .Include(x => x.PurchaseOrder)
            .Include(x => x.Vendor)
            .Include(x => x.Warehouse)
            .Include(x => x.ReceivedByUser)
            .Include(x => x.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GRNId == id);

        if (g == null) return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt note not found");

        return ApiResponse<GoodsReceiptDto>.Ok(MapToGoodsReceiptDto(g));
    }

    public async Task<ApiResponse<List<PurchaseOrderDto>>> GetPendingPOsForReceivingAsync()
    {
        // Eligible PO statuses: Approved, SentToVendor, PartiallyReceived
        var eligibleStatuses = new[]
        {
            PurchaseOrderStatus.Approved,
            PurchaseOrderStatus.SentToVendor,
            PurchaseOrderStatus.PartiallyReceived
        };

        var list = await _context.PurchaseOrders
            .Include(p => p.Vendor)
            .Include(p => p.Warehouse)
            .Include(p => p.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .Where(p => eligibleStatuses.Contains(p.Status) && p.Items.Any(i => i.ReceivedQuantity < i.Quantity))
            .OrderByDescending(p => p.PODate)
            .Select(p => new PurchaseOrderDto
            {
                POId = p.POId,
                PONumber = p.PONumber,
                VendorId = p.VendorId,
                VendorName = p.Vendor != null ? p.Vendor.CompanyName : string.Empty,
                WarehouseId = p.WarehouseId,
                WarehouseName = p.Warehouse != null ? p.Warehouse.WarehouseName : string.Empty,
                PODate = p.PODate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                Status = p.Status,
                GrandTotal = p.GrandTotal,
                Items = p.Items.Select(i => new PurchaseOrderItemDto
                {
                    POItemId = i.POItemId,
                    MaterialId = i.MaterialId,
                    MaterialCode = i.Material != null ? i.Material.MaterialCode : string.Empty,
                    MaterialName = i.Material != null ? i.Material.MaterialName : string.Empty,
                    UOMCode = i.Material != null && i.Material.BaseUOM != null ? i.Material.BaseUOM.UOMCode : string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ReceivedQuantity = i.ReceivedQuantity
                }).ToList()
            })
            .ToListAsync();

        return ApiResponse<List<PurchaseOrderDto>>.Ok(list);
    }

    public async Task<ApiResponse<GoodsReceiptDto>> CreateGoodsReceiptAsync(int userId, GoodsReceiptCreateDto dto)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.POId == dto.POId);

        if (po == null) return ApiResponse<GoodsReceiptDto>.Fail("Purchase order not found");

        if (po.Status == PurchaseOrderStatus.Cancelled || po.Status == PurchaseOrderStatus.Closed)
        {
            return ApiResponse<GoodsReceiptDto>.Fail($"Cannot receive goods against a PO with status '{po.Status}'");
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ApiResponse<GoodsReceiptDto>.Fail("Goods receipt must contain at least one item");
        }

        // Validate quantities across all items before opening transaction
        foreach (var itemDto in dto.Items)
        {
            var poItem = po.Items.FirstOrDefault(i => i.POItemId == itemDto.POItemId);
            if (poItem == null)
            {
                return ApiResponse<GoodsReceiptDto>.Fail($"PO line item {itemDto.POItemId} not found in PO {po.PONumber}");
            }

            decimal remainingOpen = poItem.Quantity - poItem.ReceivedQuantity;

            if (itemDto.CurrentReceivedQuantity <= 0)
            {
                return ApiResponse<GoodsReceiptDto>.Fail("Received quantity must be greater than zero");
            }

            if (itemDto.CurrentReceivedQuantity > remainingOpen)
            {
                return ApiResponse<GoodsReceiptDto>.Fail($"Over-receipt prevented: Received quantity {itemDto.CurrentReceivedQuantity} exceeds remaining open PO balance of {remainingOpen}");
            }

            if (itemDto.AcceptedQuantity + itemDto.RejectedQuantity != itemDto.CurrentReceivedQuantity)
            {
                return ApiResponse<GoodsReceiptDto>.Fail("Accepted quantity plus rejected quantity must equal current received quantity");
            }

            if (itemDto.RejectedQuantity > 0 && string.IsNullOrWhiteSpace(itemDto.RejectionReason))
            {
                return ApiResponse<GoodsReceiptDto>.Fail("Rejection reason is required when rejected quantity is greater than zero");
            }
        }

        // Execute atomic database transaction
        using var transaction = await _context.BeginTransactionAsync();
        try
        {
            var count = await _context.GoodsReceipts.CountAsync() + 1;
            var grnNumber = $"GRN-{DateTime.UtcNow.Year}-{count:D4}";

            var grn = new GoodsReceipt
            {
                GRNNumber = grnNumber,
                POId = po.POId,
                VendorId = po.VendorId,
                WarehouseId = dto.WarehouseId,
                ReceiptDate = DateTime.UtcNow,
                DeliveryChallanNumber = dto.DeliveryChallanNumber.Trim(),
                ReceivedByUserId = userId,
                InspectionStatus = dto.InspectionStatus,
                Remarks = dto.Remarks?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            foreach (var itemDto in dto.Items)
            {
                var poItem = po.Items.First(i => i.POItemId == itemDto.POItemId);

                // 1. Add GRN line item
                grn.Items.Add(new GoodsReceiptItem
                {
                    POItemId = poItem.POItemId,
                    MaterialId = itemDto.MaterialId,
                    OrderedQuantity = poItem.Quantity,
                    PreviouslyReceivedQuantity = poItem.ReceivedQuantity,
                    CurrentReceivedQuantity = itemDto.CurrentReceivedQuantity,
                    AcceptedQuantity = itemDto.AcceptedQuantity,
                    RejectedQuantity = itemDto.RejectedQuantity,
                    RejectionReason = itemDto.RejectionReason?.Trim()
                });

                // 2. Update PO line item received quantity (accepted + rejected both arrive physically at dock)
                poItem.ReceivedQuantity += itemDto.CurrentReceivedQuantity;

                // 3. Atomically update inventory balance (for ACCEPTED quantity only!)
                if (itemDto.AcceptedQuantity > 0)
                {
                    var balance = await _context.InventoryBalances
                        .FirstOrDefaultAsync(b => b.MaterialId == itemDto.MaterialId && b.WarehouseId == dto.WarehouseId);

                    if (balance == null)
                    {
                        balance = new InventoryBalance
                        {
                            MaterialId = itemDto.MaterialId,
                            WarehouseId = dto.WarehouseId,
                            AvailableStock = itemDto.AcceptedQuantity,
                            ReservedStock = 0,
                            LastUpdated = DateTime.UtcNow
                        };
                        await _context.InventoryBalances.AddAsync(balance);
                    }
                    else
                    {
                        balance.AvailableStock += itemDto.AcceptedQuantity;
                        balance.LastUpdated = DateTime.UtcNow;
                    }

                    // 4. Update material global stock
                    var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == itemDto.MaterialId);
                    if (material != null)
                    {
                        material.CurrentStock += itemDto.AcceptedQuantity;
                        material.UpdatedAt = DateTime.UtcNow;
                    }

                    // 5. Write immutable inventory transaction record
                    await _context.InventoryTransactions.AddAsync(new InventoryTransaction
                    {
                        MaterialId = itemDto.MaterialId,
                        WarehouseId = dto.WarehouseId,
                        TransactionType = InventoryTransactionType.GoodsReceipt,
                        Quantity = itemDto.AcceptedQuantity,
                        ReferenceDocumentType = "GoodsReceipt",
                        ReferenceDocumentNumber = grnNumber,
                        TransactionDate = DateTime.UtcNow,
                        CreatedByUserId = userId,
                        Remarks = $"GRN {grnNumber} against PO {po.PONumber}. Accepted: {itemDto.AcceptedQuantity}"
                    });
                }
            }

            // 6. Update PO status: check if all items are fully received
            bool allFullyReceived = po.Items.All(i => i.ReceivedQuantity >= i.Quantity);
            po.Status = allFullyReceived ? PurchaseOrderStatus.FullyReceived : PurchaseOrderStatus.PartiallyReceived;
            po.UpdatedAt = DateTime.UtcNow;

            await _context.GoodsReceipts.AddAsync(grn);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetGoodsReceiptByIdAsync(grn.GRNId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<GoodsReceiptDto>.Fail($"Failed to process goods receipt: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<InventoryBalanceDto>>> GetStockBalancesAsync(int? warehouseId, int? categoryId, bool? lowStockOnly, int page, int pageSize)
    {
        var query = _context.InventoryBalances
            .Include(b => b.Material)
                .ThenInclude(m => m!.Category)
            .Include(b => b.Material)
                .ThenInclude(m => m!.BaseUOM)
            .Include(b => b.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(b => b.WarehouseId == warehouseId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(b => b.Material != null && b.Material.CategoryId == categoryId.Value);
        }

        if (lowStockOnly == true)
        {
            query = query.Where(b => b.Material != null && b.AvailableStock <= b.Material.ReorderLevel);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Material!.MaterialCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new InventoryBalanceDto
            {
                BalanceId = b.BalanceId,
                MaterialId = b.MaterialId,
                MaterialCode = b.Material != null ? b.Material.MaterialCode : string.Empty,
                MaterialName = b.Material != null ? b.Material.MaterialName : string.Empty,
                CategoryName = b.Material != null && b.Material.Category != null ? b.Material.Category.CategoryName : string.Empty,
                UOMCode = b.Material != null && b.Material.BaseUOM != null ? b.Material.BaseUOM.UOMCode : string.Empty,
                UnitPrice = b.Material != null ? b.Material.UnitPrice : 0,
                WarehouseId = b.WarehouseId,
                WarehouseName = b.Warehouse != null ? b.Warehouse.WarehouseName : string.Empty,
                AvailableStock = b.AvailableStock,
                ReservedStock = b.ReservedStock,
                ReorderLevel = b.Material != null ? b.Material.ReorderLevel : 0,
                LastUpdated = b.LastUpdated
            })
            .ToListAsync();

        return ApiResponse<PagedResult<InventoryBalanceDto>>.Ok(new PagedResult<InventoryBalanceDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<PagedResult<InventoryTransactionDto>>> GetInventoryTransactionsAsync(int? materialId, int page, int pageSize)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Material)
            .Include(t => t.Warehouse)
            .AsNoTracking()
            .AsQueryable();

        if (materialId.HasValue)
        {
            query = query.Where(t => t.MaterialId == materialId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new InventoryTransactionDto
            {
                TransactionId = t.TransactionId,
                MaterialId = t.MaterialId,
                MaterialName = t.Material != null ? t.Material.MaterialName : string.Empty,
                WarehouseId = t.WarehouseId,
                WarehouseName = t.Warehouse != null ? t.Warehouse.WarehouseName : string.Empty,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                ReferenceDocumentType = t.ReferenceDocumentType,
                ReferenceDocumentNumber = t.ReferenceDocumentNumber,
                TransactionDate = t.TransactionDate,
                CreatedByUserId = t.CreatedByUserId,
                Remarks = t.Remarks
            })
            .ToListAsync();

        return ApiResponse<PagedResult<InventoryTransactionDto>>.Ok(new PagedResult<InventoryTransactionDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<bool>> RecordStockAdjustmentAsync(int userId, StockAdjustmentDto dto)
    {
        var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == dto.MaterialId);
        if (material == null) return ApiResponse<bool>.Fail("Material not found");

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == dto.WarehouseId);
        if (warehouse == null) return ApiResponse<bool>.Fail("Warehouse not found");

        var balance = await _context.InventoryBalances
            .FirstOrDefaultAsync(b => b.MaterialId == dto.MaterialId && b.WarehouseId == dto.WarehouseId);

        if (balance == null)
        {
            balance = new InventoryBalance
            {
                MaterialId = dto.MaterialId,
                WarehouseId = dto.WarehouseId,
                AvailableStock = 0,
                ReservedStock = 0,
                LastUpdated = DateTime.UtcNow
            };
            await _context.InventoryBalances.AddAsync(balance);
        }

        if (balance.AvailableStock + dto.Quantity < 0)
        {
            return ApiResponse<bool>.Fail($"Stock adjustment would result in negative available inventory ({balance.AvailableStock + dto.Quantity})");
        }

        balance.AvailableStock += dto.Quantity;
        balance.LastUpdated = DateTime.UtcNow;

        material.CurrentStock += dto.Quantity;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.InventoryTransactions.AddAsync(new InventoryTransaction
        {
            MaterialId = dto.MaterialId,
            WarehouseId = dto.WarehouseId,
            TransactionType = InventoryTransactionType.StockAdjustment,
            Quantity = dto.Quantity,
            ReferenceDocumentType = "Adjustment",
            ReferenceDocumentNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
            TransactionDate = DateTime.UtcNow,
            CreatedByUserId = userId,
            Remarks = dto.Remarks.Trim()
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Stock adjustment recorded successfully");
    }

    private static GoodsReceiptDto MapToGoodsReceiptDto(GoodsReceipt g) => new()
    {
        GRNId = g.GRNId,
        GRNNumber = g.GRNNumber,
        POId = g.POId,
        PONumber = g.PurchaseOrder?.PONumber ?? string.Empty,
        VendorId = g.VendorId,
        VendorName = g.Vendor?.CompanyName ?? string.Empty,
        WarehouseId = g.WarehouseId,
        WarehouseName = g.Warehouse?.WarehouseName ?? string.Empty,
        ReceiptDate = g.ReceiptDate,
        DeliveryChallanNumber = g.DeliveryChallanNumber,
        ReceivedByUserId = g.ReceivedByUserId,
        ReceivedByUserName = g.ReceivedByUser?.FullName ?? string.Empty,
        InspectionStatus = g.InspectionStatus,
        Remarks = g.Remarks,
        Items = g.Items.Select(i => new GoodsReceiptItemDto
        {
            GRNItemId = i.GRNItemId,
            POItemId = i.POItemId,
            MaterialId = i.MaterialId,
            MaterialCode = i.Material?.MaterialCode ?? string.Empty,
            MaterialName = i.Material?.MaterialName ?? string.Empty,
            UOMCode = i.Material?.BaseUOM?.UOMCode ?? string.Empty,
            OrderedQuantity = i.OrderedQuantity,
            PreviouslyReceivedQuantity = i.PreviouslyReceivedQuantity,
            CurrentReceivedQuantity = i.CurrentReceivedQuantity,
            AcceptedQuantity = i.AcceptedQuantity,
            RejectedQuantity = i.RejectedQuantity,
            RejectionReason = i.RejectionReason
        }).ToList()
    };
}
