# Software Requirements Specification (SRS)
## TAM – TrackFun Accessories Management

**Version:** 1.0.0  
**Date:** 2026-05-30  
**Platform:** Windows Desktop (WPF, .NET 8)

---

## 1. Introduction

### 1.1 Purpose
TAM is a Windows desktop application for a garments shop owner to track accessories inventory: inward (purchase), outward (dispatch), returns, and stock levels.

### 1.2 Scope
The system covers end-to-end accessory lifecycle management from vendor management and purchase ordering through to outward dispatch and returns. All data is persisted locally as JSON files.

### 1.3 Glossary
| Term | Meaning |
|---|---|
| Accessory | A garment accessory item (button, zip, thread, etc.) |
| Vendor | Supplier from whom accessories are purchased |
| PO | Purchase Order – draft request for accessories from a vendor |
| Inward Order | Confirmation of received goods (from PO or direct) |
| Outward Order | Record of accessories dispatched/given out |
| Return Order | Goods returned from an outward dispatch |

---

## 2. Overall Description

### 2.1 Architecture
- **Pattern:** MVVM (Model-View-ViewModel)
- **UI:** WPF (Windows Presentation Foundation), pink-themed, white background
- **Data Storage:** JSON files in `%AppData%\TAM\Data\`
- **Audit Log:** JSON file in `%AppData%\TAM\Logs\audit.json`, max 5,000 entries (oldest purged)
- **Reports:** Excel via ClosedXML, PDF via PdfSharpCore
- **Backup:** Full data export/import as JSON

### 2.2 Navigation Structure
```
Sidebar (pink) → Main Content Area (white)
├── Dashboard
├── Vendors
├── Accessories
├── [ORDERS section]
│   ├── Purchase Orders
│   ├── Inward Orders
│   ├── Outward Orders
│   └── Return Orders
├── [REPORTS section]
│   ├── Summary
│   ├── Audit Log
│   └── Backup & Restore
```

---

## 3. Data Models

### 3.1 Vendor
| Field | Type | Notes |
|---|---|---|
| VendorId | string (GUID) | Primary key |
| VendorCode | string | Auto-generated (VEN0001) |
| Name | string | Required |
| ContactPerson | string | Optional |
| Phone | string | Optional |
| Email | string | Optional |
| Address | string | Optional |
| IsActive | bool | Default true |
| CreatedAt | DateTime | Auto-set |
| UpdatedAt | DateTime | Auto-updated |
| EditHistory | List<EditHistoryEntry> | Version history |

### 3.2 Accessory
| Field | Type | Notes |
|---|---|---|
| AccessoryId | string (GUID) | Primary key |
| AccessoryCode | string | Auto-generated (ACC0001) |
| Name | string | **Unique** – enforced |
| Category | string | Optional grouping |
| Unit | string | pcs/meters/kg/etc. |
| CurrentStock | decimal | Updated on inward/outward/return |
| MinimumStock | decimal | Alert threshold |
| Description | string | Optional |
| IsActive | bool | Default true |
| EditHistory | List<EditHistoryEntry> | Version history |

### 3.3 Purchase Order (PO)
| Field | Type | Notes |
|---|---|---|
| POId | string (GUID) | Primary key |
| PONumber | string | Auto-generated (PO-YYYY-NNNN) |
| VendorId | string | FK to Vendor |
| Status | POStatus | Draft / PartiallyInward / Completed / Cancelled |
| Items | List<PurchaseOrderItem> | Line items |
| Notes | string | Free text |
| EditHistory | List<EditHistoryEntry> | Version history |

**PurchaseOrderItem:** AccessoryId, RequestedQuantity, ReceivedQuantity, UnitPrice

### 3.4 Inward Order
| Field | Type | Notes |
|---|---|---|
| InwardId | string (GUID) | Primary key |
| InwardNumber | string | Auto-generated (INW-YYYY-NNNN) |
| POId | string? | FK to PO (null = direct inward) |
| BillNo | string | **Required** – vendor bill number |
| VendorId | string | FK to Vendor |
| InwardDate | DateTime | User-specified |
| Items | List<InwardOrderItem> | Line items |
| EditHistory | List<EditHistoryEntry> | Version history |

**Effect:** On save, `Accessory.CurrentStock += quantity` for each item.  
**On edit:** Stock is reversed (old) then re-applied (new).

### 3.5 Outward Order
| Field | Type | Notes |
|---|---|---|
| OutwardId | string (GUID) | Primary key |
| OutwardNumber | string | Auto-generated (OUT-YYYY-NNNN) |
| Recipient | string | Required |
| Purpose | string | Optional |
| Status | OutwardOrderStatus | Active / PartiallyReturned / FullyReturned |
| Items | List<OutwardOrderItem> | Line items |
| EditHistory | List<EditHistoryEntry> | Version history |

**OutwardOrderItem:** AccessoryId, Quantity, ReturnedQuantity  
**Effect:** On save, `Accessory.CurrentStock -= quantity`.  
**Stock check:** Validated before save; error shown if insufficient.

### 3.6 Return Order
| Field | Type | Notes |
|---|---|---|
| ReturnId | string (GUID) | Primary key |
| ReturnNumber | string | Auto-generated (RET-YYYY-NNNN) |
| OutwardId | string | FK to Outward Order |
| IsFullReturn | bool | True = full, False = partial |
| Items | List<ReturnOrderItem> | Items being returned |
| ReturnDate | DateTime | User-specified |
| EditHistory | List<EditHistoryEntry> | Version history |

**Effect:** On save, `Accessory.CurrentStock += returnedQuantity`.  
**Outward Status:** Updated based on total returned vs dispatched.

### 3.7 Audit Log Entry
| Field | Type | Notes |
|---|---|---|
| LogId | string (GUID) | Primary key |
| Timestamp | DateTime | Auto-set |
| Action | string | CREATE / UPDATE / DELETE / IMPORT / EXPORT / APP |
| Module | string | Vendor / Accessory / PurchaseOrder / etc. |
| Description | string | Human-readable detail |
| EntityId | string | ID of affected entity |

### 3.8 Edit History Entry
| Field | Type | Notes |
|---|---|---|
| EntryId | string (GUID) | Primary key |
| ChangedAt | DateTime | When the edit happened |
| ChangeDescription | string | Description of change |
| SnapshotJson | string | JSON snapshot before change |

---

## 4. Functional Requirements

### 4.1 Vendor Management
- Add, Edit, Delete vendors
- Fields: Name (required), Contact Person, Phone, Email, Address, Active status
- Search/filter by name, code, phone, email
- Edit history tracked per vendor

### 4.2 Accessory Management
- Add, Edit, Delete accessories
- Accessory **name must be unique** (enforced)
- Filter by category, search by name/code
- CurrentStock shown with low-stock highlight (when at or below MinimumStock)
- Edit history tracked per accessory

### 4.3 Purchase Order Management
- Create PO with vendor selection and items (accessory + quantity + unit price)
- Status: Draft → PartiallyInward → Completed
- Filter by status, search by PO number / vendor
- Only Draft POs can be edited or deleted
- Fully editable when in Draft state
- **Convert to Inward:** For Draft/PartiallyInward POs:
  - User enters received quantities per item
  - Full: PO → Completed
  - Partial: original PO updated to PartiallyInward, new PO created for remaining items

### 4.4 Inward Order Management
- Create direct inward orders (with Bill No, vendor, items)
- Created via PO conversion (see 4.3)
- Bill Number is mandatory
- Edit any inward order (stock is reversed and re-applied correctly)
- View edit history

### 4.5 Outward Order Management
- Create outward orders (recipient, purpose, items with quantities)
- Stock validation: error if insufficient stock
- Status: Active / PartiallyReturned / FullyReturned
- Edit any outward order (stock is reversed and re-applied)
- Filter by status
- **Process Return:** Select items and enter return quantities
  - Full: entire order returned, status → FullyReturned
  - Partial: partial return, status → PartiallyReturned; remaining items stay in order
- View edit history

### 4.6 Return Order Management
- View all return orders
- Edit existing return orders (stock is correctly adjusted)
- Filter/search by return number or linked outward number
- View edit history

### 4.7 Dashboard
- Live stat cards: Vendors, Accessories, POs (total + pending), Inward, Outward, Returns, Low Stock
- Recent inward orders (last 5)
- Recent outward orders (last 5)
- Refreshes on navigation

### 4.8 Summary / Stock Report
- Tabular view of all accessories with:
  - Current stock, min stock, total inward, total outward, total returned
  - Stock status (OK / Low Stock) with color coding
  - Vendor(s) supplying each accessory
- Filter by vendor, filter by accessory, free text search
- Export as Excel (.xlsx) or PDF
- Stock summary refreshes on navigation

### 4.9 Audit Log
- Shows last N events (default 50, user-configurable)
- Total count displayed (max 5,000 stored; oldest auto-purged)
- Columns: Timestamp, Action, Module, Description, Entity ID
- Refresh button

### 4.10 Backup & Restore
- **Export Backup:** Save all data as JSON to user-selected path
- **Import Backup:**
  - Preview: shows counts from backup file
  - Mode: Append (duplicates by ID skipped) or Replace (clears all data)
  - Confirmation dialog for Replace mode
- **Report Exports:** Inward, Outward, PO reports as Excel

---

## 5. Non-Functional Requirements

### 5.1 Performance
- Application startup < 3 seconds
- Data operations (save/load) < 500ms for typical dataset sizes

### 5.2 UI/UX
- Pink sidebar (#AD1457), white content area
- Minimalistic, professional, Segoe UI font
- All add/edit operations in modal popup dialogs
- Color-coded status badges (PO status, outward status, low stock)
- DataGrids with alternating rows and pink headers

### 5.3 Data Integrity
- Stock updates are atomic (save accessories + orders together)
- Edit operations reverse old stock impact before applying new
- Unique accessory name enforced at application level

### 5.4 Storage
- All data in `%AppData%\TAM\Data\` (6 JSON files)
- Audit log in `%AppData%\TAM\Logs\audit.json`
- Max 5,000 audit entries; oldest entries removed when limit exceeded

---

## 6. File Structure

```
TAM/
├── TAM.csproj
├── App.xaml + App.xaml.cs
├── MainWindow.xaml + MainWindow.xaml.cs
├── SRS.md
├── Models/
│   ├── EditHistoryEntry.cs
│   ├── Vendor.cs
│   ├── Accessory.cs
│   ├── PurchaseOrder.cs
│   ├── InwardOrder.cs
│   ├── OutwardOrder.cs
│   ├── ReturnOrder.cs
│   ├── AuditLogEntry.cs
│   └── AppData.cs
├── Helpers/
│   ├── RelayCommand.cs
│   └── Converters.cs
├── Services/
│   ├── DataService.cs        ← Singleton, all CRUD + stock management
│   ├── AuditService.cs       ← Singleton, 5000-entry rolling log
│   ├── ReportService.cs      ← Excel + PDF generation
│   └── BackupService.cs      ← Export/import JSON backup
├── ViewModels/
│   ├── BaseViewModel.cs      ← INotifyPropertyChanged base
│   ├── MainViewModel.cs      ← Navigation + current view
│   ├── DashboardViewModel.cs
│   ├── VendorViewModel.cs
│   ├── AccessoryViewModel.cs
│   ├── PurchaseOrderViewModel.cs
│   ├── InwardOrderViewModel.cs
│   ├── OutwardOrderViewModel.cs
│   ├── ReturnOrderViewModel.cs
│   ├── SummaryViewModel.cs
│   ├── AuditLogViewModel.cs
│   └── BackupViewModel.cs
├── Views/
│   ├── DashboardView.xaml
│   ├── VendorView.xaml
│   ├── AccessoryView.xaml
│   ├── PurchaseOrderView.xaml
│   ├── InwardOrderView.xaml
│   ├── OutwardOrderView.xaml
│   ├── ReturnOrderView.xaml
│   ├── SummaryView.xaml
│   ├── AuditLogView.xaml
│   └── BackupView.xaml
└── Dialogs/
    ├── VendorDialog.xaml
    ├── AccessoryDialog.xaml
    ├── PurchaseOrderDialog.xaml
    ├── InwardOrderDialog.xaml
    ├── OutwardOrderDialog.xaml
    ├── ReturnOrderDialog.xaml
    ├── ImportDialog.xaml
    └── EditHistoryDialog.xaml
