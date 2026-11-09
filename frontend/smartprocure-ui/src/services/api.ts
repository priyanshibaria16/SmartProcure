import axios from 'axios';
import {
  User,
  Vendor,
  Material,
  Department,
  Warehouse,
  Category,
  Uom,
  Requisition,
  PurchaseOrder,
  GoodsReceipt,
  InventoryBalance,
  InventoryTransaction,
  SupplierInvoice,
  Payment,
  DashboardKpis,
  MonthlySpend,
  PoStatusDistribution,
  DeptSpend,
  TopVendor,
  LowStockItem,
  RecentActivity,
  VendorQuotation
} from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor to inject JWT token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('smartprocure_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor for 401 handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('smartprocure_token');
      localStorage.removeItem('smartprocure_user');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

// --- Auth Endpoints ---
export const authApi = {
  login: (data: { email: string; password: string }) =>
    api.post<{ success: boolean; data: { token: string; user: User }; message: string }>('/auth/login', data),
  me: () => api.get<{ success: boolean; data: User }>('/auth/me'),
  changePassword: (data: { currentPassword: string; newPassword: string }) =>
    api.post('/auth/change-password', data),
  getUsers: (params?: { search?: string; roleId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: User[]; totalCount: number } }>('/users', { params }),
  createUser: (data: any) => api.post('/users', data),
  updateUser: (id: number, data: any) => api.put(`/users/${id}`, data),
  toggleUserStatus: (id: number) => api.patch(`/users/${id}/status`),
};

// --- Master Data Endpoints ---
export const masterApi = {
  getVendors: (params?: { search?: string; status?: boolean; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: Vendor[]; totalCount: number } }>('/vendors', { params }),
  getVendor: (id: number) => api.get<{ success: boolean; data: Vendor }>(`/vendors/${id}`),
  createVendor: (data: Partial<Vendor>) => api.post<{ success: boolean; data: Vendor }>('/vendors', data),
  updateVendor: (id: number, data: Partial<Vendor>) => api.put(`/vendors/${id}`, data),
  toggleVendorStatus: (id: number) => api.patch(`/vendors/${id}/status`),

  getMaterials: (params?: { search?: string; categoryId?: number; lowStockOnly?: boolean; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: Material[]; totalCount: number } }>('/materials', { params }),
  getMaterial: (id: number) => api.get<{ success: boolean; data: Material }>(`/materials/${id}`),
  createMaterial: (data: any) => api.post<{ success: boolean; data: Material }>('/materials', data),
  updateMaterial: (id: number, data: any) => api.put(`/materials/${id}`, data),

  getDepartments: () => api.get<{ success: boolean; data: Department[] }>('/departments'),
  getWarehouses: () => api.get<{ success: boolean; data: Warehouse[] }>('/warehouses'),
  getCategories: () => api.get<{ success: boolean; data: Category[] }>('/categories'),
  getUOMs: () => api.get<{ success: boolean; data: Uom[] }>('/uom'),
};

// --- Requisitions Endpoints ---
export const requisitionsApi = {
  getAll: (params?: { status?: string; departmentId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: Requisition[]; totalCount: number } }>('/requisitions', { params }),
  getById: (id: number) => api.get<{ success: boolean; data: Requisition }>(`/requisitions/${id}`),
  create: (data: any) => api.post<{ success: boolean; data: Requisition }>('/requisitions', data),
  submit: (id: number) => api.post(`/requisitions/${id}/submit`),
  approve: (id: number, data: { action: string; remarks: string }) => api.post(`/requisitions/${id}/approve`, data),
  reject: (id: number, data: { action: string; remarks: string }) => api.post(`/requisitions/${id}/reject`, data),
  cancel: (id: number) => api.post(`/requisitions/${id}/cancel`),
};

// --- Quotations Endpoints ---
export const quotationsApi = {
  getByPR: (requisitionId: number) =>
    api.get<{ success: boolean; data: VendorQuotation[] }>('/quotations', { params: { requisitionId } }),
  create: (data: any) => api.post('/quotations', data),
  select: (id: number, data: { selectionJustification: string }) => api.post(`/quotations/${id}/select`, data),
};

// --- Purchase Orders Endpoints ---
export const purchaseOrdersApi = {
  getAll: (params?: { status?: string; vendorId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: PurchaseOrder[]; totalCount: number } }>('/purchase-orders', { params }),
  getById: (id: number) => api.get<{ success: boolean; data: PurchaseOrder }>(`/purchase-orders/${id}`),
  create: (data: any) => api.post<{ success: boolean; data: PurchaseOrder }>('/purchase-orders', data),
  send: (id: number) => api.post(`/purchase-orders/${id}/send`),
  cancel: (id: number, reason: string) => api.post(`/purchase-orders/${id}/cancel`, JSON.stringify(reason), {
    headers: { 'Content-Type': 'application/json' }
  }),
  downloadPdf: (id: number) =>
    api.get(`/purchase-orders/${id}/pdf`, { responseType: 'blob' }),
};

// --- Goods Receipts Endpoints ---
export const goodsReceiptsApi = {
  getAll: (params?: { poId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: GoodsReceipt[]; totalCount: number } }>('/goods-receipts', { params }),
  getById: (id: number) => api.get<{ success: boolean; data: GoodsReceipt }>(`/goods-receipts/${id}`),
  getPendingPOs: () => api.get<{ success: boolean; data: PurchaseOrder[] }>('/goods-receipts/pending-pos'),
  create: (data: any) => api.post<{ success: boolean; data: GoodsReceipt }>('/goods-receipts', data),
};

// --- Inventory Endpoints ---
export const inventoryApi = {
  getBalances: (params?: { warehouseId?: number; categoryId?: number; lowStockOnly?: boolean; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: InventoryBalance[]; totalCount: number } }>('/inventory', { params }),
  getLowStock: (params?: { page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: InventoryBalance[]; totalCount: number } }>('/inventory/low-stock', { params }),
  getTransactions: (params?: { materialId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: InventoryTransaction[]; totalCount: number } }>('/inventory/transactions', { params }),
  recordAdjustment: (data: { materialId: number; warehouseId: number; quantity: number; remarks: string }) =>
    api.post('/inventory/adjustment', data),
};

// --- Invoices Endpoints ---
export const invoicesApi = {
  getAll: (params?: { status?: string; vendorId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: SupplierInvoice[]; totalCount: number } }>('/invoices', { params }),
  getById: (id: number) => api.get<{ success: boolean; data: SupplierInvoice }>(`/invoices/${id}`),
  create: (data: any) => api.post<{ success: boolean; data: SupplierInvoice }>('/invoices', data),
  verify: (id: number) => api.post(`/invoices/${id}/verify`),
  approve: (id: number, remarks: string) => api.post(`/invoices/${id}/approve`, JSON.stringify(remarks), {
    headers: { 'Content-Type': 'application/json' }
  }),
  reject: (id: number, remarks: string) => api.post(`/invoices/${id}/reject`, JSON.stringify(remarks), {
    headers: { 'Content-Type': 'application/json' }
  }),
};

// --- Payments Endpoints ---
export const paymentsApi = {
  getAll: (params?: { invoiceId?: number; vendorId?: number; page?: number; pageSize?: number }) =>
    api.get<{ success: boolean; data: { items: Payment[]; totalCount: number } }>('/payments', { params }),
  record: (data: { invoiceId: number; paymentAmount: number; paymentMethod: string; transactionReference: string; remarks?: string }) =>
    api.post<{ success: boolean; data: Payment }>('/payments', data),
};

// --- Dashboard Endpoints ---
export const dashboardApi = {
  getKpis: () => api.get<{ success: boolean; data: DashboardKpis }>('/dashboard/kpi'),
  getCharts: () => api.get<{
    success: boolean;
    data: {
      monthlySpend: MonthlySpend[];
      poStatus: PoStatusDistribution[];
      deptSpend: DeptSpend[];
      topVendors: TopVendor[];
      lowStock: LowStockItem[];
    };
  }>('/dashboard/charts'),
  getActivities: () => api.get<{ success: boolean; data: RecentActivity[] }>('/dashboard/activities'),
};

// --- Reports Endpoints ---
export const reportsApi = {
  exportProcurement: (params?: { fromDate?: string; toDate?: string }) =>
    api.get('/reports/procurement', { params, responseType: 'blob' }),
  exportInventory: () => api.get('/reports/inventory', { responseType: 'blob' }),
  exportVendorPerformance: () => api.get('/reports/vendor-performance', { responseType: 'blob' }),
  exportOutstandingPayments: () => api.get('/reports/outstanding-payments', { responseType: 'blob' }),
};

export default api;

