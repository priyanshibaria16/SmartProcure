import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import {
  ShoppingCart,
  Plus,
  Search,
  Download,
  Send,
  XCircle,
  Truck,
  Eye,
  CheckCircle2,
  Trash2
} from 'lucide-react';
import { purchaseOrdersApi, requisitionsApi, masterApi } from '../services/api';
import type { PurchaseOrder, Requisition, Vendor, Warehouse, Material } from '../types';
import { useAuth } from '../context/AuthContext';

const STATUS_BADGES: Record<string, { bg: string; text: string }> = {
  Draft: { bg: 'bg-slate-100', text: 'text-slate-700' },
  PendingApproval: { bg: 'bg-amber-100', text: 'text-amber-700' },
  Approved: { bg: 'bg-emerald-100', text: 'text-emerald-700' },
  SentToVendor: { bg: 'bg-blue-100', text: 'text-blue-700' },
  PartiallyReceived: { bg: 'bg-sky-100', text: 'text-sky-800 font-semibold' },
  FullyReceived: { bg: 'bg-teal-100', text: 'text-teal-800 font-semibold' },
  Closed: { bg: 'bg-slate-200', text: 'text-slate-600' },
  Cancelled: { bg: 'bg-rose-100', text: 'text-rose-700' },
};

export const PurchaseOrdersPage: React.FC = () => {
  const { isPurchasing, isAdmin } = useAuth();
  const [searchParams] = useSearchParams();
  const convertPrId = searchParams.get('convertPrId');

  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [materials, setMaterials] = useState<Material[]>([]);
  const [approvedPRs, setApprovedPRs] = useState<Requisition[]>([]);
  const [loading, setLoading] = useState(true);

  // Filters
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [vendorFilter, setVendorFilter] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Create Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [selectedPrId, setSelectedPrId] = useState<number | undefined>(
    convertPrId ? parseInt(convertPrId) : undefined
  );
  const [selectedVendorId, setSelectedVendorId] = useState<number>(1);
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<number>(1);
  const [deliveryDate, setDeliveryDate] = useState<string>(
    new Date(Date.now() + 10 * 86400000).toISOString().split('T')[0]
  );
  const [paymentTerms, setPaymentTerms] = useState<string>('Net 30');
  const [discountAmount, setDiscountAmount] = useState<number>(0);
  const [orderItems, setOrderItems] = useState<
    { materialId: number; quantity: number; unitPrice: number; taxRate: number }[]
  >([{ materialId: 1, quantity: 100, unitPrice: 68.50, taxRate: 18 }]);

  // Cancellation Modal
  const [cancelModal, setCancelModal] = useState<{ show: boolean; poId: number; reason: string }>({
    show: false,
    poId: 0,
    reason: '',
  });

  const loadData = async () => {
    setLoading(true);
    try {
      const [posRes, vendorsRes, whRes, matsRes, prsRes] = await Promise.all([
        purchaseOrdersApi.getAll({
          status: statusFilter || undefined,
          vendorId: vendorFilter ? parseInt(vendorFilter) : undefined,
          page: 1,
          pageSize: 50,
        }),
        masterApi.getVendors({ status: true, page: 1, pageSize: 100 }),
        masterApi.getWarehouses(),
        masterApi.getMaterials({ page: 1, pageSize: 100 }),
        requisitionsApi.getAll({ status: 'Approved', page: 1, pageSize: 50 }),
      ]);

      if (posRes.data.success) setOrders(posRes.data.data.items);
      if (vendorsRes.data.success) setVendors(vendorsRes.data.data.items);
      if (whRes.data.success) setWarehouses(whRes.data.data);
      if (matsRes.data.success) setMaterials(matsRes.data.data.items);
      if (prsRes.data.success) setApprovedPRs(prsRes.data.data.items);
    } catch (err) {
      console.error('Failed to load purchase orders', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [statusFilter, vendorFilter]);

  useEffect(() => {
    if (convertPrId) {
      const prIdNum = parseInt(convertPrId);
      setSelectedPrId(prIdNum);
      setShowCreateModal(true);

      // Pre-fill items from PR if available
      requisitionsApi.getById(prIdNum).then((res) => {
        if (res.data.success && res.data.data) {
          const pr = res.data.data;
          if (pr.items.length > 0) {
            setOrderItems(
              pr.items.map((i) => ({
                materialId: i.materialId,
                quantity: i.quantity,
                unitPrice: i.estimatedUnitPrice,
                taxRate: 18,
              }))
            );
          }
        }
      });
    }
  }, [convertPrId]);

  const handlePrSelect = async (prId: number | undefined) => {
    setSelectedPrId(prId);
    if (prId) {
      try {
        const res = await requisitionsApi.getById(prId);
        if (res.data.success && res.data.data) {
          const pr = res.data.data;
          setOrderItems(
            pr.items.map((i) => ({
              materialId: i.materialId,
              quantity: i.quantity,
              unitPrice: i.estimatedUnitPrice,
              taxRate: 18,
            }))
          );
        }
      } catch (err) {
        console.error('Failed to fetch PR details', err);
      }
    }
  };

  const handleAddItem = () => {
    const firstMat = materials[0];
    setOrderItems([
      ...orderItems,
      {
        materialId: firstMat ? firstMat.materialId : 1,
        quantity: 50,
        unitPrice: firstMat ? firstMat.unitPrice : 100,
        taxRate: 18,
      },
    ]);
  };

  const handleRemoveItem = (index: number) => {
    if (orderItems.length > 1) {
      setOrderItems(orderItems.filter((_, i) => i !== index));
    }
  };

  const handleMaterialChange = (index: number, matId: number) => {
    const mat = materials.find((m) => m.materialId === matId);
    const updated = [...orderItems];
    updated[index].materialId = matId;
    if (mat) {
      updated[index].unitPrice = mat.unitPrice;
    }
    setOrderItems(updated);
  };

  const calculateSubtotal = () => orderItems.reduce((acc, i) => acc + i.quantity * i.unitPrice, 0);
  const calculateTax = () => orderItems.reduce((acc, i) => acc + i.quantity * i.unitPrice * (i.taxRate / 100), 0);
  const calculateGrandTotal = () => Math.max(0, calculateSubtotal() + calculateTax() - discountAmount);

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await purchaseOrdersApi.create({
        requisitionId: selectedPrId,
        vendorId: selectedVendorId,
        expectedDeliveryDate: deliveryDate,
        paymentTerms,
        warehouseId: selectedWarehouseId,
        discountAmount,
        items: orderItems,
      });

      if (res.data.success) {
        setShowCreateModal(false);
        loadData();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to create purchase order');
    }
  };

  const handleSendToVendor = async (poId: number) => {
    try {
      await purchaseOrdersApi.send(poId);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to send PO');
    }
  };

  const handleExecuteCancel = async () => {
    if (!cancelModal.reason.trim()) {
      alert('Cancellation reason is required');
      return;
    }

    try {
      await purchaseOrdersApi.cancel(cancelModal.poId, cancelModal.reason);
      setCancelModal({ show: false, poId: 0, reason: '' });
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to cancel PO');
    }
  };

  const handleDownloadPdf = async (poId: number, poNumber: string) => {
    try {
      const res = await purchaseOrdersApi.downloadPdf(poId);
      const blob = new Blob([res.data], { type: 'text/html' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${poNumber}.html`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Failed to download PO document', err);
      alert('Could not download PO document');
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
    }).format(val);
  };

  const filteredOrders = orders.filter((o) => {
    if (!searchTerm) return true;
    const s = searchTerm.toLowerCase();
    return o.poNumber.toLowerCase().includes(s) || o.vendorName.toLowerCase().includes(s);
  });

  return (
    <div className="space-y-5">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Purchase Orders (PO)</h1>
          <p className="text-xs text-slate-500">
            Official commercial procurement commitments dispatched to qualified suppliers
          </p>
        </div>
        {(isPurchasing || isAdmin) && (
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold shadow-xs transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span>Generate Purchase Order</span>
          </button>
        )}
      </div>

      {/* Filter Toolbar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-xs flex flex-wrap items-center gap-3">
        <div className="relative flex-1 min-w-[200px]">
          <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search by PO #, vendor company..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-8 pr-3 py-1.5 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
          />
        </div>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 focus:outline-none"
        >
          <option value="">All Statuses</option>
          <option value="Approved">Approved</option>
          <option value="SentToVendor">Sent to Vendor</option>
          <option value="PartiallyReceived">Partially Received</option>
          <option value="FullyReceived">Fully Received</option>
          <option value="Closed">Closed</option>
          <option value="Cancelled">Cancelled</option>
        </select>

        <select
          value={vendorFilter}
          onChange={(e) => setVendorFilter(e.target.value)}
          className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 focus:outline-none"
        >
          <option value="">All Vendors</option>
          {vendors.map((v) => (
            <option key={v.vendorId} value={v.vendorId}>
              {v.companyName}
            </option>
          ))}
        </select>
      </div>

      {/* Purchase Orders Table */}
      <div className="bg-white rounded-xl border border-slate-200/80 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="bg-slate-50/80 border-b border-slate-200 text-slate-500 uppercase font-semibold text-[11px]">
                <th className="py-3 px-4">PO Number</th>
                <th className="py-3 px-3">Date</th>
                <th className="py-3 px-3">Vendor</th>
                <th className="py-3 px-3">Warehouse</th>
                <th className="py-3 px-3">Expected Delivery</th>
                <th className="py-3 px-3 text-right">Subtotal</th>
                <th className="py-3 px-3 text-right">Grand Total (GST)</th>
                <th className="py-3 px-3 text-center">Status</th>
                <th className="py-3 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loading ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    Loading Purchase Orders...
                  </td>
                </tr>
              ) : filteredOrders.length === 0 ? (
                <tr>
                  <td colSpan={9} className="py-12 text-center text-slate-400">
                    No purchase orders found.
                  </td>
                </tr>
              ) : (
                filteredOrders.map((po) => (
                  <tr key={po.poId} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4 font-bold text-blue-700 font-mono">
                      {po.poNumber}
                    </td>
                    <td className="py-3 px-3 text-slate-500 whitespace-nowrap">
                      {new Date(po.poDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-3 font-medium text-slate-800">{po.vendorName}</td>
                    <td className="py-3 px-3 text-slate-600">{po.warehouseName}</td>
                    <td className="py-3 px-3 text-slate-600 whitespace-nowrap">
                      {new Date(po.expectedDeliveryDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-3 text-right text-slate-600 font-mono">
                      {formatCurrency(po.subtotal)}
                    </td>
                    <td className="py-3 px-3 text-right font-bold text-slate-900 font-mono">
                      {formatCurrency(po.grandTotal)}
                    </td>
                    <td className="py-3 px-3 text-center">
                      <span
                        className={`inline-block px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${
                          STATUS_BADGES[po.statusName]?.bg || 'bg-slate-100'
                        } ${STATUS_BADGES[po.statusName]?.text || 'text-slate-700'}`}
                      >
                        {po.statusName}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-right whitespace-nowrap space-x-1">
                      {/* Send to Vendor */}
                      {po.statusName === 'Approved' && (isPurchasing || isAdmin) && (
                        <button
                          onClick={() => handleSendToVendor(po.poId)}
                          className="px-2 py-1 bg-blue-50 text-blue-700 hover:bg-blue-100 rounded text-[11px] font-semibold"
                          title="Mark Sent to Vendor"
                        >
                          Send
                        </button>
                      )}

                      {/* Download PDF Document */}
                      <button
                        onClick={() => handleDownloadPdf(po.poId, po.poNumber)}
                        className="px-2 py-1 bg-slate-100 text-slate-700 hover:bg-slate-200 rounded text-[11px] font-medium inline-flex items-center gap-1"
                        title="Download official PO document"
                      >
                        <Download className="w-3 h-3" />
                        <span>PDF</span>
                      </button>

                      {/* Cancel PO */}
                      {(po.statusName === 'Approved' || po.statusName === 'SentToVendor') && (isPurchasing || isAdmin) && (
                        <button
                          onClick={() => setCancelModal({ show: true, poId: po.poId, reason: '' })}
                          className="px-2 py-1 text-rose-600 hover:bg-rose-50 rounded text-[11px]"
                          title="Cancel Order"
                        >
                          Cancel
                        </button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* CREATE PURCHASE ORDER MODAL */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl max-h-[90vh] overflow-y-auto p-6 space-y-5 animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <h2 className="text-base font-bold text-slate-900">Issue Purchase Order</h2>
                <p className="text-xs text-slate-500">Commercial contract commitment to supplier</p>
              </div>
              <button onClick={() => setShowCreateModal(false)} className="text-slate-400 hover:text-slate-600 p-1">
                ✕
              </button>
            </div>

            <form onSubmit={handleCreateSubmit} className="space-y-4 text-xs">
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Convert Approved PR (Optional)</label>
                  <select
                    value={selectedPrId || ''}
                    onChange={(e) => handlePrSelect(e.target.value ? parseInt(e.target.value) : undefined)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 font-mono"
                  >
                    <option value="">Direct PO (No PR Reference)</option>
                    {approvedPRs.map((pr) => (
                      <option key={pr.requisitionId} value={pr.requisitionId}>
                        {pr.requisitionNumber} - {pr.departmentName} ({formatCurrency(pr.totalEstimatedAmount)})
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Vendor</label>
                  <select
                    value={selectedVendorId}
                    onChange={(e) => setSelectedVendorId(parseInt(e.target.value))}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                    required
                  >
                    {vendors.map((v) => (
                      <option key={v.vendorId} value={v.vendorId}>
                        {v.companyName} ({v.city})
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Delivery Destination Warehouse</label>
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
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Expected Delivery Date</label>
                  <input
                    type="date"
                    value={deliveryDate}
                    onChange={(e) => setDeliveryDate(e.target.value)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                    required
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Payment Terms</label>
                  <input
                    type="text"
                    value={paymentTerms}
                    onChange={(e) => setPaymentTerms(e.target.value)}
                    placeholder="e.g. Net 30, Net 45"
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                  />
                </div>
              </div>

              {/* Items Line Grid */}
              <div className="border border-slate-200 rounded-xl p-3 bg-slate-50/50">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-bold text-slate-800 uppercase tracking-wider text-[11px]">PO Line Items</span>
                  <button
                    type="button"
                    onClick={handleAddItem}
                    className="text-blue-600 font-semibold hover:text-blue-700 flex items-center gap-1 text-[11px]"
                  >
                    + Add Item
                  </button>
                </div>

                <div className="space-y-2">
                  {orderItems.map((item, index) => (
                    <div key={index} className="grid grid-cols-12 gap-2 bg-white p-2.5 rounded-lg border border-slate-200 items-center">
                      <div className="col-span-5">
                        <label className="text-[10px] text-slate-400 block mb-0.5">Material</label>
                        <select
                          value={item.materialId}
                          onChange={(e) => handleMaterialChange(index, parseInt(e.target.value))}
                          className="w-full bg-slate-50 border border-slate-200 rounded p-1 text-xs"
                        >
                          {materials.map((m) => (
                            <option key={m.materialId} value={m.materialId}>
                              {m.materialCode} - {m.materialName} ({m.baseUOMCode})
                            </option>
                          ))}
                        </select>
                      </div>

                      <div className="col-span-2">
                        <label className="text-[10px] text-slate-400 block mb-0.5">Quantity</label>
                        <input
                          type="number"
                          step="any"
                          min="0.001"
                          value={item.quantity}
                          onChange={(e) => {
                            const updated = [...orderItems];
                            updated[index].quantity = parseFloat(e.target.value) || 0;
                            setOrderItems(updated);
                          }}
                          className="w-full bg-slate-50 border border-slate-200 rounded p-1 text-xs text-right"
                          required
                        />
                      </div>

                      <div className="col-span-2">
                        <label className="text-[10px] text-slate-400 block mb-0.5">Unit Price (₹)</label>
                        <input
                          type="number"
                          step="any"
                          value={item.unitPrice}
                          onChange={(e) => {
                            const updated = [...orderItems];
                            updated[index].unitPrice = parseFloat(e.target.value) || 0;
                            setOrderItems(updated);
                          }}
                          className="w-full bg-slate-50 border border-slate-200 rounded p-1 text-xs text-right"
                          required
                        />
                      </div>

                      <div className="col-span-2 text-right">
                        <label className="text-[10px] text-slate-400 block mb-0.5">Total (+18% GST)</label>
                        <div className="font-bold text-slate-800 text-xs py-1">
                          ₹{(item.quantity * item.unitPrice * 1.18).toFixed(2)}
                        </div>
                      </div>

                      <div className="col-span-1 text-right">
                        <button
                          type="button"
                          onClick={() => handleRemoveItem(index)}
                          className="text-slate-400 hover:text-rose-600 mt-3"
                          disabled={orderItems.length <= 1}
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="mt-3 pt-2 border-t border-slate-200 space-y-1 text-right">
                  <div className="flex justify-between text-slate-500">
                    <span>Subtotal:</span>
                    <span className="font-mono">{formatCurrency(calculateSubtotal())}</span>
                  </div>
                  <div className="flex justify-between text-slate-500">
                    <span>GST (Tax):</span>
                    <span className="font-mono">{formatCurrency(calculateTax())}</span>
                  </div>
                  <div className="flex justify-between text-slate-900 font-bold text-sm pt-1 border-t border-slate-100">
                    <span>Grand Total:</span>
                    <span className="text-blue-700">{formatCurrency(calculateGrandTotal())}</span>
                  </div>
                </div>
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
                  Issue Purchase Order
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* CANCELLATION MODAL */}
      {cancelModal.show && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md p-5 space-y-4">
            <h3 className="text-sm font-bold text-slate-900">Cancel Purchase Order</h3>
            <p className="text-xs text-slate-500">
              Cancelled purchase orders can no longer receive dock shipments. Please provide an audit reason.
            </p>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">Cancellation Reason</label>
              <textarea
                rows={3}
                value={cancelModal.reason}
                onChange={(e) => setCancelModal({ ...cancelModal, reason: e.target.value })}
                placeholder="e.g. Supplier notified inability to fulfill agreed delivery schedule"
                className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 text-xs"
              />
            </div>

            <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setCancelModal({ show: false, poId: 0, reason: '' })}
                className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-medium text-slate-600"
              >
                Close
              </button>
              <button
                type="button"
                onClick={handleExecuteCancel}
                className="px-3 py-1.5 bg-rose-600 hover:bg-rose-700 text-white rounded-lg text-xs font-semibold shadow-xs"
              >
                Confirm Cancellation
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

