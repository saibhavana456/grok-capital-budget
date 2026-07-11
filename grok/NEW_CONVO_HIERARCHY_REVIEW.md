# New conversation review (hierarchy / login) — facts only

Cross-checked against: Priyadarshini, Manager, Savitha, PDFs/master sheet, Personal (SCV), Diary, current `IT_CAPITAL_FULL_SCHEMA.sql`.  
**No assumptions. No schema change until you confirm.**

---

## A. What the NEW conversation confirmed (said clearly)

### Login / landing (demo vs production)
- Demo login for flow: **PF number + password** (password can be bypass for demo).
- Speaker said demo login is temporary: *“at the end we won’t use the login page later”* — to show full flow.
- After login show: **Department first (auto)**, then **reporting chain**, then **section / project / team**.
- Name + PF visible after login.

### Department / section / project rules
- ~**15 departments**; **only DIT** has Capital **and** Revenue; other depts Capital only. *(Matches Priyadarshini.)*
- One person **cannot be in two departments** (except noted head/CEO edge — not for normal staff).
- DIT top of chain: **CTO** → then CGM / GM / DGM / AGM / CM → lower members.
- **One CTO per department.** After that: more GMs, more DGMs possible; CGM can be one or more.
- **CM can have more than one section.**
- Lower person (example Arjun): **one section**; can have **multiple projects**.
- Assistant manager can have **multiple projects**.

### Who can assign / fill
- **Till AGM:** can assign people and sections.
- **CM and below:** section is **assigned** — shown, **not a free “change section” dropdown**. If CM has 2 sections, **both displayed**; CM picks which section to work, then sees people + projects.
- **CM:** can **add projects** and **add team members** for their section(s).
- **Above CM (AGM+ / CGM etc.):** manage CMs/sections/people; **they don’t fill** the budget entry the same way; section “assignment UI” not the same as CM.
- **Lowest / project person:** sees own dept + chain + own section + own project(s) + **fill form**.
- Same project: if budget already written, **others on that project cannot write it** (they agreed “correct”).

### PF fetch
- Enter PF → **fetch name + designation** → can add to team. *(Matches manager.)*

### Still to confirm with client (“her”)
- Hierarchy display / reporting-chain UI — they said confirm with her before locking.

### Monday note
- Logic “almost correct”; need cleaner login-based flow (not select-to-simulate).

---

## B. Match vs earlier conversations

| Topic | Priyadarshini | Manager / Savitha | New convo | Status |
|-------|---------------|-------------------|-----------|--------|
| DIT only Revenue; Capital has projects | Yes | Yes | Yes | **Aligned** |
| CM → many sections | — | Yes | Yes | **Aligned** |
| PF → name/designation | — | Yes | Yes | **Aligned** |
| Soft delete / masters | — | Yes | (admin section pages shown) | Aligned direction |
| Capital hard-block / Revenue warn | Yes | — | Not discussed here | Still from client |
| Power BI by client; portal = entry | Yes | — | Not discussed here | Still from client |
| AD login (SRS / your Personal ask) | SRS AD | auto-fetch on login | **Demo = PF+password bypass** | **Conflict — need your call** |
| CTO in hierarchy | — | Diary has CTO | **Yes, CTO top of DIT** | **Our DDL missing CTO** |

---

## C. Personal (SCV) — login (re-checked in code)

Production path in Personal:
1. Username + password + captcha  
2. AD API `validateDomainUser`  
3. JWT + Oracle `USER_TOKEN`  
4. Staff/access via SQL Server `STAFF_DETAILS` (`EMP_ID`…) / `BRANCH_USER_ACCESS`  
5. `ADMINS(PF_NUMBER, IS_DELETED)` for admin list  

**Your earlier ask:** production login like Personal = **AD**.  
**New convo:** temporary **PF + password bypass** only to demo flow.  
→ For **production-ready** app: follow **Personal AD + JWT** (unless you say otherwise). Demo bypass can exist only for UAT if you want.

---

## D. Diary — re-checked (reference only)

| Item | Fact |
|------|------|
| Login | **None** (`anonymousAuthentication: true`) |
| `SECTIONS` | Columns: `TEAM_LEAD`, `AGM`, `DGM`, `GM`, `CGM`, **`CTO`**, `DEPT_ID` |
| `EMPLOYEE_MASTER` | `PF_NO`, `SECTION_ID` only |
| `MANPOWER_DETAILS` | Full staff by PF (name, dept, job, …) |
| Budget | `BUDGET_UTILIZATION` + MudBlazor **pie** on `BudgetUtilizationDashboard` — Excel/dashboard, **not** Priyadarshini monthly Capital/Revenue form |
| Charts | Many MudChart pie/donut/bar pages (Budget, Project, PMS, Network, etc.) — **UI reference only** |

---

## E. Gaps in OUR current DDL vs new conversation (must decide)

Current file: `grok/IT_CAPITAL_FULL_SCHEMA.sql`

| # | Issue | Why |
|---|--------|-----|
| 1 | **`CTO` not in `SECTION_AUTHORITY` roles** | New convo + Diary have CTO as department top |
| 2 | **`UNIQUE (SECTION_ID, AUTHORITY_ROLE)`** | New convo: **multiple GMs / multiple DGMs** allowed → one-role-per-section unique is **wrong** if we follow this |
| 3 | **No `REPORTS_TO` / chain table** | New convo needs reporting chain display; today we only store role+PF per section, not “who reports to whom” |
| 4 | **No employee master table** | PF fetch needs a source — still **OPEN** (you never chose 1/2/3) |
| 5 | **TEAM_LEAD / AM** | Mentioned in new convo / Diary; not in our role list |
| 6 | **Login tables** | We have `APP_USER` + `USER_SECTION_MAP`; we did **not** copy Personal `USER_TOKEN` (session) — needed if we mirror SCV login |
| 7 | **One writer per project/month** | New convo: if one wrote budget, others cannot — **not** in DDL yet (app rule or lock column — not confirmed how) |

**What already fits:** Dept/Section/Project; CM multi-section via maps; Capital project entry; Revenue section-only; soft delete `IS_ACTIVE`; DIT `HAS_REVENUE`.

---

## F. Still OPEN — need your answers (no guessing)

1. **Production login:** AD like Personal (recommended by your earlier ask + SRS), or PF+password only?  
2. **Employee / PF lookup:** (1) own employee table, (2) no table store name on row, (3) bank org DB like Personal `STAFF_DETAILS`?  
3. **Add CTO (and TEAM_LEAD?)** to authority roles?  
4. **Allow multiple people with same role** under a section/dept (multi GM/DGM)? → drop/change unique constraint?  
5. **Reporting chain storage:** add `REPORTS_TO_PF` (or similar) — yes/no?  
6. **Same project write lock:** enforce in DB/app that only one active writer per project+FY+month?

---

## G. Do not do yet
- No UI/app code until you say **do it**  
- No DDL change until you answer the OPEN items above  
