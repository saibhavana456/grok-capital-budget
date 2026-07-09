# Final Schema + Personal / Diary Review

**No assumptions.** Everything below is from repo code/SQL, uploaded PDFs, or confirmed user decisions.

**DDL (create tables from this):** `grok/IT_CAPITAL_FULL_SCHEMA.sql`  
**Field-level FK image:** `grok/diagrams/IT_CAPITAL_field_level_FK.png`  
**Schema name:** `IT_CAPITAL` | **Stack:** MudBlazor + .NET 8 + Oracle (new app)

---

## 1. Personal repo (production) — what login actually does

Repo: `github.com/saibhavana456/Personal`  
App: Surprise Cash Verification — **Angular 19 UI + .NET 8 API + Oracle** (`SURPRISE_CASH_VERIFICATION`) + **SQL Server** org DB (`STAFF_DETAILS`).

### Login flow (verified in code)

1. UI (`login.component.ts`): username + password + captcha → encrypt → `authenticate`.
2. API (`UserManagementServices.validateUser`):
   - Decrypt credentials + captcha; captcha must match.
   - Config admin bypass **or** HTTP POST to **AD API** `…/validateDomainUser` (Basic auth to middleware service).
   - On success: create **JWT** (~120 min), write/check **`USER_TOKEN`** (blocks if previous session exists).
   - Authorize via SQL Server **`STAFF_DETAILS`** (designation/location rules) **or** Oracle **`BRANCH_USER_ACCESS`**.
3. Token stored in browser `sessionStorage`; API middleware validates JWT.

### Personal Oracle tables used for auth (from `SCV_UAT2.sql`)

| Table | Key columns (actual) | Role |
|-------|----------------------|------|
| `ADMINS` | `ID`, `PF_NUMBER`, `IS_DELETED` | Admin PF list + soft delete (`N`/`Y`) |
| `USER_TOKEN` | `USERID`, `LASTTOKEN`, `HASH_TOKEN` | Session / single-login |
| `LOGIN_TYPE` | `EMP_ID`, `USER_TYPE`, `LOCATION`, `EMP_DESGN`, … | Login type by employee |
| `USERS` | `USER_ID`, `USERNAME`, `DESIGNATION`, ZO/RO/BR codes | App users |
| `BRANCH_USER_ACCESS` | `RO_USERID`, `BRANCH_USERID`, `STATUS`, … | Extra access map |
| `STAFF_ROLES` | `ROLE_NAME`, `ROLE_CODE` | Role codes |

### Personal SQL Server (org) — staff lookup

`STAFF_DETAILS`: `EMP_ID`, `EMP_NAME`, `DEPTID`, `DEPT_ID_DESC`, `EMP_DESGN`, `EMP_DESGN_DESC`, location/region/branch fields, etc.

**Link to manager ask:** Personal proves production pattern = **AD validate → JWT → token table → PF/EMP lookup for org context**. It does **not** have Dept→Section→Project budget hierarchy for IT Capital.

---

## 2. Diary repo — what exists (no real login)

Repo: `github.com/saibhavana456/Diary`  
App: Blazor Server + MudBlazor + Oracle `DIT_DIARY`.  
`launchSettings.json`: **`anonymousAuthentication: true`** — no AD/JWT login like Personal.

### Relevant tables (from `exportfinalditdiary.sql`)

| Table | Columns (actual) | Notes |
|-------|------------------|-------|
| `DEPARTMENTS` | `DEPT_ID`, `DEPT_NAME`, `DEPT_HEAD`, `CREATED_AT` | Simple dept master |
| `SECTIONS` | `SECTION_ID`, `NAME`, `DESCRIPTION`, `TEAM_LEAD`, `AGM`, `DGM`, `GM`, `CGM`, `CTO`, `DEPT_ID`, timestamps | Hierarchy **as columns** (names, not PF rows) |
| `EMPLOYEE_MASTER` | `PF_NO`, `SECTION_ID` | PF ↔ section only |
| `MANPOWER_DETAILS` | `PF_NO`, `NAME`, dept/location/job/team… | Staff detail by PF |
| `TEAM_MASTER` | `TEAM_CODE`, `TEAM_NAME`, `CM_EMAIL`, `AGM_EMAIL`, `DGM_EMAIL`, … | Emails, not PF authority rows |
| `BUDGET_UTILIZATION` | FY, Month, Vertical, Project, Fresh, Spill, Actual, Type, Total, Section | Dashboard/Excel upload — **not** Priyadarshini dual-panel form; **no** next-month estimates, justification, 18 revenue heads, or proper FKs |
| `PROJECT_MASTER` / `PROJECTS_MASTER` | project inventory style | Not Capital allotment/entry model |

**No FOREIGN KEY constraints** found in Diary export DDL (logical links only).

---

## 3. Manager asks ↔ what exists in the two repos

