# FINAL PLAN — IT Capital & Revenue Budget Monitoring and Management System

**Status:** DRAFT FOR USER APPROVAL — **no application code until you say “do it”.**  
**Date:** 2026-07-09  
**Rule:** No assumptions. Items marked OPEN/BLOCKED stay blocked until you answer.

---

## 1. What this system is (from documents)

A bank intranet web application to replace Excel-based monthly collection of **IT Capital** and **IT Revenue** budget utilization for Union Bank of India (DIT and other departments for Capital), store data in **Oracle**, and support later analytics (Power BI / dashboard).

Primary ticket/docs: **TEMP009275**, BRD UBI_BRD_V1.0, SRS UBI_SRS v1.1, UI presentation by Priyadarshini.

---

## 2. Sources reviewed (complete list we have)

| # | Source | Used for |
|---|--------|----------|
| 1 | `Presentation_-_Updated_67b0.pdf` (8 pages) | Capital/Revenue entry UI + dashboard mock |
| 2 | `Compressed_Images_66e6.pdf` (SRS photos) | Tech stack, capital validation, NFR |
| 3 | `Ubi_1__compressed_dacb.pdf` (BRD photos) | Scope, PO/justification, validation, Power BI |
| 4 | Client conversation (Priyadarshini + Divya) | Real user flow Capital + Revenue |
| 5 | Manager briefing (EN + HI) | Masters, hierarchy, team, project master |
| 6 | Savitha conversation | Schema/UI clarification, conflicts |

**Not received yet:** Excel master file; BB/Project Management repo; Oracle/AD environment details.

---

## 3. Recommended delivery approach (industry-style, phased)

Because client scope and manager scope **conflict**, the safe production approach is **phased**, with an explicit MVP gate.

### Phase 0 — Discovery freeze (CURRENT)
- Ingest docs/conversations → memory
- List conflicts & blockers
- Wait for Excel + MVP choice + conflict decisions
- **No app code**

### Phase 1 — Foundation (after your “do it” + answers)
Greenfield schema in Oracle (or confirmed reuse from BB if provided):
1. Department Master  
2. Section Master (FK → Department, soft-delete flag)  
3. Project Master (FK → Section/Department; FY spillover/fresh/total) — Capital  
4. Expenditure Head Master — Revenue  
5. Monthly Capital Utilization (transaction)  
6. Monthly Revenue Utilization (transaction, by head)  
7. Audit columns on all tables (created/updated by/at; active Y/N)

Seed from Excel via controlled scripts (not hardcoded mockup lists).

### Phase 2 — Entry portal (core business value)
Screens aligned to approved navigation decision:
1. Department select  
2. DIT-only Capital vs Revenue branch (**if** CONFLICT-06 resolved as client)  
3. Section select (filtered)  
4. Capital: Project select → Capital entry form  
5. Revenue: Section entry form (heads)  
6. Submit → persist → next month cumulative/previous panel from DB  

Validations:
- Capital: **hard block** over allotment (SRS/BRD) — exact formula confirmed with Excel example  
- Revenue: **warn only** (client) — if confirmed  
- No negatives; auto totals; read-only allotment & previous panels  

### Phase 3 — Org / ownership layer (manager requirements)
Only if approved into scope:
- Reporting hierarchy (CM/AGM/DGM/GM/CGM)
- Team members by PF (lookup + bulk + soft delete)
- Admin maintenance for masters
- Login auto-fetch of dept/section/projects
- Role rights (Admin vs CM vs member)

### Phase 4 — Reporting
- Either portal dashboard (SRS mock) **or** DB views/feeds for Power BI (client statement)
- Filters: FY, Month, Department, Section, Project (as approved)

### Phase 5 — Production hardening
- AD login, HTTPS, encryption, structured logging, environment parity, code review gates

---

## 4. Proposed target architecture (SRS-aligned — confirm before build)

```
[Angular SPA]  --HTTPS/Intranet-->  [.NET Core 8 Web API]
                                         |
                                         v
                                   [Oracle 19c]
                                         |
                                         v
                              [Views / Data Lake feed] --> [Power BI]
```

Patterns to follow (bank/enterprise):
- Clear **master vs transaction** separation  
- Soft delete on masters  
- Server-side validation (never UI-only)  
- Audit trail on every write  
- Idempotent monthly submit rules (define: upsert per Dept/Section/Project/FY/Month?) — **OPEN**  
- No secrets in repo  

**Do not** start Angular/.NET scaffolding until you confirm stack for *this* repo (SRS says this stack; this git repo is currently empty).

---

## 5. Screen inventory (from presentation — subject to conflict decisions)

| Screen | Purpose | Editable? |
|--------|---------|-----------|
| Capital — Department | Pick department | Yes |
| Capital — Section | Pick section for dept | Yes |
| Capital — Project | Pick project for section | Yes |
| Capital — Entry | Allotment RO + previous RO + current entry + justification + submit | Partial |
| Revenue — Landing (DIT) | Enter revenue flow | Nav |
| Revenue — Entry | Allotment RO + previous heads RO + current heads + submit | Partial |
| Dashboard | KPIs/charts/filters | RO (ownership OPEN) |

