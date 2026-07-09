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
See plain-language explanation: `MVP_EXPLAINED.md`.

Choose one (or write a custom split):
1. **MVP-A (Client-leaning):** Capital + Revenue entry screens + DB + save/fetch/validate. No portal dashboard. No org hierarchy UI first.
2. **MVP-B (Manager-leaning):** Masters + hierarchy/team + entry + dashboard filters; integrate Diary patterns.
3. **MVP-C (Phased):** Phase 1 = A; Phase 2 = masters/hierarchy/team; Phase 3 = dashboard/AD.

**Also choose deployment shape:**
- **D1:** New app in `grok-capital-budget` repo (greenfield code)
- **D2:** Add feature modules into existing Diary (`saibhavana456/Diary`)
- **D3:** New app code, but reuse/share Diary Oracle schema (`DIT_DIARY`) where suitable

**Need user to pick A/B/C and D1/D2/D3.**

## B3. Diary / BB reference repo — PARTIALLY RESOLVED
User provided: https://github.com/saibhavana456/Diary  
Analysis saved: `sources/DIARY_REPO_ANALYSIS.md`

**Verified present:** DEPARTMENTS, SECTIONS, BUDGET_UTILIZATION, PROJECT_MASTER, PROJECTS_MASTER, MANPOWER_DETAILS, TEAM_MASTER, Budget dashboard/details Blazor pages, Team Management.

**Still need user decision:**
- Extend Diary’s `BUDGET_UTILIZATION` vs create new tables for Priyadarshini entry model
- D1 / D2 / D3 above

## B4. Conflict resolutions
Need decisions on CONFLICT-01 through CONFLICT-11 in `memory/CONFLICTS.md` (especially 01, 02, 05, 07, 08, 09).

**CONFLICT-06 (navigation):** Client path restated carefully in `sources/NAVIGATION_CLIENT_PRIYADARSHINI.md`. Waiting for user confirmation that we follow **client navigation** as source of truth for entry flow.

**CONFLICT-10 (BB repo):** Diary provided and inspected — update status to partially resolved; still need reuse vs copy decision.

## B5. Environment facts (production)
Not provided yet — ask when implementation starts:
- Oracle connection / schema naming standards (Diary uses `DIT_DIARY` — confirm if same)
- AD / SSO integration approach for bank intranet
- Hosting (IIS? internal URL?)
- Git branching policy of bank team
- Stack confirmation: SRS says Angular + .NET Core 8; **Diary is Blazor Server + .NET 8 + Oracle**. User will confirm later.

## B6. Sample end-to-end scenario
Need one worked example from Excel (one Capital project + one Revenue section) with:
- Allotments
- April/May historical entries
- June current entry
- Expected validation outcome

User said they will test on desktop and share inputs/outputs — that can satisfy B6 later, but schema design is safer with one example earlier.
