using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Services;
using SmartProcure.Domain.Entities;
using SmartProcure.Domain.Enums;
using SmartProcure.Infrastructure.Data;
using Xunit;

namespace SmartProcure.Tests;

public class EnterpriseWorkflowTests
{
    private (SmartProcureDbContext context, AuthService auth, MasterDataService master, ProcurementService proc, InventoryService inv, FinanceService fin, DashboardService dash, ReportService rep) CreateTestServices()
    {
        var dbName = "SmartProcure_Test_" + Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<SmartProcureDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new SmartProcureDbContext(options);

        // Seed basic configuration
        DatabaseSeeder.SeedAsync(context).GetAwaiter().GetResult();

        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", "SmartProcure_Enterprise_Super_Secret_Key_2026_Secure_JWT_Token_Generator_Key!" },
            { "Jwt:Issuer", "SmartProcure.API" },
            { "Jwt:Audience", "SmartProcure.Client" }
        };
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        var auth = new AuthService(context, config);
        var master = new MasterDataService(context);
        var proc = new ProcurementService(context);
        var inv = new InventoryService(context);
        var fin = new FinanceService(context);
        var dash = new DashboardService(context);
        var rep = new ReportService(context);

        return (context, auth, master, proc, inv, fin, dash, rep);
    }

    // Scenario 1: User login with valid credentials
    [Fact]
    public async Task TC01_UserLogin_ValidCredentials_ReturnsSuccessAndToken()
    {
        var (_, auth, _, _, _, _, _, _) = CreateTestServices();
        var result = await auth.LoginAsync(new LoginRequest
        {
            Email = "admin@smartprocure.com",
            Password = "Admin@123"
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrEmpty(result.Data.Token));
        Assert.Equal("Administrator", result.Data.User.RoleName);
    }

    // Scenario 2: Invalid login attempts
    [Fact]
    public async Task TC02_UserLogin_InvalidCredentials_ReturnsUnauthorized()
    {
        var (_, auth, _, _, _, _, _, _) = CreateTestServices();
        var result = await auth.LoginAsync(new LoginRequest
        {
            Email = "admin@smartprocure.com",
            Password = "WrongPassword@999"
        });

        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    // Scenario 3: Unauthorized user access / Inactive user
    [Fact]
    public async Task TC03_UserLogin_InactiveAccount_ReturnsFailure()
    {
        var (context, auth, _, _, _, _, _, _) = CreateTestServices();
        var user = await context.Users.FirstAsync(u => u.Email == "requester@smartprocure.com");
        user.IsActive = false;
        await context.SaveChangesAsync();

        var result = await auth.LoginAsync(new LoginRequest
        {
            Email = "requester@smartprocure.com",
            Password = "Request@123"
        });

        Assert.False(result.Success);
    }

    // Scenario 4: Vendor creation
    [Fact]
    public async Task TC04_VendorCreation_ValidData_PersistsSuccessfully()
    {
        var (_, _, master, _, _, _, _, _) = CreateTestServices();
        var result = await master.CreateVendorAsync(new VendorCreateDto
        {
            VendorCode = "VND-TEST-009",
            CompanyName = "Test Industrial Bearings India",
            ContactPerson = "Ramesh Gupta",
            Email = "ramesh@testind.in",
            Phone = "+91 99887 76655",
            Address = "GIDC Phase 4",
            City = "Vadodara",
            State = "Gujarat",
            Country = "India",
            PostalCode = "390010",
            GSTIN = "24AABCT9988Z1Z9",
            PAN = "AABCT9988Z",
            PaymentTerms = "Net 30",
            BankAccountRef = "HDFC-11223344",
            BankName = "HDFC Bank",
            IFSCCode = "HDFC0001122",
            VendorCategory = "Mechanical Spares & Bearings",
            VendorRating = 4.5m
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("VND-TEST-009", result.Data.VendorCode);
    }

    // Scenario 5: Duplicate vendor prevention
    [Fact]
    public async Task TC05_VendorCreation_DuplicateCode_ReturnsFailure()
    {
        var (_, _, master, _, _, _, _, _) = CreateTestServices();
        // VND-SHREE-01 already seeded
        var result = await master.CreateVendorAsync(new VendorCreateDto
        {
            VendorCode = "VND-SHREE-01",
            CompanyName = "Duplicate Corp",
            Email = "dup@test.com",
            GSTIN = "27AAACS1234F1Z1",
            PAN = "AAACS1234F"
        });

        Assert.False(result.Success);
        Assert.Contains("already exists", result.Message);
    }

    // Scenario 6: Material creation
    [Fact]
    public async Task TC06_MaterialCreation_ValidData_PersistsSuccessfully()
    {
        var (_, _, master, _, _, _, _, _) = CreateTestServices();
        var result = await master.CreateMaterialAsync(new MaterialCreateDto
        {
            MaterialCode = "MAT-TEST-999",
            MaterialName = "Hardened Steel Shaft 40mm",
            Description = "Induction hardened chrome plated shaft",
            CategoryId = 1,
            BaseUOMId = 1,
            UnitPrice = 145.00m,
            ReorderLevel = 100m,
            MaximumStockLevel = 1000m,
            StorageLocation = "Bay A9"
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.CurrentStock);
    }

    // Scenario 7: Invalid material quantity / negative price
    [Fact]
    public async Task TC07_MaterialCreation_NegativeUnitPrice_ReturnsValidationError()
    {
        var (_, _, master, _, _, _, _, _) = CreateTestServices();
        var result = await master.CreateMaterialAsync(new MaterialCreateDto
        {
            MaterialCode = "MAT-TEST-NEG",
            MaterialName = "Negative Price Material",
            CategoryId = 1,
            BaseUOMId = 1,
            UnitPrice = -50m,
            ReorderLevel = 10m,
            MaximumStockLevel = 100m
        });

        Assert.False(result.Success);
        Assert.Contains("negative", result.Message.ToLower());
    }

    // Scenario 8: Purchase requisition creation
    [Fact]
    public async Task TC08_PurchaseRequisition_CreateDraft_CalculatesTotalEstimatedAmount()
    {
        var (_, _, _, proc, _, _, _, _) = CreateTestServices();
        var result = await proc.CreateRequisitionAsync(2, new RequisitionCreateDto
        {
            DepartmentId = 1,
            RequiredDeliveryDate = DateTime.UtcNow.AddDays(14),
            Priority = PriorityLevel.High,
            BusinessJustification = "Production chassis materials",
            SubmitImmediately = false,
            Items = new List<RequisitionItemCreateDto>
            {
                new() { MaterialId = 1, Quantity = 100m, EstimatedUnitPrice = 68.50m }, // 6850
                new() { MaterialId = 6, Quantity = 50m, EstimatedUnitPrice = 320.00m }   // 16000 -> Total: 22850
            }
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(22850.00m, result.Data.TotalEstimatedAmount);
        Assert.Equal(RequisitionStatus.Draft, result.Data.Status);
    }

    // Scenario 9: Purchase requisition submission
    [Fact]
    public async Task TC09_PurchaseRequisition_Submission_TransitionsToPendingApproval()
    {
        var (context, _, _, proc, _, _, _, _) = CreateTestServices();
        var pr = await context.PurchaseRequisitions.FirstAsync(r => r.Status == RequisitionStatus.Draft);

        var result = await proc.SubmitRequisitionAsync(pr.RequisitionId, 2);
        Assert.True(result.Success);

        var updated = await proc.GetRequisitionByIdAsync(pr.RequisitionId);
        Assert.Equal(RequisitionStatus.PendingApproval, updated.Data!.Status);
    }

    // Scenario 10: Approval workflow
    [Fact]
    public async Task TC10_ApprovalWorkflow_Approve_UpdatesStatusAndLogsHistory()
    {
        var (context, _, _, proc, _, _, _, _) = CreateTestServices();
        var pr = await context.PurchaseRequisitions.FirstAsync(r => r.Status == RequisitionStatus.PendingApproval);

        var result = await proc.ApproveRequisitionAsync(pr.RequisitionId, 3, new ApprovalActionDto
        {
            Action = "Approve",
            Remarks = "Approved within quarterly budget"
        });

        Assert.True(result.Success);
        var updated = await proc.GetRequisitionByIdAsync(pr.RequisitionId);
        Assert.Equal(RequisitionStatus.Approved, updated.Data!.Status);
        Assert.Contains(updated.Data.ApprovalHistories, h => h.Action == "Approved");
    }

    // Scenario 11: Rejected requisition handling
    [Fact]
    public async Task TC11_ApprovalWorkflow_Reject_RequiresRemarksAndSetsRejectedStatus()
    {
        var (context, _, _, proc, _, _, _, _) = CreateTestServices();
        var pr = await context.PurchaseRequisitions.FirstAsync(r => r.Status == RequisitionStatus.PendingApproval);

        // Blank remarks should fail
        var blankFail = await proc.RejectRequisitionAsync(pr.RequisitionId, 3, new ApprovalActionDto
        {
            Action = "Reject",
            Remarks = ""
        });
        Assert.False(blankFail.Success);

        // Valid remarks should succeed
        var result = await proc.RejectRequisitionAsync(pr.RequisitionId, 3, new ApprovalActionDto
        {
            Action = "Reject",
            Remarks = "Budget depleted for this cost center"
        });
        Assert.True(result.Success);

        var updated = await proc.GetRequisitionByIdAsync(pr.RequisitionId);
        Assert.Equal(RequisitionStatus.Rejected, updated.Data!.Status);
    }

    // Scenario 12: Purchase order generation
    [Fact]
    public async Task TC12_PurchaseOrder_Generation_CreatesPOAndMarksPRConverted()
    {
        var (context, _, _, proc, _, _, _, _) = CreateTestServices();
        var approvedPR = await context.PurchaseRequisitions.FirstAsync(r => r.Status == RequisitionStatus.Approved);

        var result = await proc.CreatePurchaseOrderAsync(3, new PurchaseOrderCreateDto
        {
            RequisitionId = approvedPR.RequisitionId,
            VendorId = 1,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(10),
            PaymentTerms = "Net 30",
            WarehouseId = 1,
            DiscountAmount = 500m,
            Items = new List<PurchaseOrderItemCreateDto>
            {
                new() { MaterialId = 1, Quantity = 100m, UnitPrice = 70.00m, TaxRate = 18m } // 7000 + 1260 - 500 = 7760
            }
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(7760.00m, result.Data.GrandTotal);

        var prCheck = await proc.GetRequisitionByIdAsync(approvedPR.RequisitionId);
        Assert.Equal(RequisitionStatus.ConvertedToPO, prCheck.Data!.Status);
    }

    // Scenario 13: Duplicate PO prevention
    [Fact]
    public async Task TC13_PurchaseOrder_DuplicatePRConversion_ReturnsFailure()
    {
        var (context, _, _, proc, _, _, _, _) = CreateTestServices();
        var convertedPR = await context.PurchaseRequisitions.FirstAsync(r => r.Status == RequisitionStatus.ConvertedToPO);

        var result = await proc.CreatePurchaseOrderAsync(3, new PurchaseOrderCreateDto
        {
            RequisitionId = convertedPR.RequisitionId,
            VendorId = 1,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(10),
            Items = new List<PurchaseOrderItemCreateDto>
            {
                new() { MaterialId = 1, Quantity = 10m, UnitPrice = 68.50m }
            }
        });

        Assert.False(result.Success);
        Assert.Contains("already been converted", result.Message);
    }

    // Scenario 14: Partial goods receipt
    [Fact]
    public async Task TC14_GoodsReceipt_PartialReceipt_UpdatesPOStatusToPartiallyReceived()
    {
        var (context, _, _, _, inv, _, _, _) = CreateTestServices();
        var po = await context.PurchaseOrders
            .Include(p => p.Items)
            .FirstAsync(p => p.Status == PurchaseOrderStatus.SentToVendor && p.Items.First().Quantity >= 50m);

        var poItem = po.Items.First();
        var result = await inv.CreateGoodsReceiptAsync(4, new GoodsReceiptCreateDto
        {
            POId = po.POId,
            WarehouseId = po.WarehouseId,
            DeliveryChallanNumber = "DC/PARTIAL/101",
            InspectionStatus = InspectionStatus.Passed,
            Remarks = "First partial batch",
            Items = new List<GoodsReceiptItemCreateDto>
            {
                new()
                {
                    POItemId = poItem.POItemId,
                    MaterialId = poItem.MaterialId,
                    CurrentReceivedQuantity = 20m,
                    AcceptedQuantity = 20m,
                    RejectedQuantity = 0m
                }
            }
        });

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var updatedPO = await context.PurchaseOrders.FindAsync(po.POId);
        Assert.Equal(PurchaseOrderStatus.PartiallyReceived, updatedPO!.Status);
    }

    // Scenario 15: Complete goods receipt
    [Fact]
    public async Task TC15_GoodsReceipt_CompleteReceipt_UpdatesPOStatusToFullyReceived()
    {
        var (context, _, _, _, inv, _, _, _) = CreateTestServices();
        var po = await context.PurchaseOrders
            .Include(p => p.Items)
            .FirstAsync(p => p.Status == PurchaseOrderStatus.PartiallyReceived);

        var poItem = po.Items.First();
        decimal remaining = poItem.Quantity - poItem.ReceivedQuantity;

        var result = await inv.CreateGoodsReceiptAsync(4, new GoodsReceiptCreateDto
        {
            POId = po.POId,
            WarehouseId = po.WarehouseId,
            DeliveryChallanNumber = "DC/COMPLETE/999",
            InspectionStatus = InspectionStatus.Passed,
            Remarks = "Final delivery batch",
            Items = new List<GoodsReceiptItemCreateDto>
            {
                new()
                {
                    POItemId = poItem.POItemId,
                    MaterialId = poItem.MaterialId,
                    CurrentReceivedQuantity = remaining,
                    AcceptedQuantity = remaining,
                    RejectedQuantity = 0m
                }
            }
        });

        Assert.True(result.Success);
        var updatedPO = await context.PurchaseOrders.FindAsync(po.POId);
        Assert.Equal(PurchaseOrderStatus.FullyReceived, updatedPO!.Status);
    }

    // Scenario 16: Over-receipt prevention
    [Fact]
    public async Task TC16_GoodsReceipt_OverReceiptQuantity_ReturnsBadRequest()
    {
        var (context, _, _, _, inv, _, _, _) = CreateTestServices();
        var po = await context.PurchaseOrders
            .Include(p => p.Items)
            .FirstAsync(p => p.Status == PurchaseOrderStatus.SentToVendor);

        var poItem = po.Items.First();
        var excessiveQty = poItem.Quantity + 500m; // Exceeds open balance!

        var result = await inv.CreateGoodsReceiptAsync(4, new GoodsReceiptCreateDto
        {
            POId = po.POId,
            WarehouseId = po.WarehouseId,
            DeliveryChallanNumber = "DC/EXCESS/001",
            Items = new List<GoodsReceiptItemCreateDto>
            {
                new()
                {
                    POItemId = poItem.POItemId,
                    MaterialId = poItem.MaterialId,
                    CurrentReceivedQuantity = excessiveQty,
                    AcceptedQuantity = excessiveQty,
                    RejectedQuantity = 0m
                }
            }
        });

        Assert.False(result.Success);
        Assert.Contains("Over-receipt prevented", result.Message);
    }

    // Scenario 17: Rejected goods exclusion from inventory
    [Fact]
    public async Task TC17_GoodsReceipt_RejectedItems_DoNotIncreaseAvailableStock()
    {
        var (context, _, _, _, inv, _, _, _) = CreateTestServices();
        var po = await context.PurchaseOrders
            .Include(p => p.Items)
            .FirstAsync(p => p.Status == PurchaseOrderStatus.SentToVendor);

        var poItem = po.Items.First();
        var initialStock = (await context.InventoryBalances.FirstOrDefaultAsync(b => b.MaterialId == poItem.MaterialId && b.WarehouseId == po.WarehouseId))?.AvailableStock ?? 0;

        var result = await inv.CreateGoodsReceiptAsync(4, new GoodsReceiptCreateDto
        {
            POId = po.POId,
            WarehouseId = po.WarehouseId,
            DeliveryChallanNumber = "DC/REJECT/002",
            InspectionStatus = InspectionStatus.PartiallyAccepted,
            Remarks = "Damage inspection test",
            Items = new List<GoodsReceiptItemCreateDto>
            {
                new()
                {
                    POItemId = poItem.POItemId,
                    MaterialId = poItem.MaterialId,
                    CurrentReceivedQuantity = 30m,
                    AcceptedQuantity = 20m,
                    RejectedQuantity = 10m,
                    RejectionReason = "Surface dents & oxidation"
                }
            }
        });

        Assert.True(result.Success);
        var newStock = (await context.InventoryBalances.FirstOrDefaultAsync(b => b.MaterialId == poItem.MaterialId && b.WarehouseId == po.WarehouseId))?.AvailableStock ?? 0;

        // Exactly 20 accepted units should be added, not 30!
        Assert.Equal(initialStock + 20m, newStock);
    }

    // Scenario 18: Inventory stock updates & transaction logging
    [Fact]
    public async Task TC18_Inventory_PhysicalStockAdjustment_LogsAuditTransaction()
    {
        var (context, _, _, _, inv, _, _, _) = CreateTestServices();
        var matId = 1;
        var whId = 1;

        var beforeBal = (await context.InventoryBalances.FirstAsync(b => b.MaterialId == matId && b.WarehouseId == whId)).AvailableStock;

        var result = await inv.RecordStockAdjustmentAsync(1, new StockAdjustmentDto
        {
            MaterialId = matId,
            WarehouseId = whId,
            Quantity = 150m,
            Remarks = "Quarterly audited physical surplus reconciliation"
        });

        Assert.True(result.Success);
        var afterBal = (await context.InventoryBalances.FirstAsync(b => b.MaterialId == matId && b.WarehouseId == whId)).AvailableStock;
        Assert.Equal(beforeBal + 150m, afterBal);

        var tx = await context.InventoryTransactions
            .OrderByDescending(t => t.TransactionId)
            .FirstAsync(t => t.MaterialId == matId && t.WarehouseId == whId);

        Assert.Equal(InventoryTransactionType.StockAdjustment, tx.TransactionType);
        Assert.Equal(150m, tx.Quantity);
    }

    // Scenario 19: Low-stock alerts
    [Fact]
    public async Task TC19_Inventory_LowStockQuery_ReturnsMaterialsAtOrBelowReorderLevel()
    {
        var (_, _, _, _, inv, _, _, _) = CreateTestServices();
        var result = await inv.GetStockBalancesAsync(null, null, true, 1, 50);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.All(result.Data.Items, item => Assert.True(item.AvailableStock <= item.ReorderLevel));
    }

    // Scenario 20: Invoice three-way matching (Perfect Match)
    [Fact]
    public async Task TC20_ThreeWayMatch_PerfectMatch_MarksInvoiceAsMatched()
    {
        var (context, _, _, _, _, fin, _, _) = CreateTestServices();
        // PO 3 and GRN 3: 300 units @ 320 = 96000 + 17280 tax = 113280
        var newInvoice = await fin.CreateInvoiceAsync(new SupplierInvoiceCreateDto
        {
            InvoiceNumber = "INV-PERFECT-MATCH-01",
            VendorId = 4,
            POId = 3,
            GRNId = 3,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Subtotal = 96000m,
            TaxAmount = 17280m,
            TotalAmount = 113280m,
            Items = new List<SupplierInvoiceItemCreateDto>
            {
                new() { MaterialId = 6, BilledQuantity = 300m, BilledUnitPrice = 320m }
            }
        });

        Assert.True(newInvoice.Success);

        var verifyResult = await fin.VerifyInvoiceAsync(newInvoice.Data!.InvoiceId, 5);
        Assert.True(verifyResult.Success);
        Assert.True(verifyResult.Data!.IsMatchSuccessful);
        Assert.Equal("None", verifyResult.Data.VarianceType);

        var invoice = await fin.GetInvoiceByIdAsync(newInvoice.Data.InvoiceId);
        Assert.Equal(InvoiceStatus.Matched, invoice.Data!.Status);
    }

    // Scenario 21: Invoice mismatch detection
    [Fact]
    public async Task TC21_ThreeWayMatch_VarianceDetected_MarksInvoiceAsMismatched()
    {
        var (_, _, _, _, _, fin, _, _) = CreateTestServices();
        // PO 3: 300 units @ 320. Vendor bills 300 units @ 450 (Rate deviation!)
        var newInvoice = await fin.CreateInvoiceAsync(new SupplierInvoiceCreateDto
        {
            InvoiceNumber = "INV-MISMATCH-RATE-01",
            VendorId = 4,
            POId = 3,
            GRNId = 3,
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Subtotal = 135000m,
            TaxAmount = 24300m,
            TotalAmount = 159300m,
            Items = new List<SupplierInvoiceItemCreateDto>
            {
                new() { MaterialId = 6, BilledQuantity = 300m, BilledUnitPrice = 450m }
            }
        });

        Assert.True(newInvoice.Success);

        var verifyResult = await fin.VerifyInvoiceAsync(newInvoice.Data!.InvoiceId, 5);
        Assert.False(verifyResult.Data!.IsMatchSuccessful);
        Assert.Contains("deviates from PO rate", verifyResult.Data.Remarks);

        var invoice = await fin.GetInvoiceByIdAsync(newInvoice.Data.InvoiceId);
        Assert.Equal(InvoiceStatus.Mismatched, invoice.Data!.Status);
    }

    // Scenario 22: Partial payments
    [Fact]
    public async Task TC22_Payments_PartialPayment_UpdatesInvoiceOutstandingAndStatus()
    {
        var (context, _, _, _, _, fin, _, _) = CreateTestServices();
        var invoice = await context.SupplierInvoices.FirstAsync(i => i.Status == InvoiceStatus.Approved);

        decimal initialOutstanding = invoice.OutstandingAmount;
        decimal payAmount = Math.Round(initialOutstanding * 0.4m, 2);

        var result = await fin.RecordPaymentAsync(5, new PaymentCreateDto
        {
            InvoiceId = invoice.InvoiceId,
            PaymentAmount = payAmount,
            PaymentMethod = PaymentMethod.NEFT,
            TransactionReference = "NEFT-TEST-PARTIAL-101",
            Remarks = "Installment 1"
        });

        Assert.True(result.Success);
        var updated = await fin.GetInvoiceByIdAsync(invoice.InvoiceId);
        Assert.Equal(InvoiceStatus.PartiallyPaid, updated.Data!.Status);
        Assert.Equal(initialOutstanding - payAmount, updated.Data.OutstandingAmount);
    }

    // Scenario 23: Overpayment prevention
    [Fact]
    public async Task TC23_Payments_Overpayment_ReturnsBadRequest()
    {
        var (context, _, _, _, _, fin, _, _) = CreateTestServices();
        var invoice = await context.SupplierInvoices.FirstAsync(i => i.Status == InvoiceStatus.Approved);

        var result = await fin.RecordPaymentAsync(5, new PaymentCreateDto
        {
            InvoiceId = invoice.InvoiceId,
            PaymentAmount = invoice.OutstandingAmount + 50000m, // Excess!
            PaymentMethod = PaymentMethod.RTGS,
            TransactionReference = "RTGS-EXCESS-001"
        });

        Assert.False(result.Success);
        Assert.Contains("Overpayment prevented", result.Message);
    }

    // Scenario 24: Audit log creation
    [Fact]
    public async Task TC24_AuditLogs_ContainSeededOperationalRecords()
    {
        var (context, _, _, _, _, _, _, _) = CreateTestServices();
        var logs = await context.AuditLogs.ToListAsync();

        Assert.NotEmpty(logs);
        Assert.Contains(logs, l => l.Action == "DatabaseSeed");
    }

    // Scenario 25: Report filtering and CSV export
    [Fact]
    public async Task TC25_Reports_ExportProcurementCsv_ReturnsValidCsvBytes()
    {
        var (_, _, _, _, _, _, _, rep) = CreateTestServices();
        var csvBytes = await rep.GenerateProcurementReportCsvAsync(null, null);

        Assert.NotNull(csvBytes);
        Assert.True(csvBytes.Length > 0);

        var csvText = System.Text.Encoding.UTF8.GetString(csvBytes);
        Assert.Contains("PO Number", csvText);
        Assert.Contains("PO-2026-", csvText);
    }
}
