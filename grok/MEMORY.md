# MEMORY

**MudBlazor. Schema: IT_CAPITAL.** Plan: `grok/BUILD_PLAN.md`  
DDL: `grok/IT_CAPITAL_FULL_SCHEMA.sql` — freeze after open answers. No UI until “do it”.

## Latest BRD (Pdf_new… 11 pages, Priyadarshini UB_BRD_V1.0, With Revenue Updated)
- Capital + Revenue web entry → validate in UI → Oracle DB → Power BI/Data Lake
- Capital: spillover/fresh/estimates; hard block if actual > allotted (spill+fresh)
- Revenue: DIT heads (FMS/AMC/ATS/NER/…); dynamic rows; hard block + exact error message
- Scope lists PO/invoice/milestones (Capital) — sample UI does not show separate fields
- Maker-Checker **not** in this BRD (is in other Word photo doc) → OPEN
- Out of scope: financial txn processing; external financial system integration unless planned

## Personal SCV (reuse pattern only)
- AD validateDomainUser + captcha + JWT + USER_TOKEN
- EncryptoData AES; SQL Server STAFF_DETAILS + login_question; Oracle app DB
- Finacle NOT for login

## Diary (reference only)
- DEPARTMENTS/SECTIONS pattern; BUDGET_UTILIZATION similar only; no login; MudBlazor/.NET8

## Locked earlier
- Month names; 18 heads from master; TOTAL calc; one justification; AD+captcha login; STAFF_DETAILS lookup; hierarchy tables designed; write lock UK

## Must confirm before build
1. Maker-Checker yes/no
2. PO/invoice/milestones separate vs justification only
3. Revenue hard-block (BRD) vs earlier warn-only talk
