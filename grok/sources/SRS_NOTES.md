# SRS Notes (Compressed_Images_66e6.pdf)

Document: Software Requirements Specification — IT Capital Budget Monitoring and Management System — TEMP009275  
UBI_SRS v1.1 (template history also shown)

## Objectives / Scope (aligned with BRD)
- Automate monthly collection/consolidation
- Track actual vs estimated vs allotted
- Reduce Excel dependency
- In scope: spillover & fresh utilization; PO/invoice/status/milestones capture; monthly/yearly tracking; dept/section filtering; web entry; DB + data lake/Power BI
- Out of scope: direct financial transactions; external financial integrations unless planned

## Tech stack (explicit)
- Backend: .Net Core 8
- Frontend: Angular
- Database: Oracle 19C
- OS: Windows
- Access: Intranet only
- Auth note: AD login (security attributes)
- Performance: ≤ 15 seconds page response
- Git + code reviews; logging; HTTPS; DB encryption

## Functional highlights
- Department dropdown; Section dropdown for DIT
- Capital entry previous vs current panels
- Hard validation against allotted budget (reject over)
- Dashboard features + Power BI integration language

## Important
SRS text focus is **IT Capital**. Revenue is covered more in presentation + client call than in this SRS photo set.
