# User Acceptance Testing (UAT) Test Cases & Traceability Matrix

**Project:** SmartProcure — SAP MM Inspired ERP System  
**Organization:** Apex Manufacturing Industries Pvt. Ltd.  
**Author:** Lead QA Engineer & SAP MM Solution Architect  
**Total Test Cases:** 25 Enterprise Scenarios  

---

### Requirement Traceability Matrix (RTM)

| Req ID | Module | Scenario Description | Test Case ID | Severity |
| :--- | :--- | :--- | :--- | :--- |
| **FR-AUTH-001** | Auth | Valid User Login & JWT Issuance | `TC-SEC-01` | Critical |
| **FR-AUTH-001** | Auth | Invalid Credential Handling | `TC-SEC-02` | High |
| **FR-AUTH-002** | RBAC | Role-Based Access Enforcement | `TC-SEC-03` | Critical |
| **FR-MST-001** | Master Data | New Vendor Onboarding | `TC-MST-01` | High |
| **FR-MST-001** | Master Data | Duplicate Vendor Code Rejection | `TC-MST-02` | High |
| **FR-MST-002** | Master Data | Material Master Creation | `TC-MST-03` | High |
| **FR-MST-002** | Master Data | Negative / Invalid Price Rejection | `TC-MST-04` | Medium |
| **FR-PR-001** | Requisition | Draft PR Creation & Calculation | `TC-PR-01` | High |
| **FR-PR-001** | Requisition | PR Submission with Delivery Date Check | `TC-PR-02` | High |
| **FR-PR-002** | Approval | Multi-Tier Approval Matrix Execution | `TC-PR-03` | Critical |
| **FR-PR-002** | Approval | Rejection Workflow with Mandatory Remarks | `TC-PR-04` | High |
| **FR-PO-001** | Purchase Order | PO Generation from Approved Requisition | `TC-PO-01` | Critical |
| **FR-PO-001** | Purchase Order | Prevention of Duplicate PO Conversion | `TC-PO-02` | High |
| **FR-GRN-001** | Warehouse GRN | Partial Goods Receipt Recording | `TC-GRN-01` | High |
| **FR-GRN-001** | Warehouse GRN | Complete Goods Receipt & PO Closure | `TC-GRN-02` | High |
| **FR-GRN-001** | Warehouse GRN | Over-Receipt Prevention (Qty Ceiling) | `TC-GRN-03` | Critical |
| **FR-GRN-001** | Quality Insp | Rejected Goods Exclusion from Stock | `TC-GRN-04` | Critical |
| **FR-INV-001** | Inventory | Real-Time Atomic Stock Balance Updates | `TC-INV-01` | Critical |
| **FR-INV-002** | Inventory | Low-Stock Automated Reorder Trigger | `TC-INV-02` | High |
| **FR-AP-001** | Accounts Payable | Three-Way Matching (Perfect Match) | `TC-FIN-01` | Critical |
| **FR-AP-001** | Accounts Payable | Three-Way Matching Mismatch Detection | `TC-FIN-02` | Critical |
| **FR-AP-002** | Payments | Partial Payment Ledger Settlement | `TC-FIN-03` | High |
| **FR-AP-002** | Payments | Overpayment Prevention Guardrail | `TC-FIN-04` | Critical |
| **FR-AUD-001** | Audit Trail | System-Wide Audit Log Generation | `TC-AUD-01` | Medium |
| **FR-REP-001** | Analytics | Operational Report Filtering & CSV Export | `TC-REP-01` | Medium |

---

### Detailed Test Scenarios

#### 1. TC-SEC-01: Valid User Login
- **Preconditions:** User `admin@smartprocure.com` active in database.
- **Steps:** Submit valid credentials to `/api/auth/login`.
- **Expected Result:** HTTP 200 OK, valid JWT token returned containing claims `Role: Administrator`.
- **Status:** PASS

#### 2. TC-SEC-02: Invalid Login Attempts
- **Preconditions:** None.
- **Steps:** Submit wrong password `Wrong@123` to `/api/auth/login`.
- **Expected Result:** HTTP 401 Unauthorized, generic error message without leaking sensitive stack traces.
- **Status:** PASS

#### 3. TC-SEC-03: Unauthorized User Access
- **Preconditions:** Requisition creator token (`requester@smartprocure.com`).
- **Steps:** Call payment recording endpoint `POST /api/payments`.
- **Expected Result:** HTTP 403 Forbidden.
- **Status:** PASS

#### 4. TC-MST-01: Vendor Creation
- **Preconditions:** Authenticated as Purchase Manager.
- **Steps:** Post new vendor payload with valid GSTIN and PAN.
- **Expected Result:** HTTP 201 Created, record persisted with status `Active`.
- **Status:** PASS

#### 5. TC-MST-02: Duplicate Vendor Prevention
- **Preconditions:** Vendor with code `VND-001` exists.
- **Steps:** Attempt creating another vendor with code `VND-001`.
- **Expected Result:** HTTP 400 Bad Request: "Vendor code already exists".
- **Status:** PASS

#### 6. TC-MST-03: Material Creation
- **Preconditions:** Valid UOM and Category exist.
- **Steps:** Create material `Steel Sheet 2mm (HR)` with unit price ₹4,500.
- **Expected Result:** HTTP 201 Created with initial calculated stock 0.
- **Status:** PASS

#### 7. TC-MST-04: Invalid Material Price / Quantity
- **Preconditions:** None.
- **Steps:** Post material with `UnitPrice: -500`.
- **Expected Result:** HTTP 400 Validation Error: "Unit price cannot be negative".
- **Status:** PASS