| Manager ask | Personal | Diary | IT_CAPITAL (this schema) |
|-------------|----------|-------|---------------------------|
| AD login | Yes — `validateDomainUser` + JWT + `USER_TOKEN` | No (anonymous) | `APP_USER.AD_LOGIN_ID` + roles; **reuse Personal AD/JWT pattern at app layer** (not copy SCV tables wholesale) |
| Auto-fetch dept/section on login | Via `STAFF_DETAILS` / access tables (branch/RO world) | `EMPLOYEE_MASTER` PF→section; `SECTIONS.DEPT_ID` | `APP_USER.PF_NO` + `USER_SECTION_MAP` + `SECTION`/`DEPARTMENT` |
| Dept / Section / Project masters | No (different domain) | Dept + Section + project masters (different purpose) | `DEPARTMENT`, `SECTION`, `PROJECT` |
| CM / AGM / DGM / GM / CGM | Roles via `LOGIN_TYPE` / `STAFF_ROLES` (SCV roles) | Columns on `SECTIONS` (+ CTO); `TEAM_MASTER` emails | `SECTION_AUTHORITY` rows: role + **PF_NO** (one CM → many sections = many rows) |
| Team by PF + soft delete | `ADMINS.IS_DELETED`; access `STATUS` | `EMPLOYEE_MASTER`; `TEAM_MASTER_AUDIT.DEL_FLAG` | `SECTION_TEAM_MEMBER.IS_ACTIVE`; all masters `IS_ACTIVE` |
| Admin vs CM | `ADMINS` + `LOGIN_TYPE.USER_TYPE` | Not app-login based | `APP_USER.ROLE_CODE` = ADMIN / CM / MEMBER |
| Capital/Revenue entry form | N/A | `BUDGET_UTILIZATION` only (upload/dashboard) | `CAPITAL_MONTHLY_ENTRY`, `REVENUE_MONTHLY_*` |

**Conversation link:** Manager login/hierarchy requirements map to **Personal’s AD+JWT pattern** + **Diary’s org/hierarchy shape**, implemented cleanly in **new** `IT_CAPITAL` tables — not by editing Personal or Diary.

---

## 4. Final IT_CAPITAL tables (13) — all fields + connections

### Field-level FKs (exact)

1. `SECTION.DEPT_ID` → `DEPARTMENT.DEPT_ID`  
2. `SECTION_AUTHORITY.SECTION_ID` → `SECTION.SECTION_ID`  
3. `SECTION_TEAM_MEMBER.SECTION_ID` → `SECTION.SECTION_ID`  
4. `PROJECT.SECTION_ID` → `SECTION.SECTION_ID`  
5. `PROJECT_FY_ALLOTMENT.PROJECT_ID` → `PROJECT.PROJECT_ID`  
6. `CAPITAL_MONTHLY_ENTRY.PROJECT_ID` → `PROJECT.PROJECT_ID`  
7. `SECTION_FY_REVENUE_ALLOTMENT.SECTION_ID` → `SECTION.SECTION_ID`  
8. `REVENUE_MONTHLY_ENTRY.SECTION_ID` → `SECTION.SECTION_ID`  
9. `REVENUE_MONTHLY_ENTRY_LINE.ENTRY_ID` → `REVENUE_MONTHLY_ENTRY.ENTRY_ID`  
10. `REVENUE_MONTHLY_ENTRY_LINE.HEAD_ID` → `REVENUE_HEAD.HEAD_ID`  
11. `USER_SECTION_MAP.USER_ID` → `APP_USER.USER_ID`  
12. `USER_SECTION_MAP.SECTION_ID` → `SECTION.SECTION_ID`  

**Logical (not FK):** `APP_USER.PF_NO` ↔ `SECTION_AUTHORITY.PF_NO` ↔ `SECTION_TEAM_MEMBER.PF_NO` (same PF string for login/hierarchy lookup — same idea as Personal `EMP_ID` / `PF_NUMBER`).

### Confirmed business rules baked into DDL

- Month = **name** (`April`…`March`)  
- `TOTAL_*` = **app-calculated** (spillover + fresh), stored  
- Justification = **one** column `JUSTIFICATION_TEXT`  
- Previous-month panels = **computed** from prior rows (no extra tables)  
- Revenue: **18** seeded heads; DIT only via `DEPARTMENT.HAS_REVENUE='Y'`  
- Soft delete = `IS_ACTIVE` `'Y'`/`'N'`

---

## 5. What was NOT copied from Personal into IT_CAPITAL DDL

These are SCV-domain tables — **not** assumed for IT Capital unless you later ask to mirror them:

- `USER_TOKEN`, captcha/`LOGIN_QUESTIONS`, `BRANCH_USER_ACCESS`, annexure tables  
- Direct SQL Server `STAFF_DETAILS` table inside `IT_CAPITAL`  

**App implementation later** should follow Personal’s **AD → JWT → session** flow; staff name/designation lookup source (Diary `MANPOWER_DETAILS` vs bank org DB) stays **open until you confirm**.

---

## 6. Images

| File | What it shows |
|------|----------------|
| `grok/diagrams/IT_CAPITAL_field_level_FK.png` | **All fields** + **which column connects to which** (this deliverable) |
| `grok/diagrams/IT_CAPITAL_schema_diagram.png` | Earlier ER overview |
| `grok/diagrams/IT_CAPITAL_table_links.png` | Earlier 1–N link overview |
