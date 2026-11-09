import React, { useState, useEffect } from 'react';
import {
  PackageSearch,
  Search,
  Filter,
  Plus,
  RefreshCw,
  AlertTriangle,
  CheckCircle,
  XCircle,
  Boxes,
  Layers,
  Edit2,
  Eye,
  Warehouse
} from 'lucide-react';
import { masterApi } from '../services/api';
import type { Material, Category, Uom, Vendor } from '../types';
import { useAuth } from '../context/AuthContext';

export const MaterialsPage: React.FC = () => {
  const { user, isPurchasing, isAdmin } = useAuth();

  const [materials, setMaterials] = useState<Material[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [uoms, setUoms] = useState<Uom[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [categoryFilter, setCategoryFilter] = useState<string>('All');
  const [lowStockFilter, setLowStockFilter] = useState<boolean>(false);

  // Modals
  const [showAddModal, setShowAddModal] = useState<boolean>(false);
  const [showDetailModal, setShowDetailModal] = useState<boolean>(false);
  const [selectedMaterial, setSelectedMaterial] = useState<Material | null>(null);
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [actionLoading, setActionLoading] = useState<boolean>(false);

  // Form state
  const [formData, setFormData] = useState<Partial<Material>>({
    materialCode: '',
    materialName: '',
    description: '',
    categoryId: 1,
    materialType: 'ROH',
    baseUOMId: 1,
    unitPrice: 100,
    reorderLevel: 50,
    maximumStockLevel: 500,
    storageLocation: 'SLOC-PUN-01',
    status: true
  });

  useEffect(() => {
    fetchMaterials();
    fetchLookups();
  }, [categoryFilter, lowStockFilter]);

  const fetchMaterials = async () => {
    try {
      setLoading(true);
      const params: any = { pageSize: 100 };
      if (categoryFilter !== 'All') params.categoryId = parseInt(categoryFilter);
      if (lowStockFilter) params.lowStockOnly = true;
      const res = await masterApi.getMaterials(params);
      if (res.data?.success) {
        setMaterials(res.data.data.items);
      }
    } catch (err) {
      console.error('Failed to load materials:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchLookups = async () => {
    try {
      const [catRes, uomRes, venRes] = await Promise.all([
        masterApi.getCategories(),
        masterApi.getUOMs(),
        masterApi.getVendors({ pageSize: 100 })
      ]);
      if (catRes.data?.success) setCategories(catRes.data.data);
      if (uomRes.data?.success) setUoms(uomRes.data.data);
      if (venRes.data?.success) setVendors(venRes.data.data.items);
    } catch (err) {
      console.error('Failed to load lookups:', err);
    }
  };

  const handleOpenAddModal = () => {
    setIsEditing(false);
    setFormData({
      materialCode: `MAT-RM-${Math.floor(100 + Math.random() * 900)}`,
      materialName: '',
      description: '',
      categoryId: categories.length > 0 ? categories[0].categoryId : 1,
      materialType: 'ROH',
      baseUOMId: uoms.length > 0 ? uoms[0].uomId : 1,
      unitPrice: 250,
      reorderLevel: 50,
      maximumStockLevel: 1000,
      preferredVendorId: vendors.length > 0 ? vendors[0].vendorId : undefined,
      storageLocation: 'SLOC-PUN-01',
      status: true
    });
    setShowAddModal(true);
  };

  const handleOpenEditModal = (mat: Material) => {
    setIsEditing(true);
    setSelectedMaterial(mat);
    setFormData({ ...mat });
    setShowAddModal(true);
  };

  const handleSaveMaterial = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setActionLoading(true);
      if (isEditing && selectedMaterial) {
        const res = await masterApi.updateMaterial(selectedMaterial.materialId, formData);
        if (res.data?.success) {
          setShowAddModal(false);
          fetchMaterials();
        }
      } else {
        const res = await masterApi.createMaterial(formData);
        if (res.data?.success) {
          setShowAddModal(false);
          fetchMaterials();
        }
      }
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to save material.');
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

  const filteredMaterials = materials.filter(
    (m) =>
      m.materialName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      m.materialCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
      m.categoryName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-6">
      {/* Top Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-5">
        <div>
          <div className="flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-lg bg-teal-600 flex items-center justify-center text-white shadow-md shadow-teal-500/20">
              <PackageSearch className="w-5 h-5" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-slate-900 tracking-tight">Material Master (MM01/MM03)</h1>
              <p className="text-xs text-slate-500">
                Direct/Indirect Materials, Reorder Thresholds, Preferred Vendors & Storage Locations
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2.5">
          <button
            onClick={fetchMaterials}
            className="flex items-center gap-1.5 px-3 py-2 border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium rounded-lg shadow-2xs transition-colors"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? 'animate-spin' : ''}`} />
            <span>Refresh</span>
          </button>

          {(isPurchasing || isAdmin) && (
            <button
              onClick={handleOpenAddModal}
              className="flex items-center gap-1.5 px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white text-xs font-semibold rounded-lg shadow-sm shadow-teal-600/30 transition-all"
            >
              <Plus className="w-4 h-4" />
              <span>Create Material</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Highlights */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-slate-500 font-medium">Total SKU Materials</div>
          <div className="text-2xl font-bold text-slate-900 mt-1">{materials.length}</div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-rose-600 font-medium">Low Stock Alerts</div>
          <div className="text-2xl font-bold text-rose-600 mt-1">
            {materials.filter((m) => m.currentStock <= m.reorderLevel).length}
          </div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-teal-600 font-medium">Active Categories</div>
          <div className="text-2xl font-bold text-teal-600 mt-1">{categories.length}</div>
        </div>
        <div className="bg-white p-4 rounded-xl border border-slate-200/80 shadow-2xs">
          <div className="text-xs text-slate-500 font-medium">Catalog Valuation</div>
          <div className="text-2xl font-bold text-slate-900 mt-1">
            {formatCurrency(materials.reduce((acc, m) => acc + m.currentStock * m.unitPrice, 0))}
          </div>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-2xs flex flex-col md:flex-row gap-4 items-center justify-between">
        <div className="relative w-full md:w-96">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search by material code, name or category..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full bg-slate-50 border border-slate-200 rounded-lg pl-9 pr-4 py-2 text-xs focus:ring-2 focus:ring-teal-500/20 focus:border-teal-500"
          />
        </div>

        <div className="flex items-center gap-3 w-full md:w-auto">
          <div className="flex items-center gap-2">
            <Filter className="w-3.5 h-3.5 text-slate-400" />
            <span className="text-xs text-slate-600 font-medium">Category:</span>
          </div>
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-xs text-slate-700 font-medium focus:outline-none"
          >
            <option value="All">All Categories</option>
            {categories.map((c) => (
              <option key={c.categoryId} value={c.categoryId.toString()}>
                {c.categoryName}
              </option>
            ))}
          </select>

          <button
            onClick={() => setLowStockFilter(!lowStockFilter)}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors flex items-center gap-1.5 ${
              lowStockFilter
                ? 'bg-rose-500 text-white border-rose-600'
                : 'bg-slate-50 text-slate-700 border-slate-200 hover:bg-slate-100'
            }`}
          >
            <AlertTriangle className="w-3.5 h-3.5" />
            <span>Low Stock Only</span>
          </button>
        </div>
      </div>

      {/* Materials Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-2xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold uppercase tracking-wider">
              <tr>
                <th className="px-5 py-3">Material Code</th>
                <th className="px-5 py-3">Material Name & Type</th>
                <th className="px-5 py-3">Category</th>
                <th className="px-5 py-3">Base UOM</th>
                <th className="px-5 py-3 text-right">Standard Price</th>
                <th className="px-5 py-3 text-right">Current Stock</th>
                <th className="px-5 py-3 text-right">Reorder Level</th>
                <th className="px-5 py-3">Stock Health</th>
                <th className="px-5 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 text-slate-700">
              {loading ? (
                <tr>
                  <td colSpan={9} className="px-5 py-10 text-center text-slate-400">
                    <RefreshCw className="w-5 h-5 animate-spin mx-auto mb-2 text-teal-500" />
                    Loading materials catalog...
                  </td>
                </tr>
              ) : filteredMaterials.length === 0 ? (
                <tr>
                  <td colSpan={9} className="px-5 py-12 text-center text-slate-400">
                    No materials found.
                  </td>
                </tr>
              ) : (
                filteredMaterials.map((mat) => {
                  const isLow = mat.currentStock <= mat.reorderLevel;
                  return (
                    <tr key={mat.materialId} className="hover:bg-slate-50/70">
                      <td className="px-5 py-3.5 font-mono font-semibold text-teal-700">
                        {mat.materialCode}
                      </td>
                      <td className="px-5 py-3.5">
                        <div className="font-semibold text-slate-900">{mat.materialName}</div>
                        <div className="text-[11px] text-slate-400">Type: {mat.materialType || 'ROH'}</div>
                      </td>
                      <td className="px-5 py-3.5">
                        <span className="px-2 py-0.5 rounded text-[11px] font-medium bg-slate-100 text-slate-700 border border-slate-200">
                          {mat.categoryName}
                        </span>
                      </td>
                      <td className="px-5 py-3.5 font-medium">{mat.baseUOMCode}</td>
                      <td className="px-5 py-3.5 text-right font-medium text-slate-900">
                        {formatCurrency(mat.unitPrice)}
                      </td>
                      <td className="px-5 py-3.5 text-right font-bold text-slate-900">
                        {mat.currentStock}
                      </td>
                      <td className="px-5 py-3.5 text-right text-slate-500 font-medium">
                        {mat.reorderLevel}
                      </td>
                      <td className="px-5 py-3.5">
                        {isLow ? (
                          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200">
                            <AlertTriangle className="w-3 h-3 text-rose-500" /> Low Stock
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-50 text-emerald-700 border border-emerald-200">
                            <CheckCircle className="w-3 h-3 text-emerald-500" /> Optimal
                          </span>
                        )}
                      </td>
                      <td className="px-5 py-3.5 text-right space-x-1">
                        <button
                          onClick={() => {
                            setSelectedMaterial(mat);
                            setShowDetailModal(true);
                          }}
                          className="p-1 text-slate-500 hover:text-slate-800 hover:bg-slate-100 rounded"
                          title="View Details"
                        >
                          <Eye className="w-3.5 h-3.5" />
                        </button>
                        {(isPurchasing || isAdmin) && (
                          <button
                            onClick={() => handleOpenEditModal(mat)}
                            className="p-1 text-slate-500 hover:text-slate-800 hover:bg-slate-100 rounded"
                            title="Edit Material"
                          >
                            <Edit2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Material Detail Modal */}
      {showDetailModal && selectedMaterial && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-lg p-6 space-y-5 animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between border-b border-slate-200 pb-3">
              <div>
                <span className="font-mono text-xs font-bold text-teal-600 bg-teal-50 px-2 py-0.5 rounded border border-teal-200">
                  {selectedMaterial.materialCode}
                </span>
                <h3 className="text-base font-bold text-slate-900 mt-1">{selectedMaterial.materialName}</h3>
                <p className="text-xs text-slate-500">Category: {selectedMaterial.categoryName}</p>
              </div>
              <button
                onClick={() => setShowDetailModal(false)}
                className="w-8 h-8 rounded-full hover:bg-slate-200 text-slate-500 flex items-center justify-center"
              >
                ✕
              </button>
            </div>

            <div className="grid grid-cols-2 gap-3 text-xs">
              <div className="bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-1.5">
                <div className="text-[11px] uppercase tracking-wider text-slate-400 font-bold">Standard Pricing</div>
                <div className="text-sm font-bold text-slate-900">{formatCurrency(selectedMaterial.unitPrice)}</div>
                <div className="text-slate-500">Base UOM: <strong>{selectedMaterial.baseUOMCode}</strong></div>
                <div className="text-slate-500">Type: <strong>{selectedMaterial.materialType}</strong></div>
              </div>

              <div className="bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-1.5">
                <div className="text-[11px] uppercase tracking-wider text-slate-400 font-bold">Stock Thresholds</div>
                <div className="text-sm font-bold text-slate-900">{selectedMaterial.currentStock} in stock</div>
                <div className="text-slate-500">Reorder Level: <strong>{selectedMaterial.reorderLevel}</strong></div>
                <div className="text-slate-500">Max Capacity: <strong>{selectedMaterial.maximumStockLevel}</strong></div>
              </div>
            </div>

            <div className="bg-slate-50 p-3 rounded-xl border border-slate-200 text-xs space-y-1">
              <div>Storage Location: <strong>{selectedMaterial.storageLocation}</strong></div>
              <div>Preferred Vendor: <strong>{selectedMaterial.preferredVendorName || 'Tata Steel Ltd'}</strong></div>
              <div>Description: <span className="text-slate-600">{selectedMaterial.description || 'Enterprise grade raw material.'}</span></div>
            </div>

            <div className="flex justify-end pt-2 border-t border-slate-200">
              <button
                onClick={() => setShowDetailModal(false)}
                className="px-4 py-2 border border-slate-200 bg-white text-slate-700 rounded-lg text-xs font-semibold hover:bg-slate-50"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add / Edit Material Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-2xl max-h-[90vh] flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-150">
            <div className="p-5 border-b border-slate-200 flex items-center justify-between bg-slate-50">
              <h3 className="text-base font-bold text-slate-900">
                {isEditing ? 'Edit Material Record' : 'Create New Material (MM01)'}
              </h3>
              <button
                onClick={() => setShowAddModal(false)}
                className="w-8 h-8 rounded-full hover:bg-slate-200 text-slate-500 flex items-center justify-center"
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSaveMaterial} className="p-6 space-y-4 flex-1 overflow-y-auto text-xs">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Material Code *</label>
                  <input
                    type="text"
                    required
                    value={formData.materialCode}
                    onChange={(e) => setFormData({ ...formData, materialCode: e.target.value })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 font-mono"
                  />
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Material Name *</label>
                  <input
                    type="text"
                    required
                    value={formData.materialName}
                    onChange={(e) => setFormData({ ...formData, materialName: e.target.value })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  />
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Category *</label>
                  <select
                    value={formData.categoryId}
                    onChange={(e) => setFormData({ ...formData, categoryId: parseInt(e.target.value) })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  >
                    {categories.map((c) => (
                      <option key={c.categoryId} value={c.categoryId}>
                        {c.categoryName}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Material Type *</label>
                  <select
                    value={formData.materialType}
                    onChange={(e) => setFormData({ ...formData, materialType: e.target.value })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  >
                    <option value="ROH">ROH (Raw Material)</option>
                    <option value="VERP">VERP (Packaging)</option>
                    <option value="HALB">HALB (Semi-Finished)</option>
                    <option value="FERT">FERT (Finished Product)</option>
                    <option value="HIBE">HIBE (Operating Supplies)</option>
                  </select>
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Base UOM *</label>
                  <select
                    value={formData.baseUOMId}
                    onChange={(e) => setFormData({ ...formData, baseUOMId: parseInt(e.target.value) })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  >
                    {uoms.map((u) => (
                      <option key={u.uomId} value={u.uomId}>
                        {u.uomCode} - {u.uomName}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Standard Unit Price (₹) *</label>
                  <input
                    type="number"
                    min="0"
                    step="any"
                    required
                    value={formData.unitPrice}
                    onChange={(e) => setFormData({ ...formData, unitPrice: parseFloat(e.target.value) || 0 })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Reorder Level *</label>
                  <input
                    type="number"
                    min="0"
                    required
                    value={formData.reorderLevel}
                    onChange={(e) => setFormData({ ...formData, reorderLevel: parseInt(e.target.value) || 0 })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  />
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Max Stock Level *</label>
                  <input
                    type="number"
                    min="0"
                    required
                    value={formData.maximumStockLevel}
                    onChange={(e) =>
                      setFormData({ ...formData, maximumStockLevel: parseInt(e.target.value) || 0 })
                    }
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Preferred Vendor</label>
                  <select
                    value={formData.preferredVendorId || ''}
                    onChange={(e) =>
                      setFormData({
                        ...formData,
                        preferredVendorId: e.target.value ? parseInt(e.target.value) : undefined
                      })
                    }
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  >
                    <option value="">-- None --</option>
                    {vendors.map((v) => (
                      <option key={v.vendorId} value={v.vendorId}>
                        {v.companyName} ({v.vendorCode})
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Storage Location</label>
                  <input
                    type="text"
                    value={formData.storageLocation}
                    onChange={(e) => setFormData({ ...formData, storageLocation: e.target.value })}
                    className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                  />
                </div>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Description</label>
                <textarea
                  rows={2}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full bg-slate-50 border border-slate-200 rounded-lg px-3 py-2"
                />
              </div>

              <div className="pt-3 border-t border-slate-200 flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setShowAddModal(false)}
                  className="px-4 py-2 border border-slate-200 bg-white text-slate-700 rounded-lg text-xs font-semibold hover:bg-slate-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={actionLoading}
                  className="px-5 py-2 bg-teal-600 hover:bg-teal-700 text-white rounded-lg text-xs font-semibold shadow-sm"
                >
                  {actionLoading ? 'Saving...' : 'Save Material'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

