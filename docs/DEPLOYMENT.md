# Deployment and rollback guide

## Supported deployment

The application targets .NET 8 and SQL Server. A host needs the ASP.NET Core 8 Hosting Bundle (or the .NET 8 runtime behind a supported reverse proxy), a SQL Server login with normal application permissions, and a writable persistent directory for `wwwroot/images/products`.

Do not store credentials in `appsettings*.json`. Supply production settings through environment variables or the hosting platform's secret provider:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=...;User ID=...;Password=...;Encrypt=True;TrustServerCertificate=False
AllowedHosts=marketplace.example.edu
```

The process stops at startup if `ConnectionStrings:DefaultConnection` is absent. Session cookies are HttpOnly, essential, SameSite Strict, and Secure outside Development. HTTPS redirection, HSTS, and the production exception handler are enabled outside Development.

### Reverse proxy

Leave `ReverseProxy__Enabled=false` unless the application is behind a trusted proxy. When enabled, specify every trusted proxy address; never trust all networks:

```text
ReverseProxy__Enabled=true
ReverseProxy__KnownProxies__0=127.0.0.1
```

The proxy must forward only `X-Forwarded-For` and `X-Forwarded-Proto`. The application accepts one forwarding hop.

## Database deployment

1. Stop writes and take a verified full backup.
2. Record migration history, table counts, order quantity, order total, negative-stock checks, reservation totals, and duplicate operation keys.
3. Restore the backup under a different database name and rehearse every step there first.
4. Grant the migration operator schema-change permission. The normal application login should receive only the data permissions it needs and should not normally run migrations.
5. From the release directory, set the target connection string in the environment and run:

```powershell
dotnet ef database update --connection "$env:ConnectionStrings__DefaultConnection"
```

6. Apply `DatabaseScripts/Phase8_RetireLegacyListings.sql` only after verifying that the four named historical listings are the approved retirement targets. The script is transactional, idempotent, keeps every record, and aborts unless exactly four matching rows exist.
7. Repeat reconciliation. Do not continue if historical totals, quantities, ownership, or credentials changed.

Migrations are never applied automatically during application startup.

## Release build and publish

```powershell
dotnet restore
dotnet test .\DigitalAgriculturalMarketPlace.Tests\DigitalAgriculturalMarketPlace.Tests.csproj
dotnet publish .\DigitalAgriculturalMarketPlace.csproj -c Release -o .\artifacts\publish --no-self-contained
```

Deploy the complete publish directory as one versioned release. Preserve `wwwroot/images/products` as persistent application data; the web process needs read/write permission only on that directory. Product image validation permits JPEG or PNG content up to 5 MB. The request limit is 6 MB to allow multipart overhead.

## Startup verification

1. Start the process with `ASPNETCORE_ENVIRONMENT=Production` and the secret connection string.
2. Confirm the process starts without configuration or database errors.
3. Confirm HTTP redirects to HTTPS and the public marketplace loads.
4. Sign in with a designated smoke-test account and verify its dashboard.
5. Verify an unauthorized direct URL redirects to login or returns the safe not-found response.
6. Check logs for warnings and errors. Never enable sensitive-data logging in production.
7. Confirm a small test image can be uploaded and removed using the application workflow.

## Logging

Production and staging use structured ASP.NET Core console/event logging. Framework and EF command logging are limited to Warning. Forward logs to the host's protected log collector, apply retention, restrict access, and avoid adding passwords, cookies, connection strings, message contents, or personal information to log properties.

## Rollback

1. Stop the new application version and block writes.
2. Preserve logs and take a diagnostic backup of the failed state.
3. Redeploy the previous versioned publish artifact.
4. If database changes were applied, restore the verified pre-deployment backup to a new database name and point the previous application at it. Prefer restore-and-switch over destructive down-migrations.
5. Reconcile users, orders, totals, and inventory before reopening traffic.
6. Retain the failed database for investigation until recovery is accepted.

Never overwrite the only production database, delete historical listings, or use `database drop`, `EnsureDeleted`, or automatic schema creation in deployment.
