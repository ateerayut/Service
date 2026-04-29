# Database Migration Quick Reference

This is a quick reference guide for common migration tasks. For detailed explanations, see [README.md](README.md#database-migrations).

## Quick Commands

### Create a Migration

```powershell
dotnet ef migrations add MigrationName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Apply Migrations

```powershell
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Rollback to Previous Migration

```powershell
dotnet ef database update PreviousMigrationName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Remove Last Unapplied Migration

```powershell
dotnet ef migrations remove `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### List All Migrations

```powershell
dotnet ef migrations list `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Generate SQL Script

```powershell
# All pending migrations
dotnet ef migrations script `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj > migration.sql

# Specific range
dotnet ef migrations script FromMigration ToMigration `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj > migration.sql
```

---

## Common Patterns

### Pattern 1: Add a Column to Existing Table

**1. Edit Entity** - `src/Service.Domain/Products/Product.cs`:
```csharp
public string Sku { get; private set; } = string.Empty;
```

**2. Update DbContext** - `src/Service.Infrastructure/Persistence/AppDbContext.cs`:
```csharp
entity.Property(p => p.Sku)
    .IsRequired()
    .HasMaxLength(50);
```

**3. Create & Apply Migration**:
```powershell
dotnet ef migrations add AddSkuToProduct `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

---

### Pattern 2: Create New Entity and Table

**1. Create Entity** - `src/Service.Domain/Categories/Category.cs`:
```csharp
namespace Service.Domain.Categories;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Category() { }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Category
        {
            Id = Guid.CreateVersion7(),
            Name = name
        };
    }
}
```

**2. Update DbContext** - `src/Service.Infrastructure/Persistence/AppDbContext.cs`:
```csharp
public DbSet<Category> Categories => Set<Category>();

// In OnModelCreating()
modelBuilder.Entity<Category>(entity =>
{
    entity.HasKey(c => c.Id);
    entity.Property(c => c.Name)
        .IsRequired()
        .HasMaxLength(100);
});
```

**3. Create & Apply Migration**:
```powershell
dotnet ef migrations add AddCategoryTable `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

---

### Pattern 3: Add Foreign Key Relationship

**1. Update Entity** - `src/Service.Domain/Products/Product.cs`:
```csharp
public Guid? CategoryId { get; private set; }
public Category? Category { get; private set; }
```

**2. Update DbContext** - `src/Service.Infrastructure/Persistence/AppDbContext.cs`:
```csharp
entity.HasOne(p => p.Category)
    .WithMany()
    .HasForeignKey(p => p.CategoryId)
    .OnDelete(DeleteBehavior.SetNull);
```

**3. Create & Apply Migration**:
```powershell
dotnet ef migrations add AddCategoryForeignKeyToProduct `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

---

### Pattern 4: Add Required Column to Existing Table (with data)

**1. Update Entity** - `src/Service.Domain/Customers/Customer.cs`:
```csharp
public string Email { get; private set; } = string.Empty;
```

**2. Update DbContext** - `src/Service.Infrastructure/Persistence/AppDbContext.cs`:
```csharp
entity.Property(c => c.Email)
    .IsRequired()
    .HasMaxLength(255);
```

**3. Create Migration**:
```powershell
dotnet ef migrations add AddEmailToCustomer `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

**4. Edit Generated Migration** - `src/Service.Infrastructure/Migrations/<timestamp>_AddEmailToCustomer.cs`:

Before (will fail if table has data):
```csharp
migrationBuilder.AddColumn<string>(
    name: "Email",
    table: "Customers",
    nullable: false);
```

After (add default value):
```csharp
migrationBuilder.AddColumn<string>(
    name: "Email",
    table: "Customers",
    nullable: false,
    defaultValue: "no-email@example.com");
```

**5. Apply Migration**:
```powershell
dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

---

### Pattern 5: Make Column Optional (Required → Optional)

**1. Update Entity** - `src/Service.Domain/Orders/Order.cs`:
```csharp
public string? Notes { get; private set; }
```

