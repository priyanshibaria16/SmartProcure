# Functional Requirements Document (FRD)
## Project: SmartProcure Enterprise ERP

---

### Module 1: Authentication & User Administration

#### FR-AUTH-001: User Login & Session Creation
- **Actor:** All registered system users.
- **Preconditions:** User account exists and is marked `IsActive = true`.
- **Main Flow:**
  1. User enters Email and Password on login screen.
  2. Frontend sends `POST /api/auth/login`.
  3. Backend verifies password hash.
  4. Backend generates JWT token containing User ID, Email, Full Name, Department ID, and Role claim.
  5. Frontend stores token securely and redirects user to Dashboard matching role permissions.
- **Alternative Flow:**
  - Inactive account or bad credentials returns HTTP 401 with message "Invalid credentials or account deactivated".
- **Validation:** Email format must be valid; password length $\ge 6$.

#### FR-AUTH-002: Role-Based Authorization
- **Actor:** System Security Middleware.
- **Preconditions:** Incoming request includes valid `Bearer <token>`.
- **Main Flow:** Token claims are inspected against `[Authorize(Roles = "...")]` attributes.
- **Alternative Flow:** User with insufficient role receives HTTP 403 Forbidden with clear permission error.

---

### Module 2: Master Data Management

#### FR-MST-001: Vendor Onboarding & Maintenance
- **Actor:** Purchase Manager, Administrator.
- **Preconditions:** User has `Administrator` or `PurchaseManager` role.
- **Fields:** VendorCode, CompanyName, ContactPerson, Email, Phone, Address, City, State, Country, GSTIN, PAN, PaymentTerms, BankAccountRef, VendorCategory, VendorRating, Status.
- **Validation:**
  - VendorCode must be unique across the tenant.
  - GSTIN must match Indian 15-digit alphanumeric standard (`^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$`).
  - PAN must match 10-digit standard (`^[A-Z]{5}[0-9]{4}[A-Z]{1}$`).
- **Business Rule:** Inactive vendors are blocked from being selected in new Purchase Orders or Quotations.

#### FR-MST-002: Material Master Governance
- **Actor:** Purchase Manager, Warehouse Manager, Administrator.
- **Fields:** MaterialCode, MaterialName, Description, CategoryId, MaterialType, BaseUOMId, UnitPrice, ReorderLevel, MaximumStockLevel, CurrentStock, PreferredVendorId, StorageLocation, Status.
- **Validation:**
  - MaterialCode unique.
  - UnitPrice $\ge 0$.
  - ReorderLevel $\ge 0$.
  - CurrentStock is calculated exclusively from confirmed warehouse inventory transactions.

---

### Module 3: Purchase Requisitions (PR)

#### FR-PR-001: Requisition Creation & Draft Saving
- **Actor:** Requisition Creator, Department Heads.
- **Preconditions:** User authenticated as `RequisitionCreator` or higher.
- **Main Flow:**
  1. User initiates new PR. System assigns temporary draft number.
  2. User selects Department, Required Delivery Date, Priority (Low, Medium, High, Urgent), and Business Justification.
  3. User adds line items: Material selection, Quantity, Estimated Unit Price. System computes Line Total and Total Estimated Amount.
  4. User clicks "Save as Draft" (`POST /api/requisitions`) or "Submit" (`POST /api/requisitions/{id}/submit`).
- **Validation:**
  - Required Delivery Date cannot be prior to the Requisition Date.
  - Quantity must be $> 0$.
  - At least one valid material line item is required.

#### FR-PR-002: Multi-Tier Approval Workflow
- **Actor:** Department Manager, Purchase Manager, Senior Executive.
- **Rules:**
  - Requisition Total $\le$ ₹25,000 $\implies$ Tier 1 Approval.
  - ₹25,001 to ₹1,00,000 $\implies$ Tier 2 Approval (Purchase Manager).
  - $>$ ₹1,00,000 $\implies$ Tier 3 Approval (Senior Management / Plant Head).
- **Actions:**
  - **Approve:** Moves status to `Approved`. Records approver ID, timestamp, and optional approval remarks in `ApprovalHistory`.
  - **Reject:** Sets status to `Rejected`. Mandatory rejection remarks required.
  - **Request Changes:** Sets status back to `Draft` / `ChangesRequested` with feedback note.

---

### Module 4: Vendor Quotation & Quotation Comparison

#### FR-QT-001: RFQ Logging & Quotation Entry
- **Actor:** Purchase Manager.
- **Preconditions:** Requisition is in `Approved` status.
- **Flow:** Purchase Manager solicits rates from multiple qualified vendors, logging: QuotedUnitPrice, TaxPercentage, DeliveryDays, PaymentTerms, ValidityDate.

