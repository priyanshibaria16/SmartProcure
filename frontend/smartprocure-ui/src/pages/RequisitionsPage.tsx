import React, { useEffect, useState } from 'react';
import {
  Plus,
  Search,
  Filter,
  CheckCircle,
  XCircle,
  Send,
  Trash2,
  Clock,
  Eye,
  AlertCircle
} from 'lucide-react';
import { requisitionsApi, masterApi } from '../services/api';
import type { Requisition, Material, Department, PriorityLevel } from '../types';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';

const STATUS_BADGES: Record<string, { bg: string; text: string }> = {
  Draft: { bg: 'bg-slate-100', text: 'text-slate-700' },
  Submitted: { bg: 'bg-blue-100', text: 'text-blue-700' },
  PendingApproval: { bg: 'bg-amber-100', text: 'text-amber-700' },
  Approved: { bg: 'bg-emerald-100', text: 'text-emerald-700' },
  Rejected: { bg: 'bg-rose-100', text: 'text-rose-700' },
  ChangesRequested: { bg: 'bg-purple-100', text: 'text-purple-700' },
  ConvertedToPO: { bg: 'bg-indigo-100', text: 'text-indigo-700' },
  Cancelled: { bg: 'bg-slate-200', text: 'text-slate-500' },
};

const PRIORITY_BADGES: Record<string, { bg: string; text: string }> = {
  Low: { bg: 'bg-slate-100', text: 'text-slate-600' },
  Medium: { bg: 'bg-blue-100', text: 'text-blue-700' },
  High: { bg: 'bg-orange-100', text: 'text-orange-700' },
  Urgent: { bg: 'bg-rose-100', text: 'text-rose-700 font-bold animate-pulse' },
};

