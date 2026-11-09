using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartProcure.Domain.Entities;

namespace SmartProcure.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<Department> Departments { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<User> Users { get; }
    DbSet<MaterialCategory> MaterialCategories { get; }
    DbSet<UnitOfMeasure> UnitsOfMeasure { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<Material> Materials { get; }

    DbSet<PurchaseRequisition> PurchaseRequisitions { get; }
    DbSet<PurchaseRequisitionItem> PurchaseRequisitionItems { get; }
    DbSet<ApprovalHistory> ApprovalHistories { get; }
    DbSet<VendorQuotation> VendorQuotations { get; }
    DbSet<VendorQuotationItem> VendorQuotationItems { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsReceiptItem> GoodsReceiptItems { get; }
    DbSet<InventoryBalance> InventoryBalances { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }

    DbSet<SupplierInvoice> SupplierInvoices { get; }
    DbSet<SupplierInvoiceItem> SupplierInvoiceItems { get; }
    DbSet<InvoiceVerification> InvoiceVerifications { get; }
    DbSet<Payment> Payments { get; }

    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SystemSetting> SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
