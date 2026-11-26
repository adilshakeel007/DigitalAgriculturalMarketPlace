# FYP demonstration plan

## Safe demo environment

Use a separate database created from the current 13-migration schema. Use fictional names and addresses. Prepare one admin, one buyer, and two seller accounts; set their passwords locally and never place them in slides, source control, screenshots, or this document. Keep a verified database backup immediately after setup so the demonstration can be reset.

Recommended fictional setup:

| Role | Display name | Prepared data |
|---|---|---|
| Admin | GreenMarket Demo Admin | Catalog moderation access |
| Buyer | Crescent Foods Demo | Draft/open RFQ and account dashboard |
| Seller A | Riverbend Farms Demo | Verified profile, wheat and potato listings |
| Seller B | Meadow Harvest Demo | Verified profile, wheat and potato listings |

Use approved Wheat and Potatoes catalog entries in kilograms. Give both sellers sufficient decimal stock. Prepare one direct order and one multi-item RFQ; leave at least one action unfinished so it can be performed live.

## Short presentation version (5–7 minutes)

1. Explain that fragmented agricultural procurement makes price discovery and bulk sourcing difficult.
2. Show that one account can switch naturally between buying and selling while admin remains separate.
3. Open the governed shared catalog and one seller listing with decimal inventory.
4. Place a small direct purchase and point out the reservation.
5. Open the prepared bulk RFQ and show two private competing quotations.
6. Accept partial quantities from each seller and show the separate seller orders.
7. Complete an order and show inventory movement, notifications, and dashboard totals.
8. Close with transaction safety, ownership validation, decimal precision, and preserved history.

## Detailed examiner demonstration (15–20 minutes)

1. **Introduction:** state the problem, users, scope, and MVC architecture.
2. **Account model:** sign in as the buyer and explain unified buyer/seller capability.
3. **Seller onboarding:** show seller-profile activation and database-validated access.
4. **Catalog governance:** submit a fictional product proposal, switch to admin, approve it, and explain why shared catalog data avoids duplicate product definitions.
5. **Listing and inventory:** create or open an active listing with unit, decimal available stock, price, minimum quantity, and image validation.
6. **Direct purchase:** buy a fractional quantity. Show available stock decreasing, reserved stock increasing, order history, and seller notification.
7. **Order transition:** confirm and complete the order. Explain that completion consumes the reservation exactly once.
8. **Cancellation:** cancel a separate eligible fractional order and show exact stock restoration.
9. **Bulk RFQ:** publish the prepared Wheat/Potatoes RFQ with explicit units and requested quantities.
10. **Competition and privacy:** sign in as Seller A and Seller B to submit quotations. Demonstrate that neither seller can inspect the competitor's private quote.
11. **Partial acceptance:** as buyer, accept part from Seller A and the remainder from Seller B.
12. **Separate orders:** show seller-specific orders and immutable allocation records.
13. **Dashboards:** compare buyer purchases/RFQs, seller sales/inventory, and admin marketplace totals.
14. **Security:** demonstrate a protected direct URL, antiforgery behavior conceptually, and server-side ownership checks.
15. **Integrity:** show migration history, unique operation keys, row versions, reservations, movements, and the 66 passing SQL Server tests.

## Recovery plan

Keep two browser profiles ready for sellers, pre-open important pages, use a local HTTPS development certificate, and keep screenshots of the final RFQ and dashboards. If live data becomes unsuitable, stop the application, restore the prepared demo backup under a new name, update only the demo connection string, and restart.
