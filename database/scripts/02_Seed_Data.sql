-- ==============================================================================
-- SMARTPROCURE ENTERPRISE ERP - T-SQL SEED DATA SCRIPT
-- Engine: Microsoft SQL Server 2019 / 2022 / Azure SQL
-- Target Organization: Apex Manufacturing Industries Pvt. Ltd.
-- ==============================================================================

USE SmartProcureDb;
GO

SET NOCOUNT ON;

-- 1. Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Administrator')
BEGIN
    INSERT INTO dbo.Roles (RoleName, Description) VALUES
    ('Administrator', 'Full system administration and configuration'),
    ('RequisitionCreator', 'Creates and tracks purchase requisitions'),
    ('PurchaseManager', 'Evaluates quotes, approves PRs, issues Purchase Orders'),
    ('WarehouseManager', 'Inspects and receives materials, manages warehouse inventory'),
    ('FinanceManager', 'Verifies invoices via 3-way matching and records payments'),
    ('ManagementViewer', 'Read-only executive analytics and reporting');
END
GO

-- 2. Departments
IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentCode = 'DEP-PROD')
BEGIN
    INSERT INTO dbo.Departments (DepartmentCode, DepartmentName, ManagerName, Budget, Status) VALUES
    ('DEP-PROD', 'Production & Assembly', 'Rajesh Sharma', 15000000.00, 1),
    ('DEP-PROC', 'Procurement & Sourcing', 'Vikram Malhotra', 5000000.00, 1),
    ('DEP-WHSE', 'Warehouse & Logistics', 'Sunil Verma', 3500000.00, 1),
    ('DEP-FIN', 'Finance & Accounts', 'Anjali Deshmukh', 2000000.00, 1),
    ('DEP-MAINT', 'Plant Maintenance', 'Manoj Patel', 4000000.00, 1),
    ('DEP-IT', 'Information Technology', 'Arun Mehta', 2500000.00, 1);
END
GO

-- 3. Warehouses
IF NOT EXISTS (SELECT 1 FROM dbo.Warehouses WHERE WarehouseCode = 'WH-PUN-01')
BEGIN
    INSERT INTO dbo.Warehouses (WarehouseCode, WarehouseName, Location, CapacitySqFt, Status) VALUES
    ('WH-PUN-01', 'Central Raw Material Depot', 'Plot 42, MIDC Bhosari, Pune, Maharashtra', 45000.00, 1),
    ('WH-AHM-01', 'Gujarat Spares & Components Hub', 'Sanand GIDC Phase II, Ahmedabad, Gujarat', 30000.00, 1),
    ('WH-VAD-01', 'Finished Goods & Consumables Store', 'Makarpura Industrial Estate, Vadodara, Gujarat', 25000.00, 1);
END
GO

-- 4. Material Categories & UOM
IF NOT EXISTS (SELECT 1 FROM dbo.MaterialCategories WHERE CategoryCode = 'CAT-MET')
BEGIN
    INSERT INTO dbo.MaterialCategories (CategoryCode, CategoryName, Description) VALUES
    ('CAT-MET', 'Raw Metals & Structural Steel', 'Steel plates, coils, billets and bars'),
    ('CAT-ELEC', 'Electrical & Automation', 'Wires, PLCs, drives, contactors and sensors'),
    ('CAT-MECH', 'Mechanical Spares & Bearings', 'Bearings, couplings, fasteners and gears'),
    ('CAT-CHEM', 'Lubricants, Oils & Chemicals', 'Hydraulic fluids, cutting oils, greases'),
    ('CAT-PKG', 'Packaging & Corrugated Materials', 'Boxes, straps, stretch films, pallets'),
    ('CAT-CONS', 'Industrial Consumables & PPE', 'Welding rods, safety gear, grinding wheels');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.UnitsOfMeasure WHERE UOMCode = 'KG')
BEGIN
    INSERT INTO dbo.UnitsOfMeasure (UOMCode, UOMName, Description) VALUES
    ('KG', 'Kilogram', 'Weight measure'),
    ('MTR', 'Meter', 'Length measure'),
    ('PCS', 'Pieces', 'Unit count'),
    ('LTR', 'Liter', 'Liquid volume'),
    ('BOX', 'Box', 'Packaging unit'),
    ('SET', 'Set', 'Assembly set'),
    ('TON', 'Metric Ton', 'Heavy weight'),
    ('ROLL', 'Roll', 'Continuous roll');
END
GO

