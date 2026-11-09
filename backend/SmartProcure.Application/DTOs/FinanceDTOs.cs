using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.DTOs;

public class SupplierInvoiceItemDto
{
    public int InvoiceItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal BilledQuantity { get; set; }
    public decimal BilledUnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class SupplierInvoiceDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int POId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int? GRNId { get; set; }
    public string? GRNNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? VerificationRemarks { get; set; }
    public int? VerifiedByUserId { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public List<SupplierInvoiceItemDto> Items { get; set; } = new();
    public List<InvoiceVerificationDto> Verifications { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class SupplierInvoiceCreateDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public int POId { get; set; }
    public int? GRNId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SupplierInvoiceItemCreateDto> Items { get; set; } = new();
}

public class SupplierInvoiceItemCreateDto
{
    public int MaterialId { get; set; }
    public decimal BilledQuantity { get; set; }
    public decimal BilledUnitPrice { get; set; }
}

public class InvoiceVerificationDto
{
    public int VerificationId { get; set; }
    public int InvoiceId { get; set; }
    public int POId { get; set; }
    public int? GRNId { get; set; }
    public DateTime MatchedDate { get; set; }
    public decimal PriceVariance { get; set; }
    public decimal QuantityVariance { get; set; }
    public string VarianceType { get; set; } = string.Empty;
    public bool IsMatchSuccessful { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public int VerifiedByUserId { get; set; }
    public string? VerifiedByUserName { get; set; }
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal PaymentAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod.ToString();
    public string TransactionReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int RecordedByUserId { get; set; }
    public string? RecordedByUserName { get; set; }
    public string? Remarks { get; set; }
}

public class PaymentCreateDto
{
    public int InvoiceId { get; set; }
    public decimal PaymentAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NEFT;
    public string TransactionReference { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
