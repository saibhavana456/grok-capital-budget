# GROK.md — Model Working File (Grok)

## Purpose
Persistent working memory for this production planning engagement. Update this file whenever new facts, decisions, or blockers appear.

## Current mode
**PLANNING ONLY.** No application code until user explicitly says to implement.

## Repo state (verified)
- Repo: `github.com/saibhavana456/grok-capital-budget`
- Branch for planning docs: `cursor/planning-docs-and-memory-cf3f`
- Application code: **none yet** (only README + planning docs)
- Existing BB / portal repo: **not present in this workspace** — must be provided or linked

## Documents ingested (uploaded PDFs)
| File | What it is | Pages | Notes |
|------|------------|-------|-------|
| `Presentation_-_Updated_67b0.pdf` | UI mockups (Capital + Revenue + Dashboard) by Priyadharshini T | 8 | Best visual source for entry screens |
| `Compressed_Images_66e6.pdf` | SRS photos (UBI_SRS v1.1, TEMP009275) | 7 | Tech stack + capital validation + dashboard |
| `Ubi_1__compressed_dacb.pdf` | BRD photos (UBI_BRD_V1.0) | 8 | Scope, PO/justification, capital validation, Power BI |

## Conversations ingested
1. Client call: Sai Bharga ↔ Priyadarshini (+ Divya) — capital + revenue entry flow
2. Manager briefing (English summary + Hindi original) — masters, hierarchy, team, project master, budget entry
3. Savitha conversation — schema/UI clarification, capital vs revenue binding

## Excel
**NOT RECEIVED YET.** User said they will provide Excel with departments, sections, projects, capital columns, revenue columns/heads. Schema cannot be finalized without it.

## Immediate next user actions needed
See `grok/open-questions/BLOCKERS.md`.

## Do not forget
- Capital and Revenue are different concepts (project-wise vs section-wise).
- Client said dashboard/Power BI is **their** side after DB is ready; Manager still talks about dashboard filters in portal — conflict.
- Client said master allotment updated **directly in DB** (no admin UI for allotment); Manager wants Admin for Department/Section masters — different scopes.
- Capital validation: SRS/BRD = **hard block** if over allotment; Revenue (client) = **warning popup only, do not block**.
- Revenue UI: client later preferred **show all expenditure heads as rows** (not only dropdown+add row). Manager preferred dropdown of applicable heads. Conflict.
