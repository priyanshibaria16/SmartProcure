# SmartProcure — Enterprise ERP Portfolio Project

> **SAP MM Inspired Procurement & Inventory Management System**  
> Built for Apex Manufacturing Industries Pvt. Ltd., Pune, India  
> Portfolio Project for SAP Functional Consultant (MM) Candidates

---

## 🚀 Quick Start

### 1. Backend (ASP.NET Core 10 / .NET 10)

```bash
cd backend
dotnet run --project SmartProcure.API/SmartProcure.API.csproj --urls "http://localhost:5000"
```

> **Database:** Automatically detects SQL Server. Falls back to SQLite (`smartprocure.db`) if SQL Server is unavailable.  
> **Swagger UI:** http://localhost:5000/swagger  
> **Seed Data:** Auto-seeded on first run with Apex Manufacturing data.

### 2. Frontend (React 18 + Vite + TypeScript)

```bash
cd frontend/smartprocure-ui
npm install
npm run dev
```

> **Dev Server:** http://localhost:5173  
> **API URL:** Configure in `.env` with `VITE_API_URL=http://localhost:5000/api`

---

## 🔐 Demo Credentials

| Role | Email | Password | Capabilities |
|------|-------|----------|--------------|
| **Administrator** | `admin@smartprocure.com` | `Admin@123` | Full system + user management |
| **Purchase Manager** | `purchasemanager@smartprocure.com` | `Purchase@123` | Approve PRs, issue POs, vendor eval |
| **Warehouse Manager** | `warehouse@smartprocure.com` | `Warehouse@123` | GRN dock receiving, stock audit |
| **Finance Manager** | `finance@smartprocure.com` | `Finance@123` | 3-Way matching, AP settlement |
| **Requisition Creator** | `requester@smartprocure.com` | `Request@123` | Create & submit PRs |
| **Management Viewer** | `management@smartprocure.com` | `Manage@123` | Read-only KPIs & dashboards |

> **1-Click Role Switcher** is available in the top header bar for instant persona switching during demos.

---

## 🏗️ Architecture

```
SmartProcure/
├── backend/
│   ├── SmartProcure.Domain/        # Entities, Enums
│   ├── SmartProcure.Application/   # DTOs, Interfaces, Services
│   ├── SmartProcure.Infrastructure/# EF Core DbContext, Seeder
│   ├── SmartProcure.API/           # Controllers, Program.cs, JWT
│   └── SmartProcure.Tests/         # 25 Enterprise Integration Tests
├── frontend/
│   └── smartprocure-ui/            # React + TypeScript + Vite + Tailwind
├── database/
│   └── scripts/
│       ├── 01_Create_Schema.sql    # Full SQL Server DDL (26 tables)
│       ├── 02_Seed_Data.sql        # Apex Manufacturing seed data
│       └── 03_PowerBI_Views.sql    # 4 Analytical Views + DAX
└── docs/
    ├── BRD.md                      # Business Requirements Document
    ├── SRS.md                      # Software Requirements Specification
    ├── FRD.md                      # Functional Requirements Document
    ├── ERD.md                      # Entity Relationship Diagram
    ├── API_DOCUMENTATION.md        # REST API Catalog
    ├── BUSINESS_PROCESS.md         # P2P Process & SAP T-Code Mapping
    └── UAT_TEST_CASES.md           # 25 UAT Test Scenarios
```

---

## 📦 Core Modules

| Module | SAP Equivalent | Description |
|--------|---------------|-------------|
| **Dashboard** | — | Live KPIs, Recharts analytics, activity stream |
| **Purchase Requisitions** | ME51N | Multi-item PR, approval workflow, priority levels |
| **Vendor Quotations** | ME41/ME47 | Multi-vendor comparison matrix, bid selection |
| **Purchase Orders** | ME21N | PO creation, PDF export, send to vendor |
| **Goods Receipts (GRN)** | MIGO | Dock receiving, quality inspection, partial receipt |
| **Perpetual Inventory** | MB52/MMBE | Multi-warehouse stock, valuation, audit ledger |
| **Supplier Invoices** | MIRO | 3-Way matching (PO vs GRN vs Invoice), variance |
| **Payments** | F-53 | NEFT/RTGS/IMPS/Cheque, UTR reference, AP ledger |
| **Vendor Master** | XK01/XK03 | GSTIN/PAN, ratings, banking details, lifecycle |
| **Material Master** | MM01/MM03 | SKU catalog, reorder levels, preferred vendors |
| **Reports & Power BI** | ME80FN/MB52 | CSV exports + 4 DirectQuery SQL Views |
| **User Governance** | — | RBAC, department segregation, audit |

---

## 🧪 Test Results

```
Passed: 25 / 25 | Failed: 0 | Skipped: 0
```

All 25 enterprise workflow tests pass, covering:
- Full P2P lifecycle integration
- 3-Way matching verification
- Partial goods receipts and inventory atomicity
- Role-based authorization enforcement
- Invoice payment reconciliation

**Run tests:**
```bash
cd backend
dotnet test SmartProcure.Tests/SmartProcure.Tests.csproj
```

---

## 📊 Power BI Integration

Connect Power BI Desktop or Fabric to your SQL Server instance and run:

```sql
-- Located in: database/scripts/03_PowerBI_Views.sql
SELECT * FROM vw_ProcurementSpending;     -- P2P spend analytics
SELECT * FROM vw_InventoryValuation;      -- Perpetual stock valuation
SELECT * FROM vw_InvoiceAgingAndLiabilities; -- AP aging buckets
SELECT * FROM vw_DepartmentExpenditure;   -- Budget utilization
```

---

## 🛠️ Technology Stack

| Layer | Technologies |
|-------|-------------|
| **Frontend** | React 18, TypeScript, Vite, Tailwind CSS, Recharts, Lucide React |
| **Backend** | C# / ASP.NET Core 10, Entity Framework Core, JWT Auth |
| **Database** | SQL Server (prod) / SQLite (dev fallback), EF Core Code-First |
| **Testing** | xUnit, EF Core In-Memory, 25 integration test cases |
| **Analytics** | Power BI DirectQuery, 4 SQL Views, CSV Export APIs |

---

## ⚠️ Disclaimer

SmartProcure is an independent ERP-inspired portfolio application. It is **not** SAP software and does not perform actual SAP configuration, integration, or connectivity. SAP T-Code references are for educational and mapping context only.

---

*Built with enterprise precision for portfolio demonstration — Apex Manufacturing Industries Pvt. Ltd., Pune 411001, Maharashtra, India.*
