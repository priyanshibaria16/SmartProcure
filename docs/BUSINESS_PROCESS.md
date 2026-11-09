# Business Process Documentation: Procure-to-Pay (P2P) Lifecycle

**Project:** SmartProcure — SAP MM Inspired ERP System  
**Prepared By:** SAP MM Functional Consultant & Solution Architect  
**Target Enterprise:** Apex Manufacturing Industries Pvt. Ltd.  

---

### 1. SAP MM Process Mapping

In SAP ERP (ECC 6.0 and S/4HANA), the Procure-to-Pay (P2P) cycle is executed through a series of standard transaction codes (T-Codes), movement types, and organizational master records. SmartProcure mirrors these exact business logic flows in an intuitive web-based interface:

| SAP MM T-Code | SAP MM Process Description | SmartProcure Equivalent Feature | Module / Endpoint |
| :--- | :--- | :--- | :--- |
| **MM01 / MM02 / MM03** | Material Master Creation & Maintenance | Material Master Management (with UOM, Categories, Safety Stock, Reorder Point) | Module 4 (`/api/materials`) |
| **XK01 / BP** | Vendor Master / Business Partner Maintenance | Vendor Master Management (with GSTIN, PAN, Terms, Bank Ref) | Module 3 (`/api/vendors`) |
| **ME51N / ME52N / ME53N** | Create, Change, Display Purchase Requisition | Purchase Requisition Management (Line items, Priorities, Delivery dates) | Module 6 (`/api/requisitions`) |
| **ME28 / ME54N** | Release Purchase Requisition / Release Strategy | Multi-Tier Approval Workflow Engine (Tier 1, Tier 2, Tier 3 limits) | Module 7 (`/api/requisitions/{id}/approve`) |
| **ME41 / ME47 / ME49** | Request for Quotation & Price Comparison | Vendor Quotation Comparison Matrix & Selection Justification | Module 8 (`/api/quotations`) |
| **ME21N / ME22N / ME23N** | Create, Change, Display Purchase Order | Purchase Order Generation & PO PDF Dispatch | Module 9 (`/api/purchase-orders`) |
| **MIGO (Movement 101)** | Goods Receipt for Purchase Order into Warehouse | Goods Receipt Note (GRN) with Acceptance & Rejection Inspection | Module 10 (`/api/goods-receipts`) |
| **MB52 / MMBE** | Display Warehouse Stocks of Material | Perpetual Stock Ledger, Warehouse Balances & Low-Stock Alerts | Module 11 (`/api/inventory`) |
| **MIRO** | Enter Incoming Invoice (Three-Way Verification) | Supplier Invoice Verification (3-Way Match: PO vs GRN vs Invoice) | Module 12 (`/api/invoices/verify`) |
| **F-53 / F-58** | Post Outgoing Payment (Accounts Payable) | Payment Recording & Outstanding Liability Reconciliation | Module 13 (`/api/payments`) |
| **MC.9 / ME80FN** | Purchasing Information System & Analysis | Executive Procurement Dashboard, KPI Cards & Power BI Datasets | Module 14 (`/api/reports`) |

---

### 2. As-Is vs. To-Be Process Comparison

#### 2.1 The "As-Is" Manual Process
```mermaid
flowchart TD
    A1["Shop Floor Clerk identifies stock shortage"] --> B1["Manual paper purchase requisition drafted"]
    B1 --> C1["Physical paper routed for signatures across desks (3-7 days delay)"]
    C1 --> D1["Purchase officer calls 2 local vendors over phone for rates"]
    D1 --> E1["Purchase Order created manually in Excel and emailed"]
    E1 --> F1["Truck arrives at dock; gatekeeper writes receipt in paper register"]
    F1 --> G1["Quality check done informally; stock manually updated at month-end"]
    G1 --> H1["Vendor paper invoice sent directly to Finance"]
    H1 --> I1["Finance manually hunts for PO and gate register to verify invoice"]
    I1 --> J1["Frequent over-invoicing and duplicate payments discovered during annual audit"]
```