```

---

## 7. Data Flow Examples

### 7.1 Purchase → Stock Increase
```
Create PO (Draft) 
  → Convert to Inward Order (Full/Partial, enter Bill No)
  → Stock += received quantities
  → PO status → Completed (or PartiallyInward + new PO for remainder)
  → Inward Order saved with audit log entry
```

### 7.2 Outward → Stock Decrease
```
Create Outward Order (enter recipient, items, quantities)
  → Stock check: CurrentStock >= Quantity for each item
  → If OK: Stock -= quantities, Outward Order saved
  → If NOT OK: Error shown, not saved
```

### 7.3 Return → Stock Restore
```
Select Outward Order → Process Return
  → Full Return: all items returned, Stock += all quantities
  → Partial Return: selected items returned, Stock += returned quantities
  → Outward Status updated (PartiallyReturned / FullyReturned)
  → Return Order saved with audit log entry
```

### 7.4 Edit with Stock Correction
```
Edit Inward Order:
  → Reverse old stock impact (Stock -= old quantities)
  → Apply new stock impact (Stock += new quantities)
  → Edit history snapshot saved
  → Audit log entry written

Edit Outward Order:
  → Add back old stock impact (Stock += old quantities)
  → Check and apply new (Stock -= new quantities)
  → Edit history snapshot saved
```
