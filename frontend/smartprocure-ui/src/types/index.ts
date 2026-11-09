export type UserRole =
  | 'Administrator'
  | 'RequisitionCreator'
  | 'PurchaseManager'
  | 'WarehouseManager'
  | 'FinanceManager'
  | 'ManagementViewer';

export interface User {
  userId: number;
  email: string;
  fullName: string;
  phoneNumber: string;
  departmentId: number;
  departmentName: string;
  roleId: number;
  roleName: UserRole;
  isActive: boolean;
  lastLoginAt?: string;
}

export interface Department {
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  managerName: string;
  budget: number;
  status: boolean;
}

export interface Warehouse {
  warehouseId: number;
  warehouseCode: string;
  warehouseName: string;
  location: string;
  capacitySqFt: number;
  status: boolean;
}

export interface Category {
  categoryId: number;
  categoryCode: string;
  categoryName: string;
  description: string;
}

export interface Uom {
  uomId: number;
  uomCode: string;
  uomName: string;
  description: string;
}

export interface Vendor {
  vendorId: number;
  vendorCode: string;
  companyName: string;
  contactPerson: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
  gstin: string;
  pan: string;
  paymentTerms: string;
  bankAccountRef: string;
  bankName: string;
  ifscCode: string;
  vendorCategory: string;
  vendorRating: number;
  status: boolean;
  createdAt: string;
}

export interface Material {
  materialId: number;
  materialCode: string;
  materialName: string;
  description: string;
  categoryId: number;
  categoryName: string;
  materialType: string;
  baseUOMId: number;
  baseUOMCode: string;
  unitPrice: number;
  reorderLevel: number;
  maximumStockLevel: number;
  currentStock: number;
  preferredVendorId?: number;
  preferredVendorName?: string;
  storageLocation: string;
  status: boolean;
  isLowStock: boolean;
}

export type PriorityLevel = 'Low' | 'Medium' | 'High' | 'Urgent';
export type RequisitionStatus =
  | 'Draft'
  | 'Submitted'
  | 'PendingApproval'
  | 'Approved'
  | 'Rejected'
  | 'ChangesRequested'
  | 'ConvertedToPO'
  | 'Cancelled';

export interface RequisitionItem {
  itemId: number;
  materialId: number;
  materialCode: string;
  materialName: string;
  uomCode: string;
  quantity: number;
  estimatedUnitPrice: number;
  lineTotal: number;
  remarks?: string;
}

export interface ApprovalHistory {
  historyId: number;
  approverUserId: number;
  approverName: string;
  action: string;
  remarks: string;
  actionDate: string;
}

export interface Requisition {
  requisitionId: number;
  requisitionNumber: string;
  requestedByUserId: number;
  requestedByUserName: string;
  departmentId: number;
  departmentName: string;
  requestDate: string;
  requiredDeliveryDate: string;
  priority: PriorityLevel;
  priorityName: string;
  totalEstimatedAmount: number;
  businessJustification: string;
  status: RequisitionStatus;
  statusName: string;
  approvalRemarks?: string;
  approvedByUserId?: number;
  approvedByUserName?: string;
  approvedDate?: string;
  items: RequisitionItem[];
  approvalHistories: ApprovalHistory[];
}

export type PurchaseOrderStatus =
  | 'Draft'
  | 'PendingApproval'
  | 'Approved'
  | 'SentToVendor'
  | 'PartiallyReceived'
  | 'FullyReceived'
  | 'Closed'
  | 'Cancelled';

export interface PurchaseOrderItem {
  poItemId: number;
  materialId: number;
  materialCode: string;
  materialName: string;
  uomCode: string;
  quantity: number;
  unitPrice: number;
  taxRate: number;
  lineSubtotal: number;
  lineTax: number;
  lineTotal: number;
  receivedQuantity: number;
  pendingQuantity: number;
}

export interface PurchaseOrder {
  poId: number;
  poNumber: string;
  vendorId: number;
  vendorName: string;
  vendorGSTIN: string;
  vendorPAN: string;
  vendorAddress: string;
  requisitionId?: number;
  requisitionNumber?: string;
  poDate: string;
  expectedDeliveryDate: string;
  paymentTerms: string;
  shippingAddress: string;
  billingAddress: string;
  warehouseId: number;
  warehouseName: string;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  grandTotal: number;
  status: PurchaseOrderStatus;
  statusName: string;
  createdByUserId: number;
  approvedByUserId?: number;
  approvedByUserName?: string;
  approvedDate?: string;
  cancellationReason?: string;
  items: PurchaseOrderItem[];
}

export type InspectionStatus = 'Pending' | 'Passed' | 'PartiallyAccepted' | 'Rejected';

export interface GoodsReceiptItem {
  grnItemId: number;
  poItemId: number;
  materialId: number;
  materialCode: string;
  materialName: string;
  uomCode: string;
  orderedQuantity: number;
  previouslyReceivedQuantity: number;
  currentReceivedQuantity: number;
  acceptedQuantity: number;
  rejectedQuantity: number;
  rejectionReason?: string;
}

