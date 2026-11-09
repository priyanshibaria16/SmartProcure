import React, { useState, useEffect } from 'react';
import {
  Receipt,
  Search,
  Filter,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Plus,
  RefreshCw,
  FileCheck,
  Eye,
  CreditCard,
  ArrowRight
} from 'lucide-react';
import { invoicesApi, purchaseOrdersApi, masterApi } from '../services/api';
import type { SupplierInvoice, PurchaseOrder, Vendor } from '../types';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export const InvoicesPage: React.FC = () => {
  const { isFinance, isAdmin } = useAuth();
  const navigate = useNavigate();

  const [invoices, setInvoices] = useState<SupplierInvoice[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('All');
  const [selectedInvoice, setSelectedInvoice] = useState<SupplierInvoice | null>(null);

  // Modals
  const [showCreateModal, setShowCreateModal] = useState<boolean>(false);
  const [showDetailModal, setShowDetailModal] = useState<boolean>(false);
  const [showVerifyModal, setShowVerifyModal] = useState<boolean>(false);
  const [showRejectModal, setShowRejectModal] = useState<boolean>(false);
  const [actionRemarks, setActionRemarks] = useState<string>('');
  const [actionLoading, setActionLoading] = useState<boolean>(false);

  // Form State for new invoice
  const [approvedPOs, setApprovedPOs] = useState<PurchaseOrder[]>([]);
  const [selectedPO, setSelectedPO] = useState<PurchaseOrder | null>(null);
  const [invoiceNumber, setInvoiceNumber] = useState<string>('');
  const [invoiceDate, setInvoiceDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [dueDate, setDueDate] = useState<string>(
    new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  );
  const [invoiceItems, setInvoiceItems] = useState<
    { materialId: number; materialName: string; billedQuantity: number; billedUnitPrice: number; lineTotal: number }[]
  >([]);

  useEffect(() => {
    fetchInvoices();
    fetchVendors();
  }, [statusFilter]);

  const fetchInvoices = async () => {
    try {
      setLoading(true);
      const params: any = { pageSize: 50 };
      if (statusFilter !== 'All') params.status = statusFilter;
      const res = await invoicesApi.getAll(params);
      if (res.data?.success) {
        setInvoices(res.data.data.items);
      }
    } catch (err) {
      console.error('Failed to fetch invoices:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchVendors = async () => {
    try {
      const res = await masterApi.getVendors({ pageSize: 100 });
      if (res.data?.success) {
        setVendors(res.data.data.items);
      }
    } catch (err) {
      console.error('Failed to fetch vendors:', err);
    }
  };

  const handleOpenCreateModal = async () => {
    try {
      const res = await purchaseOrdersApi.getAll({ pageSize: 50 });
      if (res.data?.success) {
        // Filter POs that are either SentToVendor, PartiallyReceived, or FullyReceived
        const eligible = res.data.data.items.filter(
          (p) =>
            p.status === 'SentToVendor' ||
            p.status === 'PartiallyReceived' ||
            p.status === 'FullyReceived'
        );
        setApprovedPOs(eligible);
        setShowCreateModal(true);
      }
    } catch (err) {
      console.error('Failed to load POs:', err);
    }
  };

  const handleSelectPO = (poIdStr: string) => {
    const poId = parseInt(poIdStr);
    const po = approvedPOs.find((p) => p.poId === poId) || null;
    setSelectedPO(po);
    if (po) {
      setInvoiceItems(
        po.items.map((item) => ({
          materialId: item.materialId,
          materialName: item.materialName,
          billedQuantity: item.receivedQuantity > 0 ? item.receivedQuantity : item.quantity,
          billedUnitPrice: item.unitPrice,
          lineTotal: (item.receivedQuantity > 0 ? item.receivedQuantity : item.quantity) * item.unitPrice
        }))
      );
    } else {
      setInvoiceItems([]);
    }
  };

  const handleItemChange = (index: number, field: 'billedQuantity' | 'billedUnitPrice', val: number) => {
    setInvoiceItems((prev) => {
      const updated = [...prev];
      updated[index] = {
        ...updated[index],
        [field]: val,
        lineTotal:
          field === 'billedQuantity'
            ? val * updated[index].billedUnitPrice
            : val * updated[index].billedQuantity
      };
      return updated;
    });
  };

  const handleCreateInvoice = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPO || !invoiceNumber.trim() || invoiceItems.length === 0) {
      alert('Please select a PO and provide a vendor invoice number.');
      return;
    }

    try {
      setActionLoading(true);
      const subtotal = invoiceItems.reduce((acc, i) => acc + i.lineTotal, 0);
      const taxAmount = subtotal * 0.18; // 18% standard GST
      const totalAmount = subtotal + taxAmount;

      const payload = {
        invoiceNumber: invoiceNumber.trim(),
        vendorId: selectedPO.vendorId,
        poId: selectedPO.poId,
        invoiceDate,
        dueDate,
        subtotal,
        taxAmount,
        totalAmount,
        items: invoiceItems.map((i) => ({
          materialId: i.materialId,
          billedQuantity: i.billedQuantity,
          billedUnitPrice: i.billedUnitPrice,
          lineTotal: i.lineTotal
        }))
      };

      const res = await invoicesApi.create(payload);
      if (res.data?.success) {
        setShowCreateModal(false);
        resetCreateForm();
        fetchInvoices();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to create invoice.');
    } finally {
      setActionLoading(false);
    }
  };

  const resetCreateForm = () => {
    setSelectedPO(null);
    setInvoiceNumber('');
    setInvoiceItems([]);
  };

  const handleTriggerVerification = async (invoiceId: number) => {
    try {
      setActionLoading(true);
      const res = await invoicesApi.verify(invoiceId);
      if (res.data?.success) {
        alert(res.data.message || '3-Way Matching completed.');
        fetchInvoices();
        if (selectedInvoice && selectedInvoice.invoiceId === invoiceId) {
          const updated = await invoicesApi.getById(invoiceId);
          if (updated.data?.success) setSelectedInvoice(updated.data.data);
        }
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Verification check failed.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleApproveInvoice = async (invoiceId: number) => {
    try {
      setActionLoading(true);
      const res = await invoicesApi.approve(invoiceId, actionRemarks || 'Approved after successful 3-way match');
      if (res.data?.success) {
        alert('Invoice approved for payment settlement.');
        setShowDetailModal(false);
        fetchInvoices();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to approve invoice.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRejectInvoice = async () => {
    if (!selectedInvoice) return;
    if (!actionRemarks.trim()) {
      alert('Rejection remarks are mandatory.');
      return;
    }

    try {
      setActionLoading(true);
      const res = await invoicesApi.reject(selectedInvoice.invoiceId, actionRemarks);
      if (res.data?.success) {
        alert('Invoice rejected.');
        setShowRejectModal(false);
        setShowDetailModal(false);
        setActionRemarks('');
        fetchInvoices();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to reject invoice.');
    } finally {
      setActionLoading(false);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 2
    }).format(val);
  };

  const filteredInvoices = invoices.filter(
    (inv) =>
      inv.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      inv.vendorName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      inv.poNumber.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'PendingVerification':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-50 text-amber-700 border border-amber-200">
            <AlertTriangle className="w-3 h-3 text-amber-500" /> Pending Match
          </span>
        );
      case 'Matched':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700 border border-blue-200">
            <CheckCircle className="w-3 h-3 text-blue-500" /> 3-Way Matched
          </span>
        );
      case 'Mismatched':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-rose-50 text-rose-700 border border-rose-200">
            <XCircle className="w-3 h-3 text-rose-500" /> Mismatched
          </span>
        );
      case 'Approved':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700 border border-emerald-200">
            <CheckCircle className="w-3 h-3 text-emerald-600" /> Approved
          </span>
        );
      case 'Rejected':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-50 text-red-700 border border-red-200">
            <XCircle className="w-3 h-3 text-red-600" /> Rejected
          </span>
        );
      case 'PartiallyPaid':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-50 text-indigo-700 border border-indigo-200">
            <CreditCard className="w-3 h-3 text-indigo-500" /> Partial Paid
          </span>
        );
      case 'Paid':
        return (
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-teal-50 text-teal-700 border border-teal-200">
            <CheckCircle className="w-3 h-3 text-teal-600" /> Fully Paid
          </span>
        );
      default:
        return (
          <span className="px-2.5 py-0.5 rounded-full text-xs font-medium bg-slate-100 text-slate-700">
            {status}
          </span>
        );
    }
  };

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-6">
      {/* Top Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-5">
        <div>
          <div className="flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-lg bg-indigo-600 flex items-center justify-center text-white shadow-md shadow-indigo-500/20">
              <Receipt className="w-5 h-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Supplier Invoices & 3-Way Match</h1>
              <p className="text-xs text-slate-500">
                SAP MM MIRO Equivalent: Purchase Order vs. Goods Receipt vs. Vendor Billed Invoice
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2.5">
          <button
            onClick={fetchInvoices}
            className="flex items-center gap-1.5 px-3 py-2 border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium rounded-lg shadow-2xs transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
            <span>Refresh</span>
          </button>

          {(isFinance || isAdmin) && (
            <button
              onClick={handleOpenCreateModal}
              className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-indigo-600/30 transition-all"
            >
              <Plus className="w-4 h-4" />
              <span>Record Supplier Invoice</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Overview Pills */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-slate-500 font-medium">Total Invoices</div>
          <div className="text-2xl font-bold text-slate-900 mt-1">{invoices.length}</div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-amber-600 font-medium">Pending Verification</div>
          <div className="text-2xl font-bold text-amber-600 mt-1">
            {invoices.filter((i) => i.status === 'PendingVerification').length}
          </div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-rose-600 font-medium">Mismatched Invoices</div>
          <div className="text-2xl font-bold text-rose-600 mt-1">
            {invoices.filter((i) => i.status === 'Mismatched').length}
          </div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-emerald-600 font-medium">Approved / Ready to Pay</div>
          <div className="text-2xl font-bold text-emerald-600 mt-1">
            {invoices.filter((i) => i.status === 'Approved' || i.status === 'PartiallyPaid').length}
          </div>
        </div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-2xs flex flex-col md:flex-row gap-4 items-center justify-between">
        <div className="relative w-full md:w-96">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search by invoice #, vendor, or PO #..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all"
          />
        </div>

        <div className="flex items-center gap-3 w-full md:w-auto">
          <div className="flex items-center gap-2">
            <Filter className="w-3.5 h-3.5 text-slate-400" />
            <span className="text-xs text-slate-600 font-medium">Status:</span>
          </div>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 font-medium focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500"
          >
            <option value="All">All Statuses</option>
            <option value="PendingVerification">Pending Verification</option>
            <option value="Matched">Matched</option>
            <option value="Mismatched">Mismatched</option>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="PartiallyPaid">Partially Paid</option>
            <option value="Paid">Fully Paid</option>
          </select>
        </div>
      </div>

      {/* Invoices Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
              <tr>
                <th className="px-5 py-3">Invoice Number</th>
                <th className="px-5 py-3">Vendor</th>
                <th className="px-5 py-3">PO Reference</th>
                <th className="px-5 py-3">Invoice Date</th>
                <th className="px-5 py-3">Due Date</th>
                <th className="px-5 py-3 text-right">Total Amount</th>
                <th className="px-5 py-3 text-right">Outstanding</th>
                <th className="px-5 py-3">Status</th>
                <th className="px-5 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 text-slate-700">
              {loading ? (
                <tr>
                  <td colSpan={9} className="px-5 py-10 text-center text-slate-400">
                    <RefreshCw className="w-5 h-5 animate-spin mx-auto mb-2 text-indigo-500" />
                    Loading supplier invoices...
                  </td>
                </tr>
              ) : filteredInvoices.length === 0 ? (
                <tr>
                  <td colSpan={9} className="px-5 py-12 text-center text-slate-400">
                    No supplier invoices found matching current filters.
                  </td>
                </tr>
              ) : (
                filteredInvoices.map((inv) => (
                  <tr key={inv.invoiceId} className="hover:bg-slate-50/70 transition-colors">
                    <td className="px-5 py-3.5 font-semibold text-slate-900">
                      {inv.invoiceNumber}
                    </td>
                    <td className="px-5 py-3.5">
                      <div className="font-medium text-slate-900">{inv.vendorName}</div>
                      <div className="text-[11px] text-slate-400">ID: VEN-{inv.vendorId}</div>
                    </td>
                    <td className="px-5 py-3.5">
                      <span className="font-mono text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded border border-indigo-100">
                        {inv.poNumber}
                      </span>
                    </td>
                    <td className="px-5 py-3.5 text-slate-500">
                      {new Date(inv.invoiceDate).toLocaleDateString()}
                    </td>
                    <td className="px-5 py-3.5 text-slate-500">
                      {new Date(inv.dueDate).toLocaleDateString()}
                    </td>
                    <td className="px-5 py-3.5 text-right font-medium text-slate-900">
                      {formatCurrency(inv.totalAmount)}
                    </td>
                    <td className="px-5 py-3.5 text-right font-bold text-slate-900">
                      <span className={inv.outstandingAmount > 0 ? 'text-amber-600' : 'text-slate-400'}>
                        {formatCurrency(inv.outstandingAmount)}
                      </span>
                    </td>
                    <td className="px-5 py-3.5">{getStatusBadge(inv.status)}</td>
                    <td className="px-5 py-3.5 text-right space-x-2">
                      <button
                        onClick={() => {
                          setSelectedInvoice(inv);
                          setShowDetailModal(true);
                        }}
                        className="inline-flex items-center gap-1 px-2.5 py-1 border border-slate-200 bg-white hover:bg-slate-100 text-slate-700 rounded text-xs font-medium transition-colors"
                        title="View Details & 3-Way Match Verification"
                      >
                        <Eye className="w-3.5 h-3.5" />
                        <span>Inspect</span>
                      </button>

                      {inv.status === 'PendingVerification' && (isFinance || isAdmin) && (
                        <button
                          onClick={() => handleTriggerVerification(inv.invoiceId)}
                          disabled={actionLoading}
                          className="inline-flex items-center gap-1 px-2.5 py-1 bg-indigo-50 hover:bg-indigo-100 text-indigo-700 border border-indigo-200 rounded text-xs font-medium transition-colors"
                          title="Run 3-Way Verification"
                        >
                          <FileCheck className="w-3.5 h-3.5" />
                          <span>Run 3-Way Match</span>
                        </button>
                      )}

                      {(inv.status === 'Approved' || inv.status === 'PartiallyPaid') && (isFinance || isAdmin) && (
                        <button
                          onClick={() => navigate('/payments')}
                          className="inline-flex items-center gap-1 px-2.5 py-1 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border border-emerald-200 rounded text-xs font-medium transition-colors"
                          title="Proceed to Payment Settlement"
                        >
                          <CreditCard className="w-3.5 h-3.5" />
                          <span>Pay</span>
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

      {/* 3-Way Matching & Details Modal */}
      {showDetailModal && selectedInvoice && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-150">
            {/* Header */}
            <div className="p-6 border-b border-slate-200 flex items-center justify-between bg-slate-50/80">
              <div>
                <div className="flex items-center gap-2">
                  <span className="text-xs font-semibold uppercase tracking-wider text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded border border-indigo-200">
                    Invoice Details
                  </span>
                  <span className="text-sm font-bold text-slate-800">{selectedInvoice.invoiceNumber}</span>
                  {getStatusBadge(selectedInvoice.status)}
                </div>
                <div className="text-xs text-slate-500 mt-1">
                  Vendor: <strong>{selectedInvoice.vendorName}</strong> | PO Reference: <strong>{selectedInvoice.poNumber}</strong>
                </div>
              </div>

              <button
                onClick={() => setShowDetailModal(false)}
                className="w-8 h-8 rounded-full hover:bg-slate-200 text-slate-500 flex items-center justify-center transition-colors"
              >
                ✕
              </button>
            </div>

            {/* Modal Body */}
            <div className="p-6 space-y-6 flex-1 overflow-y-auto">
              {/* Financial Summary Strip */}
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 p-4 bg-slate-50 rounded-xl border border-slate-200">
                <div>
                  <div className="text-[11px] text-slate-500 uppercase font-semibold">Subtotal</div>
                  <div className="text-sm font-bold text-slate-900 mt-0.5">
                    {formatCurrency(selectedInvoice.subtotal)}
                  </div>
                </div>
                <div>
                  <div className="text-[11px] text-slate-500 uppercase font-semibold">Tax (GST)</div>
                  <div className="text-sm font-bold text-slate-900 mt-0.5">
                    {formatCurrency(selectedInvoice.taxAmount)}
                  </div>
                </div>
                <div>
                  <div className="text-[11px] text-slate-500 uppercase font-semibold">Total Amount</div>
                  <div className="text-base font-extrabold text-indigo-600 mt-0.5">
                    {formatCurrency(selectedInvoice.totalAmount)}
                  </div>
                </div>
                <div>
                  <div className="text-[11px] text-slate-500 uppercase font-semibold">Outstanding Due</div>
                  <div className="text-base font-extrabold text-amber-600 mt-0.5">
                    {formatCurrency(selectedInvoice.outstandingAmount)}
                  </div>
                </div>
              </div>

              {/* 3-Way Matching Verification Section */}
              <div className="border border-indigo-100 rounded-xl p-5 bg-indigo-50/30 space-y-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <FileCheck className="w-5 h-5 text-indigo-600" />
                    <h3 className="text-sm font-bold text-slate-900">
                      Three-Way Matching Verification (PO vs. GRN vs. Invoice)
                    </h3>
                  </div>

                  {selectedInvoice.status === 'PendingVerification' && (
                    <button
                      onClick={() => handleTriggerVerification(selectedInvoice.invoiceId)}
                      disabled={actionLoading}
                      className="px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded text-xs font-semibold shadow-xs"
                    >
                      {actionLoading ? 'Verifying...' : 'Execute 3-Way Match'}
                    </button>
                  )}
                </div>

                {selectedInvoice.verifications && selectedInvoice.verifications.length > 0 ? (
                  <div className="space-y-3">
                    {selectedInvoice.verifications.map((v) => (
                      <div
                        key={v.verificationId}
                        className={`p-4 rounded-lg border text-xs ${
                          v.isMatchSuccessful
                            ? 'bg-emerald-50/70 border-emerald-200 text-emerald-900'
                            : 'bg-rose-50/70 border-rose-200 text-rose-900'
                        }`}
                      >
                        <div className="flex items-center justify-between font-semibold">
                          <div className="flex items-center gap-1.5">
                            {v.isMatchSuccessful ? (
                              <CheckCircle className="w-4 h-4 text-emerald-600" />
                            ) : (
                              <XCircle className="w-4 h-4 text-rose-600" />
                            )}
                            <span>{v.varianceType}</span>
                          </div>
                          <span className="text-[11px] text-slate-500">
                            Verified on {new Date(v.matchedDate).toLocaleString()}
                          </span>
                        </div>
                        <div className="mt-2 grid grid-cols-2 gap-2 text-[11px]">
                          <div>Price Variance: <strong>{formatCurrency(v.priceVariance)}</strong></div>
                          <div>Quantity Variance: <strong>{v.quantityVariance} Units</strong></div>
                        </div>
                        <div className="mt-1 text-slate-600 italic">
                          Remarks: {v.remarks || 'Automated verification check against PO and dock GRN.'}
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-xs text-slate-500 bg-white p-4 rounded-lg border border-slate-200">
                    No verification records found yet. Click <strong>Execute 3-Way Match</strong> to run the automated reconciliation engine.
                  </div>
                )}
              </div>

              {/* Invoice Billed Items */}
              <div>
                <h3 className="text-xs font-bold text-slate-900 uppercase tracking-wider mb-2">
                  Billed Line Items
                </h3>
                <div className="border border-slate-200 rounded-lg overflow-hidden">
                  <table className="w-full text-left text-xs">
                    <thead className="bg-slate-50 text-slate-500 font-semibold uppercase">
                      <tr>
                        <th className="px-4 py-2.5">Material</th>
                        <th className="px-4 py-2.5 text-right">Billed Qty</th>
                        <th className="px-4 py-2.5 text-right">Billed Unit Price</th>
                        <th className="px-4 py-2.5 text-right">Line Total</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-100">
                      {selectedInvoice.items?.map((item) => (
                        <tr key={item.invoiceItemId} className="hover:bg-slate-50">
                          <td className="px-4 py-2.5 font-medium text-slate-800">
                            {item.materialName}
                          </td>
                          <td className="px-4 py-2.5 text-right">{item.billedQuantity}</td>
                          <td className="px-4 py-2.5 text-right">{formatCurrency(item.billedUnitPrice)}</td>
                          <td className="px-4 py-2.5 text-right font-semibold">
                            {formatCurrency(item.lineTotal)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Payments History on this invoice */}
              {selectedInvoice.payments && selectedInvoice.payments.length > 0 && (
                <div>
                  <h3 className="text-xs font-bold text-slate-900 uppercase tracking-wider mb-2">
                    Settled Payments
                  </h3>
                  <div className="border border-slate-200 rounded-lg overflow-hidden">
                    <table className="w-full text-left text-xs">
                      <thead className="bg-slate-50 text-slate-500 font-semibold uppercase">
                        <tr>
                          <th className="px-4 py-2.5">Payment #</th>
                          <th className="px-4 py-2.5">Method</th>
                          <th className="px-4 py-2.5">Txn Reference</th>
                          <th className="px-4 py-2.5">Date</th>
                          <th className="px-4 py-2.5 text-right">Amount</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-100">
                        {selectedInvoice.payments.map((p) => (
                          <tr key={p.paymentId}>
                            <td className="px-4 py-2.5 font-semibold text-slate-900">{p.paymentNumber}</td>
                            <td className="px-4 py-2.5">{p.paymentMethod}</td>
                            <td className="px-4 py-2.5 font-mono text-[11px] text-slate-600">
                              {p.transactionReference}
                            </td>
                            <td className="px-4 py-2.5">{new Date(p.paymentDate).toLocaleDateString()}</td>
                            <td className="px-4 py-2.5 text-right font-bold text-emerald-600">
                              {formatCurrency(p.paymentAmount)}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>

            {/* Footer Actions */}
            <div className="p-4 border-t border-slate-200 bg-slate-50 flex items-center justify-between">
              <button
                onClick={() => setShowDetailModal(false)}
                className="px-4 py-2 border border-slate-200 bg-white text-slate-700 rounded-lg text-xs font-medium hover:bg-slate-100 transition-colors"
              >
                Close
              </button>

              <div className="flex items-center gap-2">
                {(isFinance || isAdmin) && selectedInvoice.status === 'Matched' && (
                  <>
                    <button
                      onClick={() => setShowRejectModal(true)}
                      className="px-4 py-2 bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 rounded-lg text-xs font-semibold transition-colors"
                    >
                      Reject Invoice
                    </button>
                    <button
                      onClick={() => handleApproveInvoice(selectedInvoice.invoiceId)}
                      disabled={actionLoading}
                      className="px-5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-semibold shadow-sm transition-colors"
                    >
                      {actionLoading ? 'Approving...' : 'Approve for Payment'}
                    </button>
                  </>
                )}

                {(isFinance || isAdmin) && selectedInvoice.status === 'Mismatched' && (
                  <button
                    onClick={() => setShowRejectModal(true)}
                    className="px-4 py-2 bg-rose-600 hover:bg-rose-700 text-white rounded-lg text-xs font-semibold shadow-sm transition-colors"
                  >
                    Reject Due to Variance
                  </button>
                )}

                {(selectedInvoice.status === 'Approved' || selectedInvoice.status === 'PartiallyPaid') && (
                  <button
                    onClick={() => {
                      setShowDetailModal(false);
                      navigate('/payments');
                    }}
                    className="px-5 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-xs font-semibold shadow-sm flex items-center gap-1.5 transition-colors"
                  >
                    <span>Proceed to Settle Payment</span>
                    <ArrowRight className="w-3.5 h-3.5" />
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Reject Modal */}
      {showRejectModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl shadow-xl border border-slate-200 w-full max-w-md p-6 space-y-4">
            <h3 className="text-base font-bold text-slate-900">Reject Supplier Invoice</h3>
            <p className="text-xs text-slate-500">
              Please provide clear rejection remarks (e.g., price discrepancy, quantity mismatch, missing delivery challan).
            </p>
            <textarea
              rows={4}
              value={actionRemarks}
              onChange={(e) => setActionRemarks(e.target.value)}
              placeholder="Enter rejection reason..."
              className="w-full bg-slate-50 border border-slate-200 rounded-lg p-3 text-xs focus:outline-none focus:ring-2 focus:ring-rose-500/20 focus:border-rose-500"
            />
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setShowRejectModal(false)}
                className="px-4 py-2 border border-slate-200 bg-white text-slate-700 rounded-lg text-xs font-medium hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                onClick={handleRejectInvoice}
                disabled={actionLoading}
                className="px-4 py-2 bg-rose-600 hover:bg-rose-700 text-white rounded-lg text-xs font-semibold shadow-sm"
              >
                {actionLoading ? 'Rejecting...' : 'Confirm Rejection'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Record New Supplier Invoice Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-150">
            <div className="p-6 border-b border-slate-200 flex items-center justify-between bg-slate-50">
              <div>
                <h3 className="text-base font-bold text-slate-900">Record Incoming Supplier Invoice</h3>
                <p className="text-xs text-slate-500">
                  Select an active Purchase Order to populate line items and record vendor billing details.
                </p>
              </div>
              <button
                onClick={() => setShowCreateModal(false)}
                className="w-8 h-8 rounded-full hover:bg-slate-200 text-slate-500 flex items-center justify-center transition-colors"
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleCreateInvoice} className="p-6 space-y-5 flex-1 overflow-y-auto">
              {/* Select PO */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Select Purchase Order *
                </label>
                <select
                  required
                  onChange={(e) => handleSelectPO(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500"
                >
                  <option value="">-- Choose a Purchase Order --</option>
                  {approvedPOs.map((po) => (
                    <option key={po.poId} value={po.poId}>
                      {po.poNumber} | {po.vendorName} | {formatCurrency(po.grandTotal)} ({po.status})
                    </option>
                  ))}
                </select>
              </div>

              {selectedPO && (
                <>
                  <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    <div>
                      <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                        Vendor Invoice Number *
                      </label>
                      <input
                        type="text"
                        required
                        placeholder="e.g. INV-2026-9901"
                        value={invoiceNumber}
                        onChange={(e) => setInvoiceNumber(e.target.value)}
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                        Invoice Date *
                      </label>
                      <input
                        type="date"
                        required
                        value={invoiceDate}
                        onChange={(e) => setInvoiceDate(e.target.value)}
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                        Due Date *
                      </label>
                      <input
                        type="date"
                        required
                        value={dueDate}
                        onChange={(e) => setDueDate(e.target.value)}
                        className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500"
                      />
                    </div>
                  </div>

                  {/* Billed Items Grid */}
                  <div>
                    <label className="block text-xs font-semibold text-slate-700 mb-2">
                      Billed Items (Editable for 3-Way Variance Testing)
                    </label>
                    <div className="border border-slate-200 rounded-lg overflow-hidden">
                      <table className="w-full text-left text-xs">
                        <thead className="bg-slate-50 text-slate-500 font-semibold uppercase">
                          <tr>
                            <th className="px-4 py-2.5">Material</th>
                            <th className="px-4 py-2.5 w-32">Billed Qty</th>
                            <th className="px-4 py-2.5 w-36">Billed Price (₹)</th>
                            <th className="px-4 py-2.5 text-right">Line Total</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-100">
                          {invoiceItems.map((item, idx) => (
                            <tr key={idx}>
                              <td className="px-4 py-2 font-medium text-slate-800">{item.materialName}</td>
                              <td className="px-4 py-2">
                                <input
                                  type="number"
                                  min="0.01"
                                  step="any"
                                  required
                                  value={item.billedQuantity}
                                  onChange={(e) =>
                                    handleItemChange(idx, 'billedQuantity', parseFloat(e.target.value) || 0)
                                  }
                                  className="w-full bg-white border border-slate-200 rounded px-2 py-1 text-xs"
                                />
                              </td>
                              <td className="px-4 py-2">
                                <input
                                  type="number"
                                  min="0"
                                  step="any"
                                  required
                                  value={item.billedUnitPrice}
                                  onChange={(e) =>
                                    handleItemChange(idx, 'billedUnitPrice', parseFloat(e.target.value) || 0)
                                  }
                                  className="w-full bg-white border border-slate-200 rounded px-2 py-1 text-xs"
                                />
                              </td>
                              <td className="px-4 py-2 text-right font-bold text-slate-900">
                                {formatCurrency(item.lineTotal)}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {/* Estimated totals preview */}
                  <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 flex justify-between items-center text-xs">
                    <div>
                      <span className="text-slate-500">Standard GST Applied:</span>{' '}
                      <strong>18% (CGST 9% + SGST 9%)</strong>
                    </div>
                    <div className="text-right">
                      <div className="text-slate-500">Est. Total Invoice Value:</div>
                      <div className="text-base font-extrabold text-indigo-600">
                        {formatCurrency(
                          invoiceItems.reduce((acc, i) => acc + i.lineTotal, 0) * 1.18
                        )}
                      </div>
                    </div>
                  </div>
                </>
              )}

              <div className="pt-3 border-t border-slate-200 flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 border border-slate-200 bg-white text-slate-700 rounded-lg text-xs font-medium hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={actionLoading || !selectedPO}
                  className="px-5 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-xs font-semibold shadow-sm transition-colors"
                >
                  {actionLoading ? 'Saving...' : 'Save Supplier Invoice'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
