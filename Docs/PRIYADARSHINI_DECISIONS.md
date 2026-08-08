# Priyadarshini / Union Bank — locked decisions (IT Budget Portal)

Last updated: 2026-08-08 (from full call transcript + Admin screenshots).

Use this file as agent memory across chats. Prefer this over assumptions.

---

## Confirmed product rules

### Roles & login
- Roles: **MAKER**, **CHECKER**, **ADMIN** only.
- Login: PF + password + captcha.

### Maker / Checker assignment
| Scope | Rule |
|---|---|
| **DIT Capital** | **Project-wise** Maker + Checker |
| **DIT Revenue** | **Section-wise** Maker + Checker (no projects on revenue) |
| **Other departments** | **Capital only** — **department-wise** Maker + Checker. **No revenue.** |
| Same project/section | Maker ≠ Checker |
| Overlap | Same PF may be Maker on one project and Checker on another |
| Scale (STAFF_DETAILS) | Maker 1–4, Checker 4+ |

### Only one DIT
- Only **one** department is DIT (code `DIT`): has Capital **and** Revenue.
- Do **not** allow creating another DIT / another department with Has Revenue = Y.
- Other departments: capital only; never show/set Has Revenue as editable Y.

### Budget entry rules (already implemented unless noted)
- Capital over allotment → **block** submit.
- Revenue over allotment → **info**; allow submit.
- Checker: **Approve** and **Reject** only — **remove Return** (client confirmed Reject covers it).
- After Reject → Maker can resubmit (Resubmit on Submissions page).

### Month / deadline rules (NOW decided — was deferred)
1. **Deadline for entry month M** = last day of the **next** calendar month.  
   Example: July → deadline **31 Aug**; August → deadline **30 Sep**.
2. Maker may open/submit months that are still **within deadline** (typically current + previous while previous deadline not crossed).
3. **Sequential gap fill (mandatory):** cannot submit month M until **all earlier FY months** (April → month before M) have a submission.  
   Example: trying August with July missing → must do July first; if May/June missing → those first, in order.
4. **Admin enable (expired months):** if deadline for a month has passed and that month is still not submitted, Admin can **enable** that month so Maker can submit.  
   - Capital → enable **project-wise**  
   - Revenue → enable **section-wise**  
   - Show only months whose **deadline already expired** (and not yet submitted).  
   - Direct Admin action — **no** extra approval workflow.

### Admin masters UX (from call + images)
1. **DIT department edit:** do **not** show department Maker/Checker (Capital on Projects, Revenue on Sections).  
   Bug seen: editing with code like `DIT 01` still showed Maker/Checker because DIT check is exact code `DIT`.
2. **Has Revenue:** do not let other departments flip Has Revenue to Y in Edit (mistake risk).
3. **DIT add section:** after selecting DIT, Admin must first choose **Capital** or **Revenue**:  
   - **Revenue** → create/edit **section** with Revenue Maker/Checker + allotment (no projects).  
   - **Capital** → create/edit **section** (folder), then **Projects** under it with Capital Maker/Checker + allotment.
4. Layout: use full width; Department / Capital|Revenue / actions aligned in one filter row.
5. New mid-year project allowed; utilized-till starts at 0 on entry page (not on Admin create).
6. Changing Maker/Checker after submissions exist is sensitive — lock or warn (see plan).

### Staff lookup
- Oracle `STAFF_DETAILS` by `EMPLID` (optional SQL Server OrganisationsDb).
- Empty table → “PF not found” — **not** a connection error.

---

## Implementation plan (ordered)

### P0 — Safe / clear (do first)
1. **Remove Return**
   - Hide Return on Capital/Revenue View (Checker).
   - Checker services: accept only APPROVED / REJECTED.
   - Keep `RETURNED` in DB/status chips for old rows; treat as editable like Rejected for Maker resubmit.
