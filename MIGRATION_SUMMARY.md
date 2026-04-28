# Database Migration Summary

## Migration Applied: AddCustomersAndOrders
**Timestamp:** 20260428073737  
**Status:** ✅ Successfully Applied

---

## Changes Made

### 1. **Products Table Update**
- Modified `Name` column constraints from `text` to `character varying(200)` with `maxLength: 200`

### 2. **New Table: Customers**
```sql
CREATE TABLE "Customers" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "CreateDate" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_Customers" PRIMARY KEY ("Id")
);
```

**Domain Model:** `Service.Domain.Customers.Customer`
- `Id` (Guid) - Primary Key, generated as Version 7 UUID
- `Name` (string, max 200) - Customer name (required)
- `CreateDate` (DateTimeOffset) - Creation timestamp, auto-set to UTC now

---

### 3. **New Table: Orders**
```sql
CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "CreateDate" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId") 
        REFERENCES "Customers" ("Id") ON DELETE RESTRICT
);
```

**Domain Model:** `Service.Domain.Orders.Order`
- `Id` (Guid) - Primary Key, generated as Version 7 UUID
- `CustomerId` (Guid) - Foreign Key to Customers (OnDelete: Restrict)
- `CreateDate` (DateTimeOffset) - Creation timestamp, auto-set to UTC now
- `Items` (IReadOnlyCollection<OrderItem>) - Navigation property for order items

**Index Created:**
- `IX_Orders_CustomerId` on CustomerId column

---

### 4. **New Table: OrderItems**
```sql
CREATE TABLE "OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") 
        REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") 
        REFERENCES "Products" ("Id") ON DELETE RESTRICT
);
```

**Domain Model:** `Service.Domain.Orders.OrderItem`
- `Id` (Guid) - Primary Key, generated as Version 7 UUID
- `OrderId` (Guid) - Foreign Key to Orders (OnDelete: Cascade)
- `ProductId` (Guid) - Foreign Key to Products (OnDelete: Restrict)
- `Quantity` (int) - Number of items in order
- `UnitPrice` (decimal) - Price per unit at time of order
- `TotalPrice` (decimal) - Computed property (Quantity * UnitPrice), not persisted

**Indexes Created:**
- `IX_OrderItems_OrderId` on OrderId column
- `IX_OrderItems_ProductId` on ProductId column

---

## Database Relationships

```
Customers (1) ──────── (Many) Orders
    ↑
    │ OnDelete: Restrict

Orders (1) ──────── (Many) OrderItems
    ↑
    │ OnDelete: Cascade

Products (1) ──────── (Many) OrderItems
    ↑
    │ OnDelete: Restrict
```

---

## Key Features

✅ **Referential Integrity:**
- Customer cannot be deleted if they have orders (Restrict)
- Product cannot be deleted if it's in order items (Restrict)
- When an order is deleted, all its items are automatically deleted (Cascade)

✅ **Audit Trail:**
- Both Customers and Orders have auto-timestamped `CreateDate` fields

✅ **UUID Primary Keys:**
- All entities use GUID Version 7 for ID generation (UUIDv7)

✅ **Type Safety:**
- Domain models with validated factory methods (`Create`)
- Enforce invariants at domain level

---

## Files Involved

### Domain Models
- `src/Service.Domain/Customers/Customer.cs`
- `src/Service.Domain/Orders/Order.cs`
- `src/Service.Domain/Orders/OrderItem.cs`

### Infrastructure
- `src/Service.Infrastructure/Persistence/AppDbContext.cs` - Entity configurations
- `src/Service.Infrastructure/Migrations/20260428073737_AddCustomersAndOrders.cs` - Migration script

### DbContext Configuration
```csharp
public DbSet<Customer> Customers => Set<Customer>();
public DbSet<Order> Orders => Set<Order>();
public DbSet<OrderItem> OrderItems => Set<OrderItem>();
public DbSet<Product> Products => Set<Product>();
```

---

## Next Steps

1. **Create Repositories** - Implement `ICustomerRepository` and `IOrderRepository`
2. **Create DTOs** - Define `CustomerDto`, `OrderDto`, `OrderItemDto`
3. **Create Use Cases** - Commands/Queries for CRUD operations
4. **Create API Endpoints** - Map domain operations to HTTP endpoints
5. **Add Tests** - Unit tests for new domain models and integration tests for repositories

---

## Rollback Command (if needed)

```powershell
dotnet ef database update <PreviousMigration> --project src/Service.Infrastructure/Service.Infrastructure.csproj --startup-project src/Service.Api/Service.Api.csproj
```

Example to rollback to initial migration:
```powershell
dotnet ef database update 20260326133244_Init --project src/Service.Infrastructure/Service.Infrastructure.csproj --startup-project src/Service.Api/Service.Api.csproj
```
