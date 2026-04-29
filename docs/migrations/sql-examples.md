# Migration SQL Examples

This document shows the SQL that gets generated for common migration scenarios. Use these as reference when reviewing generated migrations.

## Current Database Schema

### Products Table

```sql
CREATE TABLE "Products" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Price" numeric NOT NULL,
    CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
);
```

### Customers Table

```sql
CREATE TABLE "Customers" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "CreateDate" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_Customers" PRIMARY KEY ("Id")
);
```

### Orders Table

```sql
CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "CreateDate" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId")
        REFERENCES "Customers" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");
```

### OrderItems Table

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

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
CREATE INDEX "IX_OrderItems_ProductId" ON "OrderItems" ("ProductId");
```

---

## Example 1: Add Email Column to Customers

### C# Domain Model Change

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;  // ← NEW
    public DateTimeOffset CreateDate { get; private set; }
}
```

### Generated Migration SQL

```sql
-- Migration: 20260505120000_AddEmailToCustomer

ALTER TABLE "Customers" ADD "Email" character varying(255) NOT NULL DEFAULT '';

-- Update constraint name if needed
UPDATE "__EFMigrationsHistory"
SET "MigrationId" = '20260505120000_AddEmailToCustomer'
WHERE "MigrationId" LIKE '20260505120000_%';
```

### Down Migration (Rollback)

```sql
ALTER TABLE "Customers" DROP COLUMN "Email";
```

---

## Example 2: Add Stock Column to Products with Default Value

### C# Domain Model Change

```csharp
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }  // ← NEW
}
```

### DbContext Configuration

```csharp
entity.Property(p => p.Stock)
    .IsRequired()
    .HasDefaultValue(0);
```

### Generated Migration SQL

```sql
-- Migration: 20260505130000_AddStockToProduct

ALTER TABLE "Products" ADD "Stock" integer NOT NULL DEFAULT 0;
```

### Down Migration (Rollback)

```sql
ALTER TABLE "Products" DROP COLUMN "Stock";
```

---

## Example 3: Create New Category Table and Entity

### C# Domain Model

```csharp
public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
}
```

### DbContext Configuration

```csharp
public DbSet<Category> Categories => Set<Category>();

modelBuilder.Entity<Category>(entity =>
{
    entity.HasKey(c => c.Id);
    entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
    entity.Property(c => c.Description).HasMaxLength(500);
});
```

### Generated Migration SQL

```sql
-- Migration: 20260505140000_AddCategoryTable

CREATE TABLE "Categories" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);
```

### Down Migration (Rollback)

```sql
DROP TABLE "Categories";
```

---

## Example 4: Add Foreign Key from Products to Categories

### C# Domain Model Change

```csharp
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid? CategoryId { get; private set; }  // ← NEW FK
    public Category? Category { get; private set; }  // ← Navigation
}
```

### DbContext Configuration

```csharp
entity.HasOne(p => p.Category)
    .WithMany()
    .HasForeignKey(p => p.CategoryId)
    .OnDelete(DeleteBehavior.SetNull);
```

### Generated Migration SQL

```sql
-- Migration: 20260505150000_AddCategoryForeignKeyToProduct

ALTER TABLE "Products" ADD "CategoryId" uuid NULL;

ALTER TABLE "Products"
ADD CONSTRAINT "FK_Products_Categories_CategoryId"
FOREIGN KEY ("CategoryId")
REFERENCES "Categories" ("Id")
ON DELETE SET NULL;

CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
```

### Down Migration (Rollback)

```sql
DROP INDEX "IX_Products_CategoryId" ON "Products";

ALTER TABLE "Products"
DROP CONSTRAINT "FK_Products_Categories_CategoryId";

ALTER TABLE "Products" DROP COLUMN "CategoryId";
```

---

## Example 5: Change Column from Required to Optional

### C# Domain Model Change

```csharp
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string? Notes { get; private set; }  // Changed from string to string?
    public DateTimeOffset CreateDate { get; private set; }
}
```

### DbContext Configuration

```csharp
entity.Property(o => o.Notes)
    .IsRequired(false)      // Changed from IsRequired() to IsRequired(false)
    .HasMaxLength(1000);
```

### Generated Migration SQL

```sql
-- Migration: 20260505160000_MakeOrderNotesOptional

ALTER TABLE "Orders" ALTER COLUMN "Notes" DROP NOT NULL;
```

### Down Migration (Rollback)

```sql
ALTER TABLE "Orders" ALTER COLUMN "Notes" SET NOT NULL;
```

---

## Example 6: Add Required Column to Table with Existing Data

### C# Domain Model Change

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;  // ← NEW REQUIRED
    public DateTimeOffset CreateDate { get; private set; }
}
```

### DbContext Configuration

```csharp
entity.Property(c => c.PhoneNumber)
    .IsRequired()
    .HasMaxLength(20);