2. **Fix DIT department edit (image 1)**
   - Treat department as DIT if `DeptCode == DIT` (case-insensitive) **or** existing HasRevenue=Y singleton.
   - On DIT edit: hide Maker/Checker fields; clear them on save; lock Code to `DIT` (or read-only).
   - Hide/disable **Has Revenue** for non-DIT (force N). For DIT force Y read-only.
3. **Enforce only one DIT / no revenue elsewhere**
   - New department: no Has Revenue control (always N); block code `DIT` if DIT already exists.
   - SaveDepartment: reject HasRevenue=Y unless code is DIT; reject second DIT.

### P1 — Admin Capital vs Revenue section flow (image 2)
4. When filter department is DIT, require **Budget type** = Capital | Revenue before Add Section.
5. Revenue mode: section form shows Rev Maker/Checker + revenue allotment; list Rev columns.
6. Capital mode: section form is Code/Name only; Cap Maker/Checker only on Projects tab under that section.
7. Relax save rule: do not require Rev M/C when creating a capital-folder section; require Rev M/C when saving in Revenue mode.
8. Widen forms to full content width; filter row alignment (already partially done — complete for DIT budget-type).

### P2 — Month deadline + sequential submit + Admin enable (largest)
9. Replace “current + previous calendar month only” with:
   - `DeadlineEnd(month)` = last day of next calendar month.
   - `IsMonthOpenForSubmit` = within deadline **or** Admin-enabled for that project/section.
10. Before submit of month M: ensure April…Previous(M) each have an active entry (PENDING or APPROVED — confirm PENDING counts; recommend **PENDING or APPROVED**, not missing/REJECTED-only).
11. Portal/entry UX: if gap, message + navigate Maker to earliest missing month.
12. New Admin UI tab or panel: **Month unlock**
    - Capital: pick Project → list expired unsubmitted months → Enable / Disable.
    - Revenue: pick Section → same.
13. New table e.g. `ENTRY_MONTH_UNLOCK` (ProjectId nullable, SectionId nullable, FinancialYear, EntryMonth, IsEnabled, EnabledBy, EnabledAt).
14. Scripts + no break to existing APPROVED/PENDING data.

### P3 — Maker/Checker edit safety (after P0–P2)
15. If project/section has PENDING/APPROVED entries: allow edit name/code/allotment; **block or strong-confirm** Maker/Checker PF change.
16. New project mid-year: allowed; allotment fields on project; utilized shown only on entry.

### Explicitly out of scope until client confirms
- Separate physical “capital-only section” vs “revenue-only section” tables (prefer one Section table + Capital/Revenue admin mode).
- Duration of Admin enable auto-expiry (propose: stays until submit or Admin disables).
- Email/daily follow-up for late sections (ops process, not portal).

---

## How month logic works (examples)

| Today | July | August | June (deadline 31 Jul) |
|---|---|---|---|
| 4 Aug | Open (deadline 31 Aug) | Open (deadline 30 Sep) | Closed unless Admin enable |
| Submit Aug | Blocked if Jul (and earlier gaps) missing | — | Must fill earliest gap first |

---

## Image findings (no assumptions)

1. **Edit DIT shows Maker/Checker** — wrong. Form code `DIT 01` ≠ `DIT` so UI thought non-DIT. Fix DIT detection + hide fields.
2. **New Section under DIT jumps straight to Revenue M/C** — wrong per call. Need Capital | Revenue choice first.
3. Empty right-side space — widen Admin forms / filter row.

---

## Test checklist (after implementation)
- [ ] Checker: only Approve / Reject; no Return button
- [ ] Edit DIT: no Maker/Checker; Has Revenue locked Y; code stable
- [ ] New non-DIT dept: no Has Revenue; Maker/Checker required
- [ ] Cannot create second DIT
- [ ] DIT → Revenue → Add Section with Rev M/C
- [ ] DIT → Capital → Add Section (no Rev M/C) → Add Project with Cap M/C
- [ ] Aug blocked if Jul missing; unlock path via Admin after deadline
- [ ] Rejected → Resubmit still works