#### 8. TC-PR-01: Purchase Requisition Creation
- **Preconditions:** User logged in as Requester.
- **Steps:** Add 2 materials with quantities 50 and 20.
- **Expected Result:** Total estimated amount accurately computed on backend; saved in `Draft` state.
- **Status:** PASS

#### 9. TC-PR-02: Purchase Requisition Submission
- **Preconditions:** Draft PR exists with delivery date in the future.
- **Steps:** Call `/api/requisitions/{id}/submit`.
- **Expected Result:** Status moves to `PendingApproval`.
- **Status:** PASS

#### 10. TC-PR-03: Multi-Tier Approval Workflow
- **Preconditions:** PR > ₹100,000 submitted.
- **Steps:** Purchase Manager reviews and approves.
- **Expected Result:** Status updated to `Approved`, `ApprovalHistory` logged with approver ID and timestamp.
- **Status:** PASS

#### 11. TC-PR-04: Rejected Requisition Handling
- **Preconditions:** PR submitted.
- **Steps:** Approver rejects with remarks "Excessive stock already present in Warehouse 2".
- **Expected Result:** Status becomes `Rejected`, remarks saved, cannot be converted to PO.
- **Status:** PASS

#### 12. TC-PO-01: Purchase Order Generation
- **Preconditions:** Approved Requisition exists.
- **Steps:** Convert PR to Purchase Order specifying Vendor.
- **Expected Result:** New PO created (`PO-2026-XXXX`), PR status marked `ConvertedToPO`. Subtotals and GST calculated.
- **Status:** PASS

#### 13. TC-PO-02: Duplicate PO Prevention
- **Preconditions:** PR already marked `ConvertedToPO`.
- **Steps:** Attempt to convert the same PR again.
- **Expected Result:** HTTP 400 Bad Request: "Requisition has already been converted to a Purchase Order".
- **Status:** PASS

#### 14. TC-GRN-01: Partial Goods Receipt
- **Preconditions:** PO has 100 units ordered.
- **Steps:** Warehouse enters GRN with received quantity 40.
- **Expected Result:** PO status updated to `PartiallyReceived`. Outstanding balance 60.
- **Status:** PASS

#### 15. TC-GRN-02: Complete Goods Receipt
- **Preconditions:** PO has 60 units remaining.
- **Steps:** Warehouse enters GRN for remaining 60 units.
- **Expected Result:** PO status becomes `FullyReceived`.
- **Status:** PASS

#### 16. TC-GRN-03: Over-Receipt Prevention
- **Preconditions:** PO has 10 units remaining.
- **Steps:** Warehouse clerk attempts GRN with received quantity 25.
- **Expected Result:** HTTP 400 Bad Request: "Received quantity exceeds remaining open PO balance".
- **Status:** PASS

#### 17. TC-GRN-04: Rejected Goods Exclusion from Inventory
- **Preconditions:** Shipment of 50 units arrives.
- **Steps:** Inspector accepts 40 units and rejects 10 damaged units.
- **Expected Result:** GRN records 50 received, 40 accepted, 10 rejected. Only 40 units added to warehouse stock balance.
- **Status:** PASS

#### 18. TC-INV-01: Inventory Stock Updates
- **Preconditions:** Initial stock of Bearing is 15.
- **Steps:** Post GRN with 25 accepted units.
- **Expected Result:** `InventoryBalances` updated to 40; transaction record written with type `GoodsReceipt`.
- **Status:** PASS

#### 19. TC-INV-02: Low-Stock Alerts
- **Preconditions:** Material reorder level is 20. Current stock drops to 12.
- **Steps:** Query `/api/inventory/low-stock`.
- **Expected Result:** Material appears in low stock list; notification triggered.
- **Status:** PASS

#### 20. TC-FIN-01: Invoice Three-Way Matching (Perfect Match)
- **Preconditions:** PO rate ₹500, GRN accepted 100 units. Invoice billed 100 units @ ₹500.
- **Steps:** Execute `/api/invoices/{id}/verify`.
- **Expected Result:** Match status: `Matched`. Price variance 0%, quantity variance 0.
- **Status:** PASS

#### 21. TC-FIN-02: Invoice Mismatch Detection
- **Preconditions:** PO rate ₹500, GRN accepted 90 units. Vendor bills 100 units @ ₹550.
- **Steps:** Run verification engine.
- **Expected Result:** Match status: `Mismatched`. Detailed error breakdown: "Quantity variance: billed 100 vs accepted 90; Rate variance: ₹550 vs PO rate ₹500".
- **Status:** PASS

#### 22. TC-FIN-03: Partial Payments
- **Preconditions:** Invoice total is ₹50,000.
- **Steps:** Post payment of ₹20,000.
- **Expected Result:** Invoice status becomes `PartiallyPaid`, outstanding balance ₹30,000.
- **Status:** PASS

#### 23. TC-FIN-04: Overpayment Prevention
- **Preconditions:** Outstanding invoice balance is ₹10,000.
- **Steps:** Attempt to record payment of ₹15,000.
- **Expected Result:** HTTP 400 Bad Request: "Payment amount ₹15,000 exceeds outstanding invoice balance of ₹10,000".
- **Status:** PASS

#### 24. TC-AUD-01: Audit Log Creation
- **Preconditions:** Any critical state change (e.g., GRN posted, Invoice approved).
- **Steps:** Query `AuditLogs` table.
- **Expected Result:** Contains User ID, Action, Entity name, Timestamp, and previous/new values.
- **Status:** PASS

#### 25. TC-REP-01: Report Filtering & CSV Export
- **Preconditions:** Historical procurement data exists.
- **Steps:** Request `/api/reports/procurement?format=csv`.
- **Expected Result:** HTTP 200 with `text/csv` attachment header and valid tabular CSV output.
- **Status:** PASS
