# Navigation — Priyadarshini (Client) — Careful Restatement

Source: client conversation only. No manager/Savitha overrides applied here.

## Capital path (all departments)

```
1) Select Department
      │
      ├─ If department = DIT ──────────────────────────────────────────┐
      │                                                                │
      │   2a) Show TWO options:                                        │
      │       • IT Capital Budget                                      │
      │       • IT Revenue Budget                                      │
      │                                                                │
      │   If user picks IT Capital Budget:                             │
      │       3) Select Section (sections of DIT)                      │
      │       4) Select Project (projects under that section)          │
      │       5) Capital entry form (allotment RO + previous RO +      │
      │          current editable + justification + Submit)            │
      │                                                                │
      │   If user picks IT Revenue Budget:  → see Revenue path below   │
      │                                                                │
      └─ If department ≠ DIT ──────────────────────────────────────────┤
                                                                       │
          2b) Go directly to Section (no Capital/Revenue chooser)      │
          3) Select Section                                            │
          4) Select Project                                            │
          5) Capital entry form                                        │
```

**Client statements supporting this:**
- Only DIT handles revenue budget.
- For DIT: after department, show IT Capital and IT Revenue; then section (and project only for capital).
- For other than DIT: no revenue; go to section name directly.
- Every capital section has project(s); user picks the project they handle.
- After project selected, show uneditable dept/section/project + allotment figures from DB.
- User edits current-month actuals + next-month estimates; totals auto; submit saves DB.
- Justification/PO on same page bottom.

## Revenue path (DIT only)

```
1) Department = DIT
2) Choose IT Revenue Budget
3) Select Section (DIT sections)     ← NO project step
4) Revenue entry form
   - Total budget allotted (section, FY) from DB (RO)
   - Utilized till previous period from prior submissions (RO)
   - Current month amounts by expenditure head
   - Submit → save DB
```

**Client statements supporting this:**
- Revenue has **no projects**, only sections.
- Heads like FMS/AMC/ATS/Rent/… (full list in Excel — pending).
- Common head list; user fills applicable.
- Exceed section allotment → **warning popup only, do not block**.
- No negative numbers.

## What client said is NOT our portal job (their words)
- Dashboard/Power BI: they collect via portal, then client connects DB and builds dashboard/report.
- Allotment master values: Priyadarshini updates in DB (she said no admin page needed for that allotment update).

## Open UI detail (still from client, not decided finally)
- Revenue heads: mock used dropdown + “+ Row”; later preference to show all heads as rows.
- Capital previous panel: agreed cumulative “till &lt;month&gt;”; back-icon month browse discussed, not finally approved.
