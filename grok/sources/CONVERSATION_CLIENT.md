# Client Conversation Notes (Priyadarshini + Divya with Sai Bharga)

Transcript quality: noisy voice-to-text. Below is careful extraction of **requirements only** (no invented details).

## Documents client referenced
- Capital-focused BRD/SRS already shared
- Updated presentation with screenshots (Capital + Revenue)
- Excel to be shared (departments/sections/projects/fields/heads) — **critical**
- Revenue BRD: client said revenue BRD “not there” as separate signed BRD in one moment; revenue explained via presentation + Excel

## Department / section / project
- ~15 departments (mock shows 6)
- DIT included; sections under DIT many (Excel; ~48 mentioned in discussion)
- Capital: after dept + section → projects under section (example IT Security has multiple projects)
- User selects project they handle → entry page

## Capital entry
- Dept/Section/Project display = uneditable after selection
- Allotment spillover/fresh/total + utilized-till = from DB, uneditable on form
- New DB to create; Priyadarshini will update allotment data in DB
- Client said **no admin page needed for allotment** (DB update)
- Current month fields entered by user; saved; next month become previous/cumulative
- Prefer label like “Budget details till May” (cumulative), current month separate
- Auto-calc totals (cols 3 and 6)
- Over allotment should not be allowed (example with 35 Cr)
- Previous-month back icon discussed; not finally approved (ticket approval pending at time)
- After submit → save DB
- PO/Justification: required; keep on **same page** bottom (client agreed vs separate page)
- Dashboard: client will build with BI after data collected; portal = entry + storage

## DIT Capital vs Revenue branching
- Only DIT has Revenue
- If DIT selected → show IT Capital and IT Revenue options, then section
- Non-DIT → section directly (capital path)
- Capital: section → project
- Revenue: section only (**no project**)

## Revenue entry
- Section allotment total from DB; utilized-till from prior submissions
- Expenditure heads (~16–20): FMS, AMC, ATS, Rent, NER, etc. (full list in Excel)
- Common head list for every section; users fill applicable
- UI: mock had dropdown + add row; later preference to show all heads as rows
- Validation: no negatives; if sum exceeds section allotment → **popup warning, do not block** (ratification outside)

## Other
- Possible older ~70% developed attempt 2 years ago — client unsure; treat as unknown unless code provided
- Contact during working hours; weekends/holidays avoid
- Mail status to broader group later (names mentioned; confirm before use)
