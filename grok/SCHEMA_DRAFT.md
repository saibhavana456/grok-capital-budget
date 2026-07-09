# SCHEMA DRAFT (for approval — no code yet)

**Stack confirmed:** MudBlazor + .NET (new app, not Diary).  
**Oracle style:** UPPER_SNAKE names (Diary pattern). Amounts: **Rs. in Cr.** → `NUMBER(20,2)`.  
**Rule:** Only fields evidenced below. Items marked **OPEN** need your yes/no before DDL.

---

## Sources re-checked for this draft
| Source | Used for |
|--------|----------|
| Client (Priyadarshini) | Navigation; Capital project entry; Revenue section+heads; PO/justification same page; allotment in DB; Capital hard-block; Revenue warn-only |
| Manager | Dept/Section/Project masters; CM/AGM/DGM/GM/CGM on section; team PF; login auto-fetch dept/section; soft delete; Admin vs CM rights; one CM → many sections |
| Savitha | Capital=project; Revenue=section; hierarchy display |
| Master sheet | 15 capital depts; DIT sections/projects; 53 DIT revenue sections; **18** revenue heads; Annexure-B justification text |
| Presentation/BRD/SRS | Capital spillover/fresh/total allotted; previous vs current month; next-month estimates; auto totals; AD login (SRS) |
| Diary | Reference patterns only — **not** copy `BUDGET_UTILIZATION` as-is (missing estimates, justification, 18 heads, proper FKs) |

---

## Why not reuse Diary `BUDGET_UTILIZATION` as-is
Diary has: FY, Month, Vertical, Project, Fresh, Spillover, Actual, Type, Total, Section.  
Missing vs confirmed needs: next-month estimates, previous/cumulative split, justification/PO, revenue 18 heads, FK to dept/section/project, separate allotment master vs monthly entry.

→ **New schema in new app** (can seed names from master sheet / Diary data later).

---

## PHASE 1 — required for Capital + Revenue entry (propose now)

### 1) `DEPARTMENT`
| Column | Type | Why |
|--------|------|-----|
| DEPT_ID | NUMBER PK | |
| DEPT_CODE | VARCHAR2(50) | optional stable code |
| DEPT_NAME | VARCHAR2(200) NOT NULL | master sheet names |
| HAS_REVENUE | CHAR(1) DEFAULT 'N' | client: only DIT has revenue → DIT='Y' |
| IS_ACTIVE | CHAR(1) DEFAULT 'Y' | manager: soft delete |
| CREATED_AT / UPDATED_AT | TIMESTAMP | audit |

### 2) `SECTION`
| Column | Type | Why |
|--------|------|-----|
| SECTION_ID | NUMBER PK | |
| DEPT_ID | NUMBER FK → DEPARTMENT | manager + client |
| SECTION_CODE | VARCHAR2(50) | optional |
| SECTION_NAME | VARCHAR2(300) NOT NULL | capital + revenue section names |
| IS_ACTIVE | CHAR(1) DEFAULT 'Y' | soft delete |
| CREATED_AT / UPDATED_AT | TIMESTAMP | |

**Note:** Hierarchy people (CM/AGM/…) → **Phase 2** columns or child table (manager). Phase 1 can run without them for entry dropdowns.

### 3) `PROJECT` (Capital only)
| Column | Type | Why |
|--------|------|-----|
| PROJECT_ID | NUMBER PK | |
| SECTION_ID | NUMBER FK → SECTION | client: project under section |
| PROJECT_CODE | VARCHAR2(50) | e.g. 10.2 |
| PROJECT_NAME | VARCHAR2(500) NOT NULL | master sheet |
| IS_ACTIVE | CHAR(1) DEFAULT 'Y' | |
| CREATED_AT / UPDATED_AT | TIMESTAMP | |

### 4) `PROJECT_FY_ALLOTMENT` (Capital master amounts — FY)
| Column | Type | Why |
|--------|------|-----|
| ALLOTMENT_ID | NUMBER PK | |
| PROJECT_ID | NUMBER FK | |
| FINANCIAL_YEAR | VARCHAR2(20) | e.g. 2025-26 |
| SPILLOVER_ALLOTTED | NUMBER(20,2) | presentation |
| FRESH_ALLOTTED | NUMBER(20,2) | presentation |
| TOTAL_ALLOTTED | NUMBER(20,2) | spillover+fresh (store or compute — **OPEN**) |
| Unique | (PROJECT_ID, FINANCIAL_YEAR) | |

Client: allotment updated in DB (not by normal user on form).

### 5) `CAPITAL_MONTHLY_ENTRY` (user monthly submit)
| Column | Type | Why |
|--------|------|-----|
| ENTRY_ID | NUMBER PK | |
| PROJECT_ID | NUMBER FK | |
| FINANCIAL_YEAR | VARCHAR2(20) | |
| ENTRY_MONTH | VARCHAR2(20) | Apr…Mar (Diary/SRS month names) |
| ACTUAL_SPILLOVER | NUMBER(20,2) | current month input |
| ACTUAL_FRESH | NUMBER(20,2) | current month input |
| ACTUAL_TOTAL | NUMBER(20,2) | auto = spillover+fresh (**OPEN** store vs compute) |
| EST_SPILLOVER_NEXT | NUMBER(20,2) | estimated next month |
| EST_FRESH_NEXT | NUMBER(20,2) | |
| EST_TOTAL_NEXT | NUMBER(20,2) | auto (**OPEN**) |
| JUSTIFICATION_TEXT | CLOB/VARCHAR2(4000) | BRD + client + Annexure-B (approvals + PO/vendor in one text — **OPEN** if split) |
| SUBMITTED_AT | TIMESTAMP | |
| SUBMITTED_BY | VARCHAR2(100) | until AD login Phase 2 |
| Unique | (PROJECT_ID, FINANCIAL_YEAR, ENTRY_MONTH) | one submit per project/month |