-- 5. Vendors
IF NOT EXISTS (SELECT 1 FROM dbo.Vendors WHERE VendorCode = 'VND-SHREE-01')
BEGIN
    INSERT INTO dbo.Vendors (VendorCode, CompanyName, ContactPerson, Email, Phone, Address, City, State, Country, PostalCode, GSTIN, PAN, PaymentTerms, BankAccountRef, BankName, IFSCCode, VendorCategory, VendorRating, Status) VALUES
    ('VND-SHREE-01', 'Shree Steel Traders Pvt. Ltd.', 'Mahesh Agarwal', 'sales@shreesteel.in', '+91 98220 44551', 'Survey 112, Chakan Industrial Phase 2', 'Pune', 'Maharashtra', 'India', '410501', '27AAACS1234F1Z1', 'AAACS1234F', 'Net 30', 'HDFC0001234-9876543210', 'HDFC Bank', 'HDFC0001234', 'Raw Metals & Structural Steel', 4.80, 1),
    ('VND-GUJ-02', 'Gujarat Industrial Supplies Corp', 'Bhavesh Patel', 'info@gujaratindustrial.com', '+91 98791 22334', 'GIDC Naroda Industrial Estate, Phase 3', 'Ahmedabad', 'Gujarat', 'India', '382330', '24AABCG5678H1Z5', 'AABCG5678H', 'Net 45', 'ICIC0000456-1122334455', 'ICICI Bank', 'ICIC0000456', 'Mechanical Spares & Bearings', 4.60, 1),
    ('VND-ELEC-03', 'ElectroTech Components India Ltd.', 'Sunita Rao', 'orders@electrotech.co.in', '+91 99801 77665', 'Electronic City Phase 1, Hosur Road', 'Bengaluru', 'Karnataka', 'India', '560100', '29AAACE9988D1ZR', 'AAACE9988D', 'Net 30', 'SBIN0004589-5544332211', 'State Bank of India', 'SBIN0004589', 'Electrical & Automation', 4.90, 1),
    ('VND-PREC-04', 'Precision Components & Bearings LLP', 'Girish Kulkarni', 'contact@precisionbearings.in', '+91 98230 88990', 'Plot 88, Pimpri Industrial Area', 'Pune', 'Maharashtra', 'India', '411018', '27AABFP1029K1ZY', 'AABFP1029K', 'Net 30', 'UTIB0000210-9988776655', 'Axis Bank', 'UTIB0000210', 'Mechanical Spares & Bearings', 4.40, 1),
    ('VND-NAT-05', 'National Packaging Solutions', 'Alok Tandon', 'sales@natpack.com', '+91 97110 55443', 'Sector 24, Industrial Hub', 'Faridabad', 'Haryana', 'India', '121005', '06AAAFN7766M1Z2', 'AAAFN7766M', 'Net 15', 'KKBK0000122-3322114455', 'Kotak Mahindra Bank', 'KKBK0000122', 'Packaging & Corrugated Materials', 4.20, 1),
    ('VND-PETRO-06', 'Indian Petrochemicals & Lubricants Co.', 'Dinesh Shah', 'dispatch@indopetro.in', '+91 98250 11992', 'Ranoli Industrial Area', 'Vadodara', 'Gujarat', 'India', '391350', '24AAACI5544L1Z9', 'AAACI5544L', 'Net 45', 'BARB0VADODA-1231231231', 'Bank of Baroda', 'BARB0VADODA', 'Lubricants, Oils & Chemicals', 4.70, 1),
    ('VND-APEX-07', 'Apex Industrial Fasteners & Hardware', 'Ramanathan Iyer', 'sales@apexfasteners.com', '+91 94440 66778', 'Ambattur Industrial Estate', 'Chennai', 'Tamil Nadu', 'India', '600058', '33AAACA9922P1ZT', 'AAACA9922P', 'Net 30', 'IDIB000A123-9900112233', 'Indian Bank', 'IDIB000A123', 'Mechanical Spares & Bearings', 4.50, 1),
    ('VND-SAFETY-08', 'Suraksha Safety Gear & Consumables', 'Harpreet Singh', 'orders@surakshasafety.in', '+91 98140 22110', 'Focal Point Phase IV', 'Ludhiana', 'Punjab', 'India', '141010', '03AAAPS4433J1Z8', 'AAAPS4433J', 'Net 30', 'PUNB0009876-8877665544', 'Punjab National Bank', 'PUNB0009876', 'Industrial Consumables & PPE', 4.30, 1);
END
GO

PRINT 'T-SQL Seed Data completed successfully for Apex Manufacturing Industries Pvt. Ltd.';
GO
