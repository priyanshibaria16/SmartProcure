using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Entities;
using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.Services;

public class ProcurementService : IProcurementService
{
    private readonly IApplicationDbContext _context;

    public ProcurementService(IApplicationDbContext context)
    {
        _context = context;
    }

    // --- Purchase Requisitions ---

    public async Task<ApiResponse<PagedResult<RequisitionDto>>> GetRequisitionsAsync(RequisitionStatus? status, int? departmentId, int page, int pageSize)
    {
        var query = _context.PurchaseRequisitions
            .Include(r => r.RequestedByUser)
            .Include(r => r.Department)
            .Include(r => r.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .Include(r => r.ApprovalHistories)
                .ThenInclude(h => h.ApproverUser)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(r => r.DepartmentId == departmentId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => MapToRequisitionDto(r))
            .ToListAsync();

        return ApiResponse<PagedResult<RequisitionDto>>.Ok(new PagedResult<RequisitionDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<RequisitionDto>> GetRequisitionByIdAsync(int id)
    {
        var r = await _context.PurchaseRequisitions
            .Include(x => x.RequestedByUser)
            .Include(x => x.Department)
            .Include(x => x.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .Include(x => x.ApprovalHistories)
                .ThenInclude(h => h.ApproverUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequisitionId == id);

        if (r == null) return ApiResponse<RequisitionDto>.Fail("Purchase requisition not found");

        return ApiResponse<RequisitionDto>.Ok(MapToRequisitionDto(r));
    }

    public async Task<ApiResponse<RequisitionDto>> CreateRequisitionAsync(int userId, RequisitionCreateDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
        {
            return ApiResponse<RequisitionDto>.Fail("Requisition must contain at least one material item");
        }

        if (dto.RequiredDeliveryDate.Date < DateTime.UtcNow.Date)
        {
            return ApiResponse<RequisitionDto>.Fail("Required delivery date cannot be in the past");
        }

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ApiResponse<RequisitionDto>.Fail("Item quantity must be greater than zero");
            if (item.EstimatedUnitPrice < 0)
                return ApiResponse<RequisitionDto>.Fail("Estimated unit price cannot be negative");
        }

        // Generate unique PR Number (e.g. PR-2026-0013)
        var count = await _context.PurchaseRequisitions.CountAsync() + 1;
        var prNumber = $"PR-{DateTime.UtcNow.Year}-{count:D4}";

        decimal totalAmount = dto.Items.Sum(i => i.Quantity * i.EstimatedUnitPrice);

        var pr = new PurchaseRequisition
        {
            RequisitionNumber = prNumber,
            RequestedByUserId = userId,
            DepartmentId = dto.DepartmentId,
            RequestDate = DateTime.UtcNow,
            RequiredDeliveryDate = dto.RequiredDeliveryDate,
            Priority = dto.Priority,
            TotalEstimatedAmount = totalAmount,
            BusinessJustification = dto.BusinessJustification.Trim(),
            Status = dto.SubmitImmediately ? RequisitionStatus.PendingApproval : RequisitionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            pr.Items.Add(new PurchaseRequisitionItem
            {
                MaterialId = itemDto.MaterialId,
                Quantity = itemDto.Quantity,
                EstimatedUnitPrice = itemDto.EstimatedUnitPrice,
                LineTotal = itemDto.Quantity * itemDto.EstimatedUnitPrice,
                Remarks = itemDto.Remarks
            });
        }

        if (dto.SubmitImmediately)
        {
            pr.ApprovalHistories.Add(new ApprovalHistory
            {
                ApproverUserId = userId,
                Action = "Submitted",
                Remarks = "Requisition submitted for approval",
                ActionDate = DateTime.UtcNow
            });
        }

        await _context.PurchaseRequisitions.AddAsync(pr);
        await _context.SaveChangesAsync();

        return await GetRequisitionByIdAsync(pr.RequisitionId);
    }

    public async Task<ApiResponse<bool>> SubmitRequisitionAsync(int id, int userId)
    {
        var pr = await _context.PurchaseRequisitions
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.RequisitionId == id);

        if (pr == null) return ApiResponse<bool>.Fail("Requisition not found");

        if (pr.Status != RequisitionStatus.Draft && pr.Status != RequisitionStatus.ChangesRequested)
        {
            return ApiResponse<bool>.Fail($"Requisition in status '{pr.Status}' cannot be submitted");
        }

        if (!pr.Items.Any())
        {
            return ApiResponse<bool>.Fail("Requisition must contain at least one item before submission");
        }

        if (pr.RequiredDeliveryDate.Date < DateTime.UtcNow.Date)
        {
            return ApiResponse<bool>.Fail("Required delivery date cannot be earlier than current date");
        }

        pr.Status = RequisitionStatus.PendingApproval;
        pr.UpdatedAt = DateTime.UtcNow;

        await _context.ApprovalHistories.AddAsync(new ApprovalHistory
        {
            RequisitionId = pr.RequisitionId,
            ApproverUserId = userId,
            Action = "Submitted",
            Remarks = "Submitted for management review and approval",
            ActionDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Purchase requisition submitted successfully for approval");
    }

    public async Task<ApiResponse<bool>> ApproveRequisitionAsync(int id, int approverUserId, ApprovalActionDto dto)
    {
        var pr = await _context.PurchaseRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == id);
        if (pr == null) return ApiResponse<bool>.Fail("Requisition not found");

        if (pr.Status != RequisitionStatus.PendingApproval && pr.Status != RequisitionStatus.Submitted)
        {
            return ApiResponse<bool>.Fail($"Requisition in status '{pr.Status}' is not eligible for approval");
        }

        pr.Status = RequisitionStatus.Approved;
        pr.ApprovalRemarks = dto.Remarks.Trim();
        pr.ApprovedByUserId = approverUserId;
        pr.ApprovedDate = DateTime.UtcNow;
        pr.UpdatedAt = DateTime.UtcNow;

        await _context.ApprovalHistories.AddAsync(new ApprovalHistory
        {
            RequisitionId = pr.RequisitionId,
            ApproverUserId = approverUserId,
            Action = "Approved",
            Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Requisition approved" : dto.Remarks.Trim(),
            ActionDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Purchase requisition approved successfully");
    }

    public async Task<ApiResponse<bool>> RejectRequisitionAsync(int id, int approverUserId, ApprovalActionDto dto)
    {
        var pr = await _context.PurchaseRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == id);
        if (pr == null) return ApiResponse<bool>.Fail("Requisition not found");

        if (pr.Status != RequisitionStatus.PendingApproval && pr.Status != RequisitionStatus.Submitted)
        {
            return ApiResponse<bool>.Fail($"Requisition in status '{pr.Status}' cannot be rejected");
        }

        if (string.IsNullOrWhiteSpace(dto.Remarks))
        {
            return ApiResponse<bool>.Fail("Rejection remarks are mandatory");
        }

        pr.Status = RequisitionStatus.Rejected;
        pr.ApprovalRemarks = dto.Remarks.Trim();
        pr.ApprovedByUserId = approverUserId;
        pr.ApprovedDate = DateTime.UtcNow;
        pr.UpdatedAt = DateTime.UtcNow;

        await _context.ApprovalHistories.AddAsync(new ApprovalHistory
        {
            RequisitionId = pr.RequisitionId,
            ApproverUserId = approverUserId,
            Action = "Rejected",
            Remarks = dto.Remarks.Trim(),
            ActionDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Purchase requisition rejected");
    }

    public async Task<ApiResponse<bool>> CancelRequisitionAsync(int id, int userId)
    {
        var pr = await _context.PurchaseRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == id);
        if (pr == null) return ApiResponse<bool>.Fail("Requisition not found");

        if (pr.Status == RequisitionStatus.ConvertedToPO)
        {
            return ApiResponse<bool>.Fail("Requisitions already converted to a Purchase Order cannot be cancelled");
        }

        pr.Status = RequisitionStatus.Cancelled;
        pr.UpdatedAt = DateTime.UtcNow;

        await _context.ApprovalHistories.AddAsync(new ApprovalHistory
        {
            RequisitionId = pr.RequisitionId,
            ApproverUserId = userId,
            Action = "Cancelled",
            Remarks = "Requisition cancelled by requester",
            ActionDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Purchase requisition cancelled");
    }

    // --- Vendor Quotations ---

    public async Task<ApiResponse<List<QuotationDto>>> GetQuotationsByPRAsync(int requisitionId)
    {
        var list = await _context.VendorQuotations
            .Include(q => q.Vendor)
            .Include(q => q.Requisition)
            .Include(q => q.Items)
                .ThenInclude(i => i.Material)
            .Where(q => q.RequisitionId == requisitionId)
            .OrderBy(q => q.TotalQuotedAmount)
            .Select(q => new QuotationDto
            {
                QuotationId = q.QuotationId,
                QuotationNumber = q.QuotationNumber,
                RequisitionId = q.RequisitionId,
                RequisitionNumber = q.Requisition != null ? q.Requisition.RequisitionNumber : string.Empty,
                VendorId = q.VendorId,
                VendorName = q.Vendor != null ? q.Vendor.CompanyName : string.Empty,
                VendorRating = q.Vendor != null ? q.Vendor.VendorRating : 0,
                QuotationDate = q.QuotationDate,
                ValidUntil = q.ValidUntil,
                Status = q.Status,
                SelectionJustification = q.SelectionJustification,
                TotalQuotedAmount = q.TotalQuotedAmount,
                PaymentTerms = q.PaymentTerms,
                DeliveryDays = q.DeliveryDays,
                Items = q.Items.Select(i => new QuotationItemDto
                {
                    QuotationItemId = i.QuotationItemId,
                    MaterialId = i.MaterialId,
                    MaterialName = i.Material != null ? i.Material.MaterialName : string.Empty,
                    QuotedUnitPrice = i.QuotedUnitPrice,
                    TaxPercentage = i.TaxPercentage,
                    LineTotal = i.LineTotal,
                    DeliveryLeadDays = i.DeliveryLeadDays,
                    Notes = i.Notes
                }).ToList()
            })
            .ToListAsync();

        return ApiResponse<List<QuotationDto>>.Ok(list);
    }

    public async Task<ApiResponse<QuotationDto>> CreateQuotationAsync(QuotationCreateDto dto)
    {
        var count = await _context.VendorQuotations.CountAsync() + 1;
        var qNumber = $"QT-{DateTime.UtcNow.Year}-{count:D4}";

        decimal totalAmount = dto.Items.Sum(i => (i.QuotedUnitPrice * (1 + (i.TaxPercentage / 100))));

        var quote = new VendorQuotation
        {
            QuotationNumber = qNumber,
            RequisitionId = dto.RequisitionId,
            VendorId = dto.VendorId,
            QuotationDate = DateTime.UtcNow,
            ValidUntil = dto.ValidUntil,
            PaymentTerms = dto.PaymentTerms,
            DeliveryDays = dto.DeliveryDays,
            TotalQuotedAmount = totalAmount,
            Status = QuotationStatus.Submitted
        };

        foreach (var i in dto.Items)
        {
            quote.Items.Add(new VendorQuotationItem
            {
                MaterialId = i.MaterialId,
                QuotedUnitPrice = i.QuotedUnitPrice,
                TaxPercentage = i.TaxPercentage,
                LineTotal = i.QuotedUnitPrice * (1 + (i.TaxPercentage / 100)),
                DeliveryLeadDays = i.DeliveryLeadDays,
                Notes = i.Notes
            });
        }

        await _context.VendorQuotations.AddAsync(quote);
        await _context.SaveChangesAsync();

        var quotes = await GetQuotationsByPRAsync(dto.RequisitionId);
        return ApiResponse<QuotationDto>.Ok(quotes.Data!.First(x => x.QuotationId == quote.QuotationId));
    }

    public async Task<ApiResponse<bool>> SelectQuotationAsync(int quotationId, QuotationSelectDto dto)
    {
        var quote = await _context.VendorQuotations.FirstOrDefaultAsync(q => q.QuotationId == quotationId);
        if (quote == null) return ApiResponse<bool>.Fail("Quotation not found");

        // Reject other quotations for this PR and mark this one as Selected
        var otherQuotes = await _context.VendorQuotations
            .Where(q => q.RequisitionId == quote.RequisitionId && q.QuotationId != quotationId)
            .ToListAsync();

        foreach (var other in otherQuotes)
        {
            other.Status = QuotationStatus.Rejected;
        }

        quote.Status = QuotationStatus.Selected;
        quote.SelectionJustification = dto.SelectionJustification;
        quote.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Quotation selected successfully");
    }

    // --- Purchase Orders ---

    public async Task<ApiResponse<PagedResult<PurchaseOrderDto>>> GetPurchaseOrdersAsync(PurchaseOrderStatus? status, int? vendorId, int page, int pageSize)
    {
        var query = _context.PurchaseOrders
            .Include(p => p.Vendor)
            .Include(p => p.Requisition)
            .Include(p => p.Warehouse)
            .Include(p => p.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == vendorId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToPurchaseOrderDto(p))
            .ToListAsync();

        return ApiResponse<PagedResult<PurchaseOrderDto>>.Ok(new PagedResult<PurchaseOrderDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<PurchaseOrderDto>> GetPurchaseOrderByIdAsync(int id)
    {
        var p = await _context.PurchaseOrders
            .Include(x => x.Vendor)
            .Include(x => x.Requisition)
            .Include(x => x.Warehouse)
            .Include(x => x.Items)
                .ThenInclude(i => i.Material)
                    .ThenInclude(m => m!.BaseUOM)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.POId == id);

        if (p == null) return ApiResponse<PurchaseOrderDto>.Fail("Purchase order not found");

        return ApiResponse<PurchaseOrderDto>.Ok(MapToPurchaseOrderDto(p));
    }

    public async Task<ApiResponse<PurchaseOrderDto>> CreatePurchaseOrderAsync(int userId, PurchaseOrderCreateDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
        {
            return ApiResponse<PurchaseOrderDto>.Fail("Purchase order must have at least one line item");
        }

        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == dto.VendorId);
        if (vendor == null || !vendor.Status)
        {
            return ApiResponse<PurchaseOrderDto>.Fail("Selected vendor is inactive or does not exist");
        }

        // If requisition reference is provided, check PR status and prevent duplicate conversion
        PurchaseRequisition? pr = null;
        if (dto.RequisitionId.HasValue)
        {
            pr = await _context.PurchaseRequisitions.FirstOrDefaultAsync(r => r.RequisitionId == dto.RequisitionId.Value);
            if (pr == null)
            {
                return ApiResponse<PurchaseOrderDto>.Fail("Referenced requisition does not exist");
            }
            if (pr.Status == RequisitionStatus.ConvertedToPO)
            {
                return ApiResponse<PurchaseOrderDto>.Fail("Requisition has already been converted to a Purchase Order");
            }
            if (pr.Status != RequisitionStatus.Approved)
            {
                return ApiResponse<PurchaseOrderDto>.Fail($"Requisition in status '{pr.Status}' cannot be converted to a Purchase Order. It must be Approved.");
            }
        }

        var count = await _context.PurchaseOrders.CountAsync() + 1;
        var poNumber = $"PO-{DateTime.UtcNow.Year}-{count:D4}";

        decimal subtotal = 0;
        decimal totalTax = 0;

        var po = new PurchaseOrder
        {
            PONumber = poNumber,
            VendorId = dto.VendorId,
            RequisitionId = dto.RequisitionId,
            PODate = DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            PaymentTerms = dto.PaymentTerms,
            ShippingAddress = dto.ShippingAddress,
            BillingAddress = dto.BillingAddress,
            WarehouseId = dto.WarehouseId,
            DiscountAmount = dto.DiscountAmount,
            Status = PurchaseOrderStatus.Approved,
            CreatedByUserId = userId,
            ApprovedByUserId = userId,
            ApprovedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var i in dto.Items)
        {
            if (i.Quantity <= 0)
                return ApiResponse<PurchaseOrderDto>.Fail("Item quantity must be greater than zero");

            decimal lineSubtotal = i.Quantity * i.UnitPrice;
            decimal lineTax = lineSubtotal * (i.TaxRate / 100);
            decimal lineTotal = lineSubtotal + lineTax;

            subtotal += lineSubtotal;
            totalTax += lineTax;

            po.Items.Add(new PurchaseOrderItem
            {
                MaterialId = i.MaterialId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TaxRate = i.TaxRate,
                LineSubtotal = lineSubtotal,
                LineTax = lineTax,
                LineTotal = lineTotal,
                ReceivedQuantity = 0
            });
        }

        po.Subtotal = subtotal;
        po.TaxAmount = totalTax;
        po.GrandTotal = Math.Max(0, subtotal + totalTax - dto.DiscountAmount);

        // Mark PR as converted if applicable
        if (pr != null)
        {
            pr.Status = RequisitionStatus.ConvertedToPO;
            pr.UpdatedAt = DateTime.UtcNow;
        }

        await _context.PurchaseOrders.AddAsync(po);
        await _context.SaveChangesAsync();

        return await GetPurchaseOrderByIdAsync(po.POId);
    }

    public async Task<ApiResponse<bool>> SendPOToVendorAsync(int poId, int userId)
    {
        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.POId == poId);
        if (po == null) return ApiResponse<bool>.Fail("Purchase order not found");

        if (po.Status != PurchaseOrderStatus.Draft && po.Status != PurchaseOrderStatus.Approved)
        {
            return ApiResponse<bool>.Fail($"PO in status '{po.Status}' cannot be sent to vendor");
        }

        po.Status = PurchaseOrderStatus.SentToVendor;
        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Purchase order marked as Sent to Vendor");
    }

    public async Task<ApiResponse<bool>> CancelPurchaseOrderAsync(int poId, int userId, string reason)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.POId == poId);

        if (po == null) return ApiResponse<bool>.Fail("Purchase order not found");

        if (po.Items.Any(i => i.ReceivedQuantity > 0))
        {
            return ApiResponse<bool>.Fail("Cannot cancel Purchase Order because materials have already been received");
        }

        po.Status = PurchaseOrderStatus.Cancelled;
        po.CancellationReason = reason.Trim();
        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Purchase order cancelled");
    }

    // --- Helper Mappers ---

    private static RequisitionDto MapToRequisitionDto(PurchaseRequisition r) => new()
    {
        RequisitionId = r.RequisitionId,
        RequisitionNumber = r.RequisitionNumber,
        RequestedByUserId = r.RequestedByUserId,
        RequestedByUserName = r.RequestedByUser?.FullName ?? string.Empty,
        DepartmentId = r.DepartmentId,
        DepartmentName = r.Department?.DepartmentName ?? string.Empty,
        RequestDate = r.RequestDate,
        RequiredDeliveryDate = r.RequiredDeliveryDate,
        Priority = r.Priority,
        TotalEstimatedAmount = r.TotalEstimatedAmount,
        BusinessJustification = r.BusinessJustification,
        Status = r.Status,
        ApprovalRemarks = r.ApprovalRemarks,
        ApprovedByUserId = r.ApprovedByUserId,
        ApprovedDate = r.ApprovedDate,
        Items = r.Items.Select(i => new RequisitionItemDto
        {
            ItemId = i.ItemId,
            MaterialId = i.MaterialId,
            MaterialCode = i.Material?.MaterialCode ?? string.Empty,
            MaterialName = i.Material?.MaterialName ?? string.Empty,
            UOMCode = i.Material?.BaseUOM?.UOMCode ?? string.Empty,
            Quantity = i.Quantity,
            EstimatedUnitPrice = i.EstimatedUnitPrice,
            LineTotal = i.LineTotal,
            Remarks = i.Remarks
        }).ToList(),
        ApprovalHistories = r.ApprovalHistories.OrderBy(h => h.ActionDate).Select(h => new ApprovalHistoryDto
        {
            HistoryId = h.HistoryId,
            ApproverUserId = h.ApproverUserId,
            ApproverName = h.ApproverUser?.FullName ?? "System",
            Action = h.Action,
            Remarks = h.Remarks,
            ActionDate = h.ActionDate
        }).ToList()
    };

    private static PurchaseOrderDto MapToPurchaseOrderDto(PurchaseOrder p) => new()
    {
        POId = p.POId,
        PONumber = p.PONumber,
        VendorId = p.VendorId,
        VendorName = p.Vendor?.CompanyName ?? string.Empty,
        VendorGSTIN = p.Vendor?.GSTIN ?? string.Empty,
        VendorPAN = p.Vendor?.PAN ?? string.Empty,
        VendorAddress = $"{p.Vendor?.Address}, {p.Vendor?.City}, {p.Vendor?.State}",
        RequisitionId = p.RequisitionId,
        RequisitionNumber = p.Requisition?.RequisitionNumber,
        PODate = p.PODate,
        ExpectedDeliveryDate = p.ExpectedDeliveryDate,
        PaymentTerms = p.PaymentTerms,
        ShippingAddress = p.ShippingAddress,
        BillingAddress = p.BillingAddress,
        WarehouseId = p.WarehouseId,
        WarehouseName = p.Warehouse?.WarehouseName ?? string.Empty,
        Subtotal = p.Subtotal,
        TaxAmount = p.TaxAmount,
        DiscountAmount = p.DiscountAmount,
        GrandTotal = p.GrandTotal,
        Status = p.Status,
        CreatedByUserId = p.CreatedByUserId,
        ApprovedByUserId = p.ApprovedByUserId,
        ApprovedDate = p.ApprovedDate,
        CancellationReason = p.CancellationReason,
        Items = p.Items.Select(i => new PurchaseOrderItemDto
        {
            POItemId = i.POItemId,
            MaterialId = i.MaterialId,
            MaterialCode = i.Material?.MaterialCode ?? string.Empty,
            MaterialName = i.Material?.MaterialName ?? string.Empty,
            UOMCode = i.Material?.BaseUOM?.UOMCode ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TaxRate = i.TaxRate,
            LineSubtotal = i.LineSubtotal,
            LineTax = i.LineTax,
            LineTotal = i.LineTotal,
            ReceivedQuantity = i.ReceivedQuantity
        }).ToList()
    };
}
