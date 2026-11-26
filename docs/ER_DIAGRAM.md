# Database ER diagram

```mermaid
erDiagram
    USER ||--o| SELLER_PROFILE : activates
    USER ||--o{ PRODUCT : owns
    UNIT_OF_MEASURE ||--o{ CATALOG_PRODUCT : defaults
    CATALOG_PRODUCT ||--o{ PRODUCT : governs
    UNIT_OF_MEASURE ||--o{ PRODUCT : measures
    USER ||--o{ ORDER : buys
    USER ||--o{ ORDER : sells
    PRODUCT ||--o{ ORDER : legacy_reference
    ORDER ||--o{ ORDER_ITEM : contains
    PRODUCT ||--o{ ORDER_ITEM : fulfills
    ORDER_ITEM ||--o| INVENTORY_RESERVATION : reserves
    PRODUCT ||--o{ INVENTORY_MOVEMENT : ledger
    ORDER ||--o{ ORDER_STATUS_HISTORY : records
    USER ||--o{ RFQ : requests
    RFQ ||--|{ RFQ_ITEM : contains
    CATALOG_PRODUCT ||--o{ RFQ_ITEM : requests
    UNIT_OF_MEASURE ||--o{ RFQ_ITEM : measures
    RFQ ||--o{ QUOTATION : receives
    USER ||--o{ QUOTATION : submits
    QUOTATION ||--|{ QUOTATION_ITEM : offers
    RFQ_ITEM ||--o{ QUOTATION_ITEM : compares
    QUOTATION_ITEM ||--o{ QUOTATION_ALLOCATION : accepts
    RFQ_ITEM ||--o{ QUOTATION_ALLOCATION : fulfills
    ORDER ||--o{ QUOTATION_ALLOCATION : creates
    USER ||--o{ REVIEW : writes
    PRODUCT ||--o{ REVIEW : receives
    USER ||--o{ MESSAGE : sends
    USER ||--o{ MESSAGE : receives
    USER ||--o{ NOTIFICATION : receives
```

All trading foreign keys use restricted deletion so historical records cannot be cascaded away. Row-version columns protect mutable aggregates. Unique operation keys make inventory, status, allocation, and notification side effects idempotent.
