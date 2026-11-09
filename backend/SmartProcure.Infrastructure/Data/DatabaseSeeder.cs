using Microsoft.EntityFrameworkCore;
using SmartProcure.Application.Common;
using SmartProcure.Domain.Entities;
using SmartProcure.Domain.Enums;

namespace SmartProcure.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(SmartProcureDbContext context)
    {
        // 1. Roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new() { RoleId = 1, RoleName = "Administrator", Description = "Full system administration and configuration" },
                new() { RoleId = 2, RoleName = "RequisitionCreator", Description = "Creates and tracks purchase requisitions" },
                new() { RoleId = 3, RoleName = "PurchaseManager", Description = "Evaluates quotes, approves PRs, issues Purchase Orders" },
                new() { RoleId = 4, RoleName = "WarehouseManager", Description = "Inspects and receives materials, manages warehouse inventory" },
                new() { RoleId = 5, RoleName = "FinanceManager", Description = "Verifies invoices via 3-way matching and records payments" },
                new() { RoleId = 6, RoleName = "ManagementViewer", Description = "Read-only executive analytics and reporting" }
            };
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Departments
        if (!await context.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new() { DepartmentId = 1, DepartmentCode = "DEP-PROD", DepartmentName = "Production & Assembly", ManagerName = "Rajesh Sharma", Budget = 15000000m, Status = true },
                new() { DepartmentId = 2, DepartmentCode = "DEP-PROC", DepartmentName = "Procurement & Sourcing", ManagerName = "Vikram Malhotra", Budget = 5000000m, Status = true },
                new() { DepartmentId = 3, DepartmentCode = "DEP-WHSE", DepartmentName = "Warehouse & Logistics", ManagerName = "Sunil Verma", Budget = 3500000m, Status = true },
                new() { DepartmentId = 4, DepartmentCode = "DEP-FIN", DepartmentName = "Finance & Accounts", ManagerName = "Anjali Deshmukh", Budget = 2000000m, Status = true },
                new() { DepartmentId = 5, DepartmentCode = "DEP-MAINT", DepartmentName = "Plant Maintenance", ManagerName = "Manoj Patel", Budget = 4000000m, Status = true },
                new() { DepartmentId = 6, DepartmentCode = "DEP-IT", DepartmentName = "Information Technology", ManagerName = "Arun Mehta", Budget = 2500000m, Status = true }
            };
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        // 3. Warehouses
        if (!await context.Warehouses.AnyAsync())
        {
            var warehouses = new List<Warehouse>
            {
                new() { WarehouseId = 1, WarehouseCode = "WH-PUN-01", WarehouseName = "Central Raw Material Depot", Location = "Plot 42, MIDC Bhosari, Pune, Maharashtra", CapacitySqFt = 45000m, Status = true },
                new() { WarehouseId = 2, WarehouseCode = "WH-AHM-01", WarehouseName = "Gujarat Spares & Components Hub", Location = "Sanand GIDC Phase II, Ahmedabad, Gujarat", CapacitySqFt = 30000m, Status = true },
                new() { WarehouseId = 3, WarehouseCode = "WH-VAD-01", WarehouseName = "Finished Goods & Consumables Store", Location = "Makarpura Industrial Estate, Vadodara, Gujarat", CapacitySqFt = 25000m, Status = true }
            };
            await context.Warehouses.AddRangeAsync(warehouses);
            await context.SaveChangesAsync();
        }

        // 4. Users (Demo credentials with PBKDF2 hashes)
        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new()
                {
                    UserId = 1,
                    Email = "admin@smartprocure.com",
                    PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                    FullName = "Siddharth Nambiar (Admin)",
                    PhoneNumber = "+91 98201 11223",
                    DepartmentId = 6, // IT
                    RoleId = 1, // Admin
                    IsActive = true
                },
                new()
                {
                    UserId = 2,
                    Email = "requester@smartprocure.com",
                    PasswordHash = PasswordHelper.HashPassword("Request@123"),
                    FullName = "Rahul Kulkarni (Shop Floor)",
                    PhoneNumber = "+91 98202 33445",
                    DepartmentId = 1, // Production
                    RoleId = 2, // RequisitionCreator
                    IsActive = true
                },
                new()
                {
                    UserId = 3,
                    Email = "purchasemanager@smartprocure.com",
                    PasswordHash = PasswordHelper.HashPassword("Purchase@123"),
                    FullName = "Vikram Malhotra (Purchase Head)",
                    PhoneNumber = "+91 98203 55667",
                    DepartmentId = 2, // Procurement
                    RoleId = 3, // PurchaseManager
                    IsActive = true
                },
                new()
                {
                    UserId = 4,
                    Email = "warehouse@smartprocure.com",
                    PasswordHash = PasswordHelper.HashPassword("Warehouse@123"),
                    FullName = "Sunil Verma (Stores Lead)",
                    PhoneNumber = "+91 98204 77889",
                    DepartmentId = 3, // Warehouse
                    RoleId = 4, // WarehouseManager
                    IsActive = true
                },
                new()
                {
                    UserId = 5,
                    Email = "finance@smartprocure.com",
                    PasswordHash = PasswordHelper.HashPassword("Finance@123"),
                    FullName = "Anjali Deshmukh (AP Finance)",
                    PhoneNumber = "+91 98205 99001",
                    DepartmentId = 4, // Finance
                    RoleId = 5, // FinanceManager
                    IsActive = true
                },
                new()
                {
                    UserId = 6,
                    Email = "management@smartprocure.com",
                    PasswordHash = PasswordHelper.HashPassword("Manage@123"),
                    FullName = "Pooja Singhania (VP Operations)",
                    PhoneNumber = "+91 98206 12345",
                    DepartmentId = 1, // Production
                    RoleId = 6, // ManagementViewer
                    IsActive = true
                }
            };
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // 5. Material Categories & Units of Measure
        if (!await context.MaterialCategories.AnyAsync())
        {
            var categories = new List<MaterialCategory>
            {
                new() { CategoryId = 1, CategoryCode = "CAT-MET", CategoryName = "Raw Metals & Structural Steel", Description = "Steel plates, coils, billets and bars" },
                new() { CategoryId = 2, CategoryCode = "CAT-ELEC", CategoryName = "Electrical & Automation", Description = "Wires, PLCs, drives, contactors and sensors" },
                new() { CategoryId = 3, CategoryCode = "CAT-MECH", CategoryName = "Mechanical Spares & Bearings", Description = "Bearings, couplings, fasteners and gears" },
                new() { CategoryId = 4, CategoryCode = "CAT-CHEM", CategoryName = "Lubricants, Oils & Chemicals", Description = "Hydraulic fluids, cutting oils, greases" },
                new() { CategoryId = 5, CategoryCode = "CAT-PKG", CategoryName = "Packaging & Corrugated Materials", Description = "Boxes, straps, stretch films, pallets" },
                new() { CategoryId = 6, CategoryCode = "CAT-CONS", CategoryName = "Industrial Consumables & PPE", Description = "Welding rods, safety gear, grinding wheels" }
            };
            await context.MaterialCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        if (!await context.UnitsOfMeasure.AnyAsync())
        {
            var uoms = new List<UnitOfMeasure>
            {
                new() { UOMId = 1, UOMCode = "KG", UOMName = "Kilogram", Description = "Weight measure" },
                new() { UOMId = 2, UOMCode = "MTR", UOMName = "Meter", Description = "Length measure" },
                new() { UOMId = 3, UOMCode = "PCS", UOMName = "Pieces", Description = "Unit count" },
                new() { UOMId = 4, UOMCode = "LTR", UOMName = "Liter", Description = "Liquid volume" },
                new() { UOMId = 5, UOMCode = "BOX", UOMName = "Box", Description = "Packaging unit" },
                new() { UOMId = 6, UOMCode = "SET", UOMName = "Set", Description = "Assembly set" },
                new() { UOMId = 7, UOMCode = "TON", UOMName = "Metric Ton", Description = "Heavy weight" },
                new() { UOMId = 8, UOMCode = "ROLL", UOMName = "Roll", Description = "Continuous roll" }
            };
            await context.UnitsOfMeasure.AddRangeAsync(uoms);
            await context.SaveChangesAsync();
        }

        // 6. Vendors (8 Indian industrial suppliers)
        if (!await context.Vendors.AnyAsync())
        {
            var vendors = new List<Vendor>
            {
                new()
                {
                    VendorId = 1,
                    VendorCode = "VND-SHREE-01",
                    CompanyName = "Shree Steel Traders Pvt. Ltd.",
                    ContactPerson = "Mahesh Agarwal",
                    Email = "sales@shreesteel.in",
                    Phone = "+91 98220 44551",
                    Address = "Survey 112, Chakan Industrial Phase 2",
                    City = "Pune",
                    State = "Maharashtra",
                    Country = "India",
                    PostalCode = "410501",
                    GSTIN = "27AAACS1234F1Z1",
                    PAN = "AAACS1234F",
                    PaymentTerms = "Net 30",
                    BankAccountRef = "HDFC0001234-9876543210",
                    BankName = "HDFC Bank",
                    IFSCCode = "HDFC0001234",
                    VendorCategory = "Raw Metals & Structural Steel",
                    VendorRating = 4.8m,
                    Status = true
                },
                new()
                {
                    VendorId = 2,
                    VendorCode = "VND-GUJ-02",
                    CompanyName = "Gujarat Industrial Supplies Corp",
                    ContactPerson = "Bhavesh Patel",
                    Email = "info@gujaratindustrial.com",
                    Phone = "+91 98791 22334",
                    Address = "GIDC Naroda Industrial Estate, Phase 3",
                    City = "Ahmedabad",
                    State = "Gujarat",
                    Country = "India",
                    PostalCode = "382330",
                    GSTIN = "24AABCG5678H1Z5",
                    PAN = "AABCG5678H",
                    PaymentTerms = "Net 45",
                    BankAccountRef = "ICIC0000456-1122334455",
                    BankName = "ICICI Bank",
                    IFSCCode = "ICIC0000456",
                    VendorCategory = "Mechanical Spares & Bearings",
                    VendorRating = 4.6m,
                    Status = true
                },
                new()
                {
                    VendorId = 3,
                    VendorCode = "VND-ELEC-03",
                    CompanyName = "ElectroTech Components India Ltd.",
                    ContactPerson = "Sunita Rao",
                    Email = "orders@electrotech.co.in",
                    Phone = "+91 99801 77665",
                    Address = "Electronic City Phase 1, Hosur Road",
                    City = "Bengaluru",
                    State = "Karnataka",
                    Country = "India",
                    PostalCode = "560100",
                    GSTIN = "29AAACE9988D1ZR",
                    PAN = "AAACE9988D",
                    PaymentTerms = "Net 30",
                    BankAccountRef = "SBIN0004589-5544332211",
                    BankName = "State Bank of India",
                    IFSCCode = "SBIN0004589",
                    VendorCategory = "Electrical & Automation",
                    VendorRating = 4.9m,
                    Status = true
                },
                new()
                {
                    VendorId = 4,
                    VendorCode = "VND-PREC-04",
                    CompanyName = "Precision Components & Bearings LLP",
                    ContactPerson = "Girish Kulkarni",
                    Email = "contact@precisionbearings.in",
                    Phone = "+91 98230 88990",
                    Address = "Plot 88, Pimpri Industrial Area",
                    City = "Pune",
                    State = "Maharashtra",
                    Country = "India",
                    PostalCode = "411018",
                    GSTIN = "27AABFP1029K1ZY",
                    PAN = "AABFP1029K",
                    PaymentTerms = "Net 30",
                    BankAccountRef = "UTIB0000210-9988776655",
                    BankName = "Axis Bank",
                    IFSCCode = "UTIB0000210",
                    VendorCategory = "Mechanical Spares & Bearings",
                    VendorRating = 4.4m,
                    Status = true
                },
                new()
                {
                    VendorId = 5,
                    VendorCode = "VND-NAT-05",
                    CompanyName = "National Packaging Solutions",
                    ContactPerson = "Alok Tandon",
                    Email = "sales@natpack.com",
                    Phone = "+91 97110 55443",
                    Address = "Sector 24, Industrial Hub",
                    City = "Faridabad",
                    State = "Haryana",
                    Country = "India",
                    PostalCode = "121005",
                    GSTIN = "06AAAFN7766M1Z2",
                    PAN = "AAAFN7766M",
                    PaymentTerms = "Net 15",
                    BankAccountRef = "KKBK0000122-3322114455",
                    BankName = "Kotak Mahindra Bank",
                    IFSCCode = "KKBK0000122",
                    VendorCategory = "Packaging & Corrugated Materials",
                    VendorRating = 4.2m,
                    Status = true
                },
                new()
                {
                    VendorId = 6,
                    VendorCode = "VND-PETRO-06",
                    CompanyName = "Indian Petrochemicals & Lubricants Co.",
                    ContactPerson = "Dinesh Shah",
                    Email = "dispatch@indopetro.in",
                    Phone = "+91 98250 11992",
                    Address = "Ranoli Industrial Area",
                    City = "Vadodara",
                    State = "Gujarat",
                    Country = "India",
                    PostalCode = "391350",
                    GSTIN = "24AAACI5544L1Z9",
                    PAN = "AAACI5544L",
                    PaymentTerms = "Net 45",
                    BankAccountRef = "BARB0VADODA-1231231231",
                    BankName = "Bank of Baroda",
                    IFSCCode = "BARB0VADODA",
                    VendorCategory = "Lubricants, Oils & Chemicals",
                    VendorRating = 4.7m,
                    Status = true
                },
                new()
                {
                    VendorId = 7,
                    VendorCode = "VND-APEX-07",
                    CompanyName = "Apex Industrial Fasteners & Hardware",
                    ContactPerson = "Ramanathan Iyer",
                    Email = "sales@apexfasteners.com",
                    Phone = "+91 94440 66778",
                    Address = "Ambattur Industrial Estate",
                    City = "Chennai",
                    State = "Tamil Nadu",
                    Country = "India",
                    PostalCode = "600058",
                    GSTIN = "33AAACA9922P1ZT",
                    PAN = "AAACA9922P",
                    PaymentTerms = "Net 30",
                    BankAccountRef = "IDIB000A123-9900112233",
                    BankName = "Indian Bank",
                    IFSCCode = "IDIB000A123",
                    VendorCategory = "Mechanical Spares & Bearings",
                    VendorRating = 4.5m,
                    Status = true
                },
                new()
                {
                    VendorId = 8,
                    VendorCode = "VND-SAFETY-08",
                    CompanyName = "Suraksha Safety Gear & Consumables",
                    ContactPerson = "Harpreet Singh",
                    Email = "orders@surakshasafety.in",
                    Phone = "+91 98140 22110",
                    Address = "Focal Point Phase IV",
                    City = "Ludhiana",
                    State = "Punjab",
                    Country = "India",
                    PostalCode = "141010",
                    GSTIN = "03AAAPS4433J1Z8",
                    PAN = "AAAPS4433J",
                    PaymentTerms = "Net 30",
                    BankAccountRef = "PUNB0009876-8877665544",
                    BankName = "Punjab National Bank",
                    IFSCCode = "PUNB0009876",
                    VendorCategory = "Industrial Consumables & PPE",
                    VendorRating = 4.3m,
                    Status = true
                }
            };
            await context.Vendors.AddRangeAsync(vendors);
            await context.SaveChangesAsync();
        }

        // 7. Materials (22 realistic manufacturing items)
        if (!await context.Materials.AnyAsync())
        {
            var materials = new List<Material>
            {
                new() { MaterialId = 1, MaterialCode = "MAT-STL-001", MaterialName = "HR Steel Sheet 2.0mm (IS 2062)", Description = "Hot rolled structural grade steel sheet 2500x1250mm", CategoryId = 1, BaseUOMId = 1, UnitPrice = 68.50m, ReorderLevel = 5000m, MaximumStockLevel = 25000m, CurrentStock = 12400m, PreferredVendorId = 1, StorageLocation = "Bay A1", Status = true },
                new() { MaterialId = 2, MaterialCode = "MAT-STL-002", MaterialName = "CR Steel Coil 1.2mm (CR1)", Description = "Cold rolled precision coil for deep drawing components", CategoryId = 1, BaseUOMId = 1, UnitPrice = 82.00m, ReorderLevel = 4000m, MaximumStockLevel = 20000m, CurrentStock = 3800m, PreferredVendorId = 1, StorageLocation = "Bay A2", Status = true }, // Low stock!
                new() { MaterialId = 3, MaterialCode = "MAT-STL-003", MaterialName = "Alloy Steel Round Bar 50mm (EN8)", Description = "Medium carbon steel round bar for shaft fabrication", CategoryId = 1, BaseUOMId = 1, UnitPrice = 95.00m, ReorderLevel = 2000m, MaximumStockLevel = 10000m, CurrentStock = 4500m, PreferredVendorId = 1, StorageLocation = "Bay A3", Status = true },
                new() { MaterialId = 4, MaterialCode = "MAT-COP-001", MaterialName = "Copper Winding Wire 18 SWG", Description = "Dual coated super enameled copper winding wire for coils", CategoryId = 2, BaseUOMId = 1, UnitPrice = 840.00m, ReorderLevel = 300m, MaximumStockLevel = 1500m, CurrentStock = 240m, PreferredVendorId = 3, StorageLocation = "Rack E1", Status = true }, // Low stock!
                new() { MaterialId = 5, MaterialCode = "MAT-PLC-001", MaterialName = "PLC Controller 24V DC (16DI/16DO)", Description = "Modular programmable automation controller with Ethernet/IP", CategoryId = 2, BaseUOMId = 3, UnitPrice = 28500.00m, ReorderLevel = 5m, MaximumStockLevel = 25m, CurrentStock = 8m, PreferredVendorId = 3, StorageLocation = "Lock Box C1", Status = true },
                new() { MaterialId = 6, MaterialCode = "MAT-BRG-001", MaterialName = "Deep Groove Ball Bearing 6205-2RS", Description = "Rubber sealed precision bearing for drive motors", CategoryId = 3, BaseUOMId = 3, UnitPrice = 320.00m, ReorderLevel = 100m, MaximumStockLevel = 800m, CurrentStock = 280m, PreferredVendorId = 4, StorageLocation = "Bin M1", Status = true },
                new() { MaterialId = 7, MaterialCode = "MAT-BRG-002", MaterialName = "Spherical Roller Bearing 22212 E", Description = "Heavy duty self-aligning bearing for conveyor rollers", CategoryId = 3, BaseUOMId = 3, UnitPrice = 2450.00m, ReorderLevel = 20m, MaximumStockLevel = 100m, CurrentStock = 14m, PreferredVendorId = 4, StorageLocation = "Bin M2", Status = true }, // Low stock!
                new() { MaterialId = 8, MaterialCode = "MAT-LUB-001", MaterialName = "Industrial Gear Oil ISO VG 220", Description = "Extreme pressure synthetic gear oil in 210L barrel", CategoryId = 4, BaseUOMId = 4, UnitPrice = 240.00m, ReorderLevel = 1000m, MaximumStockLevel = 5000m, CurrentStock = 2100m, PreferredVendorId = 6, StorageLocation = "Drum Yard D1", Status = true },
                new() { MaterialId = 9, MaterialCode = "MAT-LUB-002", MaterialName = "Hydraulic Fluid HLP 46", Description = "Anti-wear hydraulic fluid for CNC press machines", CategoryId = 4, BaseUOMId = 4, UnitPrice = 195.00m, ReorderLevel = 800m, MaximumStockLevel = 4000m, CurrentStock = 650m, PreferredVendorId = 6, StorageLocation = "Drum Yard D2", Status = true }, // Low stock!
                new() { MaterialId = 10, MaterialCode = "MAT-PKG-001", MaterialName = "Heavy Duty Corrugated Box (7 Ply)", Description = "Export grade packing box 600x400x400mm", CategoryId = 5, BaseUOMId = 3, UnitPrice = 85.00m, ReorderLevel = 500m, MaximumStockLevel = 3000m, CurrentStock = 1200m, PreferredVendorId = 5, StorageLocation = "Shed P1", Status = true },
                new() { MaterialId = 11, MaterialCode = "MAT-PKG-002", MaterialName = "HDPE Stretch Wrap Film 23 Micron", Description = "Pallet wrapping transparent stretch film roll 500mm x 300m", CategoryId = 5, BaseUOMId = 8, UnitPrice = 650.00m, ReorderLevel = 50m, MaximumStockLevel = 300m, CurrentStock = 110m, PreferredVendorId = 5, StorageLocation = "Shed P2", Status = true },
                new() { MaterialId = 12, MaterialCode = "MAT-FST-001", MaterialName = "Hex Bolt M12x60 Grade 8.8 (Galv)", Description = "High tensile structural hex bolt with washer and nut", CategoryId = 3, BaseUOMId = 3, UnitPrice = 14.50m, ReorderLevel = 2000m, MaximumStockLevel = 15000m, CurrentStock = 6200m, PreferredVendorId = 7, StorageLocation = "Bin F1", Status = true },
                new() { MaterialId = 13, MaterialCode = "MAT-FST-002", MaterialName = "Socket Head Cap Screw M8x30 (SS304)", Description = "Stainless steel Allen bolt for sanitary machine guards", CategoryId = 3, BaseUOMId = 3, UnitPrice = 18.00m, ReorderLevel = 1000m, MaximumStockLevel = 8000m, CurrentStock = 3400m, PreferredVendorId = 7, StorageLocation = "Bin F2", Status = true },
                new() { MaterialId = 14, MaterialCode = "MAT-SAF-001", MaterialName = "Heavy Duty Nitrile Dipped Safety Gloves", Description = "Cut level 5 protective industrial handling gloves", CategoryId = 6, BaseUOMId = 3, UnitPrice = 120.00m, ReorderLevel = 200m, MaximumStockLevel = 1000m, CurrentStock = 450m, PreferredVendorId = 8, StorageLocation = "PPE Rack S1", Status = true },
                new() { MaterialId = 15, MaterialCode = "MAT-SAF-002", MaterialName = "Industrial Safety Helmet (ISI Marked)", Description = "High-density polymer helmet with adjustable ratchet suspension", CategoryId = 6, BaseUOMId = 3, UnitPrice = 280.00m, ReorderLevel = 50m, MaximumStockLevel = 250m, CurrentStock = 90m, PreferredVendorId = 8, StorageLocation = "PPE Rack S2", Status = true },
                new() { MaterialId = 16, MaterialCode = "MAT-WLD-001", MaterialName = "MIG Welding Wire ER70S-6 (1.2mm)", Description = "Copper coated mild steel solid wire spool 15kg", CategoryId = 6, BaseUOMId = 8, UnitPrice = 1850.00m, ReorderLevel = 30m, MaximumStockLevel = 150m, CurrentStock = 22m, PreferredVendorId = 8, StorageLocation = "Welding Bay W1", Status = true }, // Low stock!
                new() { MaterialId = 17, MaterialCode = "MAT-CAB-001", MaterialName = "Control Cable 4 Core 1.5 sq mm", Description = "Flexible copper shielded instrumentation cable 1100V", CategoryId = 2, BaseUOMId = 2, UnitPrice = 64.00m, ReorderLevel = 500m, MaximumStockLevel = 3000m, CurrentStock = 1400m, PreferredVendorId = 3, StorageLocation = "Cable Drum C2", Status = true },
                new() { MaterialId = 18, MaterialCode = "MAT-VFD-001", MaterialName = "Variable Frequency Drive 7.5kW (VFD)", Description = "3-Phase 415V AC variable speed drive with Modbus RTU", CategoryId = 2, BaseUOMId = 3, UnitPrice = 42000.00m, ReorderLevel = 2m, MaximumStockLevel = 10m, CurrentStock = 4m, PreferredVendorId = 3, StorageLocation = "Lock Box C2", Status = true },
                new() { MaterialId = 19, MaterialCode = "MAT-VALV-001", MaterialName = "Pneumatic Solenoid Valve 5/2 Way G1/4", Description = "Pilot operated double solenoid directional control valve", CategoryId = 3, BaseUOMId = 3, UnitPrice = 1850.00m, ReorderLevel = 15m, MaximumStockLevel = 80m, CurrentStock = 32m, PreferredVendorId = 2, StorageLocation = "Bin P3", Status = true },
                new() { MaterialId = 20, MaterialCode = "MAT-SEAL-001", MaterialName = "Rotary Shaft Oil Seal 45x65x10 (NBR)", Description = "Double lip nitrile rubber oil seal for motor gearboxes", CategoryId = 3, BaseUOMId = 3, UnitPrice = 95.00m, ReorderLevel = 150m, MaximumStockLevel = 800m, CurrentStock = 310m, PreferredVendorId = 2, StorageLocation = "Bin S3", Status = true },
                new() { MaterialId = 21, MaterialCode = "MAT-PAL-001", MaterialName = "Heavy Duty Euro Wooden Pallet", Description = "Heat treated ISPM-15 compliant pallet 1200x800mm (1.5 Ton)", CategoryId = 5, BaseUOMId = 3, UnitPrice = 750.00m, ReorderLevel = 100m, MaximumStockLevel = 600m, CurrentStock = 240m, PreferredVendorId = 5, StorageLocation = "Yard P3", Status = true },
                new() { MaterialId = 22, MaterialCode = "MAT-CUT-001", MaterialName = "Water Soluble Semi-Synthetic Cutting Coolant", Description = "Bio-stable metalworking fluid concentrate in 205L drum", CategoryId = 4, BaseUOMId = 4, UnitPrice = 280.00m, ReorderLevel = 400m, MaximumStockLevel = 2000m, CurrentStock = 950m, PreferredVendorId = 6, StorageLocation = "Drum Yard D3", Status = true }
            };
            await context.Materials.AddRangeAsync(materials);
            await context.SaveChangesAsync();

            // Seed inventory balances for all materials across Warehouse 1, 2, 3
            var balances = new List<InventoryBalance>();
            var transactions = new List<InventoryTransaction>();
            foreach (var mat in materials)
            {
                decimal wh1Stock = Math.Round(mat.CurrentStock * 0.7m, 2);
                decimal wh2Stock = Math.Round(mat.CurrentStock * 0.3m, 2);

                balances.Add(new() { MaterialId = mat.MaterialId, WarehouseId = 1, AvailableStock = wh1Stock, ReservedStock = 0, LastUpdated = DateTime.UtcNow });
                balances.Add(new() { MaterialId = mat.MaterialId, WarehouseId = 2, AvailableStock = wh2Stock, ReservedStock = 0, LastUpdated = DateTime.UtcNow });
                balances.Add(new() { MaterialId = mat.MaterialId, WarehouseId = 3, AvailableStock = 0, ReservedStock = 0, LastUpdated = DateTime.UtcNow });

                transactions.Add(new()
                {
                    MaterialId = mat.MaterialId,
                    WarehouseId = 1,
                    TransactionType = InventoryTransactionType.StockAdjustment,
                    Quantity = wh1Stock,
                    ReferenceDocumentType = "InitialStock",
                    ReferenceDocumentNumber = "INIT-BALANCE-01",
                    TransactionDate = DateTime.UtcNow.AddDays(-30),
                    CreatedByUserId = 1,
                    Remarks = "Initial audited physical balance migration"
                });
            }
            await context.InventoryBalances.AddRangeAsync(balances);
            await context.InventoryTransactions.AddRangeAsync(transactions);
            await context.SaveChangesAsync();
        }

        // 8. Purchase Requisitions (16 realistic PRs in diverse states)
        if (!await context.PurchaseRequisitions.AnyAsync())
        {
            var prs = new List<PurchaseRequisition>
            {
                new()
                {
                    RequisitionId = 1,
                    RequisitionNumber = "PR-2026-0001",
                    RequestedByUserId = 2,
                    DepartmentId = 1, // Production
                    RequestDate = DateTime.UtcNow.AddDays(-28),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(-14),
                    Priority = PriorityLevel.High,
                    TotalEstimatedAmount = 685000.00m,
                    BusinessJustification = "Structural steel sheets required for Q4 chassis fabrication order #APX-9821",
                    Status = RequisitionStatus.ConvertedToPO,
                    ApprovalRemarks = "Approved within quarterly production budget allocation",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-27)
                },
                new()
                {
                    RequisitionId = 2,
                    RequisitionNumber = "PR-2026-0002",
                    RequestedByUserId = 2,
                    DepartmentId = 1,
                    RequestDate = DateTime.UtcNow.AddDays(-25),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(-10),
                    Priority = PriorityLevel.Medium,
                    TotalEstimatedAmount = 252000.00m,
                    BusinessJustification = "Copper winding wire replenishment for transformer assembly line 2",
                    Status = RequisitionStatus.ConvertedToPO,
                    ApprovalRemarks = "Approved as per production schedule",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-24)
                },
                new()
                {
                    RequisitionId = 3,
                    RequisitionNumber = "PR-2026-0003",
                    RequestedByUserId = 2,
                    DepartmentId = 5, // Maintenance
                    RequestDate = DateTime.UtcNow.AddDays(-22),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(-8),
                    Priority = PriorityLevel.Urgent,
                    TotalEstimatedAmount = 96000.00m,
                    BusinessJustification = "Replacement deep groove bearings for CNC milling spindle maintenance",
                    Status = RequisitionStatus.ConvertedToPO,
                    ApprovalRemarks = "Urgent maintenance requirement approved",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-21)
                },
                new()
                {
                    RequisitionId = 4,
                    RequisitionNumber = "PR-2026-0004",
                    RequestedByUserId = 2,
                    DepartmentId = 5,
                    RequestDate = DateTime.UtcNow.AddDays(-20),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(-5),
                    Priority = PriorityLevel.Medium,
                    TotalEstimatedAmount = 100800.00m,
                    BusinessJustification = "Scheduled replacement of industrial gear oil ISO 220 for stamping presses",
                    Status = RequisitionStatus.ConvertedToPO,
                    ApprovalRemarks = "Approved for preventive maintenance cycle",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-19)
                },
                new()
                {
                    RequisitionId = 5,
                    RequisitionNumber = "PR-2026-0005",
                    RequestedByUserId = 2,
                    DepartmentId = 1,
                    RequestDate = DateTime.UtcNow.AddDays(-18),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(-3),
                    Priority = PriorityLevel.High,
                    TotalEstimatedAmount = 142500.00m,
                    BusinessJustification = "PLC controllers for automated robotic packaging cell integration",
                    Status = RequisitionStatus.ConvertedToPO,
                    ApprovalRemarks = "Approved by Senior Management for automation upgrade",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-17)
                },
                new()
                {
                    RequisitionId = 6,
                    RequisitionNumber = "PR-2026-0006",
                    RequestedByUserId = 2,
                    DepartmentId = 1,
                    RequestDate = DateTime.UtcNow.AddDays(-14),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(7),
                    Priority = PriorityLevel.Medium,
                    TotalEstimatedAmount = 102000.00m,
                    BusinessJustification = "7-Ply corrugated export cartons for overseas shipment delivery",
                    Status = RequisitionStatus.Approved,
                    ApprovalRemarks = "Approved. Ready for RFQ and PO conversion.",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-13)
                },
                new()
                {
                    RequisitionId = 7,
                    RequisitionNumber = "PR-2026-0007",
                    RequestedByUserId = 2,
                    DepartmentId = 1,
                    RequestDate = DateTime.UtcNow.AddDays(-10),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(10),
                    Priority = PriorityLevel.Urgent,
                    TotalEstimatedAmount = 168000.00m,
                    BusinessJustification = "Critical copper winding wire replenishment due to buffer breach",
                    Status = RequisitionStatus.PendingApproval,
                    ApprovalRemarks = null
                },
                new()
                {
                    RequisitionId = 8,
                    RequisitionNumber = "PR-2026-0008",
                    RequestedByUserId = 2,
                    DepartmentId = 5,
                    RequestDate = DateTime.UtcNow.AddDays(-8),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(12),
                    Priority = PriorityLevel.Low,
                    TotalEstimatedAmount = 14500.00m,
                    BusinessJustification = "Hex bolts M12x60 stock replenishment for general fabrication shop",
                    Status = RequisitionStatus.Submitted,
                    ApprovalRemarks = null
                },
                new()
                {
                    RequisitionId = 9,
                    RequisitionNumber = "PR-2026-0009",
                    RequestedByUserId = 2,
                    DepartmentId = 5,
                    RequestDate = DateTime.UtcNow.AddDays(-5),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(5),
                    Priority = PriorityLevel.Medium,
                    TotalEstimatedAmount = 49000.00m,
                    BusinessJustification = "Spherical roller bearings for conveyor line overhaul",
                    Status = RequisitionStatus.PendingApproval,
                    ApprovalRemarks = null
                },
                new()
                {
                    RequisitionId = 10,
                    RequisitionNumber = "PR-2026-0010",
                    RequestedByUserId = 2,
                    DepartmentId = 1,
                    RequestDate = DateTime.UtcNow.AddDays(-3),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(15),
                    Priority = PriorityLevel.Low,
                    TotalEstimatedAmount = 36000.00m,
                    BusinessJustification = "Safety gloves and PPE helmets for incoming contract workforce",
                    Status = RequisitionStatus.Draft,
                    ApprovalRemarks = null
                },
                new()
                {
                    RequisitionId = 11,
                    RequisitionNumber = "PR-2026-0011",
                    RequestedByUserId = 2,
                    DepartmentId = 5,
                    RequestDate = DateTime.UtcNow.AddDays(-15),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(-2),
                    Priority = PriorityLevel.High,
                    TotalEstimatedAmount = 210000.00m,
                    BusinessJustification = "Requested 5 backup VFD drives for standby inventory",
                    Status = RequisitionStatus.Rejected,
                    ApprovalRemarks = "Rejected: Existing plant spares register indicates 2 functional units already in WH-2. Unnecessary capital lock-in.",
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-14)
                },
                new()
                {
                    RequisitionId = 12,
                    RequisitionNumber = "PR-2026-0012",
                    RequestedByUserId = 2,
                    DepartmentId = 1,
                    RequestDate = DateTime.UtcNow.AddDays(-2),
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(14),
                    Priority = PriorityLevel.High,
                    TotalEstimatedAmount = 328000.00m,
                    BusinessJustification = "Cold Rolled Steel Coil 1.2mm for export project #EXP-2026-IND",
                    Status = RequisitionStatus.PendingApproval,
                    ApprovalRemarks = null
                }
            };
            await context.PurchaseRequisitions.AddRangeAsync(prs);
            await context.SaveChangesAsync();

            // Line items for PRs
            var prItems = new List<PurchaseRequisitionItem>
            {
                new() { RequisitionId = 1, MaterialId = 1, Quantity = 10000m, EstimatedUnitPrice = 68.50m, LineTotal = 685000.00m, Remarks = "HR Steel Sheet 2mm" },
                new() { RequisitionId = 2, MaterialId = 4, Quantity = 300m, EstimatedUnitPrice = 840.00m, LineTotal = 252000.00m, Remarks = "Copper Winding Wire 18 SWG" },
                new() { RequisitionId = 3, MaterialId = 6, Quantity = 300m, EstimatedUnitPrice = 320.00m, LineTotal = 96000.00m, Remarks = "Deep Groove Ball Bearing 6205" },
                new() { RequisitionId = 4, MaterialId = 8, Quantity = 420m, EstimatedUnitPrice = 240.00m, LineTotal = 100800.00m, Remarks = "Gear Oil 210L x 2 Barrels" },
                new() { RequisitionId = 5, MaterialId = 5, Quantity = 5m, EstimatedUnitPrice = 28500.00m, LineTotal = 142500.00m, Remarks = "PLC Controllers 24V DC" },
                new() { RequisitionId = 6, MaterialId = 10, Quantity = 1200m, EstimatedUnitPrice = 85.00m, LineTotal = 102000.00m, Remarks = "7-Ply Packing Boxes" },
                new() { RequisitionId = 7, MaterialId = 4, Quantity = 200m, EstimatedUnitPrice = 840.00m, LineTotal = 168000.00m, Remarks = "Urgent copper wire" },
                new() { RequisitionId = 8, MaterialId = 12, Quantity = 1000m, EstimatedUnitPrice = 14.50m, LineTotal = 14500.00m, Remarks = "M12 Bolts" },
                new() { RequisitionId = 9, MaterialId = 7, Quantity = 20m, EstimatedUnitPrice = 2450.00m, LineTotal = 49000.00m, Remarks = "Roller Bearings" },
                new() { RequisitionId = 10, MaterialId = 14, Quantity = 300m, EstimatedUnitPrice = 120.00m, LineTotal = 36000.00m, Remarks = "Safety gloves" },
                new() { RequisitionId = 11, MaterialId = 18, Quantity = 5m, EstimatedUnitPrice = 42000.00m, LineTotal = 210000.00m, Remarks = "VFD Drives" },
                new() { RequisitionId = 12, MaterialId = 2, Quantity = 4000m, EstimatedUnitPrice = 82.00m, LineTotal = 328000.00m, Remarks = "CR Steel Coil 1.2mm" }
            };
            await context.PurchaseRequisitionItems.AddRangeAsync(prItems);

            // Approval histories
            var histories = new List<ApprovalHistory>
            {
                new() { RequisitionId = 1, ApproverUserId = 3, Action = "Approved", Remarks = "Budget verified against Q4 manufacturing plan", ActionDate = DateTime.UtcNow.AddDays(-27) },
                new() { RequisitionId = 2, ApproverUserId = 3, Action = "Approved", Remarks = "Approved for winding line", ActionDate = DateTime.UtcNow.AddDays(-24) },
                new() { RequisitionId = 3, ApproverUserId = 3, Action = "Approved", Remarks = "Spindle breakdown preventative maintenance approved", ActionDate = DateTime.UtcNow.AddDays(-21) },
                new() { RequisitionId = 4, ApproverUserId = 3, Action = "Approved", Remarks = "Lubricant schedule cleared", ActionDate = DateTime.UtcNow.AddDays(-19) },
                new() { RequisitionId = 5, ApproverUserId = 3, Action = "Approved", Remarks = "Automation upgrade approved", ActionDate = DateTime.UtcNow.AddDays(-17) },
                new() { RequisitionId = 6, ApproverUserId = 3, Action = "Approved", Remarks = "Packaging approved", ActionDate = DateTime.UtcNow.AddDays(-13) },
                new() { RequisitionId = 11, ApproverUserId = 3, Action = "Rejected", Remarks = "Excess spares in stock", ActionDate = DateTime.UtcNow.AddDays(-14) }
            };
            await context.ApprovalHistories.AddRangeAsync(histories);
            await context.SaveChangesAsync();
        }

        // 9. Purchase Orders (10 realistic enterprise POs)
        if (!await context.PurchaseOrders.AnyAsync())
        {
            var pos = new List<PurchaseOrder>
            {
                new()
                {
                    POId = 1,
                    PONumber = "PO-2026-0001",
                    VendorId = 1, // Shree Steel
                    RequisitionId = 1,
                    PODate = DateTime.UtcNow.AddDays(-26),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(-14),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 685000.00m,
                    TaxAmount = 123300.00m, // 18% GST
                    DiscountAmount = 5000.00m,
                    GrandTotal = 803300.00m,
                    Status = PurchaseOrderStatus.FullyReceived,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-26)
                },
                new()
                {
                    POId = 2,
                    PONumber = "PO-2026-0002",
                    VendorId = 3, // ElectroTech
                    RequisitionId = 2,
                    PODate = DateTime.UtcNow.AddDays(-23),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(-10),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 252000.00m,
                    TaxAmount = 45360.00m,
                    DiscountAmount = 0m,
                    GrandTotal = 297360.00m,
                    Status = PurchaseOrderStatus.PartiallyReceived,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-23)
                },
                new()
                {
                    POId = 3,
                    PONumber = "PO-2026-0003",
                    VendorId = 4, // Precision Bearings
                    RequisitionId = 3,
                    PODate = DateTime.UtcNow.AddDays(-20),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(-8),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 96000.00m,
                    TaxAmount = 17280.00m,
                    DiscountAmount = 1000.00m,
                    GrandTotal = 112280.00m,
                    Status = PurchaseOrderStatus.FullyReceived,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-20)
                },
                new()
                {
                    POId = 4,
                    PONumber = "PO-2026-0004",
                    VendorId = 6, // Indian Petrochemicals
                    RequisitionId = 4,
                    PODate = DateTime.UtcNow.AddDays(-18),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(-5),
                    PaymentTerms = "Net 45",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 100800.00m,
                    TaxAmount = 18144.00m,
                    DiscountAmount = 0m,
                    GrandTotal = 118944.00m,
                    Status = PurchaseOrderStatus.FullyReceived,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-18)
                },
                new()
                {
                    POId = 5,
                    PONumber = "PO-2026-0005",
                    VendorId = 3, // ElectroTech
                    RequisitionId = 5,
                    PODate = DateTime.UtcNow.AddDays(-16),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(2),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 142500.00m,
                    TaxAmount = 25650.00m,
                    DiscountAmount = 2500.00m,
                    GrandTotal = 165650.00m,
                    Status = PurchaseOrderStatus.SentToVendor,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-16)
                },
                new()
                {
                    POId = 6,
                    PONumber = "PO-2026-0006",
                    VendorId = 5, // National Packaging
                    RequisitionId = 6,
                    PODate = DateTime.UtcNow.AddDays(-11),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(4),
                    PaymentTerms = "Net 15",
                    ShippingAddress = "Apex Manufacturing, Makarpura, Vadodara",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 3,
                    Subtotal = 102000.00m,
                    TaxAmount = 18360.00m,
                    DiscountAmount = 0m,
                    GrandTotal = 120360.00m,
                    Status = PurchaseOrderStatus.SentToVendor,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-11)
                },
                new()
                {
                    POId = 7,
                    PONumber = "PO-2026-0007",
                    VendorId = 7, // Apex Fasteners
                    RequisitionId = null,
                    PODate = DateTime.UtcNow.AddDays(-8),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(6),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 43500.00m,
                    TaxAmount = 7830.00m,
                    DiscountAmount = 0m,
                    GrandTotal = 51330.00m,
                    Status = PurchaseOrderStatus.Approved,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-8)
                },
                new()
                {
                    POId = 8,
                    PONumber = "PO-2026-0008",
                    VendorId = 8, // Suraksha Safety
                    RequisitionId = null,
                    PODate = DateTime.UtcNow.AddDays(-6),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(8),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 36000.00m,
                    TaxAmount = 6480.00m,
                    DiscountAmount = 0m,
                    GrandTotal = 42480.00m,
                    Status = PurchaseOrderStatus.SentToVendor,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-6)
                },
                new()
                {
                    POId = 9,
                    PONumber = "PO-2026-0009",
                    VendorId = 2, // Gujarat Industrial
                    RequisitionId = null,
                    PODate = DateTime.UtcNow.AddDays(-4),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(10),
                    PaymentTerms = "Net 45",
                    ShippingAddress = "Sanand GIDC Phase II, Ahmedabad",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 2,
                    Subtotal = 55500.00m,
                    TaxAmount = 9990.00m,
                    DiscountAmount = 1000.00m,
                    GrandTotal = 64490.00m,
                    Status = PurchaseOrderStatus.Approved,
                    CreatedByUserId = 3,
                    ApprovedByUserId = 3,
                    ApprovedDate = DateTime.UtcNow.AddDays(-4)
                },
                new()
                {
                    POId = 10,
                    PONumber = "PO-2026-0010",
                    VendorId = 1, // Shree Steel
                    RequisitionId = null,
                    PODate = DateTime.UtcNow.AddDays(-2),
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(12),
                    PaymentTerms = "Net 30",
                    ShippingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    BillingAddress = "Apex Manufacturing, Plot 42, MIDC Bhosari, Pune - 410501",
                    WarehouseId = 1,
                    Subtotal = 328000.00m,
                    TaxAmount = 59040.00m,
                    DiscountAmount = 3000.00m,
                    GrandTotal = 384040.00m,
                    Status = PurchaseOrderStatus.Draft,
                    CreatedByUserId = 3,
                    ApprovedByUserId = null
                }
            };
            await context.PurchaseOrders.AddRangeAsync(pos);
            await context.SaveChangesAsync();

            // Line items for POs
            var poItems = new List<PurchaseOrderItem>
            {
                new() { POId = 1, MaterialId = 1, Quantity = 10000m, UnitPrice = 68.50m, TaxRate = 18m, LineSubtotal = 685000.00m, LineTax = 123300.00m, LineTotal = 808300.00m, ReceivedQuantity = 10000m },
                new() { POId = 2, MaterialId = 4, Quantity = 300m, UnitPrice = 840.00m, TaxRate = 18m, LineSubtotal = 252000.00m, LineTax = 45360.00m, LineTotal = 297360.00m, ReceivedQuantity = 180m },
                new() { POId = 3, MaterialId = 6, Quantity = 300m, UnitPrice = 320.00m, TaxRate = 18m, LineSubtotal = 96000.00m, LineTax = 17280.00m, LineTotal = 113280.00m, ReceivedQuantity = 300m },
                new() { POId = 4, MaterialId = 8, Quantity = 420m, UnitPrice = 240.00m, TaxRate = 18m, LineSubtotal = 100800.00m, LineTax = 18144.00m, LineTotal = 118944.00m, ReceivedQuantity = 420m },
                new() { POId = 5, MaterialId = 5, Quantity = 5m, UnitPrice = 28500.00m, TaxRate = 18m, LineSubtotal = 142500.00m, LineTax = 25650.00m, LineTotal = 168150.00m, ReceivedQuantity = 0m },
                new() { POId = 6, MaterialId = 10, Quantity = 1200m, UnitPrice = 85.00m, TaxRate = 18m, LineSubtotal = 102000.00m, LineTax = 18360.00m, LineTotal = 120360.00m, ReceivedQuantity = 0m },
                new() { POId = 7, MaterialId = 12, Quantity = 3000m, UnitPrice = 14.50m, TaxRate = 18m, LineSubtotal = 43500.00m, LineTax = 7830.00m, LineTotal = 51330.00m, ReceivedQuantity = 0m },
                new() { POId = 8, MaterialId = 14, Quantity = 300m, UnitPrice = 120.00m, TaxRate = 18m, LineSubtotal = 36000.00m, LineTax = 6480.00m, LineTotal = 42480.00m, ReceivedQuantity = 0m },
                new() { POId = 9, MaterialId = 19, Quantity = 30m, UnitPrice = 1850.00m, TaxRate = 18m, LineSubtotal = 55500.00m, LineTax = 9990.00m, LineTotal = 65490.00m, ReceivedQuantity = 0m },
                new() { POId = 10, MaterialId = 2, Quantity = 4000m, UnitPrice = 82.00m, TaxRate = 18m, LineSubtotal = 328000.00m, LineTax = 59040.00m, LineTotal = 387040.00m, ReceivedQuantity = 0m }
            };
            await context.PurchaseOrderItems.AddRangeAsync(poItems);
            await context.SaveChangesAsync();
        }

        // 10. Goods Receipts (GRNs)
        if (!await context.GoodsReceipts.AnyAsync())
        {
            var grns = new List<GoodsReceipt>
            {
                new()
                {
                    GRNId = 1,
                    GRNNumber = "GRN-2026-0001",
                    POId = 1,
                    VendorId = 1,
                    WarehouseId = 1,
                    ReceiptDate = DateTime.UtcNow.AddDays(-14),
                    DeliveryChallanNumber = "DC/SHR/2026/8912",
                    ReceivedByUserId = 4,
                    InspectionStatus = InspectionStatus.Passed,
                    Remarks = "Full shipment received in excellent condition. Certificate of Testing (TC) verified."
                },
                new()
                {
                    GRNId = 2,
                    GRNNumber = "GRN-2026-0002",
                    POId = 2,
                    VendorId = 3,
                    WarehouseId = 1,
                    ReceiptDate = DateTime.UtcNow.AddDays(-10),
                    DeliveryChallanNumber = "DC/ELC/9921",
                    ReceivedByUserId = 4,
                    InspectionStatus = InspectionStatus.PartiallyAccepted,
                    Remarks = "Partial delivery of 180 KG. 10 KG spool had damaged insulation and was rejected."
                },
                new()
                {
                    GRNId = 3,
                    GRNNumber = "GRN-2026-0003",
                    POId = 3,
                    VendorId = 4,
                    WarehouseId = 1,
                    ReceiptDate = DateTime.UtcNow.AddDays(-8),
                    DeliveryChallanNumber = "DC/PB/4412",
                    ReceivedByUserId = 4,
                    InspectionStatus = InspectionStatus.Passed,
                    Remarks = "300 pieces bearings received in hermetically sealed packaging. Cleared inspection."
                },
                new()
                {
                    GRNId = 4,
                    GRNNumber = "GRN-2026-0004",
                    POId = 4,
                    VendorId = 6,
                    WarehouseId = 1,
                    ReceiptDate = DateTime.UtcNow.AddDays(-5),
                    DeliveryChallanNumber = "DC/IPL/7711",
                    ReceivedByUserId = 4,
                    InspectionStatus = InspectionStatus.Passed,
                    Remarks = "2 Barrels x 210 Liters gear oil received with batch lab test reports."
                }
            };
            await context.GoodsReceipts.AddRangeAsync(grns);
            await context.SaveChangesAsync();

            var grnItems = new List<GoodsReceiptItem>
            {
                new() { GRNId = 1, POItemId = 1, MaterialId = 1, OrderedQuantity = 10000m, PreviouslyReceivedQuantity = 0m, CurrentReceivedQuantity = 10000m, AcceptedQuantity = 10000m, RejectedQuantity = 0m },
                new() { GRNId = 2, POItemId = 2, MaterialId = 4, OrderedQuantity = 300m, PreviouslyReceivedQuantity = 0m, CurrentReceivedQuantity = 190m, AcceptedQuantity = 180m, RejectedQuantity = 10m, RejectionReason = "Physical coating damage and dented spool" },
                new() { GRNId = 3, POItemId = 3, MaterialId = 6, OrderedQuantity = 300m, PreviouslyReceivedQuantity = 0m, CurrentReceivedQuantity = 300m, AcceptedQuantity = 300m, RejectedQuantity = 0m },
                new() { GRNId = 4, POItemId = 4, MaterialId = 8, OrderedQuantity = 420m, PreviouslyReceivedQuantity = 0m, CurrentReceivedQuantity = 420m, AcceptedQuantity = 420m, RejectedQuantity = 0m }
            };
            await context.GoodsReceiptItems.AddRangeAsync(grnItems);
            await context.SaveChangesAsync();
        }

        // 11. Supplier Invoices (Demonstrating Matched and Mismatched for 3-way matching!)
        if (!await context.SupplierInvoices.AnyAsync())
        {
            var invoices = new List<SupplierInvoice>
            {
                new()
                {
                    InvoiceId = 1,
                    InvoiceNumber = "INV-SHREE-8812",
                    VendorId = 1,
                    POId = 1,
                    GRNId = 1,
                    InvoiceDate = DateTime.UtcNow.AddDays(-12),
                    DueDate = DateTime.UtcNow.AddDays(18),
                    Subtotal = 685000.00m,
                    TaxAmount = 123300.00m,
                    TotalAmount = 803300.00m,
                    PaidAmount = 803300.00m,
                    OutstandingAmount = 0m,
                    Status = InvoiceStatus.Paid,
                    VerificationRemarks = "3-Way Match Verified: Perfect match with PO-2026-0001 and GRN-2026-0001. No variance.",
                    VerifiedByUserId = 5,
                    VerifiedDate = DateTime.UtcNow.AddDays(-11)
                },
                new()
                {
                    InvoiceId = 2,
                    InvoiceNumber = "INV-ELC-1092",
                    VendorId = 3,
                    POId = 2,
                    GRNId = 2,
                    InvoiceDate = DateTime.UtcNow.AddDays(-9),
                    DueDate = DateTime.UtcNow.AddDays(21),
                    Subtotal = 168000.00m, // 200 KG @ 840 = 168000, but only 180 KG accepted at GRN!
                    TaxAmount = 30240.00m,
                    TotalAmount = 198240.00m,
                    PaidAmount = 0m,
                    OutstandingAmount = 198240.00m,
                    Status = InvoiceStatus.Mismatched, // Demonstrates 3-way match quantity variance!
                    VerificationRemarks = "MISMATCH DETECTED: Billed quantity is 200 KG, but GRN accepted quantity is 180 KG (10 KG damaged/rejected). Awaiting credit note from vendor.",
                    VerifiedByUserId = 5,
                    VerifiedDate = DateTime.UtcNow.AddDays(-8)
                },
                new()
                {
                    InvoiceId = 3,
                    InvoiceNumber = "INV-PB-3321",
                    VendorId = 4,
                    POId = 3,
                    GRNId = 3,
                    InvoiceDate = DateTime.UtcNow.AddDays(-7),
                    DueDate = DateTime.UtcNow.AddDays(23),
                    Subtotal = 96000.00m,
                    TaxAmount = 17280.00m,
                    TotalAmount = 112280.00m,
                    PaidAmount = 50000.00m,
                    OutstandingAmount = 62280.00m,
                    Status = InvoiceStatus.PartiallyPaid, // Demonstrates partial payment recording!
                    VerificationRemarks = "3-Way Match Verified: Unit price and received quantities match exactly.",
                    VerifiedByUserId = 5,
                    VerifiedDate = DateTime.UtcNow.AddDays(-6)
                },
                new()
                {
                    InvoiceId = 4,
                    InvoiceNumber = "INV-IPL-6601",
                    VendorId = 6,
                    POId = 4,
                    GRNId = 4,
                    InvoiceDate = DateTime.UtcNow.AddDays(-4),
                    DueDate = DateTime.UtcNow.AddDays(41),
                    Subtotal = 100800.00m,
                    TaxAmount = 18144.00m,
                    TotalAmount = 118944.00m,
                    PaidAmount = 0m,
                    OutstandingAmount = 118944.00m,
                    Status = InvoiceStatus.Approved, // Ready for payment
                    VerificationRemarks = "3-Way Match Verified: Rates and barrel quantities verified against GRN-2026-0004.",
                    VerifiedByUserId = 5,
                    VerifiedDate = DateTime.UtcNow.AddDays(-3)
                }
            };
            await context.SupplierInvoices.AddRangeAsync(invoices);
            await context.SaveChangesAsync();

            // Line items for invoices
            var invItems = new List<SupplierInvoiceItem>
            {
                new() { InvoiceId = 1, MaterialId = 1, BilledQuantity = 10000m, BilledUnitPrice = 68.50m, LineTotal = 685000.00m },
                new() { InvoiceId = 2, MaterialId = 4, BilledQuantity = 200m, BilledUnitPrice = 840.00m, LineTotal = 168000.00m },
                new() { InvoiceId = 3, MaterialId = 6, BilledQuantity = 300m, BilledUnitPrice = 320.00m, LineTotal = 96000.00m },
                new() { InvoiceId = 4, MaterialId = 8, BilledQuantity = 420m, BilledUnitPrice = 240.00m, LineTotal = 100800.00m }
            };
            await context.SupplierInvoiceItems.AddRangeAsync(invItems);

            // Verifications
            var verifications = new List<InvoiceVerification>
            {
                new() { InvoiceId = 1, POId = 1, GRNId = 1, MatchedDate = DateTime.UtcNow.AddDays(-11), PriceVariance = 0m, QuantityVariance = 0m, VarianceType = "None", IsMatchSuccessful = true, Remarks = "3-Way Match Passed", VerifiedByUserId = 5 },
                new() { InvoiceId = 2, POId = 2, GRNId = 2, MatchedDate = DateTime.UtcNow.AddDays(-8), PriceVariance = 0m, QuantityVariance = 20m, VarianceType = "QuantityVariance", IsMatchSuccessful = false, Remarks = "Billed 200 KG vs 180 KG accepted at dock", VerifiedByUserId = 5 },
                new() { InvoiceId = 3, POId = 3, GRNId = 3, MatchedDate = DateTime.UtcNow.AddDays(-6), PriceVariance = 0m, QuantityVariance = 0m, VarianceType = "None", IsMatchSuccessful = true, Remarks = "3-Way Match Passed", VerifiedByUserId = 5 },
                new() { InvoiceId = 4, POId = 4, GRNId = 4, MatchedDate = DateTime.UtcNow.AddDays(-3), PriceVariance = 0m, QuantityVariance = 0m, VarianceType = "None", IsMatchSuccessful = true, Remarks = "3-Way Match Passed", VerifiedByUserId = 5 }
            };
            await context.InvoiceVerifications.AddRangeAsync(verifications);
            await context.SaveChangesAsync();
        }

        // 12. Payments
        if (!await context.Payments.AnyAsync())
        {
            var payments = new List<Payment>
            {
                new()
                {
                    PaymentId = 1,
                    PaymentNumber = "PAY-2026-0001",
                    InvoiceId = 1,
                    VendorId = 1,
                    PaymentDate = DateTime.UtcNow.AddDays(-10),
                    PaymentAmount = 803300.00m,
                    PaymentMethod = PaymentMethod.RTGS,
                    TransactionReference = "RTGS-HDFC-9988112233",
                    Status = PaymentStatus.Completed,
                    RecordedByUserId = 5,
                    Remarks = "Full settlement of Invoice #INV-SHREE-8812 against PO-2026-0001"
                },
                new()
                {
                    PaymentId = 2,
                    PaymentNumber = "PAY-2026-0002",
                    InvoiceId = 3,
                    VendorId = 4,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    PaymentAmount = 50000.00m,
                    PaymentMethod = PaymentMethod.NEFT,
                    TransactionReference = "NEFT-AXIS-4433221100",
                    Status = PaymentStatus.Completed,
                    RecordedByUserId = 5,
                    Remarks = "Partial payment installment 1 of 2 against Invoice #INV-PB-3321"
                }
            };
            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
        }

        // 13. System Settings
        if (!await context.SystemSettings.AnyAsync())
        {
            var settings = new List<SystemSetting>
            {
                new() { SettingKey = "COMPANY_NAME", SettingValue = "Apex Manufacturing Industries Pvt. Ltd.", Description = "Registered Company Name" },
                new() { SettingKey = "COMPANY_GSTIN", SettingValue = "27AAACA9999Z1Z5", Description = "Head Office GSTIN" },
                new() { SettingKey = "COMPANY_PAN", SettingValue = "AAACA9999Z", Description = "Corporate PAN Number" },
                new() { SettingKey = "CURRENCY_SYMBOL", SettingValue = "₹", Description = "Default Currency" },
                new() { SettingKey = "APPROVAL_TIER1_MAX", SettingValue = "25000", Description = "Tier 1 Approval Maximum Threshold (INR)" },
                new() { SettingKey = "APPROVAL_TIER2_MAX", SettingValue = "100000", Description = "Tier 2 Approval Maximum Threshold (INR)" },
                new() { SettingKey = "THREE_WAY_MATCH_TOLERANCE_PCT", SettingValue = "2.0", Description = "3-Way Match Price Tolerance Percentage" },
                new() { SettingKey = "THREE_WAY_MATCH_TOLERANCE_ABS", SettingValue = "100.0", Description = "3-Way Match Price Tolerance Absolute Amount (INR)" }
            };
            await context.SystemSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }

        // 14. Notifications & Audit Logs
        if (!await context.Notifications.AnyAsync())
        {
            var notifications = new List<Notification>
            {
                new() { UserId = 3, Title = "Pending Requisition Approval", Message = "PR-2026-0007 (₹1,68,000) submitted by Rahul Kulkarni requires your review.", Type = "Alert", IsRead = false, LinkUrl = "/requisitions/7" },
                new() { UserId = 3, Title = "Low Stock Alert: Copper Wire", Message = "Current stock of Copper Winding Wire 18 SWG (240 KG) has breached reorder level (300 KG).", Type = "Warning", IsRead = false, LinkUrl = "/inventory" },
                new() { UserId = 4, Title = "Expected Delivery", Message = "PO-2026-0005 (PLC Controllers) from ElectroTech is expected tomorrow at Pune Dock.", Type = "Info", IsRead = false, LinkUrl = "/purchase-orders/5" },
                new() { UserId = 5, Title = "Invoice Mismatch Flagged", Message = "INV-ELC-1092 from ElectroTech failed 3-way match (Billed 200 KG vs Accepted 180 KG).", Type = "Alert", IsRead = false, LinkUrl = "/invoices/2" }
            };
            await context.Notifications.AddRangeAsync(notifications);

            var auditLogs = new List<AuditLog>
            {
                new() { UserId = 1, Action = "DatabaseSeed", EntityName = "System", EntityId = "INIT", PreviousValuesJson = null, NewValuesJson = "Seeded Apex Manufacturing base configuration", Timestamp = DateTime.UtcNow.AddDays(-30), IPAddress = "127.0.0.1" },
                new() { UserId = 3, Action = "ApprovePR", EntityName = "PurchaseRequisition", EntityId = "1", PreviousValuesJson = "Submitted", NewValuesJson = "Approved", Timestamp = DateTime.UtcNow.AddDays(-27), IPAddress = "192.168.1.10" },
                new() { UserId = 3, Action = "CreatePO", EntityName = "PurchaseOrder", EntityId = "1", PreviousValuesJson = null, NewValuesJson = "Generated PO-2026-0001", Timestamp = DateTime.UtcNow.AddDays(-26), IPAddress = "192.168.1.10" },
                new() { UserId = 4, Action = "PostGRN", EntityName = "GoodsReceipt", EntityId = "1", PreviousValuesJson = null, NewValuesJson = "Accepted 10000 KG HR Steel", Timestamp = DateTime.UtcNow.AddDays(-14), IPAddress = "192.168.1.25" },
                new() { UserId = 5, Action = "VerifyInvoice", EntityName = "SupplierInvoice", EntityId = "1", PreviousValuesJson = "PendingVerification", NewValuesJson = "Matched", Timestamp = DateTime.UtcNow.AddDays(-11), IPAddress = "192.168.1.30" },
                new() { UserId = 5, Action = "RecordPayment", EntityName = "Payment", EntityId = "1", PreviousValuesJson = "Outstanding: 803300", NewValuesJson = "Paid: 803300 via RTGS", Timestamp = DateTime.UtcNow.AddDays(-10), IPAddress = "192.168.1.30" }
            };
            await context.AuditLogs.AddRangeAsync(auditLogs);
            await context.SaveChangesAsync();
        }
    }
}
