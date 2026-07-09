# MEMORY

**No app code until final confirmation + user says do it.**  
**New app** (not edit Diary). Stack lean: **MudBlazor** (user will confirm today). Excel later.

## Confirmed navigation (client Priyadarshini) — YES
- **Only DIT** has both **IT Capital** and **IT Revenue**
- **Other departments:** Capital only (no Revenue)
- **Capital:** Department → (DIT: pick Capital) → Section → **Project** → entry form
- **Revenue (DIT only):** Department → pick Revenue → Section → entry form (**no project**)

## What each conversation asked
| Who | Ask |
|-----|-----|
| Client | Monthly Capital/Revenue **entry portal** + Oracle DB; allotments in DB; she does Power BI later |
| Manager | Also Department/Section/Project masters, CM→CGM hierarchy, team by PF, auto-fetch on login, dashboard filters; check Diary tables |
| Savitha | Start schema; hierarchy UI; Capital=project, Revenue=section |

## Diary today (reference only — we build NEW app)
**Flow:** Blazor page → Service → EF `AppDbContext` → Oracle `DIT_DIARY`  
**Budget flow:** `/budget/dashboard` + `/budget/details` → `BudgetService` → table `BUDGET_UTILIZATION`  
**Budget logic now:** load all rows; filter Capital/Revenue + FY/Month/Section; pie/KPIs; Excel upload/export; inline edit of `ACTUAL_BUDGET` only. **No** Priyadarshini wizard, no previous/current dual panel, no next-month estimate, no justification/PO, no revenue expenditure-head lines.  
**Team flow:** `/teammanagement` → Dept/Section/Employee CRUD (`SECTIONS` has TEAM_LEAD/AGM/DGM/GM/CGM/CTO).

**Data in export:** `BUDGET_UTILIZATION` 151 rows (101 Capital / 50 Revenue), FY 2025-26, December, vertical DIT. Note: Diary Revenue rows still store a `PROJECT_NAME` value — **different from client rule** (Revenue = section only).

## DB first
Yes. Design new-app tables (can mirror Diary names/patterns) before UI. Wait for MudBlazor confirm + Excel when you can share (email/drive/link OK).
