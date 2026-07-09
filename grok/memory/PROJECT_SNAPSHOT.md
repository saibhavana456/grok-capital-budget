# Project Memory — Snapshot

Last updated: 2026-07-09

## Product name
**IT Capital & Revenue Budget Monitoring and Management System**  
Union Bank of India — Department of Information Technology (and other departments for Capital).

## Ticket / docs
- PMS Ticket: **TEMP009275** (SRS)
- BRD: UBI_BRD_V1.0 (submitted 18.05.2026 by Priyadharshini T; DGM sign-off visible on BRD last page — Shri C G Narayanan)
- SRS: UBI_SRS v1.1 (doc version history shows prepared/reviewed 07.07.2025 in photographed cover; treat dates as document metadata only)

## Stated tech stack (from SRS — do not change without permission)
- Backend: **.NET Core 8 / C#**
- Frontend: **Angular**
- Database: **Oracle 19c**
- OS: Windows
- Access: **Intranet only**
- Auth: **AD login** (SRS security attribute)
- Reporting: Data Lake / **Power BI** integration (analytics)
- Performance: page response within **15 seconds**
- Environments: Dev / UAT / Prod identical

## People mentioned (from conversations — not a complete org chart)
| Person | Role in discussion |
|--------|--------------------|
| Priyadarshini T | Client / BRD owner (DIT) |
| Divya | Client side (joined call) |
| Sai Bharga | Developer (you) |
| Savitha | Teammate |
| Manager (“Sir”) | Architecture / masters / hierarchy requirements |
| Vallika Kandikanti | SRS prepared by (cover page) |
| Nilesh Patel | SRS reviewed by |
| Mentions for mail later: Ravi Raman, Sachidanand, Mishra (CM) — **confirm before using** |

## What exists in THIS repo today
- Empty app (README only) + planning/memory docs under `grok/` and root `CLAUDE.md`
- **No** BB repo clone
- **No** Excel master data
- **No** Oracle connection details
- **No** AD / SSO config

## Explicit user instruction
1. Make a **final plan first**
2. User will say **do it** later
3. User will test on desktop and share I/O
4. Only then change code in this repo
5. Agent may create `grok/` folder + CLAUDE.md / model files
6. **No assumptions**; ask if missing
