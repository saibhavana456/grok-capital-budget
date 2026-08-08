# Staff source + Admin (SCV-aligned) — local vs prod

## Two Organisations staff tables (Personal/SCV)

| Table | Key | Used in IT Budget |
|---|---|---|
| `StaffDetails` | `EMPLID` | **Yes** — login staff, scale, email, phone (latest) |
| `STAFF_DETAILS` | `EMP_ID` | SCV annexure login profile — **not** our Maker/Checker staff source |

Local Oracle table `STAFF_DETAILS` uses the **same columns as Organisations `StaffDetails`** (`EMPLID`, `NAME`, `DESCR`, …).

**Never run DDL against Organisations.** Oracle-only scripts for testing.

## Local testing (current defaults)

| Setting | Value |
|---|---|
| `Auth:UseOrganisationsDb` | `false` |
| Staff | Oracle `STAFF_DETAILS` |
| `Auth:BypassAd` | `true` |
| Admin | `AdminApp` / `Ubi#8790` (from SCV encrypted `ADMIN_USER_ID` / `PWD`) |

### Scripts (SQL Developer, app Oracle user, `SET DEFINE OFF`)

1. `Scripts/CREATE_ORACLE_STAFF_DETAILS.sql` — align/create EMPLID columns  
2. `Scripts/SEED_STAFF_DETAILS_MAKER_CHECKER.sql` — Maker/Checker + scales  
3. `Scripts/ENSURE_SINGLE_ADMIN.sql` — deactivate `APP_USER` ADMIN  
4. `Scripts/SEED_MAKER_CHECKER_ASSIGNMENTS.sql` — APP_USER Maker/Checker + assignments (if needed)

Restart app after scripts.

### Logins

| User | Password | Role |
|---|---|---|
| `AdminApp` | `Ubi#8790` | Admin Masters (config, not AD) |
| `600110` | any when BypassAd | Maker (needs staff + APP_USER) |
| `600221` | any when BypassAd | Checker (needs staff + APP_USER) |

## Prod switch checklist

When moving to prod (Organisations + AD):

1. `Auth:UseOrganisationsDb` → **`true`**
2. Keep `ConnectionStrings:OrganisationsDb` = SCV `ConnStrOrganisations` AES string (already in appsettings). For **live** Organisations, swap to the live ciphertext from SCV appsettings comment block if UAT string is not the live DB.
3. `Auth:BypassAd` → **`false`**
4. Confirm `ApiKey:AD_API_URL`, `M_service_Name`, `M_Service_Pwd`
5. Confirm `ApiKey:ADMIN_USER_ID` + `PWD` (SCV encrypted admin pair, or bank-approved prod admin ciphertext)
6. Do **not** alter Organisations `StaffDetails` / `STAFF_DETAILS`

Maker/Checker must exist in Organisations `StaffDetails` by `EMPLID` and in Oracle `APP_USER` with MAKER/CHECKER.
