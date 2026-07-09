# CLAUDE.md — Agent Operating Rules (Production Project)

## Identity
- This repo is for **Union Bank of India — IT Capital & Revenue Budget Monitoring and Management System** (PMS Ticket TEMP009275 / related).
- Working agent folder for notes/memory: `/workspace/grok/`
- Model working notes for Grok: `/workspace/grok/GROK.md`

## Hard rules (non-negotiable)
1. **No application code changes without explicit user permission.** Planning docs and `grok/` notes are allowed; product/app code is not, until the user says “do it”.
2. **No assumptions.** If a fact is missing, incomplete, or conflicting, record it under Open Questions and ask the user. Do not invent departments, sections, projects, heads, amounts, roles, or tech choices.
3. **Always check what we already have** before asking again. Sources of truth live in:
   - `/workspace/grok/memory/`
   - `/workspace/grok/sources/`
   - `/workspace/grok/plans/`
   - `/workspace/grok/open-questions/`
4. **Prefer evidence over memory.** Re-read PDFs / Excel / conversations when unsure.
5. **Production mindset.** Treat this as a live bank intranet app: auditability, soft deletes, validation, logging, security, and clear separation of master data vs transactional entry.
6. **Do not silently resolve conflicts.** Client (Priyadarshini/Divya) vs Manager vs SRS may disagree. Document both sides and wait for user decision.
7. **Do not integrate or modify any external “BB repo” / existing portal code** until that repo/access and permission are provided.
8. **Excel / master lists are mandatory inputs** before schema finalization. Do not hardcode department/section/project/head lists from mockup screenshots alone.

## Communication
- Be direct and concise.
- When blocked, list exact missing artifacts (file name / person / decision).
- Never claim something is confirmed unless it appears in a source file under `grok/sources/` or an explicit user message.

## Change control
- Before any implementation turn: re-read `grok/plans/FINAL_PLAN.md` and `grok/open-questions/BLOCKERS.md`.
- After any approved implementation: update `grok/memory/DECISIONS.md` with what changed and why.
