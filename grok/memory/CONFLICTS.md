# Conflicts & Ambiguities (Do NOT silently pick a side)

Each item needs an explicit user/client/manager decision before coding.

## CONFLICT-01 — Scope of “what we build” in this repo
| Side | Claim |
|------|--------|
| Client (Priyadarshini) | Build **data entry portal** + DB storage. Dashboard/Power BI done by client side after DB ready. |
| Manager | Build masters + hierarchy + team + project master + capital/revenue entry + dashboard filters inside portal; integrate with existing portal/project management. |
| SRS/BRD | Includes entry **and** dashboard/Power BI integration language. |

**Impact:** Delivery boundary, screens, and timeline.  
**Status:** OPEN — need user decision on MVP vs full.

## CONFLICT-02 — Admin UI for master / allotment data
| Side | Claim |
|------|--------|
| Client | Allotment values updated **directly in DB** by Priyadarshini; **no admin page** for allotment. |
| Manager | Admin must create/update Department/Section masters; later admin powers for master data. |

**Impact:** Whether Admin module is in MVP.  
**Note:** These may both be true if Admin = org masters, and allotment = DB/script — but must confirm.  
**Status:** OPEN.

## CONFLICT-03 — Capital over-budget behavior
| Side | Claim |
|------|--------|
| BRD/SRS | **Hard reject** submit if entered utilization exceeds allotted (spillover+fresh). |
| Client call (capital example) | Discussed cumulative check vs allotted (35 Cr example) — aligned with not exceeding allotted. |

**Likely aligned** for Capital = hard block. Still confirm cumulative formula (YTD actuals + current entry ≤ allotted).  
**Status:** MOSTLY CLEAR for Capital; confirm formula with Excel examples.

## CONFLICT-04 — Revenue over-budget behavior
| Side | Claim |
|------|--------|
| Client | **Warning popup only**; do **not** restrict submit. |
| Capital SRS rule | Hard block (does not automatically apply to revenue). |

**Status:** Treat Revenue = warn-only **if** user confirms; do not apply Capital hard-block to Revenue without confirmation.

## CONFLICT-05 — Revenue heads UI
| Side | Claim |
|------|--------|
| Presentation mockup | Dropdown head + amount + “+ Row”. |
| Client (later) | Prefer show **all ~18 heads as rows**; fill what applies. |
| Manager | Dropdown of applicable heads (not all empty fields); heads managed in project/section master. |

**Status:** OPEN — pick UI pattern before build.

## CONFLICT-06 — When Capital vs Revenue choice appears
| Side | Claim |
|------|--------|
| Client | After selecting **DIT** department, show Capital/Revenue **before** section. Non-DIT → section directly (capital path). |
| Savitha | Budget type comes **after section** (and for capital after project); map capital/revenue on section/project. |
| Manager | Menus like “Submit Capital Budget” / “Submit Revenue Budget”; whoever has project can do capital; don’t hardcode “only DIT has capital”. |

**Client path carefully restated:** `sources/NAVIGATION_CLIENT_PRIYADARSHINI.md`  
**User asked to follow client conversation carefully for navigation.** Pending explicit confirmation: “use Priyadarshini navigation as source of truth”.  
**Status:** OPEN — awaiting user confirmation (leaning client per latest instruction).

## CONFLICT-07 — Does Revenue attach to Project?
| Side | Claim |
|------|--------|
| Client + Savitha (later alignment) | Revenue = **section only**, no project. |
| Manager (parts of briefing) | Revenue heads managed project-wise in project master; budget after project exists. |

**Status:** OPEN — client+Savitha lean section-only; manager text mixes project. Need final ruling.

## CONFLICT-08 — Dashboard ownership
| Side | Claim |
|------|--------|
| Client | Client builds dashboard with BI tools from DB. |
| SRS/BRD/Presentation | Portal dashboard mockups + Power BI integration requirements. |
| Manager | Dashboard with Dept/Section/Project/FY/Month filters in app. |

**Status:** OPEN — MVP include portal dashboard or DB-only for BI?

## CONFLICT-09 — Login / AD / auto-fetch
| Side | Claim |
|------|--------|
| SRS | AD login. |
| Manager/Savitha | Auto-fetch dept/section/projects from login; role-based edit rights. |
| Savitha early | “Don’t worry about login now”; still design tables with roles. |
| Client | Spoke in terms of selecting dept/section (manual dropdowns in mockups). |

**Status:** OPEN — mock login vs real AD for first delivery.

## CONFLICT-10 — Existing BB / Project Management reuse
| Side | Claim |
|------|--------|
| Manager/Savitha | Check BB repo / Project Management tables (manpower, project master, budget utilization); reuse patterns; may not create duplicate tables if already exist. |
| Client | New database; Excel will seed data. |
| User update | Provided Diary repo: https://github.com/saibhavana456/Diary |

**Diary inspection done** (`sources/DIARY_REPO_ANALYSIS.md`): related tables + budget dashboard exist; Priyadarshini entry form does **not** fully exist there.  
**Status:** PARTIALLY RESOLVED (repo found). Still need D1/D2/D3 decision (new app vs extend Diary vs share schema).

## CONFLICT-11 — Previous month display mode
| Side | Claim |
|------|--------|
| Client agreed | Show **cumulative till previous month** (“Budget details till May”). |
| Also discussed | Optional month-back icon to view April etc.; not finally approved. |
| Manager | Show previous month values as reference; current blank. |

**Status:** Cumulative-till-previous is preferred from client; navigation icon OPEN.

## AMBIGUITY-01 — Exact field list / Excel
Excel with departments, sections, projects, capital columns, revenue heads **not yet provided**. Cannot finalize schema or seed data.

## AMBIGUITY-02 — Units
Mockups use **Rs. Crore**. Confirm storage unit (Crore vs absolute INR) and decimal precision.

## AMBIGUITY-03 — Financial year calendar
Indian FY (Apr–Mar) assumed in banking context but **not explicitly confirmed** in docs we have. Confirm FY start and labeling (FY 2025-26 vs FY26).

## AMBIGUITY-04 — Maker-checker
Savitha/manager mentioned maker/checker terminology briefly then deferred. Confirm if dual-control approval is required for submissions.

## AMBIGUITY-05 — PO/Invoice/Milestones structure
BRD lists PO details, invoice details, project status, milestones. Client agreed a justification free-text on same page. Structured fields vs single textarea = OPEN.
