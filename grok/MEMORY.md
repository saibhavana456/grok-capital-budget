# MEMORY

**MudBlazor. Schema: IT_CAPITAL.** DDL: `grok/IT_CAPITAL_FULL_SCHEMA.sql`  
No UI code until user says do it. Do not invent features.

## Locked earlier
- TOTAL = spillover+fresh (app-calc). Justification = one field.
- Month names. 18 revenue heads. DIT only revenue. Soft delete `IS_ACTIVE`.
- Login: PF + AD password + captcha → JWT → USER_TOKEN (Personal/SCV).
- Staff from SQL Server STAFF_DETAILS (no Oracle employee master).
- Hierarchy tables + REPORTS_TO_PF + project assignment + write lock UK.

## New docs uploaded 2026-07-21 (photos of Word + Excel) — facts only
Files: `Excel_new_e529.pdf` (Capital master sheet FY 2026-27), `Photo_compressed_1__e327.pdf` (SRS-style UI/requirements).

### Same as Priyadarshini / earlier (NOT new)
- Home link → IT Budget portal
- Capital: Dept → Section → Project → details
- DIT: choose Capital or Revenue; non-DIT Capital only
- Revenue: Section only (no project)
- Capital page: allotment spillover/fresh/total read-only; previous month read-only; current month enter actuals + next-month estimates; auto totals
- Capital validation: actual ≤ allotted (spillover/fresh/total)
- Justification on page; 18 heads master; Oracle; Power BI reporting after DB

### NEW / clearer in this Word doc (not in our earlier locked list)
1. **Maker-Checker** for Capital AND Revenue: save pending → Checker Approve / Reject / Return
2. **One Maker + one Checker per department**; audit trails
3. Admin (DIT Admin Section) maintains masters: Department, Section, Project, Expenditure Head
4. Justification max **5000** characters (UI + DB aligned)
5. Revenue amounts: blank → store **0**; never NULL / blank / negative
6. Revenue UI: Add-row dropdown OR show all 18 heads; no duplicate head in one submit
7. Flow diagram: Portal → Oracle DB → BI views → Power BI dashboards (Oracle **19c** named)

### POSSIBLE CONFLICT — do not assume which wins
- Earlier Priyadarshini notes: Revenue over allotment = **warn, allow submit**
- New Word photo: total heads must not exceed section allotted + popup *"Revenue Utilization for the respective section has exceeded the allotted budget"*
→ Ask user/client before coding Revenue hard-block vs warn-only.

### Excel PDF
- Same Capital hierarchy master (Dept / Section / Project), amounts in **Rs. Crore excluding taxes**, sheet FY **2026-27** — master list reference, not a new screen type.
