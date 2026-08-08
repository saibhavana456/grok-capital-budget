# Priyadarshini / Union Bank — FINAL plan (IT Budget Portal)

Last updated: 2026-08-08 (full call + Admin screenshots, including New Department DIT behaviour).

Use this file as agent memory. Prefer this over assumptions.

---

## Correction from latest images (point 2)

| Observation | Verdict |
|---|---|
| New Department + Code typed as exact `DIT` → Maker/Checker **hidden** | **Already correct** — you were right to correct yourself |
| New Department with empty/other code → Maker/Checker shown | Correct for non-DIT |
| Same **name** as existing DIT accepted | **Wrong** — must block duplicate department **name** (and **code**) |
| Typing Code `DIT` again while DIT already exists | **Wrong** — client: only **one** DIT; block second DIT entirely |
| Has Revenue dropdown on New Department | **Confusing** — remove for new depts (always capital-only / N). Existing DIT is special-cased, not re-created |

**Best approach (client: only one DIT, no other DIT-type dept):**
- **Do not** allow creating another DIT via Admin.
- New Department form = **other departments only**: Code, Name, Maker PF, Checker PF (no Has Revenue, no DIT path).
- Existing DIT row: Edit = Name (optional) only; Code read-only `DIT`; no Maker/Checker; Has Revenue fixed Y (read-only or hidden).
- Unique checks: department **Code** unique; department **Name** unique (trim, case-insensitive).

---

## Locked product rules

### Roles & login
- MAKER / CHECKER / ADMIN; PF + password + captcha.

### Maker / Checker
| Scope | Rule |
|---|---|
| DIT Capital | Project-wise |
| DIT Revenue | Section-wise (no projects) |
| Other departments | Department-wise Capital only — **no revenue** |
| Same project/section | Maker ≠ Checker |
| Scale | Maker 1–4, Checker 4+ (STAFF_DETAILS) |

### Checker actions
- **Approve** + **Reject** only — **remove Return**.
- Reject → Maker Resubmit on Submissions.

### Over allotment
- Capital → block; Revenue → info, allow.

### Month / deadline (decided on call)
1. Deadline for month M = last day of **next** calendar month (July → 31 Aug).
2. Sequential: cannot submit M until April…Previous(M) each have a submission (PENDING or APPROVED).
3. After deadline, month closed unless Admin enables:
   - Capital → **project-wise**
   - Revenue → **section-wise**
   - Only expired, not-yet-submitted months; no extra approval.

### Staff
- Oracle `STAFF_DETAILS` / EMPLID; empty data ≠ connection error.

---

## FINAL implementation plan (ordered)

### P0 — Checker + Department masters (do first)
1. Remove Return (UI + service); keep old RETURNED rows editable like Rejected.
2. **New Department** = non-DIT only:
   - Fields: Code*, Name*, Maker*, Checker* (+ View).
   - Remove Has Revenue from this form (always save `N`).
   - Remove DIT special banners / “type DIT to hide M/C” path.
   - If user types code `DIT` → error: “DIT already exists; use the existing DIT row.”
3. **Uniqueness:** reject duplicate Code or Name (active rows, case-insensitive).
4. **Edit existing DIT:** no Maker/Checker; Code read-only; Has Revenue not editable (Y).
5. **Edit other dept:** Maker/Checker editable with care; no Has Revenue control (stay N).

### P1 — DIT Sections / Projects Admin
6. After selecting DIT → choose **Capital** or **Revenue** first.
7. Revenue → Add/Edit Section with Rev Maker/Checker + allotment.
8. Capital → Add/Edit Section (code/name) → Projects with Cap Maker/Checker + allotment.
9. Full-width aligned filters/forms.

### P2 — Month window + Admin unlock
10. Deadline helper + open-if-within-deadline-or-unlocked.
11. Gap-fill before submit; redirect/message to earliest missing month.
12. Admin Month Unlock (project / section) + `ENTRY_MONTH_UNLOCK` table + script.

### P3 — Safety
13. Block/warn Maker/Checker PF change when PENDING/APPROVED entries exist.
14. Mid-year new project allowed; utilized on entry page only.

---

## Out of scope until client confirms
- Auto-expiry duration of Admin unlock (propose: until submit or Admin disables).
- Separate DB tables for capital-only vs revenue-only sections (one Section table + Capital/Revenue mode).

---

## Test checklist
- [ ] Cannot create second DIT (code DIT blocked)
- [ ] Cannot save duplicate department name or code
- [ ] New Department: no Has Revenue; Maker/Checker always shown
- [ ] Edit DIT: no Maker/Checker
- [ ] Checker: Approve/Reject only
- [ ] DIT Capital vs Revenue section flow
- [ ] Month deadline + gap fill + Admin unlock
- [ ] Rejected → Resubmit
