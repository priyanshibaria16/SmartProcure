namespace SmartProcure.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Role : BaseEntity
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

public class Department : BaseEntity
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public bool Status { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<PurchaseRequisition> PurchaseRequisitions { get; set; } = new List<PurchaseRequisition>();
}

public class Warehouse : BaseEntity
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal CapacitySqFt { get; set; }
    public bool Status { get; set; } = true;

    public ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<PurchaseRequisition> RequisitionsRequested { get; set; } = new List<PurchaseRequisition>();
    public ICollection<ApprovalHistory> Approvals { get; set; } = new List<ApprovalHistory>();
}

public class MaterialCategory
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Material> Materials { get; set; } = new List<Material>();
}

public class UnitOfMeasure
{
    public int UOMId { get; set; }
    public string UOMCode { get; set; } = string.Empty;
    public string UOMName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Material> Materials { get; set; } = new List<Material>();
}

public class Vendor : BaseEntity
{
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string PostalCode { get; set; } = string.Empty;
    public string GSTIN { get; set; } = string.Empty;
    public string PAN { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = "Net 30";
    public string BankAccountRef { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string VendorCategory { get; set; } = "Raw Materials";
    public decimal VendorRating { get; set; } = 4.5m;
    public bool Status { get; set; } = true;

    public ICollection<Material> PreferredMaterials { get; set; } = new List<Material>();
    public ICollection<VendorQuotation> Quotations { get; set; } = new List<VendorQuotation>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<SupplierInvoice> SupplierInvoices { get; set; } = new List<SupplierInvoice>();
}

public class Material : BaseEntity
{
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public MaterialCategory? Category { get; set; }
    public string MaterialType { get; set; } = "Raw Material";
    public int BaseUOMId { get; set; }
    public UnitOfMeasure? BaseUOM { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumStockLevel { get; set; }
    public decimal CurrentStock { get; set; }
    public int? PreferredVendorId { get; set; }
    public Vendor? PreferredVendor { get; set; }
    public string StorageLocation { get; set; } = "Store A";
    public bool Status { get; set; } = true;

    public ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    public ICollection<PurchaseRequisitionItem> RequisitionItems { get; set; } = new List<PurchaseRequisitionItem>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
}
