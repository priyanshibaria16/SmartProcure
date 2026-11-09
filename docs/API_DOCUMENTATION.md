# SmartProcure REST API Specification

**Base URL:** `/api`  
**Authentication:** HTTP Bearer JSON Web Token (JWT) in `Authorization: Bearer <token>`  
**Content-Type:** `application/json`  

---

### 1. Authentication Endpoints

| Method | Endpoint | Description | Role Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Authenticate user with Email & Password | Public |
| `GET` | `/api/auth/me` | Fetch authenticated user profile & claims | Any authenticated user |
| `POST` | `/api/auth/change-password` | Update current user password | Any authenticated user |
| `GET` | `/api/users` | List users with pagination and search | Administrator |
| `POST` | `/api/users` | Create new system user | Administrator |
| `PUT` | `/api/users/{id}` | Update user details and role | Administrator |
| `PATCH` | `/api/users/{id}/status` | Activate / Deactivate user | Administrator |

#### Sample Request: `POST /api/auth/login`
```json
{
  "email": "purchasemanager@smartprocure.com",
  "password": "Purchase@123"
}
```

#### Sample Response: HTTP 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-11-10T07:30:00Z",
  "user": {
    "userId": 3,
    "email": "purchasemanager@smartprocure.com",
    "fullName": "Vikram Malhotra",
    "role": "PurchaseManager",
    "department": "Procurement"
  }
}
```

---

### 2. Master Data Endpoints

#### Vendors
- `GET /api/vendors` — List vendors (search, status filter, pagination).
- `GET /api/vendors/{id}` — Get vendor details with metrics (orders, spend, rating).
- `POST /api/vendors` — Create vendor (validates GSTIN, PAN, unique vendor code).
- `PUT /api/vendors/{id}` — Update vendor profile.
- `PATCH /api/vendors/{id}/status` — Toggle Active/Inactive state.

#### Materials
- `GET /api/materials` — List materials (search, category filter, low-stock filter).
- `GET /api/materials/{id}` — Material details with inventory levels across warehouses.
- `POST /api/materials` — Create material master.
- `PUT /api/materials/{id}` — Update material.

#### Departments & Warehouses
- `GET /api/departments` — List departments with budget and manager details.
- `GET /api/warehouses` — List storage locations and warehouses.
- `GET /api/categories` — List material categories.
- `GET /api/uom` — List units of measure.

---

### 3. Purchase Requisitions Endpoints

| Method | Endpoint | Description | Role Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/requisitions` | List requisitions (filter by status, dept, requester) | All authorized roles |
| `GET` | `/api/requisitions/{id}` | Detailed PR with line items & approval history | All authorized roles |
| `POST` | `/api/requisitions` | Create new PR (Draft or Submitted) | RequisitionCreator, Admin |
| `PUT` | `/api/requisitions/{id}` | Edit draft requisition | RequisitionCreator, Admin |
| `POST` | `/api/requisitions/{id}/submit` | Submit draft PR for approval | RequisitionCreator, Admin |
| `POST` | `/api/requisitions/{id}/approve` | Approve requisition with remarks | PurchaseManager, Admin |
| `POST` | `/api/requisitions/{id}/reject` | Reject requisition with mandatory remarks | PurchaseManager, Admin |
| `POST` | `/api/requisitions/{id}/cancel` | Cancel requisition | Requester, Admin |

---

### 4. Vendor Quotation Endpoints

- `GET /api/quotations?requisitionId={id}` — List quotations for a requisition.
- `POST /api/quotations` — Enter vendor quotation.
- `POST /api/quotations/{id}/select` — Select quotation with business justification.
- `GET /api/quotations/compare/{requisitionId}` — Return comparison matrix data for decision making.

---

### 5. Purchase Order Endpoints

| Method | Endpoint | Description | Role Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/purchase-orders` | List POs with status and vendor filtering | PurchaseManager, Finance, Admin, Warehouse |
| `GET` | `/api/purchase-orders/{id}` | PO details with items, GRN status, invoice status | Authorized roles |
| `POST` | `/api/purchase-orders` | Generate PO from approved requisition | PurchaseManager, Admin |
| `PUT` | `/api/purchase-orders/{id}` | Amend PO (if eligible) | PurchaseManager, Admin |
| `POST` | `/api/purchase-orders/{id}/send` | Mark PO as Sent to Vendor | PurchaseManager, Admin |
| `POST` | `/api/purchase-orders/{id}/cancel` | Cancel PO | PurchaseManager, Admin |
| `GET` | `/api/purchase-orders/{id}/pdf` | Generate printable enterprise PDF document | Authorized roles |

---

### 6. Goods Receipt Note (GRN) Endpoints

| Method | Endpoint | Description | Role Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/goods-receipts` | List GRNs with PO and warehouse filtering | WarehouseManager, Admin, Finance |
| `GET` | `/api/goods-receipts/{id}` | View GRN details, inspection, items | Authorized roles |
| `POST` | `/api/goods-receipts` | Record receipt, inspect, update stock in 1 atomic transaction | WarehouseManager, Admin |
| `GET` | `/api/goods-receipts/pending-pos` | Fetch open POs eligible for warehouse receiving | WarehouseManager, Admin |

---

### 7. Inventory Endpoints

- `GET /api/inventory` — Warehouse stock balances by material.
- `GET /api/inventory/low-stock` — List items at or below reorder level.
- `GET /api/inventory/transactions` — Perpetual inventory movement ledger.
- `POST /api/inventory/adjustment` — Record authorized physical stock adjustment.

---

### 8. Invoice Verification & Accounts Payable Endpoints

| Method | Endpoint | Description | Role Access |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/invoices` | List invoices (status, vendor, mismatch filter) | FinanceManager, Admin |
| `GET` | `/api/invoices/{id}` | View invoice, 3-way match breakdown, payment history | FinanceManager, Admin |
| `POST` | `/api/invoices` | Register supplier invoice | FinanceManager, Admin |
| `POST` | `/api/invoices/{id}/verify` | Execute 3-way match (PO vs GRN vs Invoice) | FinanceManager, Admin |
| `POST` | `/api/invoices/{id}/approve` | Approve invoice for payment settlement | FinanceManager, Admin |
| `POST` | `/api/invoices/{id}/reject` | Reject invoice with remarks | FinanceManager, Admin |

---

### 9. Payment Recording Endpoints

- `GET /api/payments` — Payment ledger with invoice and vendor details.
- `POST /api/payments` — Record payment against approved invoice (validates outstanding balance).

---

### 10. Dashboard & Analytics Endpoints

- `GET /api/dashboard/kpi` — Real-time KPI summary (Total Spend, Open POs, Low Stock, Pending Approvals, Pending Invoices, Outstanding Amount).
- `GET /api/dashboard/charts` — Trend datasets (Monthly Spending, PO Status Breakdown, Spending by Department, Top 10 Vendors, Low Stock Items).
- `GET /api/reports/procurement?format=json|csv` — Procurement spending report.
- `GET /api/reports/inventory?format=json|csv` — Stock valuation and inventory report.
- `GET /api/reports/vendor-performance?format=json|csv` — Vendor fulfillment & quality report.
- `GET /api/reports/outstanding-payments?format=json|csv` — Accounts payable aging & liabilities report.
- `GET /api/audit-logs` — Audit trail records.
- `GET /api/notifications` — In-app alerts for logged-in user.
- `PATCH /api/notifications/{id}/read` — Mark notification read.
