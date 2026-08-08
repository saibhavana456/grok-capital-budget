# IT Capital & Revenue Budget Monitoring Portal

Union Bank of India — Phase 1 (MudBlazor + .NET 8 + Oracle)

## Stack
- Blazor Interactive Server (.NET 8) + MudBlazor 8.15
- Oracle EF Core (17 tables)
- Organisations `StaffDetails` (EMPLID) when `Auth:UseOrganisationsDb=true`; else Oracle `STAFF_DETAILS` (same EMPLID columns)
- AD `validateDomainUser` + JWT + `USER_TOKEN` (Personal/SCV pattern)
- SCV-style admin: `ApiKey:ADMIN_USER_ID` / `PWD` → **AdminApp** / **Ubi#8790**
- Cookie authentication (required for `[Authorize]` / DefaultChallengeScheme)

---

## Auth:BypassAd + UseOrganisationsDb

| Setting | Local now | Prod |
|-------|-----------|------|
| `Auth:BypassAd` | `true` (password any non-empty for Maker/Checker) | `false` (real AD) |
| `Auth:UseOrganisationsDb` | `false` (Oracle STAFF_DETAILS) | `true` (Organisations StaffDetails) |
| Admin | **AdminApp** / **Ubi#8790** | same config pair (encrypted in appsettings) |

See `Docs/STAFF_ADMIN_PROD_SWITCH.md` and `Docs/TESTING_GUIDE.md` for scripts and prod flip steps.

### Keep for local laptop
```json
"Auth": { "BypassAd": true, "UseOrganisationsDb": false }
```

### Change only when using real AD on bank network
```json
"Auth": { "BypassAd": false, "UseOrganisationsDb": true },
"ApiKey": {
  "AD_API_URL": "http://app2.unionbankofindia.co.in:8222/MicroService/MicroService.svc",
  "M_service_Name": "microservice",
  "M_Service_Pwd": "<real microservice password from bank>"
}
```

`BypassAd` does **not** affect Oracle. Oracle is only `ConnectionStrings:OracleDb`.

---

## Oracle — what you need (not the browser error)

Your connection string shape is fine:
`User Id=IT_BUDGET_MONITORING_PORTAL` · `Password=...` · `HOST=localhost` · `PORT=1521` · `SERVICE_NAME=FREEPDB1`

In SQL Developer as that user, verify seed data:

```sql
SELECT COUNT(*) FROM LOGIN_CAPTCHA_QUESTION;           -- expect 5
SELECT PF_NO, ROLE_CODE, IS_ACTIVE FROM APP_USER;      -- 100001 ADMIN, 600110 MAKER, 600221 CHECKER
SELECT HEAD_CODE, HEAD_NAME FROM REVENUE_HEAD ORDER BY DISPLAY_ORDER;
SELECT COUNT(*) FROM DEPARTMENT;                       -- expect 2
```

If head names are `PRINTING 1` / `M1` (SQL Developer `&` substitution), run with **SET DEFINE OFF**:

```sql
SET DEFINE OFF;
UPDATE REVENUE_HEAD SET HEAD_NAME = 'PRINTING & STATIONERY' WHERE HEAD_CODE = 'PRINTING_STATIONERY';
UPDATE REVENUE_HEAD SET HEAD_NAME = 'M&R' WHERE HEAD_CODE = 'M_R';
COMMIT;
```

You do **not** need Oracle user `IT_CAPITAL`. Your owner is `IT_BUDGET_MONITORING_PORTAL`.

The error **"No authenticationScheme / DefaultChallengeScheme"** was an **ASP.NET auth config bug**, not SQL. Fixed in `Program.cs` (Cookie authentication).

---

## Sample logins (BypassAd = true)

| PF | Role | Password |
|----|------|----------|
| 600110 | Maker | any (e.g. `test`) |
| 600221 | Checker | any |
| 100001 | Admin | any |

Captcha: type the answer to the question shown (e.g. `7 + 4 = ?` → `11`).

## Run
```bash
dotnet restore
dotnet run
```
Open `/login` (HTTPS port from console, e.g. `https://localhost:44389/login`).

## SQL scripts
`Scripts/IT_CAPITAL_FULL_SCHEMA.sql` · `Scripts/REPAIR_REVENUE_HEAD_NAMES.sql`
