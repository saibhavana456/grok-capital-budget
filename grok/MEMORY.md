# MEMORY

**MudBlazor confirmed. Schema: IT_CAPITAL.**  
DDL: `grok/IT_CAPITAL_FULL_SCHEMA.sql`

## Clarifications (user unsure — decided from docs only)
- **TOTAL:** Client never said they enter Total first. Presentation/client: Total = Spillover + Fresh (auto). DB stores spillover + fresh; TOTAL columns filled by **app calculation** only.
- **Justification:** BRD/client = **one** field on the page. Annexure-B only explains what to write inside it. DB = **one** column `JUSTIFICATION_TEXT` (not two).

No UI code until user says do it.
