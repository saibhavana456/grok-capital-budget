# MEMORY

**MudBlazor. Schema: IT_CAPITAL.** DDL: `grok/IT_CAPITAL_FULL_SCHEMA.sql` (17 Oracle tables)  
Resolved opens: `grok/OPEN_ITEMS_RESOLVED.md`  
FK image: `grok/diagrams/IT_CAPITAL_field_level_FK.png`

## Locked
- TOTAL = spillover+fresh (app-calc). Justification = one `JUSTIFICATION_TEXT`.
- Month names. 18 revenue heads. DIT only revenue. Soft delete `IS_ACTIVE`.

## Login / staff (confirmed)
- Production: **PF + AD password + captcha** → AD `validateDomainUser` → JWT → `USER_TOKEN` (Personal/SCV pattern).
- Captcha questions: `LOGIN_CAPTCHA_QUESTION` in Oracle.
- **No** employee master in Oracle. Name/designation from **SQL Server `STAFF_DETAILS`** (`EMP_ID`=PF).

## Hierarchy (confirmed from new convo + manager)
- `DEPARTMENT_AUTHORITY`: CTO (one), CGM, multi GM, multi DGM, AGM + `REPORTS_TO_PF`
- `SECTION_AUTHORITY`: CM, TEAM_LEAD + `REPORTS_TO_PF` (CM multi-section)
- `SECTION_TEAM_MEMBER`, `PROJECT_ASSIGNMENT`
- Write lock: UK one Capital entry per project+FY+month; `SUBMITTED_BY_PF`

No UI code until user says do it.