Manager/Savitha additional screens (if Phase 3 approved): hierarchy display, team maintenance, admin masters, project master maintenance.

---

## 6. Data model sketch (logical only — not final DDL)

> Final columns wait for Excel.

**DEPARTMENT**  
`dept_id, dept_code, dept_name, is_active, audit…`

**SECTION**  
`section_id, dept_id, section_code, section_name, has_capital, has_revenue, is_active, audit…`

**PROJECT** (Capital)  
`project_id, section_id, project_code, project_name, is_active, audit…`

**PROJECT_FY_BUDGET** (Capital allotment)  
`project_id, fy, spillover_allotted, fresh_allotted, total_allotted (computed/stored), audit…`

**SECTION_FY_REVENUE_BUDGET**  
`section_id, fy, total_allotted, audit…`

**EXPENDITURE_HEAD**  
`head_id, head_code, head_name, is_active…`

**CAPITAL_MONTHLY_ENTRY**  
`id, project_id, fy, month, actual_spillover, actual_fresh, actual_total, est_spillover_next, est_fresh_next, est_total_next, justification_text, submitted_at, submitted_by, audit…`

**REVENUE_MONTHLY_ENTRY**  
`id, section_id, fy, month, submitted_at, submitted_by…`

**REVENUE_MONTHLY_ENTRY_LINE**  
`entry_id, head_id, amount…`

**Optional Phase 3:** EMPLOYEE, SECTION_AUTHORITY, SECTION_TEAM_MEMBER, USER_ROLE_MAP

---

## 7. Capital entry business rules (confirmed direction)

1. Show project context (Dept/Section/Project) read-only after selection.  
2. Show FY allotment (spillover, fresh, total) from master/DB.  
3. Left panel: cumulative / previous period figures — read-only, dynamic labels.  
4. Right panel: current month actuals + next month estimates — user input.  
5. Totals auto-calculate (1+2→3, 4+5→6).  
6. On submit: save; next month left panel derives from DB.  
7. Validation: do not allow Capital submit that exceeds allotted (SRS/BRD hard rule) — confirm cumulative formula.  
8. Justification/PO text on same page (client agreed).

---

## 8. Revenue entry business rules (confirmed direction from client)

1. DIT + Revenue path; section only.  
2. Show section FY allotment + utilized-till-previous (auto).  
3. Enter amounts by expenditure head for current month.  
4. UI pattern OPEN (all rows vs dropdown+add).  
5. Exceed allotment → **warning popup, still allow submit** (client) — confirm.  
6. No negative numbers.

---

## 9. What we will NOT do until permission

- No Angular/.NET project scaffolding  
- No DB DDL applied to any environment  
- No inventing department/section/project/head lists from mockups alone  
- No BB repo changes  
- No “resolving” conflicts by picking a side silently  

Allowed now: planning docs, `grok/` memory, questions.

---

## 10. Implementation order (only after you approve + unblock)

1. Confirm MVP letter (A/B/C) + conflict answers  
2. Ingest Excel → canonical reference tables in `grok/sources/`  
3. Produce DDL + ERD for approval  
4. Produce API contract + screen wire acceptance checklist  
5. Scaffold solution (Angular + .NET + Oracle) per SRS  
6. Implement masters seed + Capital entry vertical slice  
7. Implement Revenue entry vertical slice  
8. Add validations + audit  
9. (Optional) hierarchy/team/admin  
10. (Optional) dashboard / BI views  
11. UAT with your desktop I/O samples  
12. Prod hardening (AD, HTTPS, logging)

---

## 11. Acceptance gates (production discipline)

| Gate | Exit criteria |
|------|----------------|
| G0 | Excel received; blockers B2–B4 answered |
| G1 | DDL approved by you (and manager if required) |
| G2 | Capital E2E submit + next-month fetch demo |
| G3 | Revenue E2E submit + warning behavior demo |
| G4 | Validation cases (under/exact/over) signed off |
| G5 | UAT sign-off on intranet test env |
| G6 | Prod checklist (AD, backup, logging, rollback) |

---

## 12. Immediate ask to you

Please reply with:
1. The **Excel** file  
2. MVP choice: **A / B / C** + deployment shape **D1 / D2 / D3** (see `open-questions/MVP_EXPLAINED.md`)  
3. Confirm navigation: follow **Priyadarshini path** in `sources/NAVIGATION_CLIENT_PRIYADARSHINI.md`? (Yes/No)  
4. Answers still needed: conflicts **01, 02, 05, 07, 08, 09** (10 partially resolved via Diary)  
5. Stack: confirm later is fine — note Diary is **Blazor/.NET8/Oracle**, SRS text says **Angular/.NET8/Oracle**  

### Update after Diary inspection
- Reference patterns exist (Departments, Sections hierarchy fields, Budget Utilization Capital/Revenue dashboard, Project masters, Manpower PF).
- Priyadarshini dual-panel monthly entry + revenue heads + justification is **not** fully implemented in Diary today.
- “From scratch” = yes for the new entry product behavior; not necessarily yes for inventing every bank table if D2/D3 chosen.

Until then I will **not** write application code.
