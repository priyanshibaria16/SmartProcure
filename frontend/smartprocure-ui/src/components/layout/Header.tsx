import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Bell,
  Search,
  LogOut,
  UserCheck,
  ChevronDown,
  Building,
  CheckCircle,
  AlertTriangle,
  Info
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import type { UserRole } from '../../types';

export const Header: React.FC = () => {
  const { user, logout, quickLogin } = useAuth();
  const navigate = useNavigate();
  const [showRoleMenu, setShowRoleMenu] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);

  const demoRoles: { role: UserRole; label: string; desc: string }[] = [
    { role: 'Administrator', label: 'Admin', desc: 'Full ERP control & master data' },
    { role: 'RequisitionCreator', label: 'Requester', desc: 'Production shop-floor PR creation' },
    { role: 'PurchaseManager', label: 'Purchase Mgr', desc: 'Approve PRs, quotations, issue POs' },
    { role: 'WarehouseManager', label: 'Warehouse Mgr', desc: 'GRN dock receiving & quality check' },
    { role: 'FinanceManager', label: 'Finance Mgr', desc: '3-way matching & payment settlement' },
    { role: 'ManagementViewer', label: 'Executive Viewer', desc: 'Read-only analytics & reporting' },
  ];

  const handleRoleSwitch = async (role: UserRole) => {
    setShowRoleMenu(false);
    await quickLogin(role);
    navigate('/');
  };

  return (
    <header className="h-16 bg-white border-b border-slate-200 px-6 flex items-center justify-between shadow-xs sticky top-0 z-30">
      {/* Search Input */}
      <div className="flex items-center gap-3 w-96">
        <div className="relative w-full">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search POs, materials, vendors, requisitions..."
            className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-1.5 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all"
          />
        </div>
      </div>

      {/* Right Controls */}
      <div className="flex items-center gap-4">
        {/* Quick Demo Role Switcher */}
        <div className="relative">
          <button
            onClick={() => setShowRoleMenu(!showRoleMenu)}
            className="flex items-center gap-2 bg-blue-50 hover:bg-blue-100 text-blue-700 px-3 py-1.5 rounded-lg border border-blue-200 text-xs font-medium transition-all"
            title="Switch demo persona"
          >
            <UserCheck className="w-3.5 h-3.5 text-blue-600" />
            <span>Role: <strong>{user?.roleName || 'Select'}</strong></span>
            <ChevronDown className="w-3.5 h-3.5 text-blue-500" />
          </button>

          {showRoleMenu && (
            <div className="absolute right-0 mt-2 w-72 bg-white rounded-xl shadow-xl border border-slate-200 py-2 z-50 animate-in fade-in zoom-in-95 duration-100">
              <div className="px-3 py-1.5 text-[11px] font-semibold text-slate-400 uppercase tracking-wider border-b border-slate-100">
                1-Click ERP Role Switcher
              </div>
              <div className="py-1">
                {demoRoles.map((r) => (
                  <button
                    key={r.role}
                    onClick={() => handleRoleSwitch(r.role)}
                    className={`w-full text-left px-3.5 py-2 text-xs flex flex-col hover:bg-blue-50 transition-colors ${
                      user?.roleName === r.role ? 'bg-blue-50/70 font-semibold text-blue-700' : 'text-slate-700'
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <span className="font-medium">{r.label}</span>
                      {user?.roleName === r.role && <CheckCircle className="w-3.5 h-3.5 text-blue-600" />}
                    </div>
                    <span className="text-[11px] text-slate-400 font-normal">{r.desc}</span>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Notifications */}
        <div className="relative">
          <button
            onClick={() => setShowNotifications(!showNotifications)}
            className="relative p-2 rounded-lg text-slate-500 hover:text-slate-800 hover:bg-slate-100 transition-colors"
          >
            <Bell className="w-4 h-4" />
            <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-red-500 ring-2 ring-white"></span>
          </button>

          {showNotifications && (
            <div className="absolute right-0 mt-2 w-80 bg-white rounded-xl shadow-xl border border-slate-200 py-2 z-50">
              <div className="px-4 py-2 border-b border-slate-100 flex items-center justify-between">
                <span className="text-xs font-bold text-slate-800">Notifications</span>
                <span className="text-[10px] bg-blue-100 text-blue-700 font-semibold px-2 py-0.5 rounded-full">3 New</span>
              </div>
              <div className="divide-y divide-slate-100 max-h-72 overflow-y-auto">
                <div className="p-3 text-xs hover:bg-slate-50 cursor-pointer">
                  <div className="flex items-center gap-2 font-semibold text-amber-700">
                    <AlertTriangle className="w-3.5 h-3.5 text-amber-600" />
                    <span>Low Stock: Copper Winding Wire</span>
                  </div>
                  <p className="text-[11px] text-slate-500 mt-1">Available stock (240 KG) is below reorder point (300 KG).</p>
                </div>
                <div className="p-3 text-xs hover:bg-slate-50 cursor-pointer">
                  <div className="flex items-center gap-2 font-semibold text-blue-700">
                    <Info className="w-3.5 h-3.5 text-blue-600" />
                    <span>Requisition PR-2026-0007 Pending</span>
                  </div>
                  <p className="text-[11px] text-slate-500 mt-1">₹1,68,000 awaiting Purchase Manager approval.</p>
                </div>
                <div className="p-3 text-xs hover:bg-slate-50 cursor-pointer">
                  <div className="flex items-center gap-2 font-semibold text-rose-700">
                    <AlertTriangle className="w-3.5 h-3.5 text-rose-600" />
                    <span>Invoice INV-ELC-1092 Mismatched</span>
                  </div>
                  <p className="text-[11px] text-slate-500 mt-1">Quantity variance detected during 3-way match.</p>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* User Menu & Logout */}
        <div className="flex items-center gap-3 pl-3 border-l border-slate-200">
          <div className="text-right">
            <div className="text-xs font-semibold text-slate-800 leading-tight">{user?.fullName || 'User'}</div>
            <div className="text-[11px] text-slate-400 flex items-center justify-end gap-1">
              <Building className="w-3 h-3" />
              {user?.departmentName || 'Apex Mfg'}
            </div>
          </div>
          <button
            onClick={() => {
              logout();
              navigate('/login');
            }}
            className="p-2 rounded-lg text-slate-400 hover:text-rose-600 hover:bg-rose-50 transition-colors"
            title="Sign out"
          >
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </header>
  );
};

