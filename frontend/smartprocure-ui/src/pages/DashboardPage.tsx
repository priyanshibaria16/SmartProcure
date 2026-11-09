import React, { useEffect, useState } from 'react';
import {
  FileText,
  Clock,
  ShoppingCart,
  IndianRupee,
  Truck,
  Receipt,
  CreditCard,
  AlertTriangle,
  ArrowUpRight,
  RefreshCw,
  Building2,
  CheckCircle2
} from 'lucide-react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend
} from 'recharts';
import { dashboardApi } from '../services/api';
import {
  DashboardKpis,
  MonthlySpend,
  PoStatusDistribution,
  DeptSpend,
  TopVendor,
  LowStockItem,
  RecentActivity
} from '../types';
import { Link } from 'react-router-dom';

const PO_COLORS: Record<string, string> = {
  Draft: '#94a3b8',
  PendingApproval: '#f59e0b',
  Approved: '#10b981',
  SentToVendor: '#3b82f6',
  PartiallyReceived: '#0284c7',
  FullyReceived: '#059669',
  Closed: '#64748b',
  Cancelled: '#ef4444',
};

export const DashboardPage: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [kpis, setKpis] = useState<DashboardKpis | null>(null);
  const [monthlySpend, setMonthlySpend] = useState<MonthlySpend[]>([]);
  const [poStatus, setPoStatus] = useState<PoStatusDistribution[]>([]);
  const [deptSpend, setDeptSpend] = useState<DeptSpend[]>([]);
  const [topVendors, setTopVendors] = useState<TopVendor[]>([]);
  const [lowStock, setLowStock] = useState<LowStockItem[]>([]);
  const [activities, setActivities] = useState<RecentActivity[]>([]);

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const [kpiRes, chartsRes, actRes] = await Promise.all([
        dashboardApi.getKpis(),
        dashboardApi.getCharts(),
        dashboardApi.getActivities(),
      ]);

      if (kpiRes.data.success) setKpis(kpiRes.data.data);
      if (chartsRes.data.success && chartsRes.data.data) {
        setMonthlySpend(chartsRes.data.data.monthlySpend || []);
        setPoStatus(chartsRes.data.data.poStatus || []);
        setDeptSpend(chartsRes.data.data.deptSpend || []);
        setTopVendors(chartsRes.data.data.topVendors || []);
        setLowStock(chartsRes.data.data.lowStock || []);
      }
      if (actRes.data.success) setActivities(actRes.data.data);
    } catch (err) {
      console.error('Failed to load dashboard data', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0,
    }).format(val);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="flex flex-col items-center gap-3">
          <RefreshCw className="w-8 h-8 text-blue-600 animate-spin" />
          <p className="text-sm font-medium text-slate-500">Loading Enterprise Analytics...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Title & Refresh */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Executive Procurement Dashboard</h1>
          <p className="text-xs text-slate-500 mt-0.5">
            Real-time P2P procurement metrics, warehouse inventory balances and spend governance
          </p>
        </div>
        <button
          onClick={fetchDashboardData}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-white border border-slate-200 rounded-lg text-xs font-medium text-slate-700 hover:bg-slate-50 shadow-xs transition-colors"
        >
          <RefreshCw className="w-3.5 h-3.5 text-slate-500" />
          <span>Refresh Data</span>
        </button>
      </div>

      {/* Row 1: KPI Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Total Spend */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Total Procurement Spend</div>
            <div className="text-2xl font-bold text-slate-900 mt-1">
              {formatCurrency(kpis?.totalProcurementValue || 0)}
            </div>
            <div className="text-[11px] text-emerald-600 font-medium flex items-center gap-0.5 mt-1">
              <CheckCircle2 className="w-3 h-3" />
              <span>Across {kpis?.totalPurchaseOrders || 0} Purchase Orders</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center border border-blue-100">
            <IndianRupee className="w-6 h-6" />
          </div>
        </div>

        {/* Pending Approvals */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Pending Approvals</div>
            <div className="text-2xl font-bold text-amber-600 mt-1">{kpis?.pendingApprovals || 0}</div>
            <div className="text-[11px] text-slate-500 font-medium flex items-center gap-0.5 mt-1">
              <Clock className="w-3 h-3 text-amber-500" />
              <span>Total Requisitions: {kpis?.totalPurchaseRequisitions || 0}</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center border border-amber-100">
            <FileText className="w-6 h-6" />
          </div>
        </div>

        {/* Goods Receipts */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Goods Received (GRN)</div>
            <div className="text-2xl font-bold text-slate-900 mt-1">{kpis?.totalGoodsReceived || 0}</div>
            <div className="text-[11px] text-blue-600 font-medium flex items-center gap-0.5 mt-1">
              <Truck className="w-3 h-3" />
              <span>Active Vendors: {kpis?.totalActiveVendors || 0}</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center border border-emerald-100">
            <Truck className="w-6 h-6" />
          </div>
        </div>

        {/* Outstanding Liabilities */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Outstanding Invoices (AP)</div>
            <div className="text-2xl font-bold text-rose-600 mt-1">
              {formatCurrency(kpis?.outstandingPayments || 0)}
            </div>
            <div className="text-[11px] text-rose-500 font-medium flex items-center gap-0.5 mt-1">
              <Receipt className="w-3 h-3" />
              <span>{kpis?.pendingInvoices || 0} Pending Verification</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center border border-rose-100">
            <CreditCard className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Row 2: Charts (Spend by Month & PO Status Distribution) */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Monthly Spend Bar Chart */}
        <div className="lg:col-span-2 bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-sm font-bold text-slate-800">Monthly Procurement Spend</h2>
              <p className="text-xs text-slate-400">Total Purchase Order commitment in INR</p>
            </div>
            <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-md">
              FY 2026-27
            </span>
          </div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={monthlySpend} margin={{ top: 10, right: 10, left: 10, bottom: 20 }}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                <XAxis dataKey="month" tick={{ fontSize: 11, fill: '#64748b' }} axisLine={{ stroke: '#e2e8f0' }} />
                <YAxis
                  tick={{ fontSize: 11, fill: '#64748b' }}
                  tickFormatter={(val) => `₹${(val / 1000).toFixed(0)}k`}
                  axisLine={{ stroke: '#e2e8f0' }}
                />
                <Tooltip
                  formatter={(val: any) => [formatCurrency(Number(val)), 'Total Spend']}
                  contentStyle={{ backgroundColor: '#0f172a', color: '#fff', borderRadius: '8px', fontSize: '12px' }}
                />
                <Bar dataKey="amount" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* PO Status Donut Chart */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-sm font-bold text-slate-800">Purchase Orders by Status</h2>
              <p className="text-xs text-slate-400">Distribution across P2P lifecycle</p>
            </div>
          </div>
          <div className="h-64 flex items-center justify-center">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={poStatus}
                  dataKey="count"
                  nameKey="status"
                  cx="50%"
                  cy="50%"
                  innerRadius={50}
                  outerRadius={80}
                  paddingAngle={3}
                >
                  {poStatus.map((entry) => (
                    <Cell key={entry.status} fill={PO_COLORS[entry.status] || '#cbd5e1'} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: any, name: any) => [`${value} Orders`, name]}
                  contentStyle={{ backgroundColor: '#0f172a', color: '#fff', borderRadius: '8px', fontSize: '12px' }}
                />
                <Legend iconType="circle" wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Row 3: Spend by Department & Top Vendors */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Department Spend */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h2 className="text-sm font-bold text-slate-800">Procurement Spend by Department</h2>
              <p className="text-xs text-slate-400">Committed orders vs Annual Budget</p>
            </div>
          </div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={deptSpend} layout="vertical" margin={{ top: 5, right: 30, left: 40, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="#f1f5f9" />
                <XAxis type="number" tickFormatter={(val) => `₹${(val / 100000).toFixed(0)}L`} tick={{ fontSize: 11 }} />
                <YAxis type="category" dataKey="departmentName" tick={{ fontSize: 11 }} width={120} />
                <Tooltip
                  formatter={(val: any) => [formatCurrency(Number(val)), 'Spent']}
                  contentStyle={{ backgroundColor: '#0f172a', color: '#fff', borderRadius: '8px', fontSize: '12px' }}
                />
                <Bar dataKey="amount" fill="#0284c7" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Top Vendors Table */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs flex flex-col justify-between">
          <div>
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-sm font-bold text-slate-800">Top Strategic Vendors</h2>
                <p className="text-xs text-slate-400">Ranked by total procurement value</p>
              </div>
              <Link to="/vendors" className="text-xs font-semibold text-blue-600 hover:text-blue-700 flex items-center gap-0.5">
                View All <ArrowUpRight className="w-3 h-3" />
              </Link>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead>
                  <tr className="border-b border-slate-100 text-slate-400 uppercase font-semibold">
                    <th className="pb-2">Vendor Name</th>
                    <th className="pb-2 text-center">Rating</th>
                    <th className="pb-2 text-center">Orders</th>
                    <th className="pb-2 text-right">Total Spend</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-50">
                  {topVendors.slice(0, 5).map((v) => (
                    <tr key={v.vendorId} className="hover:bg-slate-50/80 transition-colors">
                      <td className="py-2.5 font-medium text-slate-800 flex items-center gap-2">
                        <Building2 className="w-3.5 h-3.5 text-slate-400" />
                        <span className="truncate max-w-[180px]">{v.vendorName}</span>
                      </td>
                      <td className="py-2.5 text-center">
                        <span className="bg-amber-50 text-amber-700 px-2 py-0.5 rounded font-bold text-[11px] border border-amber-200/50">
                          ★ {v.rating.toFixed(1)}
                        </span>
                      </td>
                      <td className="py-2.5 text-center font-medium text-slate-600">{v.orderCount}</td>
                      <td className="py-2.5 text-right font-bold text-slate-900">{formatCurrency(v.totalSpent)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {/* Row 4: Low Stock Alerts & Recent Operational Activities */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Low Stock Alerts */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center gap-2">
              <div className="w-2.5 h-2.5 rounded-full bg-rose-500 animate-ping"></div>
              <h2 className="text-sm font-bold text-slate-800">Critical Low Stock Alerts</h2>
            </div>
            <Link to="/inventory" className="text-xs font-semibold text-blue-600 hover:text-blue-700 flex items-center gap-0.5">
              Stock Ledger <ArrowUpRight className="w-3 h-3" />
            </Link>
          </div>
          <div className="space-y-2.5">
            {lowStock.length === 0 ? (
              <div className="text-xs text-slate-400 py-6 text-center">All materials are currently above reorder level.</div>
            ) : (
              lowStock.slice(0, 4).map((item) => (
                <div key={item.materialId} className="p-3 bg-rose-50/50 border border-rose-200/70 rounded-lg flex items-center justify-between">
                  <div>
                    <div className="font-semibold text-xs text-slate-900">{item.materialName}</div>
                    <div className="text-[11px] text-slate-500 font-mono mt-0.5">Code: {item.materialCode} | {item.categoryName}</div>
                  </div>
                  <div className="text-right">
                    <div className="text-xs font-bold text-rose-700">
                      {item.currentStock} {item.uom} <span className="font-normal text-slate-400">/ Min {item.reorderLevel}</span>
                    </div>
                    <Link
                      to="/requisitions"
                      className="text-[10px] text-blue-700 font-semibold bg-white px-2 py-0.5 rounded border border-blue-200 hover:bg-blue-50 mt-1 inline-block"
                    >
                      + Raise Requisition
                    </Link>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Recent Activities */}
        <div className="bg-white p-5 rounded-xl border border-slate-200/80 shadow-xs">
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-sm font-bold text-slate-800">Recent Procurement Activities</h2>
            <span className="text-xs text-slate-400">Live transaction stream</span>
          </div>
          <div className="space-y-3">
            {activities.slice(0, 5).map((act, index) => (
              <div key={index} className="flex items-start gap-3 text-xs pb-2.5 border-b border-slate-50 last:border-0 last:pb-0">
                <div className={`w-7 h-7 rounded-lg flex items-center justify-center shrink-0 font-bold text-[10px] ${
                  act.type === 'PO' ? 'bg-blue-100 text-blue-700' : 'bg-emerald-100 text-emerald-700'
                }`}>
                  {act.type}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="font-semibold text-slate-800 flex items-center justify-between">
                    <span>{act.title}</span>
                    <span className="text-[10px] text-slate-400 font-normal">{act.timestamp}</span>
                  </div>
                  <p className="text-[11px] text-slate-500 mt-0.5 truncate">{act.description}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