#### 2.2 The "To-Be" SmartProcure Digital ERP Process
```mermaid
flowchart TD
    A2["Automatic Low-Stock Reorder Alert or User PR Creation"] --> B2["Digital Requisition submitted in SmartProcure"]
    B2 --> C2{"Automated Approval Matrix Routing<br/>Tier 1: <= 25K<br/>Tier 2: <= 100K<br/>Tier 3: > 100K"}
    C2 -->|Approved| D2["Quotation Comparison & Preferred Vendor Selection"]
    C2 -->|Rejected / Clarification| B2
    D2 --> E2["Purchase Order Auto-Generated with Pricing & Taxes (PO PDF Created)"]
    E2 --> F2["Warehouse receives goods; inputs GRN against PO"]
    F2 --> G2["Inspection records Accepted vs. Rejected quantities"]
    G2 --> H2["Atomic Stock Update: Accepted qty increments Warehouse Available Stock"]
    H2 --> I2["Finance registers Vendor Invoice"]
    I2 --> J2{"Automated 3-Way Matching Engine<br/>PO Price vs. GRN Qty vs. Invoice Rate"}
    J2 -->|Matched within tolerance| K2["Invoice Approved for Payment"]
    J2 -->|Mismatch Detected| L2["Flagged for Variance Resolution"]
    L2 -->|Approved with Override| K2
    K2 --> M2["Payment Recorded (Partial/Full Settlement); Invoice Cleared"]
    M2 --> N2["Real-Time Dashboard & Power BI Analytics Updated"]
```

---

### 3. Procure-to-Pay (P2P) Swimlane Workflow

```mermaid
sequenceDiagram
    autonumber
    actor Requester as Requisition Creator
    actor Approver as Purchase / Dept Manager
    actor Purchasing as Purchase Officer
    actor Warehouse as Warehouse Clerk (GRN)
    actor Finance as Accounts Payable Finance
    participant System as SmartProcure ERP Engine
    participant DB as SQL Server & Inventory Ledger

    Requester->>System: 1. Create Purchase Requisition (PR-2026-XXXX)
    System->>DB: Save PR in Draft Status
    Requester->>System: 2. Submit Requisition
    System->>Approver: 3. Alert Approver (Based on Value Threshold)
    Approver->>System: 4. Review & Approve Requisition
    System->>DB: Status -> Approved, Record ApprovalHistory

    Purchasing->>System: 5. Solicit Quotes & Compare in Matrix
    Purchasing->>System: 6. Convert Approved PR to Purchase Order (PO-2026-XXXX)
    System->>DB: Save PO, Mark PR as ConvertedToPO
    System->>Purchasing: 7. Download Official PO PDF & Transmit to Vendor

    Note over Warehouse,System: Physical Shipment Arrives at Dock
    Warehouse->>System: 8. Record Goods Receipt Note (GRN) against PO
    Warehouse->>System: 9. Record Inspection (Accepted Qty, Rejected Qty)
    System->>DB: Atomic Tx: Save GRN + Increment InventoryBalance + Write InventoryTransactions
    System->>DB: Update PO Status (PartiallyReceived / FullyReceived)

    Note over Finance,System: Vendor Submits Invoice
    Finance->>System: 10. Enter Supplier Invoice (INV-XXXX)
    System->>System: 11. Run 3-Way Match (PO Rate x GRN Accepted Qty vs Invoice)
    alt Match Successful
        System-->>Finance: Status: Matched (Variance 0.0%)
        Finance->>System: 12. Approve Invoice for Payment
    else Mismatch Detected
        System-->>Finance: Status: Mismatched (Flag Rate / Qty Variance)
        Finance->>System: Review Discrepancy & Authorize with Remarks
    end

    Finance->>System: 13. Record Payment (NEFT/RTGS Reference)
    System->>DB: Update Invoice Outstanding Balance & Status (Paid / PartiallyPaid)
    System->>System: 14. Refresh Dashboard KPIs & Cash Flow Metrics
```

---

### 4. Key Business Decision Rules

1. **Reorder Point Trigger (SAP MM Consumption-Based Planning):**
   When $\text{Available Stock} \le \text{Reorder Level}$, the system automatically generates an in-app Low Stock notification, prompting the designated department to trigger a Purchase Requisition.
2. **Goods Receipt Inspection Policy:**
   - Accepted material updates the storage location stock ledger.
   - Rejected materials are segregated in scrap/return hold area and are never credited to available stock balance.
   - Over-receipt beyond open PO balance is strictly disallowed by the system.
3. **Three-Way Matching Tolerance Limits:**
   - Quantity tolerance: $0\%$ (supplier cannot invoice for more units than were physically received and accepted at the warehouse dock).
   - Price tolerance: $\pm 2\%$ or ₹100 max variance to account for minor round-offs or transport levies.
