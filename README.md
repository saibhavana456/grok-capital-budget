# IT Capital & Revenue Budget Monitoring Portal

Union Bank of India — Phase 1 (MudBlazor + .NET 8 + Oracle)

## Stack
- Blazor Interactive Server (.NET 8) + MudBlazor 8.15
- Oracle EF Core (17 tables)
- Optional SQL Server `STAFF_DETAILS`
- AD `validateDomainUser` + JWT + `USER_TOKEN` (Personal/SCV pattern)
- Cookie authentication (required for `[Authorize]` / DefaultChallengeScheme)

---

## Auth:BypassAd — what you need to do

| Value | Behaviour |
|-------|-----------|
| **`true`** (laptop now) | Captcha checked. PF must exist in Oracle `APP_USER`. **AD is not called.** Password = any non-empty text. |
| **`false`** (bank UAT/prod) | Captcha checked. PF in `APP_USER`. **Password validated by bank AD API** `validateDomainUser`. |

### Keep for local laptop (your screenshot is correct)
```json
"Auth": { "BypassAd": true }
```
No other BypassAd change needed for local.

### Change only when using real AD on bank network
```json
"Auth": { "BypassAd": false },
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
