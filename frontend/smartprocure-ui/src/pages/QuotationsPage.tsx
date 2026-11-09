import React, { useEffect, useState } from 'react';
import {
  BadgeDollarSign,
  Search,
  CheckCircle,
  XCircle,
  Plus,
  Building2,
  Calendar,
  Clock,
  Award,
  ArrowRight
} from 'lucide-react';
import { quotationsApi, requisitionsApi, masterApi } from '../services/api';
import type { VendorQuotation, Requisition, Vendor } from '../types';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export const QuotationsPage: React.FC = () => {
  const { isPurchasing, isAdmin } = useAuth();
  const navigate = useNavigate();

  const [approvedPRs, setApprovedPRs] = useState<Requisition[]>([]);
  const [selectedPRId, setSelectedPRId] = useState<number | null>(null);
  const [quotations, setQuotations] = useState<VendorQuotation[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [loading, setLoading] = useState(true);

  // Selection Modal
  const [selectionModal, setSelectionModal] = useState<{
    show: boolean;
    quoteId: number;
    vendorName: string;
    amount: number;
    justification: string;
  }>({ show: false, quoteId: 0, vendorName: '', amount: 0, justification: '' });

  // Create Quotation Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newVendorId, setNewVendorId] = useState<number>(1);
  const [newDeliveryDays, setNewDeliveryDays] = useState<number>(7);
  const [newPaymentTerms, setNewPaymentTerms] = useState<string>('Net 30');
  const [newValidDate, setNewValidDate] = useState<string>(
    new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0]
  );
  const [newQuoteItems, setNewQuoteItems] = useState<{ materialId: number; quotedUnitPrice: number; taxPercentage: number }[]>([]);

  useEffect(() => {
    const loadInitial = async () => {
      setLoading(true);
      try {
        const [prsRes, vendorsRes] = await Promise.all([
          requisitionsApi.getAll({ status: 'Approved', page: 1, pageSize: 50 }),
          masterApi.getVendors({ status: true, page: 1, pageSize: 100 }),
        ]);

        if (prsRes.data.success) {
          setApprovedPRs(prsRes.data.data.items);
          if (prsRes.data.data.items.length > 0) {
            setSelectedPRId(prsRes.data.data.items[0].requisitionId);
          }
        }
        if (vendorsRes.data.success) setVendors(vendorsRes.data.data.items);
      } catch (err) {
        console.error('Failed to load RFQ data', err);
      } finally {
        setLoading(false);
      }
    };

    loadInitial();
  }, []);

  useEffect(() => {
    if (selectedPRId) {
      loadQuotations(selectedPRId);
    }
  }, [selectedPRId]);

  const loadQuotations = async (prId: number) => {
    try {
      const res = await quotationsApi.getByPR(prId);
      if (res.data.success) {
        setQuotations(res.data.data);
      }
    } catch (err) {
      console.error('Failed to load quotes for PR', err);
    }
  };

  const handleSelectQuote = async () => {
    if (!selectionModal.justification.trim()) {
      alert('Selection justification is required for audit compliance');
      return;
    }

    try {
      await quotationsApi.select(selectionModal.quoteId, {
        selectionJustification: selectionModal.justification,
      });
      setSelectionModal({ show: false, quoteId: 0, vendorName: '', amount: 0, justification: '' });
      if (selectedPRId) loadQuotations(selectedPRId);
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to select quotation');
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
    }).format(val);
  };

  const selectedPR = approvedPRs.find((p) => p.requisitionId === selectedPRId);

  return (
    <div className="space-y-5">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Vendor Quotations & Decision Matrix</h1>
          <p className="text-xs text-slate-500">
            Compare multi-vendor price quotations, delivery lead times and payment terms
          </p>
        </div>
      </div>

      {/* PR Selection Selector Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-xs flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <label className="text-xs font-semibold text-slate-700 whitespace-nowrap">
            Select Approved Requisition:
          </label>
          <select
            value={selectedPRId || ''}
            onChange={(e) => setSelectedPRId(parseInt(e.target.value))}
            className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs font-medium text-slate-800 focus:outline-none min-w-[280px]"
          >
            {approvedPRs.map((pr) => (
              <option key={pr.requisitionId} value={pr.requisitionId}>
                {pr.requisitionNumber} - {pr.departmentName} ({formatCurrency(pr.totalEstimatedAmount)})
              </option>
            ))}
          </select>
        </div>

        {selectedPR && (
          <div className="text-xs text-slate-500 flex items-center gap-3">
            <span>
              Required by: <strong>{new Date(selectedPR.requiredDeliveryDate).toLocaleDateString()}</strong>
            </span>
            <span>
              Priority: <strong className="text-orange-600">{selectedPR.priorityName}</strong>
            </span>
          </div>
        )}
      </div>

      {/* Decision Matrix Grid */}
      {selectedPRId && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-bold text-slate-800">
              Competing Quotations ({quotations.length} Received)
            </h2>
          </div>

          {quotations.length === 0 ? (
            <div className="bg-white rounded-xl border border-dashed border-slate-300 p-12 text-center text-xs text-slate-500">
              <BadgeDollarSign className="w-10 h-10 text-slate-300 mx-auto mb-2" />
              <p className="font-semibold text-slate-700">No vendor quotations recorded yet for this Requisition.</p>
              <p className="mt-1">Vendors are invited to submit competitive bids for approved items.</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
              {quotations.map((quote) => {
                const isSelected = quote.status === 'Selected';
                const isRejected = quote.status === 'Rejected';

                return (
                  <div
                    key={quote.quotationId}
                    className={`bg-white rounded-xl border p-5 shadow-xs transition-all flex flex-col justify-between ${
                      isSelected
                        ? 'border-emerald-500 ring-2 ring-emerald-500/20 bg-emerald-50/10'
                        : isRejected
                        ? 'border-slate-200 opacity-60'
                        : 'border-slate-200 hover:border-blue-400'
                    }`}
                  >
                    <div>
                      {/* Card Header */}
                      <div className="flex items-start justify-between border-b border-slate-100 pb-3 mb-3">
                        <div>
                          <span className="font-mono text-[10px] text-slate-400 uppercase tracking-wider block">
                            {quote.quotationNumber}
                          </span>
                          <h3 className="font-bold text-sm text-slate-900 mt-0.5">{quote.vendorName}</h3>
                        </div>
                        <span className="bg-amber-50 text-amber-700 px-2 py-0.5 rounded font-bold text-xs border border-amber-200/50">
                          ★ {quote.vendorRating.toFixed(1)}
                        </span>
                      </div>

                      {/* Financials & Terms */}
                      <div className="space-y-2 text-xs mb-4">
                        <div className="flex justify-between py-1 border-b border-slate-50">
                          <span className="text-slate-500">Total Quoted Bid:</span>
                          <span className="font-bold text-slate-900 text-sm">
                            {formatCurrency(quote.totalQuotedAmount)}
                          </span>
                        </div>
                        <div className="flex justify-between py-1 border-b border-slate-50">
                          <span className="text-slate-500">Delivery Lead Time:</span>
                          <span className="font-medium text-slate-800 flex items-center gap-1">
                            <Clock className="w-3 h-3 text-slate-400" />
                            {quote.deliveryDays} Calendar Days
                          </span>
                        </div>
                        <div className="flex justify-between py-1 border-b border-slate-50">
                          <span className="text-slate-500">Payment Terms:</span>
                          <span className="font-medium text-slate-800">{quote.paymentTerms}</span>
                        </div>
                        <div className="flex justify-between py-1 border-b border-slate-50">
                          <span className="text-slate-500">Quotation Validity:</span>
                          <span className="font-medium text-slate-800">
                            Until {new Date(quote.validUntil).toLocaleDateString()}
                          </span>
                        </div>
                      </div>

                      {/* Items Breakup */}
                      <div className="bg-slate-50 rounded-lg p-2.5 mb-4 text-[11px] space-y-1">
                        <div className="font-semibold text-slate-600 mb-1 text-[10px] uppercase tracking-wider">
                          Line Items
                        </div>
                        {quote.items.map((i) => (
                          <div key={i.quotationItemId} className="flex justify-between text-slate-700">
                            <span className="truncate max-w-[160px]">{i.materialName}</span>
                            <span className="font-mono font-medium">₹{i.quotedUnitPrice.toFixed(2)} (+{i.taxPercentage}% GST)</span>
                          </div>
                        ))}
                      </div>

                      {isSelected && quote.selectionJustification && (
                        <div className="p-2.5 bg-emerald-50 border border-emerald-200 rounded-lg text-[11px] text-emerald-800 mb-4">
                          <strong>Selection Justification:</strong> {quote.selectionJustification}
                        </div>
                      )}
                    </div>

                    {/* Action */}
                    <div>
                      {isSelected ? (
                        <div className="w-full py-2 bg-emerald-600 text-white rounded-lg text-xs font-bold text-center flex items-center justify-center gap-1.5 shadow-xs">
                          <CheckCircle className="w-4 h-4" />
                          <span>Winning Supplier Selected</span>
                        </div>
                      ) : isRejected ? (
                        <div className="w-full py-2 bg-slate-100 text-slate-400 rounded-lg text-xs font-medium text-center">
                          Quotation Rejected
                        </div>
                      ) : (
                        <button
                          onClick={() =>
                            setSelectionModal({
                              show: true,
                              quoteId: quote.quotationId,
                              vendorName: quote.vendorName,
                              amount: quote.totalQuotedAmount,
                              justification: `Selected based on high supplier quality rating (★ ${quote.vendorRating}) and competitive lead time (${quote.deliveryDays} days).`,
                            })
                          }
                          className="w-full py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold text-center transition-colors shadow-xs"
                        >
                          Select Vendor & Award Order
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}

      {/* SELECTION JUSTIFICATION MODAL */}
      {selectionModal.show && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md p-5 space-y-4">
            <h3 className="text-sm font-bold text-slate-900">Award Contract to {selectionModal.vendorName}</h3>
            <p className="text-xs text-slate-500">
              Total Contract Value: <strong>{formatCurrency(selectionModal.amount)}</strong>. Please document the
              commercial or technical reason for audit compliance.
            </p>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">
                Selection Justification (Mandatory for Audit Trail)
              </label>
              <textarea
                rows={3}
                value={selectionModal.justification}
                onChange={(e) => setSelectionModal({ ...selectionModal, justification: e.target.value })}
                className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 text-xs"
                placeholder="e.g. Best overall value combining ISO certification, 7-day turnaround, and verified past quality performance."
              />
            </div>

            <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setSelectionModal({ show: false, quoteId: 0, vendorName: '', amount: 0, justification: '' })}
                className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-medium text-slate-600"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleSelectQuote}
                className="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-semibold shadow-xs"
              >
                Confirm Selection
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

