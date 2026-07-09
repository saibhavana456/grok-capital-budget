# BRD Notes (Ubi_1__compressed_dacb.pdf)

UBI_BRD_V1.0 — Project: IT Capital Budget Monitoring & Management System  
Business Vertical: DIT  
Submitted On: 18.05.2026  
Submitted By: Priyadharshini T  
Sign-off visible: DGM Shri C G Narayanan (last page)

## Executive summary
Digitize monitoring/planning; replace manual Excel consolidation of spillover/fresh, PO, invoice, status, milestones.

## Objectives
Automate monthly collection; accurate actual/estimated tracking; minimize Excel errors; real-time visibility; support FY planning.

## Scope
In: utilization collection; PO/invoice/status/milestones; monthly/yearly tracking; dept/section filter/display; web entry/reporting; DB + data lake/Power BI  
Out: direct financial txns; external financial integrations unless planned

## Solution requirements
Web app with dept/section dropdowns; display allotted spillover/fresh; previous utilization; estimates; user inputs for current + next month; centralized DB; Power BI/analytics

## Sample web page (BRD)
Dept/Section selectors; allotted/utilized/projected figures; enter actual/projected fields; **Justifications (PO details, invoice details, etc.)** textarea; Submit

## Definitions
Budget; Spill over; Fresh; Allotted (spillover/fresh); Actual current month; Projected next month

## Validation
Entered utilization must not exceed allotted spillover + fresh; else reject with specified error message; flag for review if required

## Flow
Entry → UI validation → DB → process dept/section → Power BI → dashboards → management decisions

## Impact
Operational/technical/financial/compliance/user impacts listed; audit trail & transparency called out
