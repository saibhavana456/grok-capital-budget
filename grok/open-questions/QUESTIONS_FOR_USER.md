# Questions to ask (no assumptions)

Ask the user / client / manager. Do not invent answers.

## Data
1. Please upload the **Excel** (departments, sections, projects, capital fields, revenue heads).
2. Confirm amount unit: **Crore only** on UI and in DB, or store INR and display Crore?
3. Confirm FY definition (start month) and how months roll (Apr–Mar?).
4. For Capital cumulative check: is the rule  
   `(sum of all actual totals from FY start through previous months) + (current month actual total) <= (spillover allotted + fresh allotted)` ?
5. Revenue: confirm warn-only on exceed (never block).
6. Are PO / invoice / milestones free-text only, or structured columns?

## Scope
7. Which MVP? A / B / C from BLOCKERS.md?
8. Is portal dashboard in Phase 1 or only Power BI later?
9. Is Admin UI required in Phase 1 for Department/Section/Project, or SQL/Excel load only?
10. Is org hierarchy (CM→CGM) + team PF maintenance in Phase 1?

## Navigation
11. Final rule for DIT: Capital/Revenue selector **before** section (client) or **after** section/project (Savitha/manager)?
12. Non-DIT departments: Capital only — confirm.
13. Revenue never has projects — confirm final.

## Platform
14. Must this repo be **Angular + .NET Core 8 + Oracle 19c** exactly as SRS?
15. Provide BB repo / DDLs or confirm greenfield?
16. AD login required for first demo, or temporary app login?

## Process
17. Soft delete required on all masters from day 1?
18. Maker-checker on budget submit? Yes/No.
19. Who are official mail recipients for status (Priyadarshini, Divya, others)?
20. Is ticket/project formally approved to build now, or planning-only until approval?
