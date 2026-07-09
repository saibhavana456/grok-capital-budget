# BLOCKERS — Must resolve before implementation

## B1. Excel master data (USER SAID WILL PROVIDE)
**Need:** Excel with:
- Full department list (expected ~15)
- Sections per department (DIT ~48 mentioned)
- Projects per section (Capital)
- Capital allotment columns / sample values
- Revenue expenditure head list (full)
- Any mapping of which heads apply where (if restricted)

**Without this:** Cannot finalize DB seed, dropdowns, or validation examples.

## B2. Decision on MVP scope
Choose one (or write a custom split):
1. **MVP-A (Client-leaning):** Capital + Revenue entry screens + Oracle tables + save/fetch monthly data. No portal dashboard. No org hierarchy UI. Masters loaded via SQL/Excel.
2. **MVP-B (Manager-leaning):** Department/Section/Project masters + hierarchy/team + capital/revenue entry + dashboard filters; integrate with existing portal patterns.
3. **MVP-C (Phased):** Phase 1 = MVP-A; Phase 2 = masters/hierarchy/team; Phase 3 = dashboard/AD hardening.

**Need user to pick.**

## B3. BB / existing Project Management repo
Manager asked to inspect BB repo tables. **Not in this workspace.**

**Need one of:**
- Git URL / access to BB repo, or
- Exported DDL / screenshots of Project Master, Manpower, Budget Utilization tables, or
- Written confirmation: “ignore BB for now; greenfield Oracle schema only”.

## B4. Conflict resolutions
Need decisions on CONFLICT-01 through CONFLICT-11 in `memory/CONFLICTS.md` (especially 01, 02, 05, 06, 07, 08, 09, 10).

## B5. Environment facts (production)
Not provided yet — ask when implementation starts:
- Oracle connection / schema naming standards
- AD / SSO integration approach for bank intranet
- Hosting (IIS? internal URL?)
- Git branching policy of bank team
- Whether Angular + .NET Core 8 is mandatory for this repo (SRS says yes)

## B6. Sample end-to-end scenario
Need one worked example from Excel (one Capital project + one Revenue section) with:
- Allotments
- April/May historical entries
- June current entry
- Expected validation outcome

User said they will test on desktop and share inputs/outputs — that can satisfy B6 later, but schema design is safer with one example earlier.
