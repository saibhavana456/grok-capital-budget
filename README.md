# IT Capital & Revenue Budget Monitoring Portal

Union Bank of India — Phase 1 (MudBlazor + .NET 8 + Oracle)

## Stack
- Blazor Interactive Server (.NET 8)
- MudBlazor 8.15
- Oracle EF Core (app schema — 17 tables you created)
- SQL Server `STAFF_DETAILS` lookup (optional)
- AD `validateDomainUser` + JWT + `USER_TOKEN` (Personal/SCV pattern)
- EncryptoData AES helpers (Personal/SCV)

## Configure before run
Edit `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "OracleDb": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl)));User Id=YOUR_USER;Password=YOUR_PASSWORD;",
  "OrganisationsDb": ""
},
"Auth": { "BypassAd": true }
```

- Set `OracleDb` to the same user as SQL Developer connection `IT_BUDGET_MONITORING_LOCAL`.
- Leave `OrganisationsDb` empty until SQL Server STAFF_DETAILS is available.
- `Auth:BypassAd=true` skips AD HTTP call for local laptop testing (still requires seeded `APP_USER` + captcha).
- For production: set `BypassAd=false` and configure `ApiKey:AD_API_URL` / service credentials.

## Sample logins (from seed data)
| PF | Role |
|----|------|
| 600110 | Maker |
| 600221 | Checker |
| 100001 | Admin |

Password: any value when `BypassAd=true`. Captcha answer must match the question shown.

## Run
```bash
dotnet restore
dotnet run
```
Open the HTTPS URL from the console → `/login`.

## Features
1. Login (PF + password + captcha)
2. Portal: Dept → Section → (DIT Capital/Revenue) → Project
3. Capital dual-panel entry + hard block over allotment
4. Revenue section heads entry (18 Excel heads) + hard block
5. Capital / Revenue submissions lists
6. Checker Approve / Return / Reject
7. Admin masters (Department / Section / Project soft-delete)

## SQL scripts
See `Scripts/IT_CAPITAL_FULL_SCHEMA.sql` (use `SET DEFINE OFF` for `&` in head names).