#### FR-QT-002: Comparative Decision Matrix
- **Actor:** Purchase Manager.
- **Flow:** Side-by-side comparison screen displaying Vendor Rating, Quoted Price, Landed Cost (including taxes), Lead Time, and Terms. Manager selects chosen vendor and writes justification remarks.
- **Outcome:** Selected quotation locks the vendor and pricing baseline for PO generation.

---

### Module 5: Purchase Order Management

#### FR-PO-001: Purchase Order Generation
- **Actor:** Purchase Manager.
- **Preconditions:** Approved Purchase Requisition exists.
- **Main Flow:**
  1. Manager selects approved Requisition and Vendor.
  2. System pre-fills materials, quantities, agreed unit prices from quotation.
  3. Manager specifies PO Date, Expected Delivery Date, Shipping and Billing Warehouses/Addresses.
  4. Backend verifies pricing, computes line subtotals, tax amount, discounts, and grand total.
  5. Purchase Order created with status `Approved` or `SentToVendor`.
  6. Requisition status updated to `ConvertedToPO` to prevent duplicate ordering.

#### FR-PO-002: PO PDF Export & Print
- **Actor:** Purchase Manager, Finance, Viewer.
- **Flow:** User clicks "Download PO PDF". Server renders official corporate Purchase Order with company letterhead (Apex Manufacturing), Vendor GSTIN/PAN, line item details, terms & conditions, authorized signatory box.

---

### Module 6: Goods Receipt Note (GRN) & Warehouse Receiving

#### FR-GRN-001: Receipt Recording & Inspection
- **Actor:** Warehouse Manager.
- **Preconditions:** Purchase Order is in `Approved`, `SentToVendor`, or `PartiallyReceived` status.
- **Flow:**
  1. Warehouse clerk selects PO. System presents open order lines showing Ordered Qty, Previously Received Qty, and Outstanding Balance.
  2. Clerk enters Delivery Challan No., Received Date, Warehouse/Storage Location.
  3. Clerk inputs Current Received Quantity.
  4. Inspection inputs: Accepted Quantity and Rejected Quantity (with rejection reason: damaged, out of spec, incorrect grade).
  5. System validates:
     $$\text{Accepted} + \text{Rejected} = \text{Current Received}$$
     $$\text{Current Received} \le \text{Ordered Quantity} - \text{Previously Received Quantity}$$
- **Outcome:**
  - Creates GoodsReceipt record.
  - Updates PO status (`PartiallyReceived` or `FullyReceived`).
  - Increments Warehouse `InventoryBalance` by Accepted Quantity only.
  - Writes immutable entry to `InventoryTransactions` table.
  - Entire operation executed within a single database transaction.

---

### Module 7: Supplier Invoice Verification & Three-Way Matching

#### FR-INV-001: Invoice Registration & 3-Way Reconciliation
- **Actor:** Finance Manager.
- **Preconditions:** PO exists and has at least one associated Goods Receipt.
- **Flow:**
  1. Finance enters Vendor Invoice Number, Invoice Date, Due Date, Billed Quantities, and Unit Prices.
  2. System triggers Three-Way Matching Engine:
     - Check 1: Invoice Unit Price vs PO Unit Price.
     - Check 2: Invoice Billed Quantity vs GRN Accepted Quantity.
     - Check 3: Total Billed Amount vs Landed GRN Value.
  3. If variances are within tolerance ($\le 2\%$ or ₹100), invoice is marked `Matched`.
  4. If variances exceed tolerance, invoice is flagged as `Mismatched` with specific variance reasons (e.g., "Billed Qty exceeds GRN accepted qty by 10 units", "Rate ₹550 exceeds PO rate ₹500").
  5. Finance Manager can either `Approve` (with variance override explanation) or `Reject` (returns invoice to vendor).

---

### Module 8: Payment Recording

#### FR-PAY-001: Accounts Payable Settlement
- **Actor:** Finance Manager.
- **Preconditions:** Invoice is in `Approved` or `PartiallyPaid` status.
- **Flow:**
  1. Finance views outstanding balance: $\text{Outstanding} = \text{Invoice Total} - \sum \text{Previous Payments}$.
  2. Enters Payment Amount, Payment Date, Payment Method (NEFT, RTGS, IMPS, Cheque), and Transaction Reference Number.
  3. System validates: $\text{Payment Amount} \le \text{Outstanding Amount}$.
  4. Updates invoice status to `PartiallyPaid` or `Paid`.
  5. Records payment in `Payments` ledger.

---

### Module 9: Reporting & Analytics

#### FR-REP-001: Executive Dashboard & Operational Reports
- **Actor:** All roles (scoped by permission).
- **Features:**
  - Live KPI metrics (Spend, Open Orders, Pending Approvals, Low Stock Count).
  - Spend breakdown by department and vendor.
  - Interactive low-stock warnings with one-click PR reorder generation.
  - CSV / Excel export for all operational reports (Procurement, GRN, Invoices, Payments).
  - Power BI DirectQuery / SQL views for business intelligence consumption.
