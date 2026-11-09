using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Domain.Entities;

namespace SmartProcure.Infrastructure.Data;

public class SmartProcureDbContext : DbContext, IApplicationDbContext
{
    public SmartProcureDbContext(DbContextOptions<SmartProcureDbContext> options)
        : base(options)
    {
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Material> Materials => Set<Material>();

    public DbSet<PurchaseRequisition> PurchaseRequisitions => Set<PurchaseRequisition>();
    public DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems => Set<PurchaseRequisitionItem>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();
    public DbSet<VendorQuotation> VendorQuotations => Set<VendorQuotation>();
    public DbSet<VendorQuotationItem> VendorQuotationItems => Set<VendorQuotationItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    public DbSet<SupplierInvoiceItem> SupplierInvoiceItems => Set<SupplierInvoiceItem>();
    public DbSet<InvoiceVerification> InvoiceVerifications => Set<InvoiceVerification>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Master Data Constraints & Indexes ---
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.RoleName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId);
            entity.HasIndex(e => e.DepartmentCode).IsUnique();
            entity.Property(e => e.DepartmentCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.DepartmentName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ManagerName).HasMaxLength(100);
            entity.Property(e => e.Budget).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId);
            entity.HasIndex(e => e.WarehouseCode).IsUnique();
            entity.Property(e => e.WarehouseCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.WarehouseName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CapacitySqFt).HasPrecision(18, 2);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(25);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.HasIndex(e => e.CategoryCode).IsUnique();
            entity.Property(e => e.CategoryCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UnitOfMeasure>(entity =>
        {
            entity.HasKey(e => e.UOMId);
            entity.HasIndex(e => e.UOMCode).IsUnique();
            entity.Property(e => e.UOMCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.UOMName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.VendorId);
            entity.HasIndex(e => e.VendorCode).IsUnique();
            entity.Property(e => e.VendorCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CompanyName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(25);
            entity.Property(e => e.GSTIN).HasMaxLength(20);
            entity.Property(e => e.PAN).HasMaxLength(15);
            entity.Property(e => e.VendorRating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.MaterialId);
            entity.HasIndex(e => e.MaterialCode).IsUnique();
            entity.Property(e => e.MaterialCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.MaterialName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.ReorderLevel).HasPrecision(18, 3);
            entity.Property(e => e.MaximumStockLevel).HasPrecision(18, 3);
            entity.Property(e => e.CurrentStock).HasPrecision(18, 3);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Materials)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BaseUOM)
                .WithMany(u => u.Materials)
                .HasForeignKey(e => e.BaseUOMId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PreferredVendor)
                .WithMany(v => v.PreferredMaterials)
                .HasForeignKey(e => e.PreferredVendorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // --- Procurement Entities ---
        modelBuilder.Entity<PurchaseRequisition>(entity =>
        {
            entity.HasKey(e => e.RequisitionId);
            entity.HasIndex(e => e.RequisitionNumber).IsUnique();
            entity.Property(e => e.RequisitionNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.TotalEstimatedAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.RequestedByUser)
                .WithMany(u => u.RequisitionsRequested)
                .HasForeignKey(e => e.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.PurchaseRequisitions)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseRequisitionItem>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.Quantity).HasPrecision(18, 3);
            entity.Property(e => e.EstimatedUnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Requisition)
                .WithMany(r => r.Items)
                .HasForeignKey(e => e.RequisitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.RequisitionItems)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApprovalHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Remarks).HasMaxLength(500);

            entity.HasOne(e => e.Requisition)
                .WithMany(r => r.ApprovalHistories)
                .HasForeignKey(e => e.RequisitionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ApproverUser)
                .WithMany(u => u.Approvals)
                .HasForeignKey(e => e.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorQuotation>(entity =>
        {
            entity.HasKey(e => e.QuotationId);
            entity.HasIndex(e => e.QuotationNumber).IsUnique();
            entity.Property(e => e.QuotationNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.TotalQuotedAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Requisition)
                .WithMany(r => r.Quotations)
                .HasForeignKey(e => e.RequisitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.Quotations)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorQuotationItem>(entity =>
        {
            entity.HasKey(e => e.QuotationItemId);
            entity.Property(e => e.QuotedUnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercentage).HasPrecision(5, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(e => e.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.POId);
            entity.HasIndex(e => e.PONumber).IsUnique();
            entity.Property(e => e.PONumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.GrandTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.PurchaseOrders)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Requisition)
                .WithMany(r => r.PurchaseOrders)
                .HasForeignKey(e => e.RequisitionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasKey(e => e.POItemId);
            entity.Property(e => e.Quantity).HasPrecision(18, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.LineSubtotal).HasPrecision(18, 2);
            entity.Property(e => e.LineTax).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.ReceivedQuantity).HasPrecision(18, 3);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.POId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.PurchaseOrderItems)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Warehouse & Inventory Entities ---
        modelBuilder.Entity<GoodsReceipt>(entity =>
        {
            entity.HasKey(e => e.GRNId);
            entity.HasIndex(e => e.GRNNumber).IsUnique();
            entity.Property(e => e.GRNNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.DeliveryChallanNumber).HasMaxLength(50);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.GoodsReceipts)
                .HasForeignKey(e => e.POId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.GoodsReceipts)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReceivedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReceivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GoodsReceiptItem>(entity =>
        {
            entity.HasKey(e => e.GRNItemId);
            entity.Property(e => e.OrderedQuantity).HasPrecision(18, 3);
            entity.Property(e => e.PreviouslyReceivedQuantity).HasPrecision(18, 3);
            entity.Property(e => e.CurrentReceivedQuantity).HasPrecision(18, 3);
            entity.Property(e => e.AcceptedQuantity).HasPrecision(18, 3);
            entity.Property(e => e.RejectedQuantity).HasPrecision(18, 3);

            entity.HasOne(e => e.GoodsReceipt)
                .WithMany(g => g.Items)
                .HasForeignKey(e => e.GRNId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InventoryBalance>(entity =>
        {
            entity.HasKey(e => e.BalanceId);
            entity.HasIndex(e => new { e.MaterialId, e.WarehouseId }).IsUnique();
            entity.Property(e => e.AvailableStock).HasPrecision(18, 3);
            entity.Property(e => e.ReservedStock).HasPrecision(18, 3);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.InventoryBalances)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.InventoryBalances)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.Quantity).HasPrecision(18, 3);
            entity.Property(e => e.ReferenceDocumentType).HasMaxLength(50);
            entity.Property(e => e.ReferenceDocumentNumber).HasMaxLength(50);

            entity.HasOne(e => e.Material)
                .WithMany()
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Invoicing, 3-Way Match & Payments ---
        modelBuilder.Entity<SupplierInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);
            entity.HasIndex(e => new { e.VendorId, e.InvoiceNumber }).IsUnique();
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.OutstandingAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.SupplierInvoices)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.SupplierInvoices)
                .HasForeignKey(e => e.POId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GoodsReceipt)
                .WithMany(g => g.SupplierInvoices)
                .HasForeignKey(e => e.GRNId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SupplierInvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemId);
            entity.Property(e => e.BilledQuantity).HasPrecision(18, 3);
            entity.Property(e => e.BilledUnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceVerification>(entity =>
        {
            entity.HasKey(e => e.VerificationId);
            entity.Property(e => e.PriceVariance).HasPrecision(18, 2);
            entity.Property(e => e.QuantityVariance).HasPrecision(18, 3);
            entity.Property(e => e.VarianceType).HasMaxLength(50);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Verifications)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.Property(e => e.PaymentNumber).HasMaxLength(30).IsRequired();
            entity.Property(e => e.PaymentAmount).HasPrecision(18, 2);
            entity.Property(e => e.TransactionReference).HasMaxLength(100);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- System Governance ---
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.IPAddress).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.LinkUrl).HasMaxLength(255);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.SettingId);
            entity.HasIndex(e => e.SettingKey).IsUnique();
            entity.Property(e => e.SettingKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SettingValue).HasMaxLength(500).IsRequired();
        });
    }
}
