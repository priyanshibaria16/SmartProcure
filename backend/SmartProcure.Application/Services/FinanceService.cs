using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Entities;
using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.Services;

public class FinanceService : IFinanceService
{
    private readonly IApplicationDbContext _context;

    public FinanceService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<SupplierInvoiceDto>>> GetInvoicesAsync(InvoiceStatus? status, int? vendorId, int page, int pageSize)
    {
        var query = _context.SupplierInvoices
            .Include(i => i.Vendor)
            .Include(i => i.PurchaseOrder)
            .Include(i => i.GoodsReceipt)
            .Include(i => i.Items)
                .ThenInclude(it => it.Material)
            .Include(i => i.Verifications)
            .Include(i => i.Payments)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(i => i.VendorId == vendorId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => MapToSupplierInvoiceDto(i))
            .ToListAsync();

        return ApiResponse<PagedResult<SupplierInvoiceDto>>.Ok(new PagedResult<SupplierInvoiceDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<SupplierInvoiceDto>> GetInvoiceByIdAsync(int id)
    {
        var inv = await _context.SupplierInvoices
            .Include(x => x.Vendor)
            .Include(x => x.PurchaseOrder)
            .Include(x => x.GoodsReceipt)
            .Include(x => x.Items)
                .ThenInclude(it => it.Material)
            .Include(x => x.Verifications)
            .Include(x => x.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvoiceId == id);

        if (inv == null) return ApiResponse<SupplierInvoiceDto>.Fail("Supplier invoice not found");

        return ApiResponse<SupplierInvoiceDto>.Ok(MapToSupplierInvoiceDto(inv));
    }

    public async Task<ApiResponse<SupplierInvoiceDto>> CreateInvoiceAsync(SupplierInvoiceCreateDto dto)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.POId == dto.POId);

        if (po == null) return ApiResponse<SupplierInvoiceDto>.Fail("Referenced Purchase Order not found");

        if (await _context.SupplierInvoices.AnyAsync(i => i.VendorId == dto.VendorId && i.InvoiceNumber.ToLower() == dto.InvoiceNumber.Trim().ToLower()))
        {
            return ApiResponse<SupplierInvoiceDto>.Fail($"An invoice with number '{dto.InvoiceNumber}' already exists for this vendor");
        }

        if (dto.TotalAmount <= 0)
        {
            return ApiResponse<SupplierInvoiceDto>.Fail("Total invoice amount must be greater than zero");
        }

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = dto.InvoiceNumber.Trim().ToUpper(),
            VendorId = dto.VendorId,
            POId = dto.POId,
            GRNId = dto.GRNId,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            Subtotal = dto.Subtotal,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            PaidAmount = 0,
            OutstandingAmount = dto.TotalAmount,
            Status = InvoiceStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in dto.Items)
        {
            invoice.Items.Add(new SupplierInvoiceItem
            {
                MaterialId = itemDto.MaterialId,
                BilledQuantity = itemDto.BilledQuantity,
                BilledUnitPrice = itemDto.BilledUnitPrice,
                LineTotal = itemDto.BilledQuantity * itemDto.BilledUnitPrice
            });
        }

        await _context.SupplierInvoices.AddAsync(invoice);
        await _context.SaveChangesAsync();

