# MEMORY (single source for this agent)

**Mode:** planning only — no app code until user says “do it”.  
**Rule:** no assumptions; ask in chat if missing.

## Status
- Have: 3 PDFs, 4 conversations, Diary repo (schema + data)
- Missing: Excel from Priyadarshini (still useful; Diary has sample/master-ish data already)
- Stack: not confirmed yet (SRS says Angular; Diary is Blazor Server + .NET 8 + Oracle)

## Diary checked (https://github.com/saibhavana456/Diary)
- Schema user: `DIT_DIARY`
- Relevant tables exist: `DEPARTMENTS`, `SECTIONS`, `BUDGET_UTILIZATION`, `PROJECT_MASTER` / `PROJECTS_MASTER`, `MANPOWER_DETAILS`, `EMPLOYEE_MASTER`, `TEAM_MASTER`
- `BUDGET_UTILIZATION` data in export: **151 rows**, FY **2025-26**, month **December** only, vertical **DIT**, **101 Capital / 50 Revenue**, **66 section names**, **149 project names**
- `DEPARTMENTS` data: **1 row** (DIT)
- `SECTIONS` data: **10 rows** (sample/demo names — not the same list as budget section strings)
- Diary budget UI today = dashboard/details + Excel upload — **not** Priyadarshini dual-panel monthly entry form

## Navigation (Priyadarshini) — use unless user says otherwise
1. Pick Department  
2. If **DIT** → choose **Capital** or **Revenue**; else Capital only  
3. Capital: Section → Project → entry form  
4. Revenue: Section only → entry form (no project)  
5. Submit saves DB; allotments come from DB; Capital hard-block over allotment (SRS); Revenue warn-only (client)

## DB first — yes, you are right
Correct order for this project:
1. **Finalize / design DB** (reuse Diary tables where fit; add only what Priyadarshini form needs that Diary lacks)
2. Seed/map master data
3. Then UI + server logic
4. Then validations / dashboard later as decided

Do **not** start UI coding before DB design is agreed.

## Gaps vs Priyadarshini form (must design — do not invent columns yet)
Diary `BUDGET_UTILIZATION` has: FY, Month, Vertical, Project, Fresh, Spillover, Actual, Type, Total, Section.  
Missing vs client form: previous/cumulative panel fields, next-month estimates, justification/PO text, revenue **expenditure-head** line items, clear allotment-master vs monthly-entry split.

## Open questions (answer in chat)
1. Build **inside Diary** or **new app** in this repo (share Diary Oracle)?  
2. MVP: entry-only first, or masters/hierarchy/dashboard too?  
3. Confirm stack: Blazor like Diary, or Angular like SRS?  
4. Excel still coming, or use Diary data only for now?  
5. Confirm: follow Priyadarshini navigation above?
