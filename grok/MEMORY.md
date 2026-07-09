# MEMORY

**No app code until user says final / do it.**  
**New app** (not edit Diary). Stack lean: **MudBlazor** (await final confirm).

## HAVE (checked)
- PDFs: Presentation UI, SRS, BRD, Final Master Sheet (Capital depts/sections/projects + DIT Revenue)
- Conversations: Client (Priyadarshini), Manager, Savitha
- Diary repo: schema/code/budget flow (reference only)
- Screenshot: Annexure-B Revenue (reference only)
- Navigation confirmed: DIT = Capital+Revenue; others Capital only; Capital=Section→Project; Revenue=Section only
- Revenue heads confirmed **18**: RENT, TAXES, ELECTRICITY CHARGES, PRINTING & STATIONERY, POSTAGE, BANDWIDTH, SMS/EMAIL/WHATSAPP, AMC, ATS, M&R, TRAINING, PETTY OFF, AUDIT, FMS, NW RENT, PROF FEES, INSURANCE, MISC
- PO/Justification (Annexure-B): Financial Approvals + Purchase Orders/Vendor details
- Capital form fields from Presentation/BRD/SRS: spillover/fresh/total allotted; previous/current month; estimates; auto totals; hard-block over allotment
- Revenue: section allotment; 18 heads; warn-only if over (client)

## NEED before coding (blocking)
1. **MudBlazor final confirm** (yes/no for new app stack)
2. **Actual Excel `.xlsx`** still not in workspace (only PDF + screenshot). Google Sheet was private. Needed especially if Capital allotment amount columns exist in Excel beyond the PDF.
3. **Phase 1 scope:** entry forms + DB only (client), or also masters/hierarchy/team/dashboard (manager)?
4. Explicit **“do it”**

## Next step (after 1–4)
**DB design first** (tables for new app) → your approval → then UI/API. No code until then.