export const RequisitionsPage: React.FC = () => {
  const { isPurchasing, isAdmin, isRequester } = useAuth();

  const [requisitions, setRequisitions] = useState<Requisition[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [materials, setMaterials] = useState<Material[]>([]);
  const [loading, setLoading] = useState(true);

  // Filters
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [deptFilter, setDeptFilter] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');

  // Create Modal State
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newDeptId, setNewDeptId] = useState<number>(1);
  const [newDeliveryDate, setNewDeliveryDate] = useState<string>(
    new Date(Date.now() + 14 * 86400000).toISOString().split('T')[0]
  );
  const [newPriority, setNewPriority] = useState<PriorityLevel>('Medium');
  const [newJustification, setNewJustification] = useState<string>('');
  const [submitImmediately, setSubmitImmediately] = useState<boolean>(true);
  const [newItems, setNewItems] = useState<
    { materialId: number; quantity: number; estimatedUnitPrice: number; remarks: string }[]
  >([{ materialId: 1, quantity: 100, estimatedUnitPrice: 68.50, remarks: '' }]);

  // Action Modal State (Approve / Reject)
  const [actionModal, setActionModal] = useState<{
    show: boolean;
    prId: number;
    action: 'Approve' | 'Reject';
    remarks: string;
  }>({ show: false, prId: 0, action: 'Approve', remarks: '' });

  const loadData = async () => {
    setLoading(true);
    try {
      const [prsRes, deptsRes, matsRes] = await Promise.all([
        requisitionsApi.getAll({
          status: statusFilter || undefined,
          departmentId: deptFilter ? parseInt(deptFilter) : undefined,
          page: 1,
          pageSize: 50,
        }),
        masterApi.getDepartments(),
        masterApi.getMaterials({ page: 1, pageSize: 100 }),
      ]);

      if (prsRes.data.success) setRequisitions(prsRes.data.data.items);
      if (deptsRes.data.success) setDepartments(deptsRes.data.data);
      if (matsRes.data.success) setMaterials(matsRes.data.data.items);
    } catch (err) {
      console.error('Failed to load requisitions', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [statusFilter, deptFilter]);

  const handleAddItem = () => {
    const firstMat = materials[0];
    setNewItems([
      ...newItems,
      {
        materialId: firstMat ? firstMat.materialId : 1,
        quantity: 10,
        estimatedUnitPrice: firstMat ? firstMat.unitPrice : 100,
        remarks: '',
      },
    ]);
  };

  const handleRemoveItem = (index: number) => {
    if (newItems.length > 1) {
      setNewItems(newItems.filter((_, i) => i !== index));
    }
  };

  const handleMaterialChange = (index: number, matId: number) => {
    const mat = materials.find((m) => m.materialId === matId);
    const updated = [...newItems];
    updated[index].materialId = matId;
    if (mat) {
      updated[index].estimatedUnitPrice = mat.unitPrice;
    }
    setNewItems(updated);
  };

  const calculateTotal = () => {
    return newItems.reduce((acc, item) => acc + item.quantity * item.estimatedUnitPrice, 0);
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newJustification.trim()) {
      alert('Business justification is required');
      return;
    }

    try {
      const res = await requisitionsApi.create({
        departmentId: newDeptId,
        requiredDeliveryDate: newDeliveryDate,
        priority: newPriority,
        businessJustification: newJustification,
        submitImmediately,
        items: newItems,
      });

      if (res.data.success) {
        setShowCreateModal(false);
        setNewJustification('');
        loadData();
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to create requisition');
    }
  };

  const handleExecuteApproval = async () => {
    try {
      if (actionModal.action === 'Approve') {
        await requisitionsApi.approve(actionModal.prId, {
          action: 'Approve',
          remarks: actionModal.remarks,
        });
      } else {
        if (!actionModal.remarks.trim()) {
          alert('Rejection remarks are mandatory');
          return;
        }
        await requisitionsApi.reject(actionModal.prId, {
          action: 'Reject',
          remarks: actionModal.remarks,
        });
      }
      setActionModal({ show: false, prId: 0, action: 'Approve', remarks: '' });
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Operation failed');
    }
  };

  const handleSubmitDraft = async (id: number) => {
    try {
      await requisitionsApi.submit(id);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Submission failed');
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
    }).format(val);
  };

  const filteredRequisitions = requisitions.filter((r) => {
    if (!searchTerm) return true;
    const s = searchTerm.toLowerCase();
    return (
      r.requisitionNumber.toLowerCase().includes(s) ||
      r.departmentName.toLowerCase().includes(s) ||
      r.requestedByUserName.toLowerCase().includes(s) ||
      r.businessJustification.toLowerCase().includes(s)
    );
  });

  return (
    <div className="space-y-5">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-slate-900 tracking-tight">Purchase Requisitions (PR)</h1>
          <p className="text-xs text-slate-500">
            Internal material demand requests with multi-tier approval matrix
          </p>
        </div>
        {(isRequester || isAdmin) && (
          <button
            onClick={() => setShowCreateModal(true)}
            className="flex items-center gap-1.5 px-3.5 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold shadow-xs transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span>Create Purchase Requisition</span>
          </button>
        )}
      </div>

      {/* Filter Toolbar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-xs flex flex-wrap items-center gap-3">
        <div className="relative flex-1 min-w-[200px]">
          <Search className="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search by PR #, department, requester, justification..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-8 pr-3 py-1.5 text-xs focus:ring-1 focus:ring-blue-500 focus:outline-none"
          />
        </div>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 focus:outline-none"
        >
          <option value="">All Statuses</option>
          <option value="Draft">Draft</option>
          <option value="Submitted">Submitted</option>
          <option value="PendingApproval">Pending Approval</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
          <option value="ConvertedToPO">Converted to PO</option>
        </select>

        <select
          value={deptFilter}
          onChange={(e) => setDeptFilter(e.target.value)}
          className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 focus:outline-none"
        >
          <option value="">All Departments</option>
          {departments.map((d) => (
            <option key={d.departmentId} value={d.departmentId}>
              {d.departmentName}
            </option>
          ))}
        </select>
      </div>

      {/* Requisitions Table */}
      <div className="bg-white rounded-xl border border-slate-200/80 shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead>
              <tr className="bg-slate-50/80 border-b border-slate-200 text-slate-500 uppercase font-semibold text-[11px]">
                <th className="py-3 px-4">PR Number</th>
                <th className="py-3 px-3">Date</th>
                <th className="py-3 px-3">Department</th>
                <th className="py-3 px-3">Requested By</th>
                <th className="py-3 px-3">Priority</th>
                <th className="py-3 px-3 text-right">Est. Total Amount</th>
                <th className="py-3 px-3 text-center">Status</th>
                <th className="py-3 px-4 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loading ? (
                <tr>
                  <td colSpan={8} className="py-12 text-center text-slate-400">
                    Loading Purchase Requisitions...
                  </td>
                </tr>
              ) : filteredRequisitions.length === 0 ? (
                <tr>
                  <td colSpan={8} className="py-12 text-center text-slate-400">
                    No purchase requisitions found matching the selected criteria.
                  </td>
                </tr>
              ) : (
                filteredRequisitions.map((pr) => (
                  <tr key={pr.requisitionId} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4 font-bold text-blue-700 font-mono">
                      {pr.requisitionNumber}
                    </td>
                    <td className="py-3 px-3 text-slate-500 whitespace-nowrap">
                      {new Date(pr.requestDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-3 font-medium text-slate-800">{pr.departmentName}</td>
                    <td className="py-3 px-3 text-slate-600">{pr.requestedByUserName}</td>
                    <td className="py-3 px-3">
                      <span
                        className={`px-2 py-0.5 rounded text-[10px] font-medium ${
                          PRIORITY_BADGES[pr.priorityName]?.bg || 'bg-slate-100'
                        } ${PRIORITY_BADGES[pr.priorityName]?.text || 'text-slate-600'}`}
                      >
                        {pr.priorityName}
                      </span>
                    </td>
                    <td className="py-3 px-3 text-right font-bold text-slate-900">
                      {formatCurrency(pr.totalEstimatedAmount)}
                    </td>
                    <td className="py-3 px-3 text-center">
                      <span
                        className={`inline-block px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${
                          STATUS_BADGES[pr.statusName]?.bg || 'bg-slate-100'
                        } ${STATUS_BADGES[pr.statusName]?.text || 'text-slate-700'}`}
                      >
                        {pr.statusName}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-right whitespace-nowrap space-x-1">
                      {/* Submit action for Drafts */}
                      {pr.statusName === 'Draft' && (isRequester || isAdmin) && (
                        <button
                          onClick={() => handleSubmitDraft(pr.requisitionId)}
                          className="px-2 py-1 bg-blue-50 text-blue-700 hover:bg-blue-100 rounded text-[11px] font-medium"
                          title="Submit for Approval"
                        >
                          Submit
                        </button>
                      )}

                      {/* Approval actions for Purchase Manager / Admin */}
                      {(pr.statusName === 'PendingApproval' || pr.statusName === 'Submitted') && (isPurchasing || isAdmin) && (
                        <>
                          <button
                            onClick={() =>
                              setActionModal({
                                show: true,
                                prId: pr.requisitionId,
                                action: 'Approve',
                                remarks: 'Approved as per production schedule',
                              })
                            }
                            className="px-2 py-1 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 rounded text-[11px] font-semibold"
                          >
                            Approve
                          </button>
                          <button
                            onClick={() =>
                              setActionModal({
                                show: true,
                                prId: pr.requisitionId,
                                action: 'Reject',
                                remarks: '',
                              })
                            }
                            className="px-2 py-1 bg-rose-50 text-rose-700 hover:bg-rose-100 rounded text-[11px] font-semibold"
                          >
                            Reject
                          </button>
                        </>
                      )}

                      {/* Convert to PO action if Approved */}
                      {pr.statusName === 'Approved' && (isPurchasing || isAdmin) && (
                        <Link
                          to={`/purchase-orders?convertPrId=${pr.requisitionId}`}
                          className="px-2 py-1 bg-indigo-50 text-indigo-700 hover:bg-indigo-100 rounded text-[11px] font-semibold inline-block"
                        >
                          Convert to PO
                        </Link>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* CREATE PURCHASE REQUISITION MODAL */}
      {showCreateModal && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-3xl max-h-[90vh] overflow-y-auto p-6 space-y-5 animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <h2 className="text-base font-bold text-slate-900">Raise New Purchase Requisition</h2>
                <p className="text-xs text-slate-500">Internal request for material purchase authorization</p>
              </div>
              <button
                onClick={() => setShowCreateModal(false)}
                className="text-slate-400 hover:text-slate-600 p-1"
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleCreateSubmit} className="space-y-4 text-xs">
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Department</label>
                  <select
                    value={newDeptId}
                    onChange={(e) => setNewDeptId(parseInt(e.target.value))}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                  >
                    {departments.map((d) => (
                      <option key={d.departmentId} value={d.departmentId}>
                        {d.departmentName}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Required Delivery Date</label>
                  <input
                    type="date"
                    value={newDeliveryDate}
                    onChange={(e) => setNewDeliveryDate(e.target.value)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                    required
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-medium mb-1">Priority</label>
                  <select
                    value={newPriority}
                    onChange={(e) => setNewPriority(e.target.value as PriorityLevel)}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2"
                  >
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Urgent">Urgent</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-slate-600 font-medium mb-1">Business Justification</label>
                <textarea
                  rows={2}
                  placeholder="Explain why these materials are required, project codes, or plant maintenance details..."
                  value={newJustification}
                  onChange={(e) => setNewJustification(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2.5"
                  required
                />
              </div>

              {/* Line Items Table */}
              <div className="border border-slate-200 rounded-xl p-3 bg-slate-50/50">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-bold text-slate-800 uppercase tracking-wider text-[11px]">Material Items</span>
                  <button
                    type="button"
                    onClick={handleAddItem}
                    className="text-blue-600 font-semibold hover:text-blue-700 flex items-center gap-1 text-[11px]"
                  >
                    + Add Another Item
                  </button>
                </div>

                <div className="space-y-2">
                  {newItems.map((item, index) => (
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
                            const updated = [...newItems];
                            updated[index].quantity = parseFloat(e.target.value) || 0;
                            setNewItems(updated);
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
                          value={item.estimatedUnitPrice}
                          onChange={(e) => {
                            const updated = [...newItems];
                            updated[index].estimatedUnitPrice = parseFloat(e.target.value) || 0;
                            setNewItems(updated);
                          }}
                          className="w-full bg-slate-50 border border-slate-200 rounded p-1 text-xs text-right"
                          required
                        />
                      </div>

                      <div className="col-span-2 text-right">
                        <label className="text-[10px] text-slate-400 block mb-0.5">Line Total</label>
                        <div className="font-bold text-slate-800 text-xs py-1">
                          ₹{(item.quantity * item.estimatedUnitPrice).toFixed(2)}
                        </div>
                      </div>

                      <div className="col-span-1 text-right">
                        <button
                          type="button"
                          onClick={() => handleRemoveItem(index)}
                          className="text-slate-400 hover:text-rose-600 mt-3"
                          disabled={newItems.length <= 1}
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="mt-3 pt-2 border-t border-slate-200 flex justify-between items-center px-1">
                  <span className="font-semibold text-slate-600">Total Estimated Amount:</span>
                  <span className="text-base font-bold text-blue-700">{formatCurrency(calculateTotal())}</span>
                </div>
              </div>

              <div className="flex items-center gap-2 pt-2">
                <input
                  type="checkbox"
                  id="submitImmediately"
                  checked={submitImmediately}
                  onChange={(e) => setSubmitImmediately(e.target.checked)}
                  className="rounded text-blue-600"
                />
                <label htmlFor="submitImmediately" className="text-slate-700 font-medium">
                  Submit immediately for approval (bypasses draft)
                </label>
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
                  Create Requisition
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* APPROVAL / REJECTION DIALOG */}
      {actionModal.show && (
        <div className="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-xs flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md p-5 space-y-4">
            <h3 className="text-sm font-bold text-slate-900">
              {actionModal.action === 'Approve' ? 'Approve Purchase Requisition' : 'Reject Purchase Requisition'}
            </h3>
            <p className="text-xs text-slate-500">
              {actionModal.action === 'Approve'
                ? 'Approving will make this requisition eligible for quotation solicitation and purchase order generation.'
                : 'Rejecting will close this requisition. Please provide the reason below for the requester.'}
            </p>

            <div>
              <label className="block text-xs font-medium text-slate-700 mb-1">
                {actionModal.action === 'Approve' ? 'Approval Remarks (Optional)' : 'Rejection Reason (Mandatory)'}
              </label>
              <textarea
                rows={3}
                value={actionModal.remarks}
                onChange={(e) => setActionModal({ ...actionModal, remarks: e.target.value })}
                placeholder={
                  actionModal.action === 'Approve'
                    ? 'e.g. Budget verified against production schedule'
                    : 'e.g. Inventory already available in Warehouse 2'
                }
                className="w-full bg-slate-50 border border-slate-200 rounded-lg p-2 text-xs"
              />
            </div>

            <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setActionModal({ show: false, prId: 0, action: 'Approve', remarks: '' })}
                className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-medium text-slate-600"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleExecuteApproval}
                className={`px-3 py-1.5 rounded-lg text-xs font-semibold text-white ${
                  actionModal.action === 'Approve' ? 'bg-emerald-600 hover:bg-emerald-700' : 'bg-rose-600 hover:bg-rose-700'
                }`}
              >
                Confirm {actionModal.action}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

