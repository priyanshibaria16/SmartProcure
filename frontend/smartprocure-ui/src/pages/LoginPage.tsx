import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ShieldCheck,
  Lock,
  Mail,
  ArrowRight,
  AlertCircle,
  Building2,
  CheckCircle,
  Zap,
  Sparkles
} from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import type { UserRole } from '../types';

export const LoginPage: React.FC = () => {
  const { login, quickLogin } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState<string>('admin@smartprocure.com');
  const [password, setPassword] = useState<string>('Admin@123');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const demoAccounts: {
    role: UserRole;
    label: string;
    email: string;
    badge: string;
    desc: string;
  }[] = [
    {
      role: 'Administrator',
      label: 'Administrator',
      email: 'admin@smartprocure.com',
      badge: 'Full Access',
      desc: 'System setup, master data & user administration'
    },
    {
      role: 'PurchaseManager',
      label: 'Purchase Manager',
      email: 'purchasemanager@smartprocure.com',
      badge: 'Approver',
      desc: 'Approve requisitions, evaluate bids & issue POs'
    },
    {
      role: 'WarehouseManager',
      label: 'Warehouse Manager',
      email: 'warehouse@smartprocure.com',
      badge: 'Dock & MIGO',
      desc: 'Record GRN, inspect dock quality & stock audit'
    },
    {
      role: 'FinanceManager',
      label: 'Finance Manager',
      email: 'finance@smartprocure.com',
      badge: 'MIRO & AP',
      desc: '3-Way matching verification & vendor payouts'
    },
    {
      role: 'RequisitionCreator',
      label: 'Requisitioner',
      email: 'requester@smartprocure.com',
      badge: 'Shop Floor',
      desc: 'Create and submit material purchase requests'
    },
    {
      role: 'ManagementViewer',
      label: 'Executive Viewer',
      email: 'management@smartprocure.com',
      badge: 'Analytics',
      desc: 'Read-only KPI dashboards & Power BI intelligence'
    }
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await login(email, password);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid email or password.');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickDemo = async (account: typeof demoAccounts[0]) => {
    setError(null);
    setLoading(true);
    try {
      await quickLogin(account.role);
      navigate('/');
    } catch (err: any) {
      setError('Quick login failed. Ensure the backend API is running.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col justify-center py-12 sm:px-6 lg:px-8 selection:bg-blue-600 selection:text-white">
      {/* Background Glow */}
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,_var(--tw-gradient-stops))] from-blue-900/20 via-slate-950 to-slate-950 pointer-events-none" />

      <div className="sm:mx-auto sm:w-full sm:max-w-md relative z-10 text-center space-y-2">
        <div className="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-blue-600 text-white font-black text-2xl shadow-xl shadow-blue-600/30">
          SP
        </div>
        <h1 className="text-3xl font-extrabold text-white tracking-tight">SmartProcure</h1>
        <p className="text-xs text-blue-400 font-medium uppercase tracking-widest">
          Enterprise ERP Procurement & Inventory Platform
        </p>
        <div className="inline-block mt-1 px-3 py-1 rounded-full bg-slate-900/90 border border-slate-800 text-[11px] text-slate-400">
          Industrial Tenant: <strong className="text-slate-200">Apex Manufacturing Industries Pvt. Ltd.</strong>
        </div>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-xl relative z-10 space-y-6">
        {/* Main Login Card */}
        <div className="bg-slate-900/90 backdrop-blur-md py-8 px-6 sm:px-10 shadow-2xl rounded-2xl border border-slate-800">
          <form className="space-y-5" onSubmit={handleSubmit}>
            {error && (
              <div className="p-3 rounded-lg bg-rose-950/60 border border-rose-800/80 text-rose-300 text-xs flex items-center gap-2">
                <AlertCircle className="w-4 h-4 shrink-0 text-rose-400" />
                <span>{error}</span>
              </div>
            )}

            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                Corporate Email Address
              </label>
              <div className="relative">
                <Mail className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full bg-slate-950 border border-slate-800 rounded-lg pl-9 pr-4 py-2.5 text-xs text-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all font-medium"
                  placeholder="name@smartprocure.com"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">
                Password
              </label>
              <div className="relative">
                <Lock className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full bg-slate-950 border border-slate-800 rounded-lg pl-9 pr-4 py-2.5 text-xs text-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-500 transition-all font-medium"
                  placeholder="••••••••"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full flex justify-center items-center gap-2 py-2.5 px-4 border border-transparent rounded-lg shadow-sm text-xs font-bold text-white bg-blue-600 hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-all disabled:opacity-50"
            >
              {loading ? (
                <span>Authenticating with ERP Core...</span>
              ) : (
                <>
                  <span>Sign In to SmartProcure</span>
                  <ArrowRight className="w-4 h-4" />
                </>
              )}
            </button>
          </form>

          {/* Quick Demo Accounts Persona Selector */}
          <div className="mt-8 pt-6 border-t border-slate-800/80">
            <div className="flex items-center gap-2 mb-3">
              <Zap className="w-4 h-4 text-amber-400" />
              <h2 className="text-xs font-bold text-slate-200 uppercase tracking-wider">
                Instant 1-Click Demo Personas
              </h2>
            </div>
            <p className="text-[11px] text-slate-400 mb-4">
              Select any persona below to authenticate instantly with its official seeded permissions and workflows:
            </p>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
              {demoAccounts.map((account) => (
                <button
                  key={account.role}
                  type="button"
                  onClick={() => handleQuickDemo(account)}
                  disabled={loading}
                  className="text-left p-3 rounded-xl bg-slate-950/60 hover:bg-blue-950/40 border border-slate-800/80 hover:border-blue-700/60 transition-all group"
                >
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-bold text-slate-200 group-hover:text-blue-400 transition-colors">
                      {account.label}
                    </span>
                    <span className="text-[10px] font-semibold text-blue-400/90 bg-blue-950/80 px-2 py-0.5 rounded border border-blue-900/60">
                      {account.badge}
                    </span>
                  </div>
                  <div className="text-[11px] text-slate-400 mt-1 line-clamp-1">
                    {account.desc}
                  </div>
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Footer Portfolio Badge */}
        <div className="text-center space-y-1">
          <p className="text-xs text-slate-500">
            Designed for SAP MM / S/4HANA Consulting Showcase & Portfolio Presentation
          </p>
          <p className="text-[11px] text-slate-600 font-mono">
            Compliant with SAP MM T-Codes: ME51N • ME41 • ME21N • MIGO • MB52 • MIRO • F-53
          </p>
        </div>
      </div>
    </div>
  );
};

