import React from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  FileText,
  BadgeDollarSign,
  ShoppingCart,
  Truck,
  Boxes,
  Receipt,
  CheckCircle2,
  CreditCard,
  Building2,
  PackageSearch,
  BarChart3,
  Users,
  ShieldCheck
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

export const Sidebar: React.FC = () => {
  const { user, isAdmin } = useAuth();

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-all ${
      isActive
        ? 'bg-blue-600 text-white shadow-sm'
        : 'text-slate-300 hover:bg-slate-800 hover:text-white'
    }`;

  return (
    <aside className="w-64 bg-slate-900 text-slate-200 min-h-screen flex flex-col border-r border-slate-800 select-none">
      {/* Brand Header */}
      <div className="p-5 border-b border-slate-800/80">
        <div className="flex items-center gap-2.5">
          <div className="w-9 h-9 rounded-lg bg-blue-600 flex items-center justify-center font-bold text-white shadow-md shadow-blue-500/20 text-lg">
            SP
          </div>
          <div>
            <div className="font-bold text-white tracking-wide text-base leading-tight">SmartProcure</div>
            <div className="text-[11px] text-blue-400 font-medium">SAP MM Enterprise ERP</div>
          </div>
        </div>
        <div className="mt-3 px-2 py-1 bg-slate-800/60 rounded text-[11px] text-slate-400 border border-slate-700/50 flex items-center justify-between">
          <span>Tenant: Apex Mfg Pvt Ltd</span>
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
        </div>
      </div>

      {/* Navigation Sections */}
      <div className="flex-1 overflow-y-auto px-3 py-4 space-y-6">
        {/* Core */}
        <div>
          <div className="px-3 mb-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Overview
          </div>
          <NavLink to="/" className={navLinkClass}>
            <LayoutDashboard className="w-4 h-4" />
            <span>ERP Dashboard</span>
          </NavLink>
        </div>

        {/* Procurement */}
        <div>
          <div className="px-3 mb-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Procurement (P2P)
          </div>
          <div className="space-y-1">
            <NavLink to="/requisitions" className={navLinkClass}>
              <FileText className="w-4 h-4" />
              <span>Purchase Requisitions</span>
            </NavLink>
            <NavLink to="/quotations" className={navLinkClass}>
              <BadgeDollarSign className="w-4 h-4" />
              <span>Vendor Quotations</span>
            </NavLink>
            <NavLink to="/purchase-orders" className={navLinkClass}>
              <ShoppingCart className="w-4 h-4" />
              <span>Purchase Orders</span>
            </NavLink>
          </div>
        </div>

        {/* Inventory & Warehouse */}
        <div>
          <div className="px-3 mb-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Warehouse & Stock
          </div>
          <div className="space-y-1">
            <NavLink to="/goods-receipts" className={navLinkClass}>
              <Truck className="w-4 h-4" />
              <span>Goods Receipts (GRN)</span>
            </NavLink>
            <NavLink to="/inventory" className={navLinkClass}>
              <Boxes className="w-4 h-4" />
              <span>Perpetual Stock Ledger</span>
            </NavLink>
          </div>
        </div>

        {/* Finance */}
        <div>
          <div className="px-3 mb-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Finance & AP
          </div>
          <div className="space-y-1">
            <NavLink to="/invoices" className={navLinkClass}>
              <Receipt className="w-4 h-4" />
              <span>Supplier Invoices</span>
            </NavLink>
            <NavLink to="/payments" className={navLinkClass}>
              <CreditCard className="w-4 h-4" />
              <span>Payment Settlement</span>
            </NavLink>
          </div>
        </div>

        {/* Master Data */}
        <div>
          <div className="px-3 mb-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Master Data
          </div>
          <div className="space-y-1">
            <NavLink to="/vendors" className={navLinkClass}>
              <Building2 className="w-4 h-4" />
              <span>Vendor Master</span>
            </NavLink>
            <NavLink to="/materials" className={navLinkClass}>
              <PackageSearch className="w-4 h-4" />
              <span>Material Master</span>
            </NavLink>
          </div>
        </div>

        {/* Reports & System */}
        <div>
          <div className="px-3 mb-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">
            Intelligence
          </div>
          <div className="space-y-1">
            <NavLink to="/reports" className={navLinkClass}>
              <BarChart3 className="w-4 h-4" />
              <span>Reports & Power BI</span>
            </NavLink>
            {isAdmin && (
              <NavLink to="/users" className={navLinkClass}>
                <Users className="w-4 h-4" />
                <span>User Governance</span>
              </NavLink>
            )}
          </div>
        </div>
      </div>

      {/* User Badge Footer */}
      <div className="p-3 border-t border-slate-800 bg-slate-950/60">
        <div className="flex items-center gap-3 px-2 py-1.5">
          <div className="w-8 h-8 rounded-full bg-slate-700 flex items-center justify-center font-bold text-white text-xs border border-slate-600">
            {user?.fullName?.charAt(0) || 'U'}
          </div>
          <div className="flex-1 min-w-0">
            <div className="text-xs font-semibold text-white truncate">{user?.fullName || 'User'}</div>
            <div className="text-[11px] text-blue-400 truncate flex items-center gap-1">
              <ShieldCheck className="w-3 h-3" />
              {user?.roleName || 'Viewer'}
            </div>
          </div>
        </div>
      </div>
    </aside>
  );
};
