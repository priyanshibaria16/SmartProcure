import React, { useEffect, useState } from 'react';
import {
  Truck,
  Plus,
  Search,
  CheckCircle2,
  AlertTriangle,
  XCircle,
  Clock,
  ArrowRight,
  ShieldAlert
} from 'lucide-react';
import { goodsReceiptsApi, masterApi } from '../services/api';
import type { GoodsReceipt, PurchaseOrder, Warehouse, InspectionStatus } from '../types';
import { useAuth } from '../context/AuthContext';

const INSPECTION_BADGES: Record<string, { bg: string; text: string; icon: any }> = {
  Pending: { bg: 'bg-amber-100', text: 'text-amber-700', icon: Clock },
  Passed: { bg: 'bg-emerald-100', text: 'text-emerald-700', icon: CheckCircle2 },
  PartiallyAccepted: { bg: 'bg-sky-100', text: 'text-sky-800', icon: AlertTriangle },
  Rejected: { bg: 'bg-rose-100', text: 'text-rose-700', icon: XCircle },
};

export const GoodsReceiptPage: React.FC = () => {
  const { isWarehouse, isAdmin } = useAuth();

  const [activeTab, setActiveTab] = useState<'grns' | 'pending'>('grns');
  const [grns, setGrns] = useState<GoodsReceipt[]>([]);
  const [pendingPOs, setPendingPOs] = useState<PurchaseOrder[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  // Create GRN Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [selectedPO, setSelectedPO] = useState<PurchaseOrder | null>(null);
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<number>(1);
  const [deliveryChallan, setDeliveryChallan] = useState<string>('');
  const [inspectionStatus, setInspectionStatus] = useState<InspectionStatus>('Passed');
  const [remarks, setRemarks] = useState<string>('');
  const [grnItems, setGrnItems] = useState<
    {
      poItemId: number;
      materialId: number;
      materialName: string;
      orderedQty: number;
      previouslyReceivedQty: number;
      openQty: number;
      currentReceivedQty: number;
      acceptedQty: number;
      rejectedQty: number;
      rejectionReason: string;
    }[]
  >([]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [grnRes, pendingRes, whRes] = await Promise.all([
        goodsReceiptsApi.getAll({ page: 1, pageSize: 50 }),
        goodsReceiptsApi.getPendingPOs(),
        masterApi.getWarehouses(),
      ]);

      if (grnRes.data.success) setGrns(grnRes.data.data.items);
      if (pendingRes.data.success) setPendingPOs(pendingRes.data.data);
      if (whRes.data.success) setWarehouses(whRes.data.data);
    } catch (err) {
      console.error('Failed to load GRN data', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const initiateReceivingFromPO = (po: PurchaseOrder) => {
    setSelectedPO(po);
    setSelectedWarehouseId(po.warehouseId);
    setDeliveryChallan(`DC-${Math.floor(1000 + Math.random() * 9000)}`);
    setInspectionStatus('Passed');
    setRemarks('Materials inspected at dock. Packaging intact.');

    setGrnItems(
      po.items.map((i) => {
        const open = Math.max(0, i.quantity - i.receivedQuantity);
        return {
          poItemId: i.poItemId,
          materialId: i.materialId,
          materialName: i.materialName,
          orderedQty: i.quantity,
          previouslyReceivedQty: i.receivedQuantity,
          openQty: open,
          currentReceivedQty: open,
          acceptedQty: open,
          rejectedQty: 0,
          rejectionReason: '',
        };
      })
    );
    setShowCreateModal(true);
  };

  const handleItemQtyChange = (index: number, currentReceived: number, accepted: number, rejected: number) => {
    const updated = [...grnItems];
    updated[index].currentReceivedQty = currentReceived;
    updated[index].acceptedQty = accepted;
    updated[index].rejectedQty = rejected;
    setGrnItems(updated);
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPO) return;

    for (const item of grnItems) {
      if (item.currentReceivedQty <= 0) {
        alert('Current received quantity must be greater than zero');
        return;
      }
      if (item.currentReceivedQty > item.openQty) {
        alert(`Received quantity (${item.currentReceivedQty}) cannot exceed open PO balance (${item.openQty})`);
        return;
      }
      if (item.acceptedQty + item.rejectedQty !== item.currentReceivedQty) {
        alert('Accepted quantity + Rejected quantity must equal Current Received quantity');
        return;
      }
      if (item.rejectedQty > 0 && !item.rejectionReason.trim()) {
        alert('Rejection reason is required for rejected items');
        return;
      }
    }

    try {
      const res = await goodsReceiptsApi.create({
        poId: selectedPO.poId,
        warehouseId: selectedWarehouseId,
        deliveryChallanNumber: deliveryChallan,
        inspectionStatus,
        remarks,
        items: grnItems.map((i) => ({
          poItemId: i.poItemId,
          materialId: i.materialId,
          currentReceivedQuantity: i.currentReceivedQty,
          acceptedQuantity: i.acceptedQty,
          rejectedQuantity: i.rejectedQty,
          rejectionReason: i.rejectionReason,
        })),
      });

      if (res.data.success) {
        setShowCreateModal(false);
        setSelectedPO(null);
        loadData();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to record Goods Receipt');
    }
  };

  const filteredGrns = grns.filter((g) => {
    if (!searchTerm) return true;
    const s = searchTerm.toLowerCase();
    return g.grnNumber.toLowerCase().includes(s) || g.poNumber.toLowerCase().includes(s) || g.vendorName.toLowerCase().includes(s);
  });

  return (
    <div className="space-y-5">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Goods Receipt Note (GRN) & Dock Inspection</h1>
          <p className="text-xs text-slate-500">
            Physical material receiving, damage inspection and atomic inventory ledger update
          </p>
        </div>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-slate-200 gap-4 text-xs font-semibold">
        <button
          onClick={() => setActiveTab('grns')}
          className={`pb-2.5 flex items-center gap-1.5 transition-colors border-b-2 ${
            activeTab === 'grns'
              ? 'border-blue-600 text-blue-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          <Truck className="w-3.5 h-3.5" />
          <span>Goods Receipts History ({grns.length})</span>
        </button>
        <button
          onClick={() => setActiveTab('pending')}
          className={`pb-2.5 flex items-center gap-1.5 transition-colors border-b-2 ${
            activeTab === 'pending'
              ? 'border-blue-600 text-blue-600'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          }`}
        >
          <Clock className="w-3.5 h-3.5" />
          <span>Open POs Awaiting Delivery ({pendingPOs.length})</span>
        </button>
      </div>

      {/* TAB 1: GRN HISTORY */}
      {activeTab === 'grns' && (
        <div className="space-y-4">
          <div className="bg-white p-3 rounded-xl border border-slate-200/80 shadow-xs flex items-center justify-between">
            <div className="relative w-80">
              <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                placeholder="Search GRN #, PO #, vendor..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-8 pr-3 py-1.5 text-xs focus:outline-none"
              />
            </div>
          </div>

          <div className="bg-white rounded-xl border border-slate-200/80 shadow-xs overflow-hidden">
            <table className="w-full text-left text-xs">
              <thead>
                <tr className="bg-slate-50/80 border-b border-slate-200 text-slate-500 uppercase font-semibold text-[11px]">
                  <th className="py-3 px-4">GRN Number</th>
                  <th className="py-3 px-3">Receipt Date</th>
                  <th className="py-3 px-3">Purchase Order</th>
                  <th className="py-3 px-3">Vendor</th>
                  <th className="py-3 px-3">Warehouse</th>
                  <th className="py-3 px-3">Challan #</th>
                  <th className="py-3 px-3 text-center">Inspection</th>
                  <th className="py-3 px-4">Inspector Remarks</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <tr>
                    <td colSpan={8} className="py-12 text-center text-slate-400">Loading GRN Records...</td>
                  </tr>
                ) : filteredGrns.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="py-12 text-center text-slate-400">No goods receipts found.</td>
                  </tr>
                ) : (
                  filteredGrns.map((g) => {
                    const statusConfig = INSPECTION_BADGES[g.inspectionStatusName] || INSPECTION_BADGES.Passed;
                    const StatusIcon = statusConfig.icon;

                    return (
                      <tr key={g.grnId} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-3 px-4 font-bold text-blue-700 font-mono">{g.grnNumber}</td>
                        <td className="py-3 px-3 text-slate-500 whitespace-nowrap">
                          {new Date(g.receiptDate).toLocaleDateString()}
                        </td>
                        <td className="py-3 px-3 font-mono font-medium text-slate-800">{g.poNumber}</td>
                        <td className="py-3 px-3 font-medium text-slate-800">{g.vendorName}</td>
                        <td className="py-3 px-3 text-slate-600">{g.warehouseName}</td>
                        <td className="py-3 px-3 font-mono text-slate-500">{g.deliveryChallanNumber}</td>
                        <td className="py-3 px-3 text-center">
                          <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold ${statusConfig.bg} ${statusConfig.text}`}>
                            <StatusIcon className="w-3 h-3" />
                            {g.inspectionStatusName}
                          </span>
                        </td>
                        <td className="py-3 px-4 text-slate-500 text-[11px] max-w-xs truncate">{g.remarks}</td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 2: PENDING PURCHASE ORDERS AWAITING DELIVERY */}
      {activeTab === 'pending' && (
        <div className="bg-white rounded-xl border border-slate-200/80 shadow-xs overflow-hidden">
          <div className="p-4 border-b border-slate-100 flex items-center justify-between">
            <div>
              <h2 className="text-sm font-bold text-slate-800">Inbound Shipments Eligible for Receiving</h2>
              <p className="text-xs text-slate-500">Authorized POs with remaining balance pending arrival</p>
            </div>
          </div>
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="bg-slate-50/80 border-b border-slate-200 text-slate-500 uppercase font-semibold text-[11px]">
                <th className="py-3 px-4">PO Number</th>
                <th className="py-3 px-3">Date</th>
                <th className="py-3 px-3">Vendor</th>
                <th className="py-3 px-3">Warehouse</th>
                <th className="py-3 px-3">Items Status</th>
                <th className="py-3 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {pendingPOs.length === 0 ? (
                <tr>
                  <td colSpan={6} className="py-12 text-center text-slate-400">
                    No open purchase orders currently pending delivery.
                  </td>
                </tr>
              ) : (
                pendingPOs.map((po) => (
                  <tr key={po.poId} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4 font-bold text-blue-700 font-mono">{po.poNumber}</td>
                    <td className="py-3 px-3 text-slate-500 whitespace-nowrap">
                      {new Date(po.poDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-3 font-medium text-slate-800">{po.vendorName}</td>
                    <td className="py-3 px-3 text-slate-600">{po.warehouseName}</td>
                    <td className="py-3 px-3 text-slate-600">
                      {po.items.map((i) => (
                        <div key={i.poItemId} className="text-[11px]">
                          {i.materialName}: Received {i.receivedQuantity} of {i.quantity}
                        </div>
                      ))}
                    </td>
                    <td className="py-3 px-4 text-right">
                      {(isWarehouse || isAdmin) && (
                        <button
                          onClick={() => initiateReceivingFromPO(po)}
                          className="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold shadow-xs flex items-center gap-1.5 ml-auto"
                        >
                          <Truck className="w-3.5 h-3.5" />
                          <span>Receive Goods</span>
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* RECORD GOODS RECEIPT MODAL */}
      {showCreateModal && selectedPO && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl max-h-[90vh] overflow-y-auto p-6 space-y-5 animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <h2 className="text-base font-bold text-slate-900">Record Goods Receipt Note (GRN)</h2>
                <p className="text-xs text-slate-500">
                  Receiving against <strong>{selectedPO.poNumber}</strong> from <strong>{selectedPO.vendorName}</strong>
                </p>
              </div>
              <button onClick={() => setShowCreateModal(false)} className="text-slate-400 hover:text-slate-600 p-1">
                ✕
              </button>
            </div>

            <form onSubmit={handleCreateSubmit} className="space-y-4 text-xs">
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Receiving Warehouse</label>
                  <select
                    value={selectedWarehouseId}
                    onChange={(e) => setSelectedWarehouseId(parseInt(e.target.value))}
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
                  <label className="block text-slate-600 font-medium mb-1">Delivery Challan / Invoice #</label>
                  <input
                    type="text"
                    value={deliveryChallan}
                    onChange={(e) => setDeliveryChallan(e.target.value)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                    required
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Inspection Status</label>
                  <select
                    value={inspectionStatus}
                    onChange={(e) => setInspectionStatus(e.target.value as InspectionStatus)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 font-semibold"
                  >
                    <option value="Passed">Passed (100% Quality)</option>
                    <option value="PartiallyAccepted">Partially Accepted</option>
                    <option value="Rejected">Rejected</option>
                  </select>
                </div>
              </div>

              {/* Items Inspection & Quantities Grid */}
              <div className="border border-slate-200 rounded-xl p-3 bg-slate-50/50 space-y-3">
                <span className="font-bold text-slate-800 uppercase tracking-wider text-[11px] block">
                  Dock Inspection & Quantities
                </span>

                {grnItems.map((item, index) => (
                  <div key={item.poItemId} className="bg-white p-3 rounded-lg border border-slate-200 space-y-2">
                    <div className="flex justify-between items-center text-xs font-semibold text-slate-900 border-b border-slate-100 pb-1.5">
                      <span>{item.materialName}</span>
                      <span className="text-slate-400 font-mono text-[11px]">
                        Ordered: {item.orderedQty} | Prev Received: {item.previouslyReceivedQty} | Open: {item.openQty}
                      </span>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
                      <div>
                        <label className="text-[10px] text-slate-500 block mb-0.5">Current Received Qty</label>
                        <input
                          type="number"
                          step="any"
                          max={item.openQty}
                          min="0"
                          value={item.currentReceivedQty}
                          onChange={(e) => {
                            const val = parseFloat(e.target.value) || 0;
                            handleItemQtyChange(index, val, val, 0);
                          }}
                          className="w-full bg-slate-50 border border-slate-200 rounded p-1.5 text-xs font-semibold"
                          required
                        />
                      </div>
                      <div>
                        <label className="text-[10px] text-emerald-600 font-semibold block mb-0.5">
                          Accepted Qty (Added to Stock)
                        </label>
                        <input
                          type="number"
                          step="any"
                          min="0"
                          max={item.currentReceivedQty}
                          value={item.acceptedQty}
                          onChange={(e) => {
                            const accepted = parseFloat(e.target.value) || 0;
                            const rejected = Math.max(0, item.currentReceivedQty - accepted);
                            handleItemQtyChange(index, item.currentReceivedQty, accepted, rejected);
                          }}
                          className="w-full bg-emerald-50 border border-emerald-300 rounded p-1.5 text-xs font-bold text-emerald-800"
                          required
                        />
                      </div>
                      <div>
                        <label className="text-[10px] text-rose-600 font-semibold block mb-0.5">
                          Rejected Qty (Excluded from Stock)
                        </label>
                        <input
                          type="number"
                          step="any"
                          min="0"
                          max={item.currentReceivedQty}
                          value={item.rejectedQty}
                          onChange={(e) => {
                            const rejected = parseFloat(e.target.value) || 0;
                            const accepted = Math.max(0, item.currentReceivedQty - rejected);
                            handleItemQtyChange(index, item.currentReceivedQty, accepted, rejected);
                          }}
                          className="w-full bg-rose-50 border border-rose-300 rounded p-1.5 text-xs font-bold text-rose-800"
                        />
                      </div>
                    </div>

                    {item.rejectedQty > 0 && (
                      <div>
                        <label className="text-[10px] text-rose-700 font-semibold block mb-0.5">
                          Rejection Reason (Mandatory)
                        </label>
                        <input
                          type="text"
                          placeholder="e.g. Broken packaging, surface scratch, dented coils, out of tolerance"
                          value={item.rejectionReason}
                          onChange={(e) => {
                            const updated = [...grnItems];
                            updated[index].rejectionReason = e.target.value;
                            setGrnItems(updated);
                          }}
                          className="w-full bg-rose-50 border border-rose-300 rounded p-1.5 text-xs text-rose-900"
                          required
                        />
                      </div>
                    )}
                  </div>
                ))}
              </div>

              <div>
                <label className="block text-slate-600 font-medium mb-1">Inspector Remarks / Condition Notes</label>
                <textarea
                  rows={2}
                  value={remarks}
                  onChange={(e) => setRemarks(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 text-xs"
                />
              </div>

              <div className="flex justify-end gap-2 pt-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 border border-slate-200 rounded-lg font-medium text-slate-600 hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold shadow-xs"
                >
                  Post Goods Receipt (GRN)
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