**2. Update DbContext** - `src/Service.Infrastructure/Persistence/AppDbContext.cs`:
```csharp
entity.Property(o => o.Notes)
    .IsRequired(false)
    .HasMaxLength(1000);
```

**3. Create & Apply Migration**:
```powershell
dotnet ef migrations add MakeOrderNotesOptional `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

dotnet ef database update `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

---

### Pattern 6: Rename a Column

**1. Update Entity** - Change property name
**2. Update DbContext** - Use `.HasColumnName()` for old name:
```csharp
entity.Property(p => p.NewName)
    .HasColumnName("OldColumnName")
    .IsRequired();
```

**3. Create Migration** - EF will detect it as drop + add
**4. Edit Migration** - Replace with rename operation:
```csharp
migrationBuilder.RenameColumn(
    name: "OldColumnName",
    table: "Products",
    newName: "NewColumnName");
```

**5. Apply Migration**

---

## Production Deployment Workflow

### 1. Generate SQL Script

```powershell
# Generate all pending migrations as SQL
dotnet ef migrations script `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj > deployment_migration.sql

# Generate from specific range
dotnet ef migrations script 20260326133244_Init 20260505120000_AddEmailToCustomer `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj > deployment_migration.sql
```

### 2. Review SQL Script

```powershell
# Open and review the generated SQL
notepad deployment_migration.sql
```

### 3. Backup Production Database

```sql
-- For PostgreSQL
pg_dump -U postgres -d ServiceDb > backup_20260505.sql

-- For SQL Server
BACKUP DATABASE [ServiceDb] TO DISK = 'backup_20260505.bak'
```

### 4. Execute Migration Script

Connect to production database and execute the reviewed SQL script

### 5. Verify

```sql
-- Check if migrations were applied
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC;
```

---

## Troubleshooting

### Migration "Pending" but Database Already Updated

The `__EFMigrationsHistory` table might be out of sync.

```powershell
# Mark migration as applied without running
dotnet ef migrations script MigrationName MigrationName `
    --idempotent
```

### Get Error: "The entity type 'X' requires a primary key"

Make sure you have `entity.HasKey()` configured in `OnModelCreating()`:

```csharp
modelBuilder.Entity<Customer>(entity =>
{
    entity.HasKey(c => c.Id);  // ← Required
    // ... rest of config
});
```

### Cannot Create Migration - "No DbSet Found"

Make sure you added the DbSet to `AppDbContext`:

```csharp
public DbSet<MyEntity> MyEntities => Set<MyEntity>();
```

### Migration Name Already Exists

If you created a migration with the same name before:

```powershell
# Remove it first
dotnet ef migrations remove `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj

# Then create again with new name
dotnet ef migrations add UniqueNewName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj
```

### Need to Run Specific Migration Test

```powershell
# Create test database
dotnet ef database update MigrationName `
    --project src/Service.Infrastructure/Service.Infrastructure.csproj `
    --startup-project src/Service.Api/Service.Api.csproj `
    --connection "Server=localhost;Database=TestDb;User Id=sa;Password=test;"
```

---

## File Locations

- **Entity Models**: `src/Service.Domain/<Feature>/<Entity>.cs`
- **DbContext**: `src/Service.Infrastructure/Persistence/AppDbContext.cs`
- **Migrations**: `src/Service.Infrastructure/Migrations/<timestamp>_<MigrationName>.cs`
- **Migration History**: `__EFMigrationsHistory` table in database

---

## Environment Variables

For non-development environments, set connection string via environment variable:

```powershell
# PowerShell
$env:ConnectionStrings__Default="Server=prod-db;Database=ServiceDb;User Id=sa;Password=***;"

# Bash
export ConnectionStrings__Default="Server=prod-db;Database=ServiceDb;User Id=sa;Password=***;"
```

---

## Additional Resources

- [EF Core Migrations Official Docs](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Change Tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- [Project README - Database Migrations Section](README.md#database-migrations)