        return await GetInvoiceByIdAsync(invoice.InvoiceId);
    }

    public async Task<ApiResponse<InvoiceVerificationDto>> VerifyInvoiceAsync(int invoiceId, int userId)
    {
        var invoice = await _context.SupplierInvoices
            .Include(i => i.Items)
            .Include(i => i.PurchaseOrder)
                .ThenInclude(p => p!.Items)
            .Include(i => i.GoodsReceipt)
                .ThenInclude(g => g!.Items)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        if (invoice == null) return ApiResponse<InvoiceVerificationDto>.Fail("Invoice not found");

        var po = invoice.PurchaseOrder;
        if (po == null) return ApiResponse<InvoiceVerificationDto>.Fail("Invoice has no associated Purchase Order");

        // Tolerance rules (2% or INR 100)
        decimal maxPriceVariancePct = 2.0m;
        decimal maxPriceVarianceAbs = 100.0m;

        decimal totalQtyVariance = 0;
        decimal totalPriceVariance = 0;
        var varianceNotes = new List<string>();

        // Check if GRN is associated or find any GRN for this PO
        var grn = invoice.GoodsReceipt ?? await _context.GoodsReceipts
            .Include(g => g.Items)
            .OrderByDescending(g => g.ReceiptDate)
            .FirstOrDefaultAsync(g => g.POId == po.POId);

        foreach (var invItem in invoice.Items)
        {
            var poItem = po.Items.FirstOrDefault(p => p.MaterialId == invItem.MaterialId);
            if (poItem != null)
            {
                // Price Variance Check: Invoice Unit Price vs PO Unit Price
                decimal priceDiff = Math.Abs(invItem.BilledUnitPrice - poItem.UnitPrice);
                decimal priceDiffPct = poItem.UnitPrice > 0 ? (priceDiff / poItem.UnitPrice) * 100 : 0;

                if (priceDiffPct > maxPriceVariancePct && priceDiff > maxPriceVarianceAbs)
                {
                    totalPriceVariance += priceDiff;
                    varianceNotes.Add($"Material ID {invItem.MaterialId}: Billed rate ₹{invItem.BilledUnitPrice} deviates from PO rate ₹{poItem.UnitPrice} by {priceDiffPct:F1}%");
                }
            }

            // Quantity Variance Check: Invoice Billed Qty vs GRN Accepted Qty
            if (grn != null)
            {
                var grnItem = grn.Items.FirstOrDefault(g => g.MaterialId == invItem.MaterialId);
                decimal acceptedQty = grnItem?.AcceptedQuantity ?? 0;

                if (invItem.BilledQuantity > acceptedQty)
                {
                    decimal qtyDiff = invItem.BilledQuantity - acceptedQty;
                    totalQtyVariance += qtyDiff;
                    varianceNotes.Add($"Material ID {invItem.MaterialId}: Billed quantity {invItem.BilledQuantity} exceeds dock accepted quantity {acceptedQty} by {qtyDiff}");
                }
            }
        }

        bool isMatched = !varianceNotes.Any();
        string varianceType = "None";
        if (totalPriceVariance > 0 && totalQtyVariance > 0) varianceType = "Both";
        else if (totalPriceVariance > 0) varianceType = "PriceVariance";
        else if (totalQtyVariance > 0) varianceType = "QuantityVariance";

        string summaryRemarks = isMatched
            ? "3-Way Match Verified: Rates and accepted quantities match within tolerance."
            : $"MISMATCH DETECTED: {string.Join("; ", varianceNotes)}";

        var verification = new InvoiceVerification
        {
            InvoiceId = invoice.InvoiceId,
            POId = po.POId,
            GRNId = grn?.GRNId,
            MatchedDate = DateTime.UtcNow,
            PriceVariance = totalPriceVariance,
            QuantityVariance = totalQtyVariance,
            VarianceType = varianceType,
            IsMatchSuccessful = isMatched,
            Remarks = summaryRemarks,
            VerifiedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        invoice.Status = isMatched ? InvoiceStatus.Matched : InvoiceStatus.Mismatched;
        invoice.VerificationRemarks = summaryRemarks;
        invoice.VerifiedByUserId = userId;
        invoice.VerifiedDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.InvoiceVerifications.AddAsync(verification);
        await _context.SaveChangesAsync();

        return ApiResponse<InvoiceVerificationDto>.Ok(new InvoiceVerificationDto
        {
            VerificationId = verification.VerificationId,
            InvoiceId = verification.InvoiceId,
            POId = verification.POId,
            GRNId = verification.GRNId,
            MatchedDate = verification.MatchedDate,
            PriceVariance = verification.PriceVariance,
            QuantityVariance = verification.QuantityVariance,
            VarianceType = verification.VarianceType,
            IsMatchSuccessful = verification.IsMatchSuccessful,
            Remarks = verification.Remarks,
            VerifiedByUserId = verification.VerifiedByUserId
        }, isMatched ? "Invoice matched successfully" : "Invoice verification flagged discrepancies");
    }

    public async Task<ApiResponse<bool>> ApproveInvoiceAsync(int invoiceId, int userId, string remarks)
    {
        var invoice = await _context.SupplierInvoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        if (invoice == null) return ApiResponse<bool>.Fail("Invoice not found");

        if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
        {
            return ApiResponse<bool>.Fail($"Invoice in status '{invoice.Status}' cannot be approved");
        }

        invoice.Status = InvoiceStatus.Approved;
        invoice.VerificationRemarks = (invoice.VerificationRemarks + " | Approved by Finance: " + remarks).Trim();
        invoice.VerifiedByUserId = userId;
        invoice.VerifiedDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Invoice approved for payment settlement");
    }

    public async Task<ApiResponse<bool>> RejectInvoiceAsync(int invoiceId, int userId, string remarks)
    {
        var invoice = await _context.SupplierInvoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        if (invoice == null) return ApiResponse<bool>.Fail("Invoice not found");

        if (string.IsNullOrWhiteSpace(remarks))
        {
            return ApiResponse<bool>.Fail("Rejection remarks are mandatory");
        }

        invoice.Status = InvoiceStatus.Rejected;
        invoice.VerificationRemarks = "Rejected: " + remarks.Trim();
        invoice.VerifiedByUserId = userId;
        invoice.VerifiedDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Invoice rejected");
    }

    public async Task<ApiResponse<PagedResult<PaymentDto>>> GetPaymentsAsync(int? invoiceId, int? vendorId, int page, int pageSize)
    {
        var query = _context.Payments
            .Include(p => p.Invoice)
            .Include(p => p.Vendor)
            .Include(p => p.RecordedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (invoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == invoiceId.Value);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == vendorId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                PaymentNumber = p.PaymentNumber,
                InvoiceId = p.InvoiceId,
                InvoiceNumber = p.Invoice != null ? p.Invoice.InvoiceNumber : string.Empty,
                VendorId = p.VendorId,
                VendorName = p.Vendor != null ? p.Vendor.CompanyName : string.Empty,
                PaymentDate = p.PaymentDate,
                PaymentAmount = p.PaymentAmount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                Status = p.Status,
                RecordedByUserId = p.RecordedByUserId,
                RecordedByUserName = p.RecordedByUser != null ? p.RecordedByUser.FullName : string.Empty,
                Remarks = p.Remarks
            })
            .ToListAsync();

        return ApiResponse<PagedResult<PaymentDto>>.Ok(new PagedResult<PaymentDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<PaymentDto>> RecordPaymentAsync(int userId, PaymentCreateDto dto)
    {
        var invoice = await _context.SupplierInvoices
            .Include(i => i.Vendor)
            .FirstOrDefaultAsync(i => i.InvoiceId == dto.InvoiceId);

        if (invoice == null) return ApiResponse<PaymentDto>.Fail("Invoice not found");

        if (invoice.Status != InvoiceStatus.Approved && invoice.Status != InvoiceStatus.PartiallyPaid && invoice.Status != InvoiceStatus.Matched)
        {
            return ApiResponse<PaymentDto>.Fail($"Payments can only be recorded against Approved or Partially Paid invoices. Current status: '{invoice.Status}'");
        }

        if (dto.PaymentAmount <= 0)
        {
            return ApiResponse<PaymentDto>.Fail("Payment amount must be greater than zero");
        }

        if (dto.PaymentAmount > invoice.OutstandingAmount)
        {
            return ApiResponse<PaymentDto>.Fail($"Overpayment prevented: Payment amount ₹{dto.PaymentAmount:N2} exceeds outstanding invoice balance of ₹{invoice.OutstandingAmount:N2}");
        }

        var count = await _context.Payments.CountAsync() + 1;
        var payNumber = $"PAY-{DateTime.UtcNow.Year}-{count:D4}";

        var payment = new Payment
        {
            PaymentNumber = payNumber,
            InvoiceId = invoice.InvoiceId,
            VendorId = invoice.VendorId,
            PaymentDate = DateTime.UtcNow,
            PaymentAmount = dto.PaymentAmount,
            PaymentMethod = dto.PaymentMethod,
            TransactionReference = dto.TransactionReference.Trim(),
            Status = PaymentStatus.Completed,
            RecordedByUserId = userId,
            Remarks = dto.Remarks?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // Update invoice outstanding amount & status
        invoice.PaidAmount += dto.PaymentAmount;
        invoice.OutstandingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        invoice.Status = invoice.OutstandingAmount == 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        return ApiResponse<PaymentDto>.Ok(new PaymentDto
        {
            PaymentId = payment.PaymentId,
            PaymentNumber = payment.PaymentNumber,
            InvoiceId = payment.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            VendorId = payment.VendorId,
            VendorName = invoice.Vendor?.CompanyName ?? string.Empty,
            PaymentDate = payment.PaymentDate,
            PaymentAmount = payment.PaymentAmount,
            PaymentMethod = payment.PaymentMethod,
            TransactionReference = payment.TransactionReference,
            Status = payment.Status,
            RecordedByUserId = payment.RecordedByUserId,
            Remarks = payment.Remarks
        }, "Payment recorded successfully");
    }

    private static SupplierInvoiceDto MapToSupplierInvoiceDto(SupplierInvoice i) => new()
    {
        InvoiceId = i.InvoiceId,
        InvoiceNumber = i.InvoiceNumber,
        VendorId = i.VendorId,
        VendorName = i.Vendor?.CompanyName ?? string.Empty,
        POId = i.POId,
        PONumber = i.PurchaseOrder?.PONumber ?? string.Empty,
        GRNId = i.GRNId,
        GRNNumber = i.GoodsReceipt?.GRNNumber,
        InvoiceDate = i.InvoiceDate,
        DueDate = i.DueDate,
        Subtotal = i.Subtotal,
        TaxAmount = i.TaxAmount,
        TotalAmount = i.TotalAmount,
        PaidAmount = i.PaidAmount,
        OutstandingAmount = i.OutstandingAmount,
        Status = i.Status,
        VerificationRemarks = i.VerificationRemarks,
        VerifiedByUserId = i.VerifiedByUserId,
        VerifiedDate = i.VerifiedDate,
        Items = i.Items.Select(it => new SupplierInvoiceItemDto
        {
            InvoiceItemId = it.InvoiceItemId,
            MaterialId = it.MaterialId,
            MaterialName = it.Material?.MaterialName ?? string.Empty,
            BilledQuantity = it.BilledQuantity,
            BilledUnitPrice = it.BilledUnitPrice,
            LineTotal = it.LineTotal
        }).ToList(),
        Verifications = i.Verifications.Select(v => new InvoiceVerificationDto
        {
            VerificationId = v.VerificationId,
            InvoiceId = v.InvoiceId,
            POId = v.POId,
            GRNId = v.GRNId,
            MatchedDate = v.MatchedDate,
            PriceVariance = v.PriceVariance,
            QuantityVariance = v.QuantityVariance,
            VarianceType = v.VarianceType,
            IsMatchSuccessful = v.IsMatchSuccessful,
            Remarks = v.Remarks,
            VerifiedByUserId = v.VerifiedByUserId
        }).ToList(),
        Payments = i.Payments.Select(p => new PaymentDto
        {
            PaymentId = p.PaymentId,
            PaymentNumber = p.PaymentNumber,
            InvoiceId = p.InvoiceId,
            InvoiceNumber = i.InvoiceNumber,
            VendorId = p.VendorId,
            PaymentDate = p.PaymentDate,
            PaymentAmount = p.PaymentAmount,
            PaymentMethod = p.PaymentMethod,
            TransactionReference = p.TransactionReference,
            Status = p.Status,
            RecordedByUserId = p.RecordedByUserId,
            Remarks = p.Remarks
        }).ToList()
    };
}
