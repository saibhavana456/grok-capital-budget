# MVP options explained simply (what “2” meant)

You asked: *“Means what I don’t understand — we need to create all like database and UI and API everything from scratch?”*

## Short answer
**Yes — in `grok-capital-budget` we still have to build the product pieces** (database objects for this feature, screens, and server logic).  
That is true for every MVP option.

What A / B / C change is **how much feature scope** we build first — not whether we invent a bank from nothing.

Also important after checking Diary:
- Diary already has **some** related Oracle tables + a **Budget Utilization dashboard/details** module.
- Diary does **not** already implement Priyadarshini’s full monthly Capital/Revenue entry wizard.
- So “from scratch” for the **new entry experience** is still true; “from scratch for every table in the bank” is **not** true if we are allowed to reuse Diary schema/patterns.

## MVP-A — Client-leaning (smallest first delivery)
Build:
- DB tables needed for Capital monthly entry + Revenue monthly entry (new and/or extend Diary budget tables — **decision pending**)
- UI screens for Priyadarshini navigation + forms
- Server logic to save/load/validate

Skip for later (unless you say otherwise):
- Full CM/AGM hierarchy admin UI
- Team PF bulk management UI
- Portal dashboard (client said they will do BI)

## MVP-B — Manager-leaning (largest)
Everything in A, plus:
- Department/Section/Project master maintenance
- Hierarchy + team member maintenance
- Login auto-fetch ownership
- Dashboard filters inside the app
- Closer integration with Diary/portal patterns

## MVP-C — Phased (recommended default if unsure)
1. First deliver **A** (working monthly entry + DB)
2. Then add manager masters/hierarchy/team
3. Then dashboard/AD hardening

## What I still need from you for “2”
Pick **A**, **B**, or **C** (or write your own split).  
Separately tell me:
- Build as **new app in this repo**, or  
- **Add modules into Diary**, or  
- New app but **share Diary Oracle schema**

I will not choose that without your permission.
