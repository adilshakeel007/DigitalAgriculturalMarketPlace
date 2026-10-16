# Phase 8 release-preparation report

Date: 16 October 2026  
Branch baseline: `upgrade/multivendor-phase2` at `6d71123`

## Baseline and migration rehearsal

The protected development database remained at 8 migrations and the protected staging database remained at 13. Both contained 7 users, 4 products, 21 orders, 2 reviews, 15 messages, and 23 notifications. Both reported aggregate order quantity 119.000 and order total Rs 11,740.00. User and product core checksums matched.

The verified backup `DigitalAgriculturalMarketplaceDB_Phase2_20261014.bak` was restored as the isolated `DigitalAgriculturalMarketplace_Phase8Rehearsal_20261016` database. Its pre-migration counts and totals matched the 8-migration source. The five reviewed migrations advanced it to 13 migrations. The transactional retirement script then marked exactly Fresh Tomatoes, bhindi, bhindiyan, and Apple inactive without deleting or mapping them.

Post-migration reconciliation matched staging:

- 7 users, 4 products, 21 orders, 2 reviews, 15 messages, 23 notifications
- Aggregate order quantity 119.000; aggregate total Rs 11,740.00
- 2 legacy farmer users and 2 safely created seller profiles
- Available inventory 312.000; product reserved 27.000; active reservations 27.000
- 4 inventory movements and 1 active historical reservation
- 0 negative balances, duplicate operation keys, or detected orphans
- 4 retired inactive and intentionally unmapped legacy listings

The order checksum changed between the 8- and 13-migration schemas because `Orders.Quantity` changed from integer to `decimal(18,3)` and migrated status metadata was normalized. Values, ownership, row count, aggregate quantity, and aggregate total were preserved; the upgraded rehearsal checksum exactly matched staging.

## Production preparation

- Base source configuration no longer contains a database connection string.
- Development LocalDB configuration remains in `appsettings.Development.json`.
- Staging and production logging configurations avoid verbose EF command output.
- Startup fails with a safe configuration error when the connection string is absent.
- Production cookies remain Secure, HttpOnly, essential, and SameSite Strict.
- HTTPS redirection, HSTS, and safe exception handling remain enabled outside Development.
- Forwarded headers are disabled by default and accept only one hop from explicitly configured proxy IPs when enabled.
- Multipart requests are capped at 6 MB while validated JPEG/PNG image content remains capped at 5 MB.
- Migrations are not executed automatically by application startup.

## Executed browser smoke checks

The application was started locally in Production mode over HTTPS against the isolated rehearsal database. Executed checks passed for:

- Application startup and public landing page
- Fictional buyer login using a temporary rehearsal-only account
- Unified buyer dashboard
- Marketplace rendering and exclusion of all retired listings
- Seller-profile activation without loss of buyer access

Phase 7's executed browser test remains the evidence for catalog approval, listing creation, fractional purchase/cancellation, multi-product RFQ, competing private quotations, multi-seller partial allocation, seller-specific orders, notifications, completion, dashboards, and authorization. Those long workflows were not repeated in the Phase 8 browser session. They remain covered by the final SQL Server regression suite.

## Release evidence

Final build, publish, test, dependency, and model-consistency results are recorded in the completion response after the release commit. No external deployment, remote push, branch merge, Git tag, or original-database migration is authorized or performed by this phase.

## Readiness conditions

Release preparation is complete only when the final Release build, 66 SQL Server tests, NuGet vulnerability audit, model consistency check, and publish output all pass. External deployment additionally requires owner approval, production secrets, host-specific proxy/HTTPS settings, a fresh verified backup, and a rehearsed rollback window.
