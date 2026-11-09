import React, { useState, useEffect } from 'react';
import {
  CreditCard,
  Search,
  Filter,
  CheckCircle,
  Plus,
  RefreshCw,
  AlertCircle
} from 'lucide-react';
import { paymentsApi, invoicesApi, masterApi } from '../services/api';
import type { Payment, SupplierInvoice, Vendor, PaymentMethod } from '../types';
import { useAuth } from '../context/AuthContext';

export const PaymentsPage: React.FC = () => {
  const { isFinance, isAdmin } = useAuth();

  const [payments, setPayments] = useState<Payment[]>([]);
  const [approvedInvoices, setApprovedInvoices] = useState<SupplierInvoice[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [methodFilter, setMethodFilter] = useState<string>('All');

  // Modal
  const [showPayModal, setShowPayModal] = useState<boolean>(false);
  const [selectedInvoice, setSelectedInvoice] = useState<SupplierInvoice | null>(null);
  const [paymentAmount, setPaymentAmount] = useState<number>(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('NEFT');
  const [txnReference, setTxnReference] = useState<string>('');
  const [paymentRemarks, setPaymentRemarks] = useState<string>('');
  const [actionLoading, setActionLoading] = useState<boolean>(false);

  useEffect(() => {
    fetchPayments();
    fetchPayableInvoices();
    fetchVendors();
  }, []);

  const fetchPayments = async () => {
    try {
      setLoading(true);
      const res = await paymentsApi.getAll({ pageSize: 50 });
      if (res.data?.success) {
        setPayments(res.data.data.items);
      }
    } catch (err) {
      console.error('Failed to load payments:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchPayableInvoices = async () => {
    try {
      const res = await invoicesApi.getAll({ pageSize: 50 });
      if (res.data?.success) {
        // Filter invoices eligible for payment: Approved or PartiallyPaid with outstanding > 0
        const eligible = res.data.data.items.filter(
          (inv) =>
            (inv.status === 'Approved' || inv.status === 'PartiallyPaid') &&
            inv.outstandingAmount > 0
        );
        setApprovedInvoices(eligible);
      }
    } catch (err) {
      console.error('Failed to load payable invoices:', err);
    }
  };

  const fetchVendors = async () => {
    try {
      const res = await masterApi.getVendors({ pageSize: 100 });
      if (res.data?.success) {
        setVendors(res.data.data.items);
      }
    } catch (err) {
      console.error('Failed to load vendors:', err);
    }
  };

  const handleOpenPayModal = (inv?: SupplierInvoice) => {
    if (inv) {
      setSelectedInvoice(inv);
      setPaymentAmount(inv.outstandingAmount);
    } else {
      setSelectedInvoice(approvedInvoices.length > 0 ? approvedInvoices[0] : null);
      if (approvedInvoices.length > 0) {
        setPaymentAmount(approvedInvoices[0].outstandingAmount);
      }
    }
    // Generate sample UTR reference
    setTxnReference(`UTR${new Date().getFullYear()}${Math.floor(10000000 + Math.random() * 90000000)}`);
    setPaymentRemarks('');
    setShowPayModal(true);
  };

  const handleSelectInvoice = (invIdStr: string) => {
    const invId = parseInt(invIdStr);
    const found = approvedInvoices.find((i) => i.invoiceId === invId) || null;
    setSelectedInvoice(found);
    if (found) {
      setPaymentAmount(found.outstandingAmount);
    } else {
      setPaymentAmount(0);
    }
  };

  const handleRecordPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedInvoice) {
      alert('Please select an invoice to settle.');
      return;
    }

    if (paymentAmount <= 0) {
      alert('Payment amount must be greater than zero.');
      return;
    }

    if (paymentAmount > selectedInvoice.outstandingAmount) {
      alert(
        `Payment amount (₹${paymentAmount}) cannot exceed the outstanding balance (₹${selectedInvoice.outstandingAmount}).`
      );
      return;
    }

    if (!txnReference.trim()) {
      alert('Transaction / UTR Reference is required.');
      return;
    }

    try {
      setActionLoading(true);
      const payload = {
        invoiceId: selectedInvoice.invoiceId,
        paymentAmount,
        paymentMethod,
        transactionReference: txnReference.trim(),
        remarks: paymentRemarks.trim() || undefined
      };

      const res = await paymentsApi.record(payload);
      if (res.data?.success) {
        alert(`Payment ${res.data.data.paymentNumber} recorded successfully!`);
        setShowPayModal(false);
        fetchPayments();
        fetchPayableInvoices();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to record payment.');
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

  const filteredPayments = payments.filter((p) => {
    const matchSearch =
      p.paymentNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.vendorName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.transactionReference.toLowerCase().includes(searchTerm.toLowerCase());
    const matchMethod = methodFilter === 'All' || p.paymentMethod === methodFilter;
    return matchSearch && matchMethod;
  });

  const totalDisbursed = payments.reduce((acc, p) => acc + p.paymentAmount, 0);
  const totalOutstanding = approvedInvoices.reduce((acc, i) => acc + i.outstandingAmount, 0);

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-5">
        <div>
          <div className="flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-lg bg-emerald-600 flex items-center justify-center text-white shadow-md shadow-emerald-500/20">
              <CreditCard className="w-5 h-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Accounts Payable & Payment Settlement</h1>
              <p className="text-xs text-slate-500">
                SAP FI F-53 Equivalent: Vendor Outgoing Bank Transfers, NEFT/RTGS & Cheque Recording
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2.5">
          <button
            onClick={() => {
              fetchPayments();
              fetchPayableInvoices();
            }}
            className="flex items-center gap-1.5 px-3 py-2 border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium rounded-lg shadow-2xs transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
            <span>Refresh</span>
          </button>

          {(isFinance || isAdmin) && (
            <button
              onClick={() => handleOpenPayModal()}
              disabled={approvedInvoices.length === 0}
              className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-xs font-semibold rounded-lg shadow-sm shadow-emerald-600/30 transition-all"
            >
              <Plus className="w-4 h-4" />
              <span>Record Outgoing Payment</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Overview */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-2xs">
          <div className="text-xs font-semibold uppercase tracking-wider text-slate-400">Total Disbursements</div>
          <div className="text-2xl font-extrabold text-slate-900 mt-1">{formatCurrency(totalDisbursed)}</div>
          <div className="text-[11px] text-slate-400 mt-1">{payments.length} transactions processed</div>
        </div>
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-2xs">
          <div className="text-xs font-semibold uppercase tracking-wider text-amber-600">Total Outstanding Payables</div>
          <div className="text-2xl font-extrabold text-amber-600 mt-1">{formatCurrency(totalOutstanding)}</div>
          <div className="text-[11px] text-amber-700/80 mt-1">Across {approvedInvoices.length} approved invoices</div>
        </div>
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-2xs">
          <div className="text-xs font-semibold uppercase tracking-wider text-emerald-600">Settlement Compliance</div>
          <div className="text-2xl font-extrabold text-emerald-600 mt-1">100%</div>
          <div className="text-[11px] text-slate-400 mt-1">Full audit trail & UTR reference verified</div>
        </div>
      </div>

      {/* Outstanding Payables Queue (Actionable) */}
      {approvedInvoices.length > 0 && (
        <div className="bg-white rounded-xl border border-amber-200/80 shadow-2xs overflow-hidden">
          <div className="px-5 py-3.5 bg-amber-50/60 border-b border-amber-200/60 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <AlertCircle className="w-4 h-4 text-amber-600" />
              <h2 className="text-xs font-bold text-amber-900 uppercase tracking-wider">
                Approved Invoices Awaiting Settlement ({approvedInvoices.length})
              </h2>
            </div>
            <span className="text-[11px] text-amber-700 font-medium">Ready for Bank Disbursement</span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="bg-slate-50 text-slate-500 font-semibold uppercase">
                <tr>
                  <th className="px-5 py-2.5">Invoice #</th>
                  <th className="px-5 py-2.5">Vendor</th>
                  <th className="px-5 py-2.5">PO Ref</th>
                  <th className="px-5 py-2.5">Due Date</th>
                  <th className="px-5 py-2.5 text-right">Invoice Total</th>
                  <th className="px-5 py-2.5 text-right">Outstanding Balance</th>
                  <th className="px-5 py-2.5 text-right">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {approvedInvoices.map((inv) => (
                  <tr key={inv.invoiceId} className="hover:bg-amber-50/30">
                    <td className="px-5 py-3 font-semibold text-slate-900">{inv.invoiceNumber}</td>
                    <td className="px-5 py-3 font-medium text-slate-800">{inv.vendorName}</td>
                    <td className="px-5 py-3 font-mono text-slate-600">{inv.poNumber}</td>
                    <td className="px-5 py-3 text-slate-500">{new Date(inv.dueDate).toLocaleDateString()}</td>
                    <td className="px-5 py-3 text-right">{formatCurrency(inv.totalAmount)}</td>
                    <td className="px-5 py-3 text-right font-bold text-amber-600">
                      {formatCurrency(inv.outstandingAmount)}
                    </td>
                    <td className="px-5 py-3 text-right">
                      {(isFinance || isAdmin) && (
                        <button
                          onClick={() => handleOpenPayModal(inv)}
                          className="px-3 py-1 bg-emerald-600 hover:bg-emerald-700 text-white rounded text-xs font-semibold shadow-2xs transition-colors"
                        >
                          Disburse
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Payments History Ledger */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-2xs overflow-hidden space-y-4 p-5">
        <div className="flex flex-col md:flex-row justify-between items-center gap-4">
          <div className="relative w-full md:w-96">
            <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Search by Payment #, Vendor, Invoice # or UTR..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-xs focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            />
          </div>

          <div className="flex items-center gap-2">
            <Filter className="w-3.5 h-3.5 text-slate-400" />
            <span className="text-xs text-slate-600 font-medium">Method:</span>
            <select
              value={methodFilter}
              onChange={(e) => setMethodFilter(e.target.value)}
              className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 font-medium focus:outline-none"
            >
              <option value="All">All Methods</option>
              <option value="NEFT">NEFT</option>
              <option value="RTGS">RTGS</option>
              <option value="IMPS">IMPS</option>
              <option value="Cheque">Cheque</option>
              <option value="BankTransfer">Bank Transfer</option>
            </select>
          </div>
        </div>

        <div className="overflow-x-auto border border-slate-200 rounded-lg">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
              <tr>
                <th className="px-5 py-3">Payment Voucher #</th>
                <th className="px-5 py-3">Vendor</th>
                <th className="px-5 py-3">Invoice Number</th>
                <th className="px-5 py-3">Payment Date</th>
                <th className="px-5 py-3">Method</th>
                <th className="px-5 py-3">Bank UTR / Txn Ref</th>
                <th className="px-5 py-3 text-right">Amount Paid</th>
                <th className="px-5 py-3">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 text-slate-700">
              {loading ? (
                <tr>
                  <td colSpan={8} className="px-5 py-10 text-center text-slate-400">
                    <RefreshCw className="w-5 h-5 animate-spin mx-auto mb-2 text-emerald-500" />
                    Loading payment transactions...
                  </td>
                </tr>
              ) : filteredPayments.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-5 py-12 text-center text-slate-400">
                    No payment records found.
                  </td>
                </tr>
              ) : (
                filteredPayments.map((p) => (
                  <tr key={p.paymentId} className="hover:bg-slate-50/70">
                    <td className="px-5 py-3.5 font-bold text-slate-900">{p.paymentNumber}</td>
                    <td className="px-5 py-3.5">
                      <div className="font-medium text-slate-900">{p.vendorName}</div>
                    </td>
                    <td className="px-5 py-3.5 font-mono text-slate-600">{p.invoiceNumber}</td>
                    <td className="px-5 py-3.5 text-slate-500">
                      {new Date(p.paymentDate).toLocaleDateString()}
                    </td>
                    <td className="px-5 py-3.5">
                      <span className="px-2 py-0.5 rounded text-[11px] font-semibold bg-slate-100 text-slate-700 border border-slate-200">
                        {p.paymentMethod}
                      </span>
                    </td>
                    <td className="px-5 py-3.5 font-mono text-xs text-blue-600 font-semibold">
                      {p.transactionReference}
                    </td>
                    <td className="px-5 py-3.5 text-right font-extrabold text-emerald-600">
                      {formatCurrency(p.paymentAmount)}
                    </td>
                    <td className="px-5 py-3.5">
                      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700 border border-emerald-200">
                        <CheckCircle className="w-3 h-3 text-emerald-600" /> Settled
                      </span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Record Payment Modal */}
      {showPayModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-lg p-6 space-y-5 animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between border-b border-slate-200 pb-3">
              <div>
                <h3 className="text-base font-bold text-slate-900">Record Outgoing Payment</h3>
                <p className="text-xs text-slate-500">Settle supplier invoice via bank transfer or cheque</p>
              </div>
              <button
                onClick={() => setShowPayModal(false)}
                className="w-8 h-8 rounded-full hover:bg-slate-200 text-slate-500 flex items-center justify-center"
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleRecordPayment} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Select Approved Invoice *
                </label>
                <select
                  required
                  value={selectedInvoice?.invoiceId || ''}
                  onChange={(e) => handleSelectInvoice(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
                >
                  <option value="">-- Choose an Invoice --</option>
                  {approvedInvoices.map((i) => (
                    <option key={i.invoiceId} value={i.invoiceId}>
                      {i.invoiceNumber} | {i.vendorName} | Due: {formatCurrency(i.outstandingAmount)}
                    </option>
                  ))}
                </select>
              </div>

              {selectedInvoice && (
                <div className="p-3 bg-slate-50 rounded-xl border border-slate-200 space-y-1 text-xs">
                  <div className="flex justify-between">
                    <span className="text-slate-500">Vendor:</span>
                    <strong className="text-slate-800">{selectedInvoice.vendorName}</strong>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-500">PO Number:</span>
                    <strong className="text-slate-800">{selectedInvoice.poNumber}</strong>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-500">Total Invoice Amount:</span>
                    <span className="text-slate-800">{formatCurrency(selectedInvoice.totalAmount)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-500">Already Paid:</span>
                    <span className="text-slate-800">{formatCurrency(selectedInvoice.paidAmount)}</span>
                  </div>
                  <div className="flex justify-between pt-1 border-t border-slate-200 font-semibold">
                    <span className="text-amber-700">Remaining Balance:</span>
                    <span className="text-amber-700">{formatCurrency(selectedInvoice.outstandingAmount)}</span>
                  </div>
                </div>
              )}

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                    Payment Amount (₹) *
                  </label>
                  <input
                    type="number"
                    min="1"
                    max={selectedInvoice?.outstandingAmount || 99999999}
                    step="any"
                    required
                    value={paymentAmount}
                    onChange={(e) => setPaymentAmount(parseFloat(e.target.value) || 0)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 font-semibold"
                  />
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                    Payment Method *
                  </label>
                  <select
                    value={paymentMethod}
                    onChange={(e) => setPaymentMethod(e.target.value as PaymentMethod)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
                  >
                    <option value="NEFT">NEFT (National Electronic Funds Transfer)</option>
                    <option value="RTGS">RTGS (Real Time Gross Settlement)</option>
                    <option value="IMPS">IMPS (Immediate Payment)</option>
                    <option value="Cheque">Corporate Cheque</option>
                    <option value="BankTransfer">Direct Bank Transfer</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Bank UTR / Transaction Reference *
                </label>
                <input
                  type="text"
                  required
                  placeholder="e.g. AXISNEFT26000189283"
                  value={txnReference}
                  onChange={(e) => setTxnReference(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs font-mono focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1.5">
                  Remarks / Internal Reference
                </label>
                <input
                  type="text"
                  placeholder="e.g. Cleared via HDFC Treasury A/c"
                  value={paymentRemarks}
                  onChange={(e) => setPaymentRemarks(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
                />
              </div>

              <div className="pt-3 border-t border-slate-200 flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setShowPayModal(false)}
                  className="px-4 py-2 border border-slate-200 bg-white text-slate-700 rounded-lg text-xs font-medium hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={actionLoading || !selectedInvoice}
                  className="px-5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-semibold shadow-sm transition-colors"
                >
                  {actionLoading ? 'Recording...' : 'Record Payment'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
