# BUILD PLAN — IT Capital & Revenue Budget App

**Source of this plan:** modified BRD PDF `Pdf_new_pdffff_0e3f.pdf` (UB_BRD_V1.0, Priyadarshini, 12.06.2024, “With Revenue Updated”), earlier Word/photo SRS, Excel master, conversations, Personal (SCV), Diary.  
**Rule:** no assumptions. Open items listed separately. **No app code until you say do it.**

---

## 1. What this modified BRD is

Official **Business Requirement Document** (11 pages) for:
**IT Capital Budget Monitoring & Management System (With Revenue Updated)**  
Union Bank of India — DIT.

It is mostly the same Capital + Revenue portal we already knew, with Revenue sections clearly included.

---

## 2. End-to-end product (from BRD only)

```
Departments/Sections enter data (web)
        ↓
Validation in web layer (block if over allotted)
        ↓
Centralized database (Oracle for app data)
        ↓
Organize dept/section (DIT special for Revenue)
        ↓
Data Lake / Power BI
        ↓
Dashboards & management reports
```

**Out of scope (BRD):** direct financial transaction processing; integration with external financial systems (unless planned separately).

---

## 3. Same as before (confirmed again in this BRD)

| Topic | BRD fact |
|-------|----------|
| Capital entry | Spillover + Fresh actuals + next-month estimates; totals |
| Capital allotted | Spillover + Fresh = Total; display on form |
| Capital validation | Actual utilization must **not exceed** Total Allocated (spillover+fresh); block submit |
| Capital navigation | Dept / Section / Project filtering |
| Revenue | DIT; expenditure heads (FMS, AMC, ATS, NER, Bandwidth, Rent, Professional Charges, etc.) |
| Revenue UI | Dynamic rows + head dropdown + amount |
| Revenue validation | Entered ≤ Allocated; else reject + error message (exact text in BRD) |
| Reporting | Power BI / Data Lake; sample dashboard (Fresh/Spill gauges, section bars) |
| Amounts | Rs. Crore (sample pages) |

**Exact Capital/Revenue over-limit message (BRD p8):**  
*"Entered budget utilization exceeds the allocated budget. Please enter a valid amount within the approved budget limit."*

---

## 4. What this BRD adds / makes clearer vs earlier talks

| Item | Status |
|------|--------|
| **Hard block on Revenue over allotment** | **In this BRD** (block + error). Earlier conversation notes said warn-only. **→ Prefer BRD hard-block unless client re-confirms warn.** |
| **PO / invoice / project status / milestones** | Listed **In Scope** for Capital (and PO/invoice for Revenue). Sample web pages shown in BRD do **not** show separate PO/invoice/milestone fields — only Spillover/Fresh panels. Earlier Annexure/justification treated PO text as one remarks field. **→ OPEN: separate fields vs one Justification text?** |
| **Revenue “project-wise” in scope text** | Scope says section-wise **and project-wise** filtering for Revenue. Solution + sample UI = **Section + heads only** (no project on Revenue form). **→ OPEN: entry without project; report filter later?** |
| **Maker-Checker** | **Not in this BRD.** It **is** in the other Word/photo requirements doc. **→ OPEN: include Maker-Checker or not?** |
| **How Maker/Checker PF is assigned** | Still **not** specified in BRD or Word doc (only “designated user”). |

---

## 5. Personal (SCV) — what we can reuse as **pattern** (verified)

Production app: Angular UI + .NET API.

| Piece | Fact | Use for our app |
|-------|------|-----------------|
| Login | PF/username + password + captcha → AD `validateDomainUser` → JWT → Oracle `USER_TOKEN` | Same pattern |
| Encrypt | UI CryptoJS + server `EncryptoData.DecryptAes` / `EncryptString` | Same pattern for login payloads |
| SQL Server | `OrganisationsDbContext`: `STAFF_DETAILS`, `login_question` | Staff name/dept by `EMP_ID`; captcha questions (or our own Oracle captcha table) |
| Oracle | App DB: annexures, `USER_TOKEN`, `LOGIN_TYPE`, `BRANCH_USER_ACCESS`, `STAFF_ROLES` | Our budget data stays in **Oracle `IT_CAPITAL`** |
| Finacle | **Not used for login.** Only cash denomination URL / Finacle master tables for SCV cash feature | **Do not copy for login** |
| Dual DB | SQL Server (org/staff) + Oracle (app) | Same architecture |

