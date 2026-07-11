# MEMORY

**MudBlazor confirmed. Schema: IT_CAPITAL.**  
DDL: `grok/IT_CAPITAL_FULL_SCHEMA.sql`  
Reviews: `grok/FINAL_SCHEMA_AND_REPO_REVIEW.md`, `grok/NEW_CONVO_HIERARCHY_REVIEW.md`  
Field-level FK image: `grok/diagrams/IT_CAPITAL_field_level_FK.png`

## Clarifications (user unsure — decided from docs only)
- **TOTAL:** Spillover + Fresh → Total (app-calculated).
- **Justification:** one column `JUSTIFICATION_TEXT`.

## Repo facts (re-verified)
- **Personal (SCV production):** captcha → AD `validateDomainUser` → JWT → `USER_TOKEN`; staff `STAFF_DETAILS` (EMP_ID); `ADMINS(PF_NUMBER,IS_DELETED)`.
- **Diary:** no login; `SECTIONS` has TEAM_LEAD/AGM/DGM/GM/CGM/**CTO**; `EMPLOYEE_MASTER`; `MANPOWER_DETAILS`; Budget pie via MudChart — not Priyadarshini form.

## New hierarchy convo (facts) — schema NOT updated yet
- CM multi-section; CM section assigned (display, not free change); CM adds projects+team.
- Lower user: one section, multi projects, fills form.
- Till AGM: assign people/sections. Above CM: manage, don’t fill entry.
- DIT top = **CTO**; multi GM/DGM allowed; one person one department.
- Demo login PF+password bypass temporary; production AD still per earlier ask/SRS — **OPEN**.
- Employee table / PF source — **OPEN**.
- DDL gaps vs new convo: no CTO; UNIQUE(section,role) conflicts multi-GM; no REPORTS_TO; no write-lock rule.

No UI code until user says do it.
