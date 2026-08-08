# Priyadarshini / Union Bank — FINAL decisions + what was built

Last updated: 2026-08-08

## Corrections (this chat)

1. **Revenue Maker/Checker = SECTION-wise for DIT** (not project-wise). Client confirmed: under Revenue there are sections only, no projects.
2. **Capital Maker/Checker = PROJECT-wise for DIT.**
3. **Non-DIT “only projects”:** no real Sections UX. Saving a non-DIT department **auto-creates a GENERAL section** so Admin can Add Project under the department without managing sections.

## Built

- Remove Checker **Return** (Approve + Reject only)
- New Department = non-DIT only; block second DIT; unique Code + Name
- Edit DIT: no department Maker/Checker
- DIT Sections: Capital | Revenue mode first
- Non-DIT Projects via auto General section
- Month deadline = end of next calendar month + sequential gap fill + Admin Month Unlock tab
- Scripts: `CREATE_ENTRY_MONTH_UNLOCK.sql`, `SEED_MAKER_CHECKER_ASSIGNMENTS.sql`
- Test steps: `Docs/TESTING_GUIDE.md`