---

## 6. Diary — what we can use as **reference only** (verified)

| Table / feature | Verdict |
|-----------------|--------|
| `DEPARTMENTS`, `SECTIONS` | Same purpose (masters) — **pattern/names reference**; do not assume we reuse live Diary data without bank approval |
| `BUDGET_UTILIZATION` | Similar only (Capital/Revenue flag, fresh/spill) — **not** Priyadarshini dual-panel + heads + validation form |
| `EMPLOYEE_MASTER`, hierarchy columns | Similar for PF↔section / CM–CTO display |
| MudBlazor + .NET 8 Blazor Server | Stack match for UI |
| Login | **None** (anonymous) — use Personal AD pattern instead |
| Charts | MudChart pie/bar — UI reference for any in-app charts; BRD reporting is Power BI |

---

## 7. Open questions (must answer before / during build)

1. **Maker-Checker:** include (Word doc) or skip (not in this BRD)?  
2. **If Maker-Checker:** how to store Maker PF / Checker PF per department?  
3. **PO / invoice / milestones:** separate DB fields + screens, or only inside Justification text?  
4. **Revenue over allotment:** hard block (this BRD) confirmed?  
5. **Revenue project-wise:** entry without project OK?  
6. **In-app dashboard** vs Power BI only for Phase 1?  
7. **Which 18 heads final list** — master Excel (already locked earlier) vs BRD sample list (FMS, AMC, ATS, NER…)? Keep **18 from master sheet** unless client changes.

---

## 8. NEXT STEPS (ordered plan)

### Step 0 — Confirm opens (you / client)
Answer items in §7. Especially Maker-Checker + PO/milestones + Revenue hard-block.

### Step 1 — Freeze schema
Update `IT_CAPITAL` DDL only after Step 0:
- Keep Capital/Revenue entry tables we already designed  
- Add Maker-Checker columns/tables **only if confirmed**  
- Add PO/invoice/milestone tables **only if confirmed as separate fields**  
- Align Justification length (BRD/Word said 5000)  
- Keep external `STAFF_DETAILS` lookup; keep `USER_TOKEN` + captcha  

### Step 2 — Create Oracle objects
You (DBA/you) run approved DDL on Oracle 19c schema `IT_CAPITAL`.  
Seed: departments/sections/projects from Capital Excel; 18 revenue heads; captcha questions.

### Step 3 — Scaffold MudBlazor app (.NET 8)
New app (not edit Diary/Personal).  
Wire:
- Oracle `IT_CAPITAL` connection  
- SQL Server org connection for `STAFF_DETAILS`  
- AD API + encrypt/decrypt pattern from Personal  
- JWT + `USER_TOKEN` session  

### Step 4 — Masters UI (Admin)
Department, Section, Project, Expenditure Head (BRD + Word admin list). Soft delete.

### Step 5 — Capital entry flow
Login → IT Budget → Dept → (DIT: Capital) → Section → Project → dual-panel form → validate ≤ allotted → Submit.

### Step 6 — Revenue entry flow (DIT)
Login → DIT → Revenue → Section → heads dynamic rows → validate ≤ allotted → Submit.

### Step 7 — Maker-Checker screens (only if confirmed)
Pending list; Approve / Reject / Return; audit trail.

### Step 8 — Hierarchy / access (from manager conversations)
Only after confirming how it merges with Maker-Checker / BRD (CM multi-section vs one Maker per dept).

### Step 9 — Power BI readiness
Oracle views for allotted vs utilized (Fresh/Spill, dept/section).  
Dashboard sample in BRD = Power BI target (not required as MudBlazor Phase 1 unless you ask).

### Step 10 — UAT
Test with real PF login against AD + STAFF_DETAILS; Capital hard-block; Revenue hard-block; DIT vs non-DIT paths.

---

## 9. What I will NOT do until you say so

- No UI/API coding  
- No DDL change until Step 0 answers  
- No editing Diary or Personal repos  
- No inventing Maker PF mapping or Finacle login  

---

## 10. Suggested immediate reply from you

Please answer with numbers:

1. Maker-Checker: **Yes / No** for first build?  
2. PO/invoice/milestones: **separate fields** or **Justification text only**?  
3. Revenue over allotment: **hard block (BRD)** confirmed?  
4. Start coding after schema freeze: say **do it** when ready.