**Previous / “till previous month” panel:** **not stored separately** — computed from earlier `CAPITAL_MONTHLY_ENTRY` rows (client: fetch prior submissions).

### 6) `REVENUE_HEAD`
| Column | Type | Why |
|--------|------|-----|
| HEAD_ID | NUMBER PK | |
| HEAD_CODE | VARCHAR2(50) | |
| HEAD_NAME | VARCHAR2(100) NOT NULL | exactly the **18** confirmed names |
| DISPLAY_ORDER | NUMBER | |
| IS_ACTIVE | CHAR(1) DEFAULT 'Y' | |

### 7) `SECTION_FY_REVENUE_ALLOTMENT`
| Column | Type | Why |
|--------|------|-----|
| ALLOTMENT_ID | NUMBER PK | |
| SECTION_ID | NUMBER FK | DIT sections only in practice |
| FINANCIAL_YEAR | VARCHAR2(20) | |
| TOTAL_ALLOTTED | NUMBER(20,2) | presentation revenue “Total Budget Allotted” |
| Unique | (SECTION_ID, FINANCIAL_YEAR) | |

### 8) `REVENUE_MONTHLY_ENTRY` + `REVENUE_MONTHLY_ENTRY_LINE`
**Header**
| Column | Type | Why |
|--------|------|-----|
| ENTRY_ID | NUMBER PK | |
| SECTION_ID | NUMBER FK | no project (client) |
| FINANCIAL_YEAR | VARCHAR2(20) | |
| ENTRY_MONTH | VARCHAR2(20) | |
| SUBMITTED_AT / SUBMITTED_BY | | |
| Unique | (SECTION_ID, FINANCIAL_YEAR, ENTRY_MONTH) | |

**Line** (one row per head used / all 18)
| Column | Type | Why |
|--------|------|-----|
| LINE_ID | NUMBER PK | |
| ENTRY_ID | NUMBER FK | |
| HEAD_ID | NUMBER FK | |
| AMOUNT | NUMBER(20,2) | |
| Unique | (ENTRY_ID, HEAD_ID) | |

Previous-month head amounts: computed from prior month lines.

---

## PHASE 2 — manager login / hierarchy / team (design reserved, build after Phase 1 entry works)

### 9) `SECTION_AUTHORITY` (or columns on SECTION)
Manager: bind **CM, AGM, DGM, GM, CGM** (Diary SECTION has TEAM_LEAD/AGM/DGM/GM/CGM/CTO — **CM name OPEN**: use CM vs TEAM_LEAD).  
One CM → many sections (manager).

### 10) `SECTION_TEAM_MEMBER`
| Column | Evidence |
|--------|----------|
| PF_NO | manager + Diary EMPLOYEE_MASTER |
| SECTION_ID | |
| NAME, DESIGNATION | manager: auto-fetch from PF (Diary MANPOWER_DETAILS has NAME/JOB_DESC — **lookup source OPEN**) |
| ROLE | manager mentioned |
| IS_ACTIVE | soft delete (manager: soft delete only) |

### 11) Login / rights / roles
SRS: **AD login**. Manager: on login auto-fetch dept/section; Admin maintains till CM; CM edits team.  
**No AD attribute mapping confirmed yet** → tables deferred until bank AD details given.  
Possible later: `APP_USER`, `USER_SECTION_MAP`, role flags (ADMIN / CM / MEMBER).

### 12) Dashboard
Client: Power BI later. Manager: in-app filters. **No dashboard tables required for Phase 1 entry** — reporting from entry tables/views later.

---

## Validation (app logic, not only DB)
| Rule | Source |
|------|--------|
| Capital: YTD actuals + current actual ≤ spillover+fresh allotted → **block submit** | BRD/SRS |
| Revenue: sum of heads vs section allotment → **warn, allow submit** | Client |
| No negatives | Client |
| Totals auto | Client |

---

## OPEN questions (answer before final DDL)
1. **Phase 1 scope:** Entry tables 1–8 only first? Or also Phase 2 hierarchy/team/login now?  
2. Store `TOTAL_*` columns or compute only?  
3. Justification: **one** text field, or two (Approvals + PO/Vendor)?  
4. Month storage: month **name** (April) like Diary, or month **number**?  
5. Oracle schema/user name for new app? (Diary uses `DIT_DIARY` — new name **OPEN**)  
6. CM field name: `CM` or `TEAM_LEAD`?

---

## Proposed build order after you approve
1. You answer OPEN 1–6  
2. You say **approve schema / do it**  
3. Create Oracle DDL + seed DEPARTMENT/SECTION/PROJECT/REVENUE_HEAD from master sheet  
4. Then MudBlazor entry screens
