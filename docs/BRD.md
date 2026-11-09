# Business Requirements Document (BRD)
## Project: SmartProcure — Enterprise Procurement & Inventory Management System

**Organization Context:** Apex Manufacturing Industries Pvt. Ltd. (Pune / Ahmedabad / Vadodara, India)  
**Author:** Senior ERP Solution Architect & SAP MM Functional Consultant  
**Target Audience:** Executive Leadership, Purchasing, Warehouse, Finance, ERP Consulting Interviewers  
**Version:** 1.0.0  
**Date:** November 2026  

---

### 1. Executive Summary

Apex Manufacturing Industries Pvt. Ltd. is a mid-sized industrial manufacturing conglomerate specializing in precision engineering components, electrical sub-assemblies, and heavy machine fabrications. The organization handles an annual procurement spend in excess of ₹45 Crores across raw materials (structural steel, copper winding wire, specialty lubricants), direct components (bearings, programmable controllers), and indirect maintenance, repair, and operating (MRO) items.

SmartProcure is designed as an enterprise-grade, SAP Materials Management (SAP MM) inspired Procurement and Inventory Management Platform. It models the end-to-end **Procure-to-Pay (P2P)** operational lifecycle—enabling decentralized requisitioning, role-based approval governance, competitive vendor quotation comparison, standardized purchase orders, warehouse goods receipt (GRN) with inspection, inventory valuation, three-way invoice matching, and payment recording.

---

### 2. Business Problem Statement

Prior to SmartProcure, Apex Manufacturing faced significant operational friction across its procurement lifecycle:
1. **Uncontrolled Spend & Maverick Buying:** Departments placed off-contract orders through ad-hoc emails or phone calls without budgetary controls or pre-purchase approvals.
2. **Approval Bottlenecks & Lack of Auditability:** Paper purchase requests were delayed on managers' desks, leading to emergency expedited freight costs and zero visibility into who approved what and when.
3. **Inaccurate Inventory & Stockouts:** Warehouses maintained manual excel registers. Accepted goods were not reconciled with open purchase orders, leading to stock discrepancies, phantom inventory, or unexpected production stoppages.
4. **Invoice Discrepancies & Overpayments:** Finance received vendor invoices that deviated from negotiated PO rates and delivered quantities. The lack of an automated 3-way match (PO vs. GRN vs. Invoice) caused manual disputes and risk of duplicate payments.
5. **Vendor Performance Blindspots:** Purchasing managers lacked centralized data on vendor on-time delivery rates, quality rejection rates, and payment history.

---

### 3. Business Objectives & Success Criteria

| Objective ID | Business Objective | Success Metric / Target |
| :--- | :--- | :--- |
| **BO-01** | Standardize P2P Workflow | 100% of purchase transactions routed through digital Purchase Requisitions and Purchase Orders. |
| **BO-02** | Eliminate Maverick Purchasing | Strict multi-tier approval matrix with budget checks before vendor commitment. |
| **BO-03** | Tighten Warehouse Receiving | Real-time Goods Receipt Note (GRN) with accepted vs. rejected inspection recording within 2 hours of dock arrival. |
| **BO-04** | Automate Three-Way Matching | Instant cross-verification of PO, GRN, and Supplier Invoice with automated tolerance detection (price & qty). |
| **BO-05** | Inventory Transparency | Real-time perpetual inventory balances, automated low-stock reorder alerts, and complete transaction audit log. |
| **BO-06** | Financial Governance | Precise tracking of invoice aging, outstanding liabilities, and historical payment records. |

---

### 4. Stakeholders & User Roles

