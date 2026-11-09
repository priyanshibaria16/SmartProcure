import React, { useEffect, useState } from 'react';
import {
  Boxes,
  Search,
  Filter,
  AlertTriangle,
  History,
  TrendingDown,
  Layers,
  ArrowRight,
  ShieldCheck,
  Plus
} from 'lucide-react';
import { inventoryApi, masterApi } from '../services/api';
import type { InventoryBalance, InventoryTransaction, Warehouse, Category } from '../types';
import { useAuth } from '../context/AuthContext';

export const InventoryPage: React.FC = () => {
  const { isWarehouse, isAdmin } = useAuth();

  const [activeTab, setActiveTab] = useState<'balances' | 'transactions'>('balances');
  const [balances, setBalances] = useState<InventoryBalance[]>([]);
  const [transactions, setTransactions] = useState<InventoryTransaction[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  // Filters
  const [selectedWarehouse, setSelectedWarehouse] = useState<string>('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [lowStockOnly, setLowStockOnly] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Adjustment Modal
  const [showAdjModal, setShowAdjModal] = useState(false);
  const [adjMaterialId, setAdjMaterialId] = useState<number>(1);
  const [adjWarehouseId, setAdjWarehouseId] = useState<number>(1);
  const [adjQuantity, setAdjQuantity] = useState<number>(10);
  const [adjRemarks, setAdjRemarks] = useState<string>('Audited physical cycle count surplus');

  const loadData = async () => {
    setLoading(true);
    try {
      const [balRes, txRes, whRes, catRes] = await Promise.all([
        inventoryApi.getBalances({
          warehouseId: selectedWarehouse ? parseInt(selectedWarehouse) : undefined,
          categoryId: selectedCategory ? parseInt(selectedCategory) : undefined,
          lowStockOnly: lowStockOnly ? true : undefined,
          page: 1,
          pageSize: 100,
        }),
        inventoryApi.getTransactions({ page: 1, pageSize: 50 }),
        masterApi.getWarehouses(),
        masterApi.getCategories(),
      ]);

      if (balRes.data.success) setBalances(balRes.data.data.items);
      if (txRes.data.success) setTransactions(txRes.data.data.items);
      if (whRes.data.success) setWarehouses(whRes.data.data);
      if (catRes.data.success) setCategories(catRes.data.data);
    } catch (err) {
      console.error('Failed to load inventory data', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [selectedWarehouse, selectedCategory, lowStockOnly]);

  const handleAdjustmentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!adjRemarks.trim()) {
      alert('Remarks are mandatory for stock adjustments');
      return;
    }

    try {
      await inventoryApi.recordAdjustment({
        materialId: adjMaterialId,
        warehouseId: adjWarehouseId,
        quantity: adjQuantity,
        remarks: adjRemarks,
      });
      setShowAdjModal(false);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to adjust stock');
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0,
    }).format(val);
  };

  const totalValuation = balances.reduce((acc, b) => acc + (b.availableStock + b.reservedStock) * b.unitPrice, 0);
  const lowStockCount = balances.filter((b) => b.isLowStock).length;

  const filteredBalances = balances.filter((b) => {
    if (!searchTerm) return true;
    const s = searchTerm.toLowerCase();
    return b.materialCode.toLowerCase().includes(s) || b.materialName.toLowerCase().includes(s) || b.categoryName.toLowerCase().includes(s);
  });

  return (
    <div className="space-y-5">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Perpetual Inventory & Stock Valuation</h1>
          <p className="text-xs text-slate-500">
            Real-time warehouse stock ledger, safety stock monitoring and atomic transaction audit
          </p>
        </div>
        {(isWarehouse || isAdmin) && (
          <button
            onClick={() => setShowAdjModal(true)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-slate-800 hover:bg-slate-900 text-white rounded-lg text-xs font-semibold shadow-xs transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span>Audit Physical Adjustment</span>
          </button>
        )}
      </div>

      {/* Summary KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Total Inventory Valuation</div>
            <div className="text-xl font-bold text-slate-900 mt-1">{formatCurrency(totalValuation)}</div>
            <div className="text-[11px] text-slate-400 mt-0.5">Across {balances.length} warehouse balance records</div>
          </div>
          <div className="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
            <Layers className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Low Stock Deficits</div>
            <div className="text-xl font-bold text-rose-600 mt-1">{lowStockCount} Items</div>
            <div className="text-[11px] text-rose-500 mt-0.5 font-medium">At or below configured reorder point</div>
          </div>
          <div className="w-10 h-10 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
            <AlertTriangle className="w-5 h-5" />
          </div>
        </div>

        <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex items-center justify-between">
          <div>
            <div className="text-xs font-medium text-slate-500 uppercase tracking-wider">Storage Depots</div>
            <div className="text-xl font-bold text-slate-900 mt-1">{warehouses.length} Active Hubs</div>
            <div className="text-[11px] text-emerald-600 mt-0.5 font-medium">Pune, Ahmedabad & Vadodara</div>
          </div>
          <div className="w-10 h-10 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <Boxes className="w-5 h-5" />
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-slate-200 gap-4 text-xs font-semibold">
        <button
          onClick={() => setActiveTab('balances')}
          className={`pb-2.5 flex items-center gap-1.5 transition-colors border-b-2 ${
            activeTab === 'balances' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          <Boxes className="w-3.5 h-3.5" />
          <span>Warehouse Stock Balances</span>
        </button>
        <button
          onClick={() => setActiveTab('transactions')}
          className={`pb-2.5 flex items-center gap-1.5 transition-colors border-b-2 ${
            activeTab === 'transactions' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          <History className="w-3.5 h-3.5" />
          <span>Stock Movement Ledger (Audit)</span>
        </button>
      </div>

      {/* TAB 1: STOCK BALANCES */}
      {activeTab === 'balances' && (
        <div className="space-y-4">
          {/* Filters */}
          <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-xs flex flex-wrap items-center gap-3">
            <div className="relative flex-1 min-w-[200px]">
              <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Search material code, description, category..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-8 pr-3 py-1.5 text-xs focus:outline-none"
              />
            </div>

            <select
              value={selectedWarehouse}
              onChange={(e) => setSelectedWarehouse(e.target.value)}
              className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 focus:outline-none"
            >
              <option value="">All Warehouses</option>
              {warehouses.map((w) => (
                <option key={w.warehouseId} value={w.warehouseId}>
                  {w.warehouseName}
                </option>
              ))}
            </select>

            <select
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 focus:outline-none"
            >
              <option value="">All Categories</option>
              {categories.map((c) => (
                <option key={c.categoryId} value={c.categoryId}>
                  {c.categoryName}
                </option>
              ))}
            </select>

            <button
              onClick={() => setLowStockOnly(!lowStockOnly)}
              className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all ${
                lowStockOnly
                  ? 'bg-rose-50 border-rose-300 text-rose-700'
                  : 'bg-slate-50 border-slate-200 text-slate-600 hover:bg-slate-100'
              }`}
            >
              <AlertTriangle className="w-3.5 h-3.5 text-rose-500" />
              <span>Low Stock Alerts Only</span>
            </button>
          </div>

          <div className="bg-white rounded-xl border border-slate-200/80 shadow-xs overflow-hidden">
            <table className="w-full text-left text-xs">
              <thead>
                <tr className="bg-slate-50/80 border-b border-slate-200 text-slate-500 uppercase font-semibold text-[11px]">
                  <th className="py-3 px-4">Material Code</th>
                  <th className="py-3 px-3">Material Description</th>
                  <th className="py-3 px-3">Warehouse</th>
                  <th className="py-3 px-3">Category</th>
                  <th className="py-3 px-3 text-right">Standard Price</th>
                  <th className="py-3 px-3 text-right">Available Stock</th>
                  <th className="py-3 px-3 text-right">Reorder Point</th>
                  <th className="py-3 px-3 text-right">Total Valuation</th>
                  <th className="py-3 px-4 text-center">Stock Health</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <tr>
                    <td colSpan={9} className="py-12 text-center text-slate-400">Loading Stock Balances...</td>
                  </tr>
                ) : filteredBalances.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="py-12 text-center text-slate-400">No inventory balances found.</td>
                  </tr>
                ) : (
                  filteredBalances.map((b) => (
                    <tr key={b.balanceId} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-3 px-4 font-mono font-bold text-blue-700">{b.materialCode}</td>
                      <td className="py-3 px-3 font-medium text-slate-800">{b.materialName}</td>
                      <td className="py-3 px-3 text-slate-600">{b.warehouseName}</td>
                      <td className="py-3 px-3 text-slate-500">{b.categoryName}</td>
                      <td className="py-3 px-3 text-right font-mono text-slate-700">{formatCurrency(b.unitPrice)}</td>
                      <td className={`py-3 px-3 text-right font-mono font-bold ${b.isLowStock ? 'text-rose-600' : 'text-slate-900'}`}>
                        {b.availableStock} {b.uomCode}
                      </td>
                      <td className="py-3 px-3 text-right font-mono text-slate-500">
                        {b.reorderLevel} {b.uomCode}
                      </td>
                      <td className="py-3 px-3 text-right font-mono font-bold text-slate-900">
                        {formatCurrency((b.availableStock + b.reservedStock) * b.unitPrice)}
                      </td>
                      <td className="py-3 px-4 text-center">
                        {b.isLowStock ? (
                          <span className="inline-flex items-center gap-1 bg-rose-50 text-rose-700 border border-rose-200 px-2 py-0.5 rounded-full text-[10px] font-bold">
                            <AlertTriangle className="w-3 h-3 text-rose-500" />
                            Low Stock
                          </span>
                        ) : (
                          <span className="inline-block bg-emerald-50 text-emerald-700 border border-emerald-200 px-2 py-0.5 rounded-full text-[10px] font-bold">
                            Optimal
                          </span>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 2: TRANSACTIONS AUDIT LEDGER */}
      {activeTab === 'transactions' && (
        <div className="bg-white rounded-xl border border-slate-200/80 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100">
            <h2 className="text-sm font-bold text-slate-800">Immutable Inventory Transaction Ledger</h2>
            <p className="text-xs text-slate-500">Chronological history of all Goods Receipts, Issues, and Physical Adjustments</p>
          </div>
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="bg-slate-50/80 border-b border-slate-200 text-slate-500 uppercase font-semibold text-[11px]">
                <th className="py-3 px-4">Tx ID</th>
                <th className="py-3 px-3">Date</th>
                <th className="py-3 px-3">Material</th>
                <th className="py-3 px-3">Warehouse</th>
                <th className="py-3 px-3">Transaction Type</th>
                <th className="py-3 px-3 text-right">Quantity</th>
                <th className="py-3 px-3">Reference Doc</th>
                <th className="py-3 px-4">Remarks</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {transactions.map((tx) => (
                <tr key={tx.transactionId} className="hover:bg-slate-50/70 transition-colors">
                  <td className="py-3 px-4 font-mono font-bold text-slate-500">#{tx.transactionId}</td>
                  <td className="py-3 px-3 text-slate-500 whitespace-nowrap">
                    {new Date(tx.transactionDate).toLocaleDateString()}
                  </td>
                  <td className="py-3 px-3 font-medium text-slate-800">{tx.materialName}</td>
                  <td className="py-3 px-3 text-slate-600">{tx.warehouseName}</td>
                  <td className="py-3 px-3">
                    <span className="px-2 py-0.5 rounded text-[11px] font-semibold bg-blue-50 text-blue-700">
                      {tx.transactionTypeName}
                    </span>
                  </td>
                  <td className="py-3 px-3 text-right font-mono font-bold text-slate-900">
                    {tx.quantity > 0 ? `+${tx.quantity}` : tx.quantity}
                  </td>
                  <td className="py-3 px-3 font-mono text-slate-600">{tx.referenceDocumentNumber}</td>
                  <td className="py-3 px-4 text-slate-500 text-[11px]">{tx.remarks}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* ADJUSTMENT MODAL */}
      {showAdjModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md p-5 space-y-4">
            <h3 className="text-sm font-bold text-slate-900">Record Physical Stock Adjustment</h3>
            <p className="text-xs text-slate-500">
              Adjust physical warehouse stock based on authorized cycle counting or scrap reconciliation.
            </p>

            <form onSubmit={handleAdjustmentSubmit} className="space-y-3 text-xs">
              <div>
                <label className="block text-slate-600 font-medium mb-1">Material</label>
                <select
                  value={adjMaterialId}
                  onChange={(e) => setAdjMaterialId(parseInt(e.target.value))}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                >
                  {balances.map((b) => (
                    <option key={b.materialId} value={b.materialId}>
                      {b.materialCode} - {b.materialName}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-slate-600 font-medium mb-1">Warehouse</label>
                <select
                  value={adjWarehouseId}
                  onChange={(e) => setAdjWarehouseId(parseInt(e.target.value))}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                >
                  {warehouses.map((w) => (
                    <option key={w.warehouseId} value={w.warehouseId}>
                      {w.warehouseName}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-slate-600 font-medium mb-1">Quantity Adjustment (+/-)</label>
                <input
                  type="number"
                  step="any"
                  value={adjQuantity}
                  onChange={(e) => setAdjQuantity(parseFloat(e.target.value) || 0)}
                  placeholder="e.g. 50 to add, -20 to write off"
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 font-mono font-bold"
                  required
                />
              </div>

              <div>
                <label className="block text-slate-600 font-medium mb-1">Audit Remarks (Mandatory)</label>
                <textarea
                  rows={2}
                  value={adjRemarks}
                  onChange={(e) => setAdjRemarks(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                  required
                />
              </div>

              <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowAdjModal(false)}
                  className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-medium text-slate-600"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold shadow-xs"
                >
                  Post Adjustment
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

