namespace SmartProcure.Application.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public bool Status { get; set; }
}

public class WarehouseDto
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal CapacitySqFt { get; set; }
    public bool Status { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UomDto
{
    public int UOMId { get; set; }
    public string UOMCode { get; set; } = string.Empty;
    public string UOMName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class VendorDto
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
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string GSTIN { get; set; } = string.Empty;
    public string PAN { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string BankAccountRef { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string IFSCCode { get; set; } = string.Empty;
    public string VendorCategory { get; set; } = string.Empty;
    public decimal VendorRating { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VendorCreateDto
{
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
    public string VendorCategory { get; set; } = string.Empty;
    public decimal VendorRating { get; set; } = 4.0m;
}

public class VendorUpdateDto : VendorCreateDto
{
    public bool Status { get; set; }
}

public class MaterialDto
{
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string MaterialType { get; set; } = string.Empty;
    public int BaseUOMId { get; set; }
    public string BaseUOMCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumStockLevel { get; set; }
    public decimal CurrentStock { get; set; }
    public int? PreferredVendorId { get; set; }
    public string? PreferredVendorName { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool IsLowStock => CurrentStock <= ReorderLevel;
}

public class MaterialCreateDto
{
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string MaterialType { get; set; } = "Raw Material";
    public int BaseUOMId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumStockLevel { get; set; }
    public int? PreferredVendorId { get; set; }
    public string StorageLocation { get; set; } = "Store A";
}

public class MaterialUpdateDto : MaterialCreateDto
{
    public bool Status { get; set; }
}