| Role | Department | Key Responsibilities |
| :--- | :--- | :--- |
| **Administrator** | IT / Systems | User provisioning, role assignment, system configuration, department budgets, master data governance. |
| **Requisitioner** | Production / Maint / IT | Generates Purchase Requisitions (PR) specifying materials, quantities, delivery deadlines, and business justifications. |
| **Purchase Manager** | Procurement | Evaluates requisitions, approves/rejects requests up to limit, solicits vendor quotations, negotiates rates, issues Purchase Orders (PO). |
| **Warehouse Manager** | Logistics / Stores | Receives physical shipments, records Goods Receipt Notes (GRN), accepts/rejects goods, logs damage inspection, updates stock. |
| **Finance Manager** | Accounts Payable | Registers vendor invoices, executes automated 3-way matching, flags variances, approves invoices, and records payments. |
| **Executive Management** | C-Suite / Plant Head | Analyzes procurement KPIs, spend by department/category, vendor ratings, cash flow liabilities, and stock valuation. |

---

### 5. Scope of the System

#### In Scope:
- **Master Data:** Vendors (with GSTIN, PAN, bank details, rating), Materials (categorization, UOM, pricing, reorder levels), Warehouses, Departments (with budgets).
- **Requisition Lifecycle:** Creation, multi-item line entry, draft saving, submission, prioritization, cancellation.
- **Approval Engine:** Configurable financial tiers (Tier 1: <= ₹25,000 Dept Mgr; Tier 2: ₹25,001–₹1,00,000 Purchase Mgr; Tier 3: > ₹1,00,000 Senior Mgmt). Full audit history.
- **Quotation Comparison:** Vendor RFQ logging, unit price, tax, lead times, comparison matrix, selection justification.
- **Purchase Order Management:** Conversion from approved PR, tax/discount calculation, order status tracking, PDF generation.
- **Goods Receipt & Inspection:** PO-referenced receiving, partial delivery tracking, accepted vs. rejected quantities, atomic inventory update.
- **Perpetual Inventory:** Warehouse-level balances, stock movement transactions (Receipt, Issue, Adjustment, Return), low-stock alert triggers.
- **Invoice Verification:** Three-way matching (PO vs. GRN vs. Invoice) with configurable price/quantity tolerance and mismatch flagging.
- **Payment Recording:** Partial and full payments, transaction reference recording, invoice balance reconciliation.
- **Analytics & Reporting:** Live ERP dashboard, CSV exports, Power BI connection guides, DAX models.

#### Out of Scope:
- Direct electronic bank clearing / SWIFT network execution (payments are logged for internal ERP ledger reconciliation).
- Direct live SAP RFC / BAPI connectors (the system is an independent SAP MM-inspired application for portfolio demonstration).

---

### 6. High-Level Business Rules

1. **Requisition Validation:** Delivery date must be $\ge$ current date; quantity must be $> 0$; must contain at least 1 material.
2. **Approval Hierarchy:** Requisitions cannot be converted into purchase orders unless fully approved by designated threshold authorities.
3. **Vendor Qualification:** Orders cannot be placed with Inactive vendors. Vendor GSTIN and PAN must conform to Indian statutory formats.
4. **Order Authorization:** Once a PO is approved and dispatched to a vendor, line item amendments require an audit reason.
5. **Goods Receipt Integrity:** Total accepted quantity across all GRNs cannot exceed the original PO line quantity. Rejected items do not increase available warehouse stock.
6. **Atomicity of Stock Movement:** GRN creation and inventory balance increments must occur within a single ACID database transaction.
7. **Three-Way Matching Rule:** An invoice cannot be approved for payment if unit price or billed quantity deviates beyond authorized tolerance ($\pm 2\%$ or ₹100 threshold) without explicit finance approval notes.
8. **Payment Cap:** Total payments against an invoice cannot exceed the verified invoice amount. Overpayments are strictly rejected.

---

### 7. Regulatory & Compliance Context

- **GST Compliance:** Support for Indian Goods and Services Tax (CGST, SGST, IGST) with standard 15-character GSTIN structure (`24AAAAA0000A1Z5`).
- **PAN Compliance:** Indian Permanent Account Number validation (10 alphanumeric characters `AAAAA9999A`).
- **Audit Trail:** Immutable logging of user ID, action timestamp, entity name, previous values, and new values for all sensitive financial/operational transitions.
