# Diary Repo Analysis (https://github.com/saibhavana456/Diary)

Analyzed on: 2026-07-09  
Method: cloned repo, read csproj, Program.cs, entities, BudgetService, Budget pages, TeamManagement, SQL DDL exports.  
**No assumptions beyond what is in the files.**

## 1. What Diary actually is
- Project folder: `DIT_DIARY_PORTAL`
- Stack (from `.csproj` + `Program.cs`):
  - **.NET 8** (`net8.0`)
  - **Blazor Server** (Razor Components + Interactive Server)
  - **MudBlazor** UI
  - **Oracle EF Core** (`Oracle.EntityFrameworkCore`)
  - **Dapper**, **ClosedXML** (Excel)
- **Not Angular.** No `package.json` / TypeScript frontend found.
- Architecture style: Blazor pages call **C# services** + `AppDbContext` directly (not a separate public REST API project in this repo).

## 2. Schema / Oracle user
- SQL exports: `exportfinalditdiary.sql`, `exportDIT1.sql`
- Schema/user name in DDL: **`DIT_DIARY`**
- Connection keys exist in `appsettings.json` (`OracleDb`, `OracleDb_pc`). **Passwords present in that file — do not copy secrets into our repo.**

## 3. Tables most relevant to our budget app

### 3.1 `BUDGET_UTILIZATION` (exists — closest to our feature)
Columns from DDL:
- `S_NO` (identity)
- `FINANCIAL_YEAR`
- `MONTH`
- `VERTICAL`
- `PROJECT_NAME`
- `FRESH_BUDGET`
- `SPILL_OVER_BUDGET`
- `ACTUAL_BUDGET`
- `BUDGET_UTILIZATION`
- `BUDGET_TYPE` (code uses `"Capital"` / `"Revenue"`)
- `TOTAL_BUDGET_ALLOCATED`
- `SECTION`

**What Diary budget UI does today (from code):**
- Dashboard `/budget/dashboard`: Capital vs Revenue cards, FY/Quarter/Month filters, pie by vertical/section, quarterly capital breakdown
- Details `/budget/details`: Capital/Revenue toggle, section/month filters, grid, Excel upload/export
- Data entry path in service is largely **Excel upload** (`UploadBudgetExcelAsync`) + add/update row helpers — **not** the Priyadarshini previous-month / current-month dual-panel form with estimates, justification, cumulative-till, or revenue expenditure-head lines

**Gap vs Priyadarshini BRD/UI:**
- No spillover/fresh **actual vs estimated next month** split fields
- No justification / PO textarea field in this table
- No expenditure-head breakdown for Revenue (FMS/Rent/etc.)
- No explicit Dept→Section→Project wizard screens like the presentation
- Allotment vs monthly utilization not modeled as separate master vs transaction the way client described

### 3.2 `DEPARTMENTS`
- `DEPT_ID`, `DEPT_NAME`, `DEPT_HEAD`, `CREATED_AT`
- Entity has navigation to Sections

### 3.3 `SECTIONS`
- `SECTION_ID`, `NAME`, `DESCRIPTION`
- Hierarchy-ish fields: `TEAM_LEAD`, `AGM`, `DGM`, `GM`, `CGM`, `CTO`
- `DEPT_ID` FK
- `CREATED_AT`, `UPDATED_AT`
- Matches manager’s “section has CM/AGM/DGM/GM/CGM” idea closely (`TEAM_LEAD` ≈ CM/team lead naming in this table)

### 3.4 `EMPLOYEE_MASTER`
- DDL shows only: `PF_NO`, `SECTION_ID`
- Code references `EmployeeMaster` entity + Team Management pages
- **Finding:** `EmployeeMaster.cs` file is **missing** from `Models/Entities/` in this clone (referenced by AppDbContext / services / razor). Incomplete in repo as checked.

### 3.5 `MANPOWER_DETAILS`
- Rich employee attributes including `PF_NO`, `NAME`, dept/location/job/scale, `TEAM_CODE`, `TEAM_DESC`, email/contact, etc.
- Useful pattern for PF → name/designation fetch (manager ask)

### 3.6 `TEAM_MASTER` / `TEAM_MASTER_AUDIT`
- Team codes/names + CM/AGM/DGM emails; audit has `DEL_FLAG` soft-delete style

### 3.7 Project masters (two different tables)
- `PROJECT_MASTER` — large legacy-style project table (string costs, milestones 1–5, section, maker/checker, etc.)
- `PROJECTS_MASTER` — newer structured project master with dept/section owner fields, PO docs, financials, virtual `PROJECT_CODE`
- Child tables: `PROJECT_MILESTONE_DETAILS`, `PROJECT_ISSUE_DETAILS`, `PROJECT_RESOURCE_DETAILS`, `DOCUMENT`

### 3.8 Other modules (not budget, but show portal pattern)
Manpower, Audit, SMS Expenditure, Network Inventory, PMS tickets, Social Media, Hyper Automation, App Inventory, Vendor (commented in nav), FMS_DETAILS, AMC_ATS

## 4. UI / chart patterns reusable as reference
- MudBlazor layout + `NavMenu` groups (Project Management, **Budget Utilization**, Audit, Manpower, …)
- Budget dashboard: Capital/Revenue toggle cards, FY filter, quarter/month filters, MudChart pie, summary KPI papers, quarterly table
- Budget details: filters + Excel sample/upload/export via ClosedXML
- Team Management: Department / Section / Employee maintenance dialogs

## 5. What this means for our `grok-capital-budget` app

| Topic | Fact from Diary | Implication for us |
|-------|-----------------|--------------------|
| Greenfield vs reuse | Diary already has Oracle tables + Blazor budget module | We can **reference patterns**; whether we **extend Diary** vs **new app in this repo** is still a user decision |
| Stack mismatch | Diary = Blazor Server; SRS text = Angular + .NET Core 8 | User said they will confirm stack later — do not assume Angular |
| Budget feature completeness | Diary budget is dashboard + Excel-oriented utilization table | Priyadarshini monthly entry form is **new work** either way |
| Masters | DEPARTMENTS / SECTIONS / PROJECT*_MASTER / MANPOWER exist | Aligns with manager “check BB/Diary tables” instruction |
| Secrets | Live connection strings in appsettings | Never commit those into our repo |

## 6. Still missing (cannot invent)
- Whether production intent is: **add screens into Diary** OR **build separate app** that shares Oracle schema OR **new schema**
- Whether `BUDGET_UTILIZATION` should be extended vs new tables for Capital monthly entry + Revenue head lines
- Excel master from Priyadarshini (still pending)
- Confirmation of stack for *this* repo
