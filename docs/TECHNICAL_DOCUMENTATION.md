# Digital Agricultural Trading Marketplace

## Overview and scope

This ASP.NET Core MVC application connects agricultural buyers and sellers. One ordinary account may buy and sell after activating a seller profile; administrators remain a separate privileged role. Buyers can purchase listings directly or publish bulk requests for quotation (RFQs). Multiple sellers may quote privately, and a buyer may allocate partial quantities to different sellers.

The implemented scope includes custom session authentication, seller onboarding, governed catalog proposals, seller listings, decimal inventory, direct orders, RFQs, private quotations, partial allocation, notifications, reviews, messages, dashboards, and administrative moderation. It does not include online payments, delivery-provider integration, real-time chat, multi-currency accounting, identity federation, or automatic seller verification.

## Technology and architecture

- ASP.NET Core MVC and Razor Views on .NET 8
- Entity Framework Core 8.0.32 with SQL Server
- Bootstrap, project CSS, and vanilla JavaScript
- BCrypt password hashing and custom server-side session authentication

Controllers validate ownership and orchestrate application services. `InventoryService`, `OrderWorkflowService`, and `RfqWorkflowService` own transactional business operations. EF Core maps the relational model and migrations. Razor views remain server-rendered.

## Accounts and authorization

Registration creates an ordinary marketplace account. A user becomes a seller by activating a one-to-one `SellerProfile`; this does not remove buying access. Admin status remains database-controlled and is checked on protected actions. Session values identify the signed-in user, but controllers reload the user and validate activity, role, ownership, and workflow state from the database. Global automatic antiforgery validation protects unsafe MVC requests.

## Catalog and listings

The shared `CatalogProduct` table defines approved agricultural products and their default units. Sellers propose catalog products and administrators approve or reject them. New active listings require an approved catalog product and an explicit `UnitOfMeasure`. A `Product` is the seller-specific listing containing price, available quantity, reserved quantity, minimum order quantity, image, and active state. Historical retired listings remain stored but are excluded from new trading.

## Inventory lifecycle

`AvailableQuantity` is stock that may be newly committed. `ReservedQuantity` is stock held for pending or confirmed orders. Every change creates an `InventoryMovement` with before/after balances and a unique operation key. An `InventoryReservation` links the hold to an order item and moves through Active, Released, or Consumed exactly once.

- Order placement: available decreases and reserved increases.
- Eligible cancellation: reserved decreases and available increases.
- Completion: reserved decreases; stock is consumed rather than restored.

Database check constraints prevent negative balances. Row versions and serializable transactions detect competing updates. Unique keys prevent repeated side effects.

## Direct-order lifecycle

A valid purchase creates an order and order item, reserves inventory, records history, and notifies the seller in one transaction. Supported transitions enforce actor ownership and current state. Confirmation does not reserve a second time. Completion consumes the existing reservation. Cancellation is allowed only from eligible states and releases it once. Every status transition is appended to `OrderStatusHistory`.

## RFQ, quotations, and allocations

An RFQ contains one or more `RfqItem` rows with catalog product, unit, requested quantity, accepted quantity, optional budget, and specifications. Only published/open RFQs are discoverable for quoting. Each seller may submit one quotation per RFQ with item-level quantities, prices, listing references, and delivery dates.

Seller quotation details are private from competing sellers. The buyer may accept less than the offered quantity. Acceptance verifies RFQ state, quotation validity, buyer ownership, offered balance, requested balance, listing eligibility, seller ownership, unit match, and available inventory. Each accepted slice creates an immutable `QuotationAllocation` and a separate seller order. Accepted totals update atomically; the RFQ becomes partially fulfilled or fulfilled as appropriate.

## Transactions, concurrency, and idempotency

Inventory and RFQ allocation workflows use SQL Server transactions. A failed write rolls the complete operation back. Timestamp/row-version properties detect stale changes. Unique operation keys cover movements, reservations, order history, allocations, and event notifications. Repeating a terminal workflow cannot consume or restore stock twice.

## Validation and security

- Server-side data annotations and workflow validation
- Ownership checks using the authenticated database user
- Global antiforgery validation
- Secure, HttpOnly, SameSite Strict session cookies outside Development
- HTTPS redirection, HSTS, and safe production exception handling
- Explicit trusted-proxy configuration with a one-hop limit
- JPEG/PNG image extension, content-type, signature, and 5 MB validation
- Parameterized EF Core queries and no sensitive-data logging

## Testing

The SQL Server test suite contains 66 tests. It covers original registration/order behavior, unified accounts and catalog governance, inventory/order transitions, multi-seller RFQs, decimal quantities, authorization forgery, private quotations, concurrent purchases, invalid states, notifications, and upload validation. Tests use isolated SQL Server databases rather than EF InMemory for transactional and concurrency behavior.

## Deployment and rollback

See `docs/DEPLOYMENT.md`. Production secrets come from environment variables or the host secret provider. Migrations are rehearsed against a restored backup and are not applied automatically at startup. Rollback uses the previous versioned application artifact and a verified pre-deployment database restore.

## Known limitations

- Custom session authentication lacks the mature account recovery, MFA, lockout, and security-token features of a full identity platform.
- No payment gateway, shipment tracking, tax engine, dispute process, or escrow.
- Notifications are in-application only.
- Uploaded images use local persistent storage rather than object storage/CDN.
- Catalog and seller approval remain manual admin workflows.
- The legacy `Orders.ProductId` and `Products.Quantity` fields remain for backward compatibility; authoritative modern inventory is stored in decimal listing and order-item fields.
- Operational monitoring, load testing, disaster-recovery automation, and production penetration testing remain deployment responsibilities.