export interface GoodsReceipt {
  grnId: number;
  grnNumber: string;
  poId: number;
  poNumber: string;
  vendorId: number;
  vendorName: string;
  warehouseId: number;
  warehouseName: string;
  receiptDate: string;
  deliveryChallanNumber: string;
  receivedByUserId: number;
  receivedByUserName: string;
  inspectionStatus: InspectionStatus;
  inspectionStatusName: string;
  remarks?: string;
  items: GoodsReceiptItem[];
}

export interface InventoryBalance {
  balanceId: number;
  materialId: number;
  materialCode: string;
  materialName: string;
  categoryName: string;
  uomCode: string;
  unitPrice: number;
  warehouseId: number;
  warehouseName: string;
  availableStock: number;
  reservedStock: number;
  totalStock: number;
  stockValue: number;
  reorderLevel: number;
  isLowStock: boolean;
  lastUpdated: string;
}

export interface InventoryTransaction {
  transactionId: number;
  materialId: number;
  materialName: string;
  warehouseId: number;
  warehouseName: string;
  transactionType: string;
  transactionTypeName: string;
  quantity: number;
  referenceDocumentType: string;
  referenceDocumentNumber: string;
  transactionDate: string;
  createdByUserId: number;
  remarks?: string;
}

export type InvoiceStatus =
  | 'PendingVerification'
  | 'Matched'
  | 'Mismatched'
  | 'Approved'
  | 'Rejected'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Cancelled';

export interface SupplierInvoiceItem {
  invoiceItemId: number;
  materialId: number;
  materialName: string;
  billedQuantity: number;
  billedUnitPrice: number;
  lineTotal: number;
}

export interface InvoiceVerification {
  verificationId: number;
  invoiceId: number;
  poId: number;
  grnId?: number;
  matchedDate: string;
  priceVariance: number;
  quantityVariance: number;
  varianceType: string;
  isMatchSuccessful: boolean;
  remarks: string;
  verifiedByUserId: number;
}

export interface SupplierInvoice {
  invoiceId: number;
  invoiceNumber: string;
  vendorId: number;
  vendorName: string;
  poId: number;
  poNumber: string;
  grnId?: number;
  grnNumber?: string;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  status: InvoiceStatus;
  statusName: string;
  verificationRemarks?: string;
  verifiedByUserId?: number;
  verifiedDate?: string;
  items: SupplierInvoiceItem[];
  verifications: InvoiceVerification[];
  payments: Payment[];
}

export type PaymentMethod = 'NEFT' | 'RTGS' | 'IMPS' | 'Cheque' | 'BankTransfer';

export interface Payment {
  paymentId: number;
  paymentNumber: string;
  invoiceId: number;
  invoiceNumber: string;
  vendorId: number;
  vendorName: string;
  paymentDate: string;
  paymentAmount: number;
  paymentMethod: PaymentMethod;
  paymentMethodName: string;
  transactionReference: string;
  status: string;
  statusName: string;
  recordedByUserId: number;
  recordedByUserName?: string;
  remarks?: string;
}

export interface DashboardKpis {
  totalPurchaseRequisitions: number;
  pendingApprovals: number;
  totalPurchaseOrders: number;
  totalProcurementValue: number;
  totalGoodsReceived: number;
  pendingInvoices: number;
  outstandingPayments: number;
  totalActiveVendors: number;
  totalActiveMaterials: number;
  lowStockItemsCount: number;
}

export interface MonthlySpend {
  month: string;
  amount: number;
  orderCount: number;
}

export interface PoStatusDistribution {
  status: string;
  count: number;
  totalAmount: number;
}

export interface DeptSpend {
  departmentName: string;
  amount: number;
  budget: number;
}

export interface TopVendor {
  vendorId: number;
  vendorName: string;
  totalSpent: number;
  orderCount: number;
  rating: number;
}

export interface LowStockItem {
  materialId: number;
  materialCode: string;
  materialName: string;
  categoryName: string;
  uom: string;
  currentStock: number;
  reorderLevel: number;
  deficit: number;
}

export interface RecentActivity {
  id: string;
  title: string;
  description: string;
  timestamp: string;
  type: string;
  status: string;
}

export interface QuotationItem {
  quotationItemId: number;
  materialId: number;
  materialName: string;
  quotedUnitPrice: number;
  taxPercentage: number;
  lineTotal: number;
  deliveryLeadDays: number;
  notes?: string;
}

export interface VendorQuotation {
  quotationId: number;
  quotationNumber: string;
  requisitionId: number;
  requisitionNumber: string;
  vendorId: number;
  vendorName: string;
  vendorRating: number;
  quotationDate: string;
  validUntil: string;
  status: string;
  statusName: string;
  selectionJustification?: string;
  totalQuotedAmount: number;
  paymentTerms: string;
  deliveryDays: number;
  items: QuotationItem[];
}
