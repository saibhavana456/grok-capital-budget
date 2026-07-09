# MEMORY

**MudBlazor confirmed. Schema: IT_CAPITAL.**  
DDL: `grok/IT_CAPITAL_FULL_SCHEMA.sql`  
Final review: `grok/FINAL_SCHEMA_AND_REPO_REVIEW.md`  
Field-level FK image: `grok/diagrams/IT_CAPITAL_field_level_FK.png`

## Clarifications (user unsure — decided from docs only)
- **TOTAL:** Client never said they enter Total first. Presentation/client: Total = Spillover + Fresh (auto). DB stores spillover + fresh; TOTAL columns filled by **app calculation** only.
- **Justification:** BRD/client = **one** field on the page. Annexure-B only explains what to write inside it. DB = **one** column `JUSTIFICATION_TEXT` (not two).

## Repo facts (verified, no assumptions)
- **Personal (production SCV):** Angular 19 + .NET 8 + Oracle. Login = captcha → AD `validateDomainUser` → JWT → `USER_TOKEN`; staff from SQL Server `STAFF_DETAILS` (`EMP_ID`…); Oracle `ADMINS(PF_NUMBER,IS_DELETED)`, `LOGIN_TYPE`, `USERS`, `BRANCH_USER_ACCESS`, `STAFF_ROLES`.
- **Diary:** Blazor/MudBlazor/Oracle `DIT_DIARY`; **no real login** (`anonymousAuthentication: true`). Has `DEPARTMENTS`, `SECTIONS` (TEAM_LEAD/AGM/DGM/GM/CGM/CTO columns), `EMPLOYEE_MASTER(PF_NO,SECTION_ID)`, `MANPOWER_DETAILS`, `BUDGET_UTILIZATION` (dashboard/Excel — not Priyadarshini form).
- Manager login/hierarchy → map Personal AD/JWT pattern + Diary org shape into **new** IT_CAPITAL tables (`APP_USER`, `USER_SECTION_MAP`, `SECTION_AUTHORITY`, `SECTION_TEAM_MEMBER`). Do not edit Personal/Diary.

No UI code until user says do it.
