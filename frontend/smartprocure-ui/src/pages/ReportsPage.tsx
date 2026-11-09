import React, { useState } from 'react';
import {
  BarChart3,
  Download,
  Calendar,
  FileSpreadsheet,
  Layers,
  Database,
  ExternalLink,
  ShieldCheck,
  TrendingUp,
  PieChart,
  CheckCircle
} from 'lucide-react';
import { reportsApi } from '../services/api';

export const ReportsPage: React.FC = () => {
  const [downloading, setDownloading] = useState<string | null>(null);
  const [fromDate, setFromDate] = useState<string>(
    new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [toDate, setToDate] = useState<string>(new Date().toISOString().split('T')[0]);

  const handleDownloadReport = async (
    type: 'procurement' | 'inventory' | 'vendor' | 'payments',
    filename: string
  ) => {
    try {
      setDownloading(type);
      let res: any;
      if (type === 'procurement') {
        res = await reportsApi.exportProcurement({ fromDate, toDate });
      } else if (type === 'inventory') {
        res = await reportsApi.exportInventory();
      } else if (type === 'vendor') {
        res = await reportsApi.exportVendorPerformance();
      } else if (type === 'payments') {
        res = await reportsApi.exportOutstandingPayments();
      }

      // Create download blob link
      const blob = new Blob([res.data], { type: 'text/csv' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${filename}_${new Date().toISOString().split('T')[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Failed to export report:', err);
      alert('Failed to generate CSV export.');
    } finally {
      setDownloading(null);
    }
  };

  const reportsList = [
    {
      id: 'procurement',
      title: 'Procurement Spending & PO Register',
      desc: 'Complete log of purchase orders, vendor breakdowns, GST components, and line totals within date range.',
      icon: FileSpreadsheet,
      filename: 'SmartProcure_PO_Register',
      color: 'blue'
    },
    {
      id: 'inventory',
      title: 'Perpetual Inventory Valuation (MB52)',
      desc: 'Plant and warehouse stock balances, unit standard prices, total valuation in INR, and low-stock replenishment flags.',
      icon: Layers,
      filename: 'SmartProcure_Inventory_Valuation',
      color: 'emerald'
    },
    {
      id: 'vendor',
      title: 'Vendor Performance & Scorecard',
      desc: 'Supplier fulfillment metrics, total spend, rating, GST compliance status, and payment terms audit.',
      icon: TrendingUp,
      filename: 'SmartProcure_Vendor_Performance',
      color: 'indigo'
    },
    {
      id: 'payments',
      title: 'Outstanding Payables & Liability Aging',
      desc: 'Accounts payable ledger, open invoices, due dates, paid amounts, and outstanding liability balances.',
      icon: PieChart,
      filename: 'SmartProcure_Outstanding_Payables',
      color: 'amber'
    }
  ];

  const powerBiViews = [
    {
      viewName: 'vw_ProcurementSpending',
      description: 'Aggregates monthly procurement volume, PO counts, and vendor spends for P2P trend analysis.',
      daxSample: 'TotalSpend = SUM(vw_ProcurementSpending[LineTotal])',
      sapEquivalent: 'ME80FN - General Purchase Order Evaluations'
    },
    {
      viewName: 'vw_InventoryValuation',
      description: 'Calculates live perpetual stock valuation, category distribution, and low stock items.',
      daxSample: 'InventoryValuation = SUMX(vw_InventoryValuation, [TotalStock] * [UnitPrice])',
      sapEquivalent: 'MB52 - Warehouse Stocks on Hand'
    },
    {
      viewName: 'vw_InvoiceAgingAndLiabilities',
      description: 'Buckets outstanding invoices into Current, 1-30 Days, 31-60 Days, and 60+ Days aging brackets.',
      daxSample: 'AgingBucket = IF([DaysPastDue] > 60, "60+ Days", IF([DaysPastDue] > 30, "31-60 Days", "Current"))',
      sapEquivalent: 'FBL1N - Vendor Line Item Display'
    },
    {
      viewName: 'vw_DepartmentExpenditure',
      description: 'Tracks department budget utilization against authorized purchase requisitions.',
      daxSample: 'BudgetVariance = SUM(Departments[Budget]) - SUM(vw_DepartmentExpenditure[CommittedAmount])',
      sapEquivalent: 'KOB1 - Internal Orders: Actual Line Items'
    }
  ];

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-5">
        <div>
          <div className="flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-lg bg-indigo-600 flex items-center justify-center text-white shadow-md shadow-indigo-500/20">
              <BarChart3 className="w-5 h-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-slate-900 tracking-tight">
                Enterprise Reporting & Power BI Hub
              </h1>
              <p className="text-xs text-slate-500">
                Official CSV Data Feeds, Analytical SQL Views & Business Intelligence Integration
              </p>
            </div>
          </div>
        </div>

        {/* Date Filter Bar */}
        <div className="flex items-center gap-2 bg-white px-3 py-1.5 rounded-lg border border-slate-200 shadow-2xs text-xs">
          <Calendar className="w-4 h-4 text-slate-400" />
          <span className="text-slate-500 font-medium">From:</span>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="border-none bg-transparent text-slate-700 font-semibold focus:outline-none"
          />
          <span className="text-slate-400">|</span>
          <span className="text-slate-500 font-medium">To:</span>
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className="border-none bg-transparent text-slate-700 font-semibold focus:outline-none"
          />
        </div>
      </div>

      {/* CSV Export Cards */}
      <div className="space-y-4">
        <h2 className="text-sm font-bold text-slate-900 uppercase tracking-wider flex items-center gap-2">
          <FileSpreadsheet className="w-4 h-4 text-indigo-600" />
          <span>Operational Data Exports (Excel / CSV)</span>
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {reportsList.map((rep) => {
            const Icon = rep.icon;
            const isWorking = downloading === rep.id;
            return (
              <div
                key={rep.id}
                className="bg-white rounded-xl border border-slate-200 p-5 shadow-2xs hover:shadow-md transition-all flex flex-col justify-between"
              >
                <div>
                  <div className="flex items-center gap-3 mb-2">
                    <div className="w-9 h-9 rounded-lg bg-indigo-50 border border-indigo-100 flex items-center justify-center text-indigo-600">
                      <Icon className="w-5 h-5" />
                    </div>
                    <h3 className="font-bold text-slate-900 text-sm">{rep.title}</h3>
                  </div>
                  <p className="text-xs text-slate-500 leading-relaxed pl-12">{rep.desc}</p>
                </div>

                <div className="mt-5 pt-3 border-t border-slate-100 flex items-center justify-between">
                  <span className="text-[11px] text-slate-400 font-mono">Format: RFC 4180 CSV</span>
                  <button
                    onClick={() =>
                      handleDownloadReport(rep.id as any, rep.filename)
                    }
                    disabled={isWorking}
                    className="flex items-center gap-1.5 px-3.5 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-lg shadow-xs transition-colors disabled:opacity-50"
                  >
                    <Download className={`w-3.5 h-3.5 ${isWorking ? 'animate-bounce' : ''}`} />
                    <span>{isWorking ? 'Exporting...' : 'Download CSV'}</span>
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Power BI & SQL Analytics Architecture Showcase */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-bold text-slate-900 uppercase tracking-wider flex items-center gap-2">
            <Database className="w-4 h-4 text-blue-600" />
            <span>Power BI Ready Analytical Views (03_PowerBI_Views.sql)</span>
          </h2>
          <span className="text-xs text-blue-600 font-semibold bg-blue-50 px-2.5 py-0.5 rounded border border-blue-200">
            DirectQuery & Import Compatible
          </span>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {powerBiViews.map((v) => (
            <div key={v.viewName} className="bg-white rounded-xl border border-slate-200 p-5 shadow-2xs space-y-3">
              <div className="flex items-center justify-between">
                <span className="font-mono text-xs font-bold text-blue-700 bg-blue-50 px-2.5 py-1 rounded border border-blue-200">
                  {v.viewName}
                </span>
                <span className="text-[11px] font-semibold text-slate-500">{v.sapEquivalent}</span>
              </div>
              <p className="text-xs text-slate-600">{v.description}</p>
              <div className="bg-slate-900 text-slate-200 p-3 rounded-lg font-mono text-[11px] overflow-x-auto border border-slate-800">
                <div className="text-slate-400 text-[10px] uppercase font-sans mb-1">Sample DAX Measure:</div>
                <code>{v.daxSample}</code>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Architecture Card */}
      <div className="bg-slate-900 text-slate-200 rounded-2xl p-6 border border-slate-800 space-y-3">
        <div className="flex items-center gap-2 text-white font-bold text-sm">
          <ShieldCheck className="w-5 h-5 text-emerald-400" />
          <span>Enterprise Analytics Integration Instructions</span>
        </div>
        <p className="text-xs text-slate-400 leading-relaxed">
          To connect your desktop Power BI client or Fabric workspace to SmartProcure, point the Power BI SQL Server connector to your instance and select the pre-built views:
          <code className="text-blue-300 font-mono ml-1">database/scripts/03_PowerBI_Views.sql</code>. All dimensions, star-schema keys, and measure definitions are synchronized with the SAP MM Procure-to-Pay workflow.
        </p>
      </div>
    </div>
  );
};
