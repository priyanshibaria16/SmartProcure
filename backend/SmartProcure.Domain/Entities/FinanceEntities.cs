using SmartProcure.Domain.Enums;

namespace SmartProcure.Domain.Entities;

public class SupplierInvoice : BaseEntity
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public int POId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public int? GRNId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public decimal OutstandingAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.PendingVerification;
    public string? VerificationRemarks { get; set; }
    public int? VerifiedByUserId { get; set; }
    public DateTime? VerifiedDate { get; set; }

    public ICollection<SupplierInvoiceItem> Items { get; set; } = new List<SupplierInvoiceItem>();
    public ICollection<InvoiceVerification> Verifications { get; set; } = new List<InvoiceVerification>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class SupplierInvoiceItem
{
    public int InvoiceItemId { get; set; }
    public int InvoiceId { get; set; }
    public SupplierInvoice? Invoice { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public decimal BilledQuantity { get; set; }
    public decimal BilledUnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class InvoiceVerification : BaseEntity
{
    public int VerificationId { get; set; }
    public int InvoiceId { get; set; }
    public SupplierInvoice? Invoice { get; set; }
    public int POId { get; set; }
    public int? GRNId { get; set; }
    public DateTime MatchedDate { get; set; } = DateTime.UtcNow;
    public decimal PriceVariance { get; set; }
    public decimal QuantityVariance { get; set; }
    public string VarianceType { get; set; } = "None"; // None, PriceVariance, QuantityVariance, Both
    public bool IsMatchSuccessful { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public int VerifiedByUserId { get; set; }
    public User? VerifiedByUser { get; set; }
}

public class Payment : BaseEntity
{
    public int PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int InvoiceId { get; set; }
    public SupplierInvoice? Invoice { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal PaymentAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.NEFT;
    public string TransactionReference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public int RecordedByUserId { get; set; }
    public User? RecordedByUser { get; set; }
    public string? Remarks { get; set; }
}
