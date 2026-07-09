# Confirmed Requirements (Evidence-backed)

Only items below are treated as confirmed. Each line cites source. Anything not listed is either open or conflicting.

## A. Product goal
- Replace Excel-based monthly IT budget utilization collection with a web portal + centralized DB.  
  Sources: BRD Executive Summary; SRS Objective; Client call.

## B. Hierarchy (organizational)
- Structure: **Department → Section → (Project for Capital)**.  
  Sources: Presentation mockups; Client call; Manager briefing; Savitha call.
- ~**15 departments** (mockup shows 6 sample names only). Exact list = Excel (pending).  
  Source: Client call (Priyadarshini: “It’s 15”).
- DIT has many sections (client mentioned ~48 sections in Excel discussion). Exact list = Excel (pending).  
  Source: Client call.
- Capital: every section has **one or more projects**.  
  Source: Client call (Priyadarshini confirmed).
- Revenue: **section-level only — no project**.  
  Sources: Client call; Savitha call (aligned).

## C. Budget types
### C1. IT Capital Budget
- Applies across departments (not only DIT).  
  Source: Client call (non-DIT goes Department → Section → Project; no revenue branch).
- For **DIT**, after Department select, show choice: **IT Capital** vs **IT Revenue**, then Section (and Project only for Capital).  
  Source: Client call.
- Project-level allotment fields (read-only on entry screen; from DB/master):
  - Spillover Budget Allotted (FY)
  - Fresh Budget Allotted (FY)
  - Total Budget Allotted (FY) = Spillover + Fresh (**auto**)
  - Total Budget Utilized till &lt;previous period&gt; (from prior submissions / query)
  Sources: Presentation p5; SRS mockup; Client call.
- Monthly entry (user editable = current month only):
  - Actual Spillover utilization (current month)
  - Actual Fresh utilization (current month)
  - Actual Total (**auto** = spillover + fresh)
  - Estimated/Projected Spillover (next month)
  - Estimated/Projected Fresh (next month)
  - Estimated/Projected Total (**auto**)
  Sources: Presentation p5; SRS; Client call (cols 3 and 6 auto).
- Previous month panel is **read-only** and should represent **cumulative till previous month** (not only single previous month’s isolated spend), with month labels dynamic.  
  Source: Client call (agreed “Budget details till May” style).
- Optional previous-month navigation icon: discussed, **not finally approved** (ticket not approved at time of call). Treat as open.  
  Source: Client call.
- PO / Justification / invoice-style free text: required; can sit on **same page** below entry (client agreed).  
  Sources: BRD sample page; Client call.
- Capital over-allotment validation (SRS/BRD): **reject submit** if cumulative actuals would exceed allotted total; show error:  
  “Entered budget utilization exceeds the allocated budget. Please enter a valid amount within the approved budget limit.”  
  Sources: BRD p5–6; SRS validation section.

### C2. IT Revenue Budget
- **Only DIT** handles revenue budget (client).  
  Source: Client call.
- Flow: Department=DIT → choose Revenue → Section → entry (no project).  
  Source: Client call.
- Section-level allotment (read-only on form): Total Budget Allotted for FY; Budget Utilized till previous period (auto from prior entries).  
  Source: Presentation p7; Client call.
- Expenditure heads (examples seen): FMS, Rent, NER, AMC, ATS, MBR, etc. Full list = Excel (pending). Client said ~16–20 heads common to all sections.  
  Source: Client call.
- UI preference from client (later in call): prefer **show all heads as rows** on page (fill applicable; leave others empty/0) rather than only dropdown — developer freedom acknowledged, but client leaned to full list for clarity.  
  Source: Client call (Divya/Priya discussion).
- Revenue over-budget: **popup warning only; do not block submit** (ratification handled outside). Also no negative numbers.  
  Source: Client call.

## D. Master data ownership (client statement)
- New Oracle DB to be created (not reuse existing Excel as system of record).  
  Source: Client call.
- Allotment figures (spillover/fresh/total, revenue section allotment): client will **update in database** (Priyadarshini said she will update DB; **no admin page required for allotment** per that call).  
  Source: Client call.
- Monthly utilization: entered by section users via portal; saved to DB; next month previous/cumulative comes from DB.  
  Source: Client call.
- Dashboard / Power BI: client said **they** will connect DB and build dashboard after portal collects data; portal team focus = entry + storage.  
  Source: Client call.

## E. Manager / Savitha additions (organizational portal layer)
These were stated by Manager/Savitha and are **in scope for planning**, but several conflict with client scope — see Conflicts file.
- Department Master + Section Master (mapped).
- Section personnel hierarchy: CM, AGM, DGM, GM, CGM (or Reporting Authority 1–5).
- Team member maintenance under section (PF number → auto fetch name/designation; bulk upload; soft delete).
- Admin rights to create/update sections; CM can modify own team after login.
- One CM can handle multiple sections (1:many).
- Project Master mapped to Department + Section; project-wise capital allotment (spillover/fresh/total) maintained FY-wise in project master.
- On login, auto-fetch user’s department/section/projects (ownership-first).
- Monthly capital + revenue submission screens after masters exist.
- Soft delete only (Y/N).
- Dashboard filters: Department, Section, Project, FY, Month.
- Refer existing **BB / Project Management** tables for patterns (manpower, project master, budget utilization) — **repo not in workspace**.

## F. Out of scope (BRD/SRS)
- Direct financial transaction processing.
- External financial system integration (unless planned separately).

## G. Non-functional (SRS)
- HTTPS; DB encryption for sensitive data; AD login; logging; Git + code reviews; intranet only; 24/7 desirable; Windows portability.