```

### Generated Migration SQL (needs modification)

**⚠️ Important**: When adding a required column to a table with existing data, you MUST provide a default value!

**Bad (will fail on existing data):**
```sql
ALTER TABLE "Customers" ADD "PhoneNumber" character varying(20) NOT NULL;
```

**Good (provides default for existing rows):**
```sql
ALTER TABLE "Customers" 
ADD "PhoneNumber" character varying(20) NOT NULL DEFAULT '000-0000';
```

### Down Migration (Rollback)

```sql
ALTER TABLE "Customers" DROP COLUMN "PhoneNumber";
```

---

## Example 7: Add Unique Constraint

### DbContext Configuration

```csharp
modelBuilder.Entity<Customer>(entity =>
{
    entity.HasKey(c => c.Id);
    entity.Property(c => c.Name).IsRequired().HasMaxLength(200);

    // Add unique constraint on Email
    entity.HasIndex(c => c.Email).IsUnique();
});
```

### Generated Migration SQL

```sql
-- Migration: 20260505170000_AddUniqueConstraintOnCustomerEmail

CREATE UNIQUE INDEX "IX_Customers_Email" ON "Customers" ("Email");
```

### Down Migration (Rollback)

```sql
DROP INDEX "IX_Customers_Email" ON "Customers";
```

---

## Example 8: Add Composite Index

### DbContext Configuration

```csharp
modelBuilder.Entity<Order>(entity =>
{
    // Create composite index on CustomerId and CreateDate
    entity.HasIndex(o => new { o.CustomerId, o.CreateDate })
        .HasDatabaseName("IX_Orders_CustomerId_CreateDate");
});
```

### Generated Migration SQL

```sql
-- Migration: 20260505180000_AddCompositeIndexOnOrders

CREATE INDEX "IX_Orders_CustomerId_CreateDate" 
ON "Orders" ("CustomerId", "CreateDate");
```

### Down Migration (Rollback)

```sql
DROP INDEX "IX_Orders_CustomerId_CreateDate" ON "Orders";
```

---

## Example 9: Rename a Column

### DbContext Configuration (Transitional)

```csharp
entity.Property(p => p.NewName)
    .HasColumnName("OldName")  // Maps property to old column name
    .IsRequired();
```

### Manual Migration (Recommended for production)

**Instead of relying on EF's drop/add, use rename:**

```sql
-- Migration: 20260505190000_RenameProductNameToTitle

-- For PostgreSQL:
ALTER TABLE "Products" RENAME COLUMN "Name" TO "Title";

-- For SQL Server:
EXEC sp_rename 'Products.Name', 'Title', 'COLUMN';
```

### Down Migration (Rollback)

```sql
-- For PostgreSQL:
ALTER TABLE "Products" RENAME COLUMN "Title" TO "Name";

-- For SQL Server:
EXEC sp_rename 'Products.Title', 'Name', 'COLUMN';
```

---

## Example 10: Add Check Constraint

### DbContext Configuration

```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.ToTable(t => t.HasCheckConstraint(
        "CK_Products_Price_GreaterThanZero",
        "\"Price\" > 0"));
});
```

### Generated Migration SQL

```sql
-- Migration: 20260505200000_AddCheckConstraintOnProduct

ALTER TABLE "Products"
ADD CONSTRAINT "CK_Products_Price_GreaterThanZero" CHECK ("Price" > 0);
```

### Down Migration (Rollback)

```sql
ALTER TABLE "Products" 
DROP CONSTRAINT "CK_Products_Price_GreaterThanZero";
```

---

## Migration Naming Conventions

| Change Type | Good Names | Bad Names |
|------------|-----------|----------|
| Add column | `AddEmailToCustomer` | `Update`, `Fix` |
| Create table | `AddCategoryTable` | `NewTable` |
| Remove column | `RemovePhoneFromCustomer` | `Delete` |
| Add FK | `AddCategoryForeignKeyToProduct` | `Relate` |
| Change type | `ChangeProductPriceToDecimal` | `Alter` |
| Add index | `AddIndexOnCustomerEmail` | `Performance` |
| Rename | `RenameProductNameToTitle` | `Rename` |

---

## SQL Validation Commands

After applying a migration, verify the changes:

### PostgreSQL

```sql
-- List all tables
\dt

-- Describe table structure
\d products

-- List foreign keys
SELECT * FROM information_schema.table_constraints 
WHERE table_name = 'orders' AND constraint_type = 'FOREIGN KEY';

-- List indexes
SELECT * FROM pg_indexes WHERE tablename = 'products';

-- Verify column properties
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'customers';
```

### SQL Server

```sql
-- List all tables
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Describe table structure
EXEC sp_help 'Products';

-- List foreign keys
SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS;

-- List indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Products');

-- Verify column properties
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers';
```

---

## Tips & Best Practices

1. **Always review generated SQL** before applying to production
2. **Test on staging** before deploying to production
3. **Use meaningful migration names** that describe what changed
4. **Add comments** in migration code for complex changes
5. **Keep migrations small** - one logical change per migration
6. **Never modify applied migrations** - create new one to fix
7. **Use `defaultValue`** when adding required columns to existing tables
8. **Document breaking changes** in commit message
9. **Backup database** before applying production migrations
10. **Version control your migrations** - commit them with code changes
