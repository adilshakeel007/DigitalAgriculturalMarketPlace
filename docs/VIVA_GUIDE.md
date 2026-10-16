# BSCS FYP viva guide

**Why ASP.NET Core MVC?**  It provides clear separation between request handling, business logic, and server-rendered views, while giving strong validation, dependency injection, security middleware, and testability on .NET 8.

**Why Entity Framework Core?**  EF Core maps C# entities to SQL Server, supports migrations, relationships, transactions, concurrency tokens, and parameterized queries while keeping database work integrated with the application model.

**Why SQL Server?**  The workflows require relational integrity, precise decimal data, transactions, constraints, indexes, and reliable concurrent updates. SQL Server provides these features and works directly with EF Core.

**Why a shared product catalog?**  It separates the meaning of a product, such as Wheat measured in kilograms, from each seller's price and inventory. This improves search, RFQ comparison, and data quality.

**How can one account buy and sell?**  Every non-admin account is a marketplace member and can buy. Activating its one-to-one seller profile adds selling capability without replacing the buyer capability.

**Available versus reserved stock?**  Available stock can be committed to a new order. Reserved stock has already been held for an active order but has not yet been completed or cancelled.

**Why keep an inventory movement ledger?**  It gives an auditable record of every adjustment, reservation, release, and consumption with before/after balances and an operation key.

**How does RFQ quotation acceptance work?**  The buyer selects a submitted, valid quotation item and an acceptance quantity. The service validates ownership, state, remaining requested/offered quantity, listing eligibility, and stock, then creates an allocation and order transactionally.

**How are partial allocations handled?**  Accepted quantities accumulate on the RFQ and quotation items. Each allocation is immutable and may create an order for a different seller. The RFQ stays partially fulfilled until every requested item is fully allocated.

**How is overselling prevented?**  The service checks authoritative decimal inventory inside a SQL Server transaction, updates row-versioned records, and uses constraints and unique operation keys. Concurrent tests verify that only valid reservations commit.

**What is a database transaction?**  It groups related changes into one atomic unit. Either orders, reservations, movements, histories, allocations, and notifications all commit, or all roll back.

**What is optimistic concurrency?**  Each mutable row has a version maintained by SQL Server. An update using an old version affects no current row, allowing the application to reject a stale change instead of silently overwriting it.

**What is idempotency?**  Repeating the same logical operation has no additional effect. Unique operation keys prevent duplicate inventory movements, releases, allocations, histories, and event notifications.

**How are seller quotations private?**  Controller queries are scoped by the authenticated seller or RFQ buyer. A competing seller cannot retrieve another seller's quote even by changing a URL or form identifier.

**How is unauthorized access prevented?**  Protected actions require a live session, reload the active database user, validate admin privilege or ownership, enforce valid workflow state, and use antiforgery validation on state-changing requests.

**Why decimal quantities?**  Agricultural goods are commonly traded by weight, so values such as 12.375 kg must remain exact through stock, RFQs, quotations, orders, cancellation, and reporting.

**How are historical orders preserved?**  Migrations add compatible structures instead of dropping old tables or rows. Restricted foreign keys and inactive listing states keep historical references available. Deployment rehearsals compare counts, quantities, and totals before and after migration.

**Limitations and future improvements?**  The system currently lacks integrated payment, logistics, MFA/account recovery, external notifications, object storage, and production-scale monitoring. Future work can add these without replacing the MVC and transactional core.
