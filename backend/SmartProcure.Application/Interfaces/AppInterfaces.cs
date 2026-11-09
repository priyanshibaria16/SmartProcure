using SmartProcure.Application.DTOs;
using SmartProcure.Domain.Enums;

namespace SmartProcure.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserDto>> GetUserProfileAsync(int userId);
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(string? search, int? roleId, int page, int pageSize);
    Task<ApiResponse<UserDto>> CreateUserAsync(UserCreateDto dto);
    Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UserUpdateDto dto);
    Task<ApiResponse<bool>> ToggleUserStatusAsync(int id);
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}

public interface IMasterDataService
{
    Task<ApiResponse<PagedResult<VendorDto>>> GetVendorsAsync(string? search, bool? status, int page, int pageSize);
    Task<ApiResponse<VendorDto>> GetVendorByIdAsync(int id);
    Task<ApiResponse<VendorDto>> CreateVendorAsync(VendorCreateDto dto);
    Task<ApiResponse<VendorDto>> UpdateVendorAsync(int id, VendorUpdateDto dto);
    Task<ApiResponse<bool>> ToggleVendorStatusAsync(int id);

    Task<ApiResponse<PagedResult<MaterialDto>>> GetMaterialsAsync(string? search, int? categoryId, bool? lowStockOnly, int page, int pageSize);
    Task<ApiResponse<MaterialDto>> GetMaterialByIdAsync(int id);
    Task<ApiResponse<MaterialDto>> CreateMaterialAsync(MaterialCreateDto dto);
    Task<ApiResponse<MaterialDto>> UpdateMaterialAsync(int id, MaterialUpdateDto dto);

    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync();
    Task<ApiResponse<List<WarehouseDto>>> GetWarehousesAsync();
    Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();
    Task<ApiResponse<List<UomDto>>> GetUnitsOfMeasureAsync();
}

public interface IProcurementService
{
    Task<ApiResponse<PagedResult<RequisitionDto>>> GetRequisitionsAsync(RequisitionStatus? status, int? departmentId, int page, int pageSize);
    Task<ApiResponse<RequisitionDto>> GetRequisitionByIdAsync(int id);
    Task<ApiResponse<RequisitionDto>> CreateRequisitionAsync(int userId, RequisitionCreateDto dto);
    Task<ApiResponse<bool>> SubmitRequisitionAsync(int id, int userId);
    Task<ApiResponse<bool>> ApproveRequisitionAsync(int id, int approverUserId, ApprovalActionDto dto);
    Task<ApiResponse<bool>> RejectRequisitionAsync(int id, int approverUserId, ApprovalActionDto dto);
    Task<ApiResponse<bool>> CancelRequisitionAsync(int id, int userId);

    Task<ApiResponse<List<QuotationDto>>> GetQuotationsByPRAsync(int requisitionId);
    Task<ApiResponse<QuotationDto>> CreateQuotationAsync(QuotationCreateDto dto);
    Task<ApiResponse<bool>> SelectQuotationAsync(int quotationId, QuotationSelectDto dto);

    Task<ApiResponse<PagedResult<PurchaseOrderDto>>> GetPurchaseOrdersAsync(PurchaseOrderStatus? status, int? vendorId, int page, int pageSize);
    Task<ApiResponse<PurchaseOrderDto>> GetPurchaseOrderByIdAsync(int id);
    Task<ApiResponse<PurchaseOrderDto>> CreatePurchaseOrderAsync(int userId, PurchaseOrderCreateDto dto);
    Task<ApiResponse<bool>> SendPOToVendorAsync(int poId, int userId);
    Task<ApiResponse<bool>> CancelPurchaseOrderAsync(int poId, int userId, string reason);
}

public interface IInventoryService
{
    Task<ApiResponse<PagedResult<GoodsReceiptDto>>> GetGoodsReceiptsAsync(int? poId, int page, int pageSize);
    Task<ApiResponse<GoodsReceiptDto>> GetGoodsReceiptByIdAsync(int id);
    Task<ApiResponse<List<PurchaseOrderDto>>> GetPendingPOsForReceivingAsync();
    Task<ApiResponse<GoodsReceiptDto>> CreateGoodsReceiptAsync(int userId, GoodsReceiptCreateDto dto);

    Task<ApiResponse<PagedResult<InventoryBalanceDto>>> GetStockBalancesAsync(int? warehouseId, int? categoryId, bool? lowStockOnly, int page, int pageSize);
    Task<ApiResponse<PagedResult<InventoryTransactionDto>>> GetInventoryTransactionsAsync(int? materialId, int page, int pageSize);
    Task<ApiResponse<bool>> RecordStockAdjustmentAsync(int userId, StockAdjustmentDto dto);
}

public interface IFinanceService
{
    Task<ApiResponse<PagedResult<SupplierInvoiceDto>>> GetInvoicesAsync(InvoiceStatus? status, int? vendorId, int page, int pageSize);
    Task<ApiResponse<SupplierInvoiceDto>> GetInvoiceByIdAsync(int id);
    Task<ApiResponse<SupplierInvoiceDto>> CreateInvoiceAsync(SupplierInvoiceCreateDto dto);
    Task<ApiResponse<InvoiceVerificationDto>> VerifyInvoiceAsync(int invoiceId, int userId);
    Task<ApiResponse<bool>> ApproveInvoiceAsync(int invoiceId, int userId, string remarks);
    Task<ApiResponse<bool>> RejectInvoiceAsync(int invoiceId, int userId, string remarks);

    Task<ApiResponse<PagedResult<PaymentDto>>> GetPaymentsAsync(int? invoiceId, int? vendorId, int page, int pageSize);
    Task<ApiResponse<PaymentDto>> RecordPaymentAsync(int userId, PaymentCreateDto dto);
}

public interface IDashboardService
{
    Task<ApiResponse<DashboardKpiDto>> GetKpisAsync();
    Task<ApiResponse<List<MonthlySpendDto>>> GetMonthlySpendAsync();
    Task<ApiResponse<List<PoStatusDistributionDto>>> GetPoStatusDistributionAsync();
    Task<ApiResponse<List<DeptSpendDto>>> GetDepartmentSpendAsync();
    Task<ApiResponse<List<TopVendorDto>>> GetTopVendorsAsync();
    Task<ApiResponse<List<LowStockItemDto>>> GetLowStockItemsAsync();
    Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync();
}

public interface IReportService
{
    Task<byte[]> GenerateProcurementReportCsvAsync(DateTime? fromDate, DateTime? toDate);
    Task<byte[]> GenerateInventoryReportCsvAsync();
    Task<byte[]> GenerateVendorPerformanceReportCsvAsync();
    Task<byte[]> GenerateOutstandingPaymentsReportCsvAsync();
}
