using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Entities;

namespace SmartProcure.Application.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IApplicationDbContext _context;

    public MasterDataService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<VendorDto>>> GetVendorsAsync(string? search, bool? status, int page, int pageSize)
    {
        var query = _context.Vendors.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(v => v.VendorCode.ToLower().Contains(s) ||
                                     v.CompanyName.ToLower().Contains(s) ||
                                     v.City.ToLower().Contains(s) ||
                                     v.GSTIN.ToLower().Contains(s));
        }

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VendorDto
            {
                VendorId = v.VendorId,
                VendorCode = v.VendorCode,
                CompanyName = v.CompanyName,
                ContactPerson = v.ContactPerson,
                Email = v.Email,
                Phone = v.Phone,
                Address = v.Address,
                City = v.City,
                State = v.State,
                Country = v.Country,
                PostalCode = v.PostalCode,
                GSTIN = v.GSTIN,
                PAN = v.PAN,
                PaymentTerms = v.PaymentTerms,
                BankAccountRef = v.BankAccountRef,
                BankName = v.BankName,
                IFSCCode = v.IFSCCode,
                VendorCategory = v.VendorCategory,
                VendorRating = v.VendorRating,
                Status = v.Status,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<VendorDto>>.Ok(new PagedResult<VendorDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<VendorDto>> GetVendorByIdAsync(int id)
    {
        var v = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(x => x.VendorId == id);
        if (v == null) return ApiResponse<VendorDto>.Fail("Vendor not found");

        return ApiResponse<VendorDto>.Ok(new VendorDto
        {
            VendorId = v.VendorId,
            VendorCode = v.VendorCode,
            CompanyName = v.CompanyName,
            ContactPerson = v.ContactPerson,
            Email = v.Email,
            Phone = v.Phone,
            Address = v.Address,
            City = v.City,
            State = v.State,
            Country = v.Country,
            PostalCode = v.PostalCode,
            GSTIN = v.GSTIN,
            PAN = v.PAN,
            PaymentTerms = v.PaymentTerms,
            BankAccountRef = v.BankAccountRef,
            BankName = v.BankName,
            IFSCCode = v.IFSCCode,
            VendorCategory = v.VendorCategory,
            VendorRating = v.VendorRating,
            Status = v.Status,
            CreatedAt = v.CreatedAt
        });
    }

    public async Task<ApiResponse<VendorDto>> CreateVendorAsync(VendorCreateDto dto)
    {
        if (await _context.Vendors.AnyAsync(v => v.VendorCode.ToLower() == dto.VendorCode.Trim().ToLower()))
        {
            return ApiResponse<VendorDto>.Fail($"Vendor code '{dto.VendorCode}' already exists");
        }

        var vendor = new Vendor
        {
            VendorCode = dto.VendorCode.Trim().ToUpper(),
            CompanyName = dto.CompanyName.Trim(),
            ContactPerson = dto.ContactPerson.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            State = dto.State.Trim(),
            Country = dto.Country.Trim(),
            PostalCode = dto.PostalCode.Trim(),
            GSTIN = dto.GSTIN.Trim().ToUpper(),
            PAN = dto.PAN.Trim().ToUpper(),
            PaymentTerms = dto.PaymentTerms.Trim(),
            BankAccountRef = dto.BankAccountRef.Trim(),
            BankName = dto.BankName.Trim(),
            IFSCCode = dto.IFSCCode.Trim().ToUpper(),
            VendorCategory = dto.VendorCategory.Trim(),
            VendorRating = dto.VendorRating,
            Status = true
        };

        await _context.Vendors.AddAsync(vendor);
        await _context.SaveChangesAsync();

        return await GetVendorByIdAsync(vendor.VendorId);
    }

    public async Task<ApiResponse<VendorDto>> UpdateVendorAsync(int id, VendorUpdateDto dto)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == id);
        if (vendor == null) return ApiResponse<VendorDto>.Fail("Vendor not found");

        if (await _context.Vendors.AnyAsync(v => v.VendorCode.ToLower() == dto.VendorCode.Trim().ToLower() && v.VendorId != id))
        {
            return ApiResponse<VendorDto>.Fail($"Vendor code '{dto.VendorCode}' is already in use by another vendor");
        }

        vendor.VendorCode = dto.VendorCode.Trim().ToUpper();
        vendor.CompanyName = dto.CompanyName.Trim();
        vendor.ContactPerson = dto.ContactPerson.Trim();
        vendor.Email = dto.Email.Trim();
        vendor.Phone = dto.Phone.Trim();
        vendor.Address = dto.Address.Trim();
        vendor.City = dto.City.Trim();
        vendor.State = dto.State.Trim();
        vendor.Country = dto.Country.Trim();
        vendor.PostalCode = dto.PostalCode.Trim();
        vendor.GSTIN = dto.GSTIN.Trim().ToUpper();
        vendor.PAN = dto.PAN.Trim().ToUpper();
        vendor.PaymentTerms = dto.PaymentTerms.Trim();
        vendor.BankAccountRef = dto.BankAccountRef.Trim();
        vendor.BankName = dto.BankName.Trim();
        vendor.IFSCCode = dto.IFSCCode.Trim().ToUpper();
        vendor.VendorCategory = dto.VendorCategory.Trim();
        vendor.VendorRating = dto.VendorRating;
        vendor.Status = dto.Status;
        vendor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetVendorByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> ToggleVendorStatusAsync(int id)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorId == id);
        if (vendor == null) return ApiResponse<bool>.Fail("Vendor not found");

        vendor.Status = !vendor.Status;
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(vendor.Status, $"Vendor is now {(vendor.Status ? "Active" : "Inactive")}");
    }

    public async Task<ApiResponse<PagedResult<MaterialDto>>> GetMaterialsAsync(string? search, int? categoryId, bool? lowStockOnly, int page, int pageSize)
    {
        var query = _context.Materials
            .Include(m => m.Category)
            .Include(m => m.BaseUOM)
            .Include(m => m.PreferredVendor)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(m => m.MaterialCode.ToLower().Contains(s) ||
                                     m.MaterialName.ToLower().Contains(s) ||
                                     m.Description.ToLower().Contains(s));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        if (lowStockOnly == true)
        {
            query = query.Where(m => m.CurrentStock <= m.ReorderLevel);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(m => m.MaterialCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MaterialDto
            {
                MaterialId = m.MaterialId,
                MaterialCode = m.MaterialCode,
                MaterialName = m.MaterialName,
                Description = m.Description,
                CategoryId = m.CategoryId,
                CategoryName = m.Category != null ? m.Category.CategoryName : string.Empty,
                MaterialType = m.MaterialType,
                BaseUOMId = m.BaseUOMId,
                BaseUOMCode = m.BaseUOM != null ? m.BaseUOM.UOMCode : string.Empty,
                UnitPrice = m.UnitPrice,
                ReorderLevel = m.ReorderLevel,
                MaximumStockLevel = m.MaximumStockLevel,
                CurrentStock = m.CurrentStock,
                PreferredVendorId = m.PreferredVendorId,
                PreferredVendorName = m.PreferredVendor != null ? m.PreferredVendor.CompanyName : null,
                StorageLocation = m.StorageLocation,
                Status = m.Status
            })
            .ToListAsync();

        return ApiResponse<PagedResult<MaterialDto>>.Ok(new PagedResult<MaterialDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<MaterialDto>> GetMaterialByIdAsync(int id)
    {
        var m = await _context.Materials
            .Include(x => x.Category)
            .Include(x => x.BaseUOM)
            .Include(x => x.PreferredVendor)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaterialId == id);

        if (m == null) return ApiResponse<MaterialDto>.Fail("Material not found");

        return ApiResponse<MaterialDto>.Ok(new MaterialDto
        {
            MaterialId = m.MaterialId,
            MaterialCode = m.MaterialCode,
            MaterialName = m.MaterialName,
            Description = m.Description,
            CategoryId = m.CategoryId,
            CategoryName = m.Category?.CategoryName ?? string.Empty,
            MaterialType = m.MaterialType,
            BaseUOMId = m.BaseUOMId,
            BaseUOMCode = m.BaseUOM?.UOMCode ?? string.Empty,
            UnitPrice = m.UnitPrice,
            ReorderLevel = m.ReorderLevel,
            MaximumStockLevel = m.MaximumStockLevel,
            CurrentStock = m.CurrentStock,
            PreferredVendorId = m.PreferredVendorId,
            PreferredVendorName = m.PreferredVendor?.CompanyName,
            StorageLocation = m.StorageLocation,
            Status = m.Status
        });
    }

    public async Task<ApiResponse<MaterialDto>> CreateMaterialAsync(MaterialCreateDto dto)
    {
        if (dto.UnitPrice < 0)
            return ApiResponse<MaterialDto>.Fail("Unit price cannot be negative");

        if (dto.ReorderLevel < 0)
            return ApiResponse<MaterialDto>.Fail("Reorder level cannot be negative");

        if (await _context.Materials.AnyAsync(m => m.MaterialCode.ToLower() == dto.MaterialCode.Trim().ToLower()))
            return ApiResponse<MaterialDto>.Fail($"Material code '{dto.MaterialCode}' already exists");

        var material = new Material
        {
            MaterialCode = dto.MaterialCode.Trim().ToUpper(),
            MaterialName = dto.MaterialName.Trim(),
            Description = dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            MaterialType = dto.MaterialType.Trim(),
            BaseUOMId = dto.BaseUOMId,
            UnitPrice = dto.UnitPrice,
            ReorderLevel = dto.ReorderLevel,
            MaximumStockLevel = dto.MaximumStockLevel,
            CurrentStock = 0,
            PreferredVendorId = dto.PreferredVendorId,
            StorageLocation = dto.StorageLocation.Trim(),
            Status = true
        };

        await _context.Materials.AddAsync(material);
        await _context.SaveChangesAsync();

        // Initialize inventory balance across active warehouses
        var warehouses = await _context.Warehouses.Where(w => w.Status).ToListAsync();
        foreach (var wh in warehouses)
        {
            await _context.InventoryBalances.AddAsync(new InventoryBalance
            {
                MaterialId = material.MaterialId,
                WarehouseId = wh.WarehouseId,
                AvailableStock = 0,
                ReservedStock = 0,
                LastUpdated = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        return await GetMaterialByIdAsync(material.MaterialId);
    }

    public async Task<ApiResponse<MaterialDto>> UpdateMaterialAsync(int id, MaterialUpdateDto dto)
    {
        var material = await _context.Materials.FirstOrDefaultAsync(m => m.MaterialId == id);
        if (material == null) return ApiResponse<MaterialDto>.Fail("Material not found");

        if (dto.UnitPrice < 0)
            return ApiResponse<MaterialDto>.Fail("Unit price cannot be negative");

        if (dto.ReorderLevel < 0)
            return ApiResponse<MaterialDto>.Fail("Reorder level cannot be negative");

        if (await _context.Materials.AnyAsync(m => m.MaterialCode.ToLower() == dto.MaterialCode.Trim().ToLower() && m.MaterialId != id))
            return ApiResponse<MaterialDto>.Fail($"Material code '{dto.MaterialCode}' is already in use");

        material.MaterialCode = dto.MaterialCode.Trim().ToUpper();
        material.MaterialName = dto.MaterialName.Trim();
        material.Description = dto.Description.Trim();
        material.CategoryId = dto.CategoryId;
        material.MaterialType = dto.MaterialType.Trim();
        material.BaseUOMId = dto.BaseUOMId;
        material.UnitPrice = dto.UnitPrice;
        material.ReorderLevel = dto.ReorderLevel;
        material.MaximumStockLevel = dto.MaximumStockLevel;
        material.PreferredVendorId = dto.PreferredVendorId;
        material.StorageLocation = dto.StorageLocation.Trim();
        material.Status = dto.Status;
        material.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetMaterialByIdAsync(id);
    }

    public async Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync()
    {
        var list = await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                ManagerName = d.ManagerName,
                Budget = d.Budget,
                Status = d.Status
            })
            .ToListAsync();

        return ApiResponse<List<DepartmentDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<WarehouseDto>>> GetWarehousesAsync()
    {
        var list = await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.WarehouseName)
            .Select(w => new WarehouseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseCode = w.WarehouseCode,
                WarehouseName = w.WarehouseName,
                Location = w.Location,
                CapacitySqFt = w.CapacitySqFt,
                Status = w.Status
            })
            .ToListAsync();

        return ApiResponse<List<WarehouseDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
    {
        var list = await _context.MaterialCategories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                Description = c.Description
            })
            .ToListAsync();

        return ApiResponse<List<CategoryDto>>.Ok(list);
    }

    public async Task<ApiResponse<List<UomDto>>> GetUnitsOfMeasureAsync()
    {
        var list = await _context.UnitsOfMeasure
            .AsNoTracking()
            .OrderBy(u => u.UOMCode)
            .Select(u => new UomDto
            {
                UOMId = u.UOMId,
                UOMCode = u.UOMCode,
                UOMName = u.UOMName,
                Description = u.Description
            })
            .ToListAsync();

        return ApiResponse<List<UomDto>>.Ok(list);
    }
}
