# Open items — decisions (checked against conversations + Personal)

| # | Question | Decision | Evidence |
|---|----------|----------|----------|
| 1 | Production login | **AD: PF + password + captcha** → JWT → `USER_TOKEN` | You confirmed; Personal SCV same pattern (`validateDomainUser` + captcha + `USER_TOKEN`) |
| 2 | Employee / PF lookup | **No employee table in Oracle.** Lookup **SQL Server `STAFF_DETAILS`** | Personal `OrganisationsDbContext` → `STAFF_DETAILS` (`EMP_ID`, `EMP_NAME`, `DEPTID`, `EMP_DESGN_DESC`, …). You confirmed org SQL Server. |
| 3 | CTO / TEAM_LEAD | **CTO yes** (dept). **TEAM_LEAD yes** (section). AGM+ at **department**; CM at **section** | New convo: one CTO/dept; “team lead, CM, AGM…”. Section not required above CM. Manager: CM–CGM. Diary has CTO + TEAM_LEAD columns. |
| 4 | Multi GM/DGM | **Yes — allowed** | New convo Speaker 2: more than one GM, more DGMs; CGM one or two |
| 5 | `REPORTS_TO_PF` | **Yes — added** (nullable) | New convo needs “who reports to whom” display; multi-GM makes role-order alone insufficient |
| 6 | Write lock | **Yes — one entry per project+FY+month** | New convo: if budget written, others cannot write. DB: `UK_CAP_ENTRY` + `SUBMITTED_BY_PF` |

DDL updated: `grok/IT_CAPITAL_FULL_SCHEMA.sql` (17 Oracle tables).  
Diagram: `grok/diagrams/IT_CAPITAL_field_level_FK.png`
