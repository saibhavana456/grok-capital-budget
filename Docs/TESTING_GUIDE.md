# How to test (IT Budget Portal) — after latest fixes

## SQL scripts to run (order)

In SQL Developer, connected as the app Oracle user (`SET DEFINE OFF`):

1. `Scripts/ALTER_PROJECT_SECTION_MAKER_CHECKER.sql` — if not already run  
2. `Scripts/CREATE_ENTRY_MONTH_UNLOCK.sql` — Admin month unlock table  
3. `Scripts/SEED_MAKER_CHECKER_ASSIGNMENTS.sql` — APP_USER + Maker/Checker on DIT projects/sections + non-DIT depts  
4. Optional: `Scripts/FIX_CHECKER_PENDING_600221.sql` — if Checker still sees 0 pending  

Restart the app after scripts.

### Sample logins (from seed)
| PF | Role | Use |
|---|---|---|
| 600110 | MAKER | DIT capital projects + revenue sections |
| 600221 | CHECKER | DIT capital + revenue |
| 600310 | MAKER | DIGIT (non-DIT dept-level) |
| 600321 | CHECKER | DIGIT |
| 100001 | ADMIN | Admin Masters |

Password: use your `appsettings.Development.json` / AD bypass password.

---

## Correct model (no assumptions)

| What | Where Maker/Checker live |
|---|---|
| **DIT Capital** | **Project** (not section) |
| **DIT Revenue** | **Section** (no projects on revenue — client confirmed section-wise) |
| **Other departments** | **Department** only (capital only, no revenue) |

**Non-DIT “only projects”:** DB still needs one parent section. App **auto-creates GENERAL** section when you save a non-DIT department. Admin **Projects** tab: pick department → Add Project (no Sections step needed).

**Revenue is NOT project-wise** — Priyadarshini: revenue = section-wise for DIT only.

---

## Test checklist

### Admin — Departments
1. Login 100001 → Admin Masters → Departments  
2. **Add Department** → only Code, Name, Maker, Checker (no Has Revenue)  
3. Try Code `DIT` → must **fail** (DIT already exists)  
4. Try same Name as existing department → must **fail**  
5. Create new non-DIT (e.g. `OPS` / `Operations`) with Maker 600310 / Checker 600321 → Save OK  
6. Edit existing **DIT** → no Maker/Checker fields; code read-only  

### Admin — DIT Sections (Capital vs Revenue)
7. Sections → select DIT → choose **Revenue** → Add Section with Rev Maker/Checker + allotment  
8. Choose **Capital** → Add Section (code/name only, no Rev M/C)  

### Admin — Projects
9. DIT → pick Capital section → Add Project with Cap Maker/Checker + spillover/fresh  
10. Non-DIT (OPS) → Projects → select OPS → General section auto → Add Project (no project M/C)  

### Admin — Month Unlock
11. Run unlock script; open **Month Unlock** tab  
12. Capital: pick project → enable a month past deadline with no PENDING/APPROVED entry  

### Checker — no Return
13. Login 600221 → Checker → open pending → only **Approve** and **Reject** (no Return)  
14. Reject with remark → Maker sees Resubmit on Submissions  

### Maker — month rules
15. Login 600110 → Portal → Capital  
16. If earlier FY months missing, submitting a later month must show “Submit earlier months first…”  
17. Month within deadline (until end of next calendar month) opens; after deadline needs Admin unlock  

### Over budget
18. Capital over allotment → blocked  
19. Revenue over allotment → info, allow submit  

---

## Scripts summary

| Script | Purpose |
|---|---|
| `ALTER_PROJECT_SECTION_MAKER_CHECKER.sql` | PROJECT/SECTION MAKER_PF + CHECKER_PF columns |
| `CREATE_ENTRY_MONTH_UNLOCK.sql` | Admin unlock table |
| `SEED_MAKER_CHECKER_ASSIGNMENTS.sql` | Users + assign Maker/Checker everywhere |
| `FIX_CHECKER_PENDING_600221.sql` | Repair checker pending data if needed |
